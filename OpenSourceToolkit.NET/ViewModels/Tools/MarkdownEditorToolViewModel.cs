using CommunityToolkit.Mvvm.Input;
using OpenSourceToolkit.NET.Localization;
using OpenSourceToolkit.TextData;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Input;

namespace OpenSourceToolkit.NET.ViewModels.Tools
{
    public class MarkdownTemplate
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Content { get; set; }
    }

    public class MarkdownEditorToolViewModel : ToolViewModel
    {
        public override int Id => 38;
        public override string Name => ToolkitLocalization.GetString("Tool_MarkdownEditor_Name");
        public override string Description => ToolkitLocalization.GetString("Tool_MarkdownEditor_Description");
        public override string IconKey => "MarkdownIcon";

        private string _markdown = "";
        public string Markdown
        {
            get => _markdown;
            set
            {
                var oldValue = _markdown;
                if (SetProperty(ref _markdown, value))
                {
                    // Record user typing with time-based grouping (not forced)
                    if (!_isUndoRedoOperation && !string.IsNullOrEmpty(oldValue))
                        _undoManager.RecordChange(oldValue, forceNewState: false);

                    UpdatePreview();
                }
            }
        }

        private string _htmlPreview = "";
        public string HtmlPreview
        {
            get => _htmlPreview;
            set => SetProperty(ref _htmlPreview, value);
        }

        private bool _enableTables = true;
        public bool EnableTables
        {
            get => _enableTables;
            set
            {
                if (SetProperty(ref _enableTables, value))
                    UpdatePreview();
            }
        }

        private bool _enableTaskLists = true;
        public bool EnableTaskLists
        {
            get => _enableTaskLists;
            set
            {
                if (SetProperty(ref _enableTaskLists, value))
                    UpdatePreview();
            }
        }

        private bool _enableLinting = true;
        public bool EnableLinting
        {
            get => _enableLinting;
            set
            {
                if (SetProperty(ref _enableLinting, value))
                    UpdateLintResults();
            }
        }

        public ObservableCollection<MarkdownLintViolation> LintViolations { get; } = new ObservableCollection<MarkdownLintViolation>();

        private int _violationCount;
        public int ViolationCount
        {
            get => _violationCount;
            set => SetProperty(ref _violationCount, value);
        }

        private bool _hasViolations;
        public bool HasViolations
        {
            get => _hasViolations;
            set => SetProperty(ref _hasViolations, value);
        }

        private bool _isSettingsExpanded;
        public bool IsSettingsExpanded
        {
            get => _isSettingsExpanded;
            set
            {
                if (SetProperty(ref _isSettingsExpanded, value))
                    SetSetting(nameof(IsSettingsExpanded), value);
            }
        }

        // Linter section expand state is dynamically controlled by whether violations exist, not persisted
        private bool _isLinterExpanded;
        public bool IsLinterExpanded
        {
            get => _isLinterExpanded;
            set => SetProperty(ref _isLinterExpanded, value);
        }

        private bool _isGuideExpanded;
        public bool IsGuideExpanded
        {
            get => _isGuideExpanded;
            set
            {
                if (SetProperty(ref _isGuideExpanded, value))
                    SetSetting(nameof(IsGuideExpanded), value);
            }
        }

        private bool _showHtmlPreview;
        public bool ShowHtmlPreview
        {
            get => _showHtmlPreview;
            set => SetProperty(ref _showHtmlPreview, value);
        }

        // Editor font size (6-24 in steps of 2)
        private int _editorFontSize;
        public int EditorFontSize
        {
            get => _editorFontSize;
            set
            {
                var clamped = Math.Max(6, Math.Min(24, value));
                if (SetProperty(ref _editorFontSize, clamped))
                {
                    SetSetting(nameof(EditorFontSize), clamped);
                    OnPropertyChanged(nameof(EditorLineHeight));
                    IncreaseFontSizeCommand.NotifyCanExecuteChanged();
                    DecreaseFontSizeCommand.NotifyCanExecuteChanged();
                }
            }
        }

        // Line height computed from font size (approx 1.5x font size for readable spacing)
        public double EditorLineHeight => Math.Round(_editorFontSize * 1.5);

        private string _currentFileName;
        /// <summary>
        /// The filename of the currently loaded file (without path), or null if no file loaded.
        /// </summary>
        public string CurrentFileName
        {
            get => _currentFileName;
            set => SetProperty(ref _currentFileName, value);
        }

        private string _currentFilePath;
        /// <summary>
        /// The full path of the currently loaded file, used for save dialog default.
        /// </summary>
        public string CurrentFilePath
        {
            get => _currentFilePath;
            set => SetProperty(ref _currentFilePath, value);
        }

        // ═══════════════════════════════════════════════════════════════════════════
        // Undo/Redo System
        // Time-based grouping: consecutive edits within 800ms are merged.
        // Programmatic changes (auto-fix, template, file load) always create new state.
        // ═══════════════════════════════════════════════════════════════════════════

        private readonly TextUndoManager _undoManager = new TextUndoManager();
        private bool _isUndoRedoOperation; // Prevents recording during undo/redo

        public bool CanUndo => _undoManager.CanUndo;
        public bool CanRedo => _undoManager.CanRedo;
        public string UndoTooltip => CanUndo ? $"Undo ({_undoManager.UndoCount} states)" : "Undo (nothing to undo)";
        public string RedoTooltip => CanRedo ? $"Redo ({_undoManager.RedoCount} states)" : "Redo (nothing to redo)";

        public List<MarkdownTemplate> Templates { get; } = new List<MarkdownTemplate>
        {
            new MarkdownTemplate
            {
                Name = "README",
                Description = "Basic README structure",
                Content = @"# Project Name

## Description

A brief description of what this project does.

## Installation

```bash
npm install project-name
```

## Usage

```javascript
const example = require('project-name');
example.doSomething();
```

## Features

- Feature 1
- Feature 2
- Feature 3

## License

[MIT](https://choosealicense.com/licenses/mit/)
"
            },
            new MarkdownTemplate
            {
                Name = "Documentation",
                Description = "API documentation template",
                Content = @"# API Documentation

## Overview

This document describes the API endpoints.

## Authentication

```bash
curl -H ""Authorization: Bearer YOUR_API_KEY"" https://api.example.com/endpoint
```

## Endpoints

### GET /users

Retrieve a list of users.

**Parameters:**

- `limit` (optional): Number of users to return
- `offset` (optional): Number of users to skip

**Response:**

```json
{
  ""users"": [
    { ""id"": 1, ""name"": ""John Doe"" }
  ]
}
```
"
            },
            new MarkdownTemplate
            {
                Name = "Blog Post",
                Description = "Blog post structure",
                Content = @"# Blog Post Title

*Published on: " + DateTime.Now.ToString("d") + @"*

## Introduction

Write an engaging introduction here.

## Main Content

### Section 1

Your first main point.

> ""A meaningful quote.""

### Section 2

- Bullet points for clarity
- **Bold text** for emphasis
- *Italic text* for subtle emphasis

## Conclusion

Summarize your key points.

---

*Tags: #markdown #writing*
"
            },
            new MarkdownTemplate
            {
                Name = "Table Example",
                Description = "Table formatting",
                Content = @"# Tables in Markdown

## Basic Table

| Name | Age | City |
|------|-----|------|
| John | 25 | New York |
| Jane | 30 | London |

## Task List

- [x] Completed task
- [x] Another completed task
- [ ] Incomplete task
- [ ] Another incomplete task
"
            }
        };

        public ICommand ClearCommand { get; }
        public ICommand LoadTemplateCommand { get; }
        public ICommand InsertBoldCommand { get; }
        public ICommand InsertItalicCommand { get; }
        public ICommand InsertCodeCommand { get; }
        public ICommand InsertLinkCommand { get; }
        public ICommand InsertH1Command { get; }
        public ICommand InsertH2Command { get; }
        public ICommand InsertListCommand { get; }
        public ICommand InsertQuoteCommand { get; }
        public ICommand RunLinterCommand { get; }
        public ICommand AutoFixCommand { get; }
        public ICommand FixViolationCommand { get; }
        public ICommand OpenFileCommand { get; }
        public ICommand SaveFileCommand { get; }
        public ICommand GoToViolationCommand { get; }
        public RelayCommand UndoCommand { get; }
        public RelayCommand RedoCommand { get; }
        public RelayCommand IncreaseFontSizeCommand { get; }
        public RelayCommand DecreaseFontSizeCommand { get; }

        /// <summary>
        /// Action to open file picker, wired from View code-behind.
        /// Returns the selected file path or null if cancelled.
        /// </summary>
        public Func<System.Threading.Tasks.Task<string>> OpenFileAction { get; set; }

        /// <summary>
        /// Action to save file picker, wired from View code-behind.
        /// Accepts a suggested filename and returns the selected file path or null if cancelled.
        /// </summary>
        public Func<string, System.Threading.Tasks.Task<string>> SaveFileAction { get; set; }

        /// <summary>
        /// Action to scroll editor to a specific line number, wired from View code-behind.
        /// </summary>
        public Action<int> ScrollToLineAction { get; set; }

        /// <summary>
        /// Action to insert text at cursor position, wired from View code-behind.
        /// Parameters: prefix, suffix, placeholderText
        /// </summary>
        public Action<string, string, string> InsertAtCursorAction { get; set; }

        public MarkdownEditorToolViewModel()
        {
            // Load persisted collapsible section states (linter state is dynamic, not persisted)
            _isSettingsExpanded = GetSetting(nameof(IsSettingsExpanded), true);
            _isGuideExpanded = GetSetting(nameof(IsGuideExpanded), false);
            _editorFontSize = GetSetting(nameof(EditorFontSize), 13);

            ClearCommand = new RelayCommand(ExecuteClear);
            LoadTemplateCommand = new RelayCommand<MarkdownTemplate>(LoadTemplate);
            InsertBoldCommand = new RelayCommand(() => InsertMarkdown("**", "**"));
            InsertItalicCommand = new RelayCommand(() => InsertMarkdown("*", "*"));
            InsertCodeCommand = new RelayCommand(() => InsertMarkdown("`", "`"));
            InsertLinkCommand = new RelayCommand(() => InsertMarkdown("[", "](url)"));
            InsertH1Command = new RelayCommand(() => InsertMarkdown("# ", ""));
            InsertH2Command = new RelayCommand(() => InsertMarkdown("## ", ""));
            InsertListCommand = new RelayCommand(() => InsertMarkdown("- ", ""));
            InsertQuoteCommand = new RelayCommand(() => InsertMarkdown("> ", ""));
            RunLinterCommand = new RelayCommand(UpdateLintResults);
            AutoFixCommand = new RelayCommand(AutoFixViolations, () => HasViolations);
            FixViolationCommand = new RelayCommand<MarkdownLintViolation>(FixSingleViolation);
            OpenFileCommand = new AsyncRelayCommand(OpenFileAsync);
            SaveFileCommand = new AsyncRelayCommand(SaveFileAsync);
            GoToViolationCommand = new RelayCommand<MarkdownLintViolation>(GoToViolation);
            UndoCommand = new RelayCommand(ExecuteUndo, () => CanUndo);
            RedoCommand = new RelayCommand(ExecuteRedo, () => CanRedo);
            IncreaseFontSizeCommand = new RelayCommand(() => EditorFontSize += 2, () => EditorFontSize < 24);
            DecreaseFontSizeCommand = new RelayCommand(() => EditorFontSize -= 2, () => EditorFontSize > 6);

            _undoManager.StateChanged += (s, e) => UpdateUndoRedoState();

            UpdatePreview();
        }

        private void ExecuteClear()
        {
            if (!string.IsNullOrEmpty(Markdown))
            {
                _undoManager.RecordChange(Markdown, forceNewState: true);
                SetMarkdownWithoutRecording("");
            }
            // Reset file tracking
            CurrentFileName = null;
            CurrentFilePath = null;
        }

        private void ExecuteUndo()
        {
            var previousText = _undoManager.Undo(Markdown);
            if (previousText != null)
                SetMarkdownWithoutRecording(previousText);
        }

        private void ExecuteRedo()
        {
            var nextText = _undoManager.Redo(Markdown);
            if (nextText != null)
                SetMarkdownWithoutRecording(nextText);
        }

        /// <summary>
        /// Sets Markdown without recording to undo history (used during undo/redo operations).
        /// </summary>
        private void SetMarkdownWithoutRecording(string text)
        {
            _isUndoRedoOperation = true;
            Markdown = text;
            _isUndoRedoOperation = false;
        }

        private void UpdateUndoRedoState()
        {
            OnPropertyChanged(nameof(CanUndo));
            OnPropertyChanged(nameof(CanRedo));
            OnPropertyChanged(nameof(UndoTooltip));
            OnPropertyChanged(nameof(RedoTooltip));
            UndoCommand.NotifyCanExecuteChanged();
            RedoCommand.NotifyCanExecuteChanged();
        }

        /// <summary>
        /// Records current state before a programmatic change.
        /// </summary>
        private void RecordStateBeforeChange()
        {
            if (!string.IsNullOrEmpty(Markdown))
                _undoManager.RecordChange(Markdown, forceNewState: true);
        }

        private async System.Threading.Tasks.Task OpenFileAsync()
        {
            if (OpenFileAction == null)
                return;

            var filePath = await OpenFileAction();
            if (string.IsNullOrEmpty(filePath))
                return;

            try
            {
                RecordStateBeforeChange();
                SetMarkdownWithoutRecording(System.IO.File.ReadAllText(filePath));
                // Track the loaded file
                CurrentFilePath = filePath;
                CurrentFileName = System.IO.Path.GetFileName(filePath);
            }
            catch
            {
                // Ignore file read errors
            }
        }

        private async System.Threading.Tasks.Task SaveFileAsync()
        {
            if (SaveFileAction == null)
                return;

            // Use current filename as suggestion, or default to "document.md"
            var suggestedName = CurrentFileName ?? "document.md";
            var filePath = await SaveFileAction(suggestedName);
            if (string.IsNullOrEmpty(filePath))
                return;

            try
            {
                System.IO.File.WriteAllText(filePath, Markdown);
                // Update file tracking after successful save
                CurrentFilePath = filePath;
                CurrentFileName = System.IO.Path.GetFileName(filePath);
            }
            catch
            {
                // Ignore file write errors
            }
        }

        private void LoadTemplate(MarkdownTemplate template)
        {
            if (template != null)
            {
                RecordStateBeforeChange();
                SetMarkdownWithoutRecording(template.Content);
            }
        }

        private void InsertMarkdown(string prefix, string suffix)
        {
            InsertAtCursorAction?.Invoke(prefix, suffix, "text");
        }

        private void UpdatePreview()
        {
            HtmlPreview = ConvertMarkdownToHtml(Markdown);
            UpdateLintResults();
        }

        private void UpdateLintResults()
        {
            LintViolations.Clear();

            if (!EnableLinting || string.IsNullOrEmpty(Markdown))
            {
                ViolationCount = 0;
                HasViolations = false;
                IsLinterExpanded = false;
                ((RelayCommand)AutoFixCommand).NotifyCanExecuteChanged();
                return;
            }

            var violations = MarkdownLinter.Lint(Markdown);
            foreach (var v in violations)
                LintViolations.Add(v);

            ViolationCount = violations.Count;
            HasViolations = violations.Count > 0;
            // Auto-expand/collapse linter section based on violations
            IsLinterExpanded = HasViolations;
            ((RelayCommand)AutoFixCommand).NotifyCanExecuteChanged();
        }

        private void AutoFixViolations()
        {
            if (string.IsNullOrEmpty(Markdown))
                return;

            RecordStateBeforeChange();
            SetMarkdownWithoutRecording(MarkdownLinter.AutoFix(Markdown));
            UpdateLintResults();
        }

        /// <summary>
        /// Fixes a single linter violation by applying a targeted fix to the specific line.
        /// </summary>
        private void FixSingleViolation(MarkdownLintViolation violation)
        {
            if (violation == null || string.IsNullOrEmpty(Markdown))
                return;

            RecordStateBeforeChange();
            var fixedMarkdown = MarkdownLinter.FixSingleViolation(Markdown, violation);
            SetMarkdownWithoutRecording(fixedMarkdown);
            UpdateLintResults();
        }

        private void GoToViolation(MarkdownLintViolation violation)
        {
            if (violation != null)
                ScrollToLineAction?.Invoke(violation.LineNumber);
        }

        private string ConvertMarkdownToHtml(string md)
        {
            if (string.IsNullOrEmpty(md)) return "<p style='color: #888;'>Start typing to see the preview...</p>";

            var html = md;

            // Escape HTML
            html = html.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");

            // Code blocks (process first)
            html = Regex.Replace(html, @"```(\w+)?\r?\n([\s\S]*?)\r?\n```",
                m => $"<pre style='background:#2d2d2d;color:#f8f8f2;padding:12px;border-radius:6px;overflow-x:auto;font-family:Consolas,monospace;'><code>{m.Groups[2].Value.Trim()}</code></pre>");

            // Inline code
            html = Regex.Replace(html, "`([^`]+)`",
                "<code style='background:#3d3d3d;color:#f8f8f2;padding:2px 6px;border-radius:4px;font-family:Consolas,monospace;'>$1</code>");

            // Headers
            html = Regex.Replace(html, "^### (.+)$", "<h3 style='font-size:1.1em;font-weight:600;margin:16px 0 8px;'>$1</h3>", RegexOptions.Multiline);
            html = Regex.Replace(html, "^## (.+)$", "<h2 style='font-size:1.3em;font-weight:600;margin:20px 0 10px;'>$1</h2>", RegexOptions.Multiline);
            html = Regex.Replace(html, "^# (.+)$", "<h1 style='font-size:1.6em;font-weight:700;margin:24px 0 12px;'>$1</h1>", RegexOptions.Multiline);

            // Bold and Italic
            html = Regex.Replace(html, @"\*\*\*(.+?)\*\*\*", "<strong><em>$1</em></strong>");
            html = Regex.Replace(html, @"\*\*(.+?)\*\*", "<strong>$1</strong>");
            html = Regex.Replace(html, "__(.+?)__", "<strong>$1</strong>");
            html = Regex.Replace(html, @"\*(.+?)\*", "<em>$1</em>");
            html = Regex.Replace(html, "_(.+?)_", "<em>$1</em>");

            // Links
            html = Regex.Replace(html, @"\[([^\]]+)\]\(([^)]+)\)",
                "<a href='$2' style='color:#58a6ff;text-decoration:underline;'>$1</a>");

            // Blockquotes
            html = Regex.Replace(html, "^&gt; (.+)$",
                "<blockquote style='border-left:4px solid #444;padding-left:16px;margin:12px 0;color:#aaa;font-style:italic;'>$1</blockquote>",
                RegexOptions.Multiline);

            // Horizontal rules
            html = Regex.Replace(html, "^---$", "<hr style='border:none;border-top:1px solid #444;margin:20px 0;'/>", RegexOptions.Multiline);

            // Task lists
            if (EnableTaskLists)
            {
                html = Regex.Replace(html, @"^- \[x\] (.+)$",
                    "<div style='display:flex;align-items:center;gap:8px;margin:4px 0;'><input type='checkbox' checked disabled style='accent-color:#58a6ff;'/><span>$1</span></div>",
                    RegexOptions.Multiline);
                html = Regex.Replace(html, @"^- \[ \] (.+)$",
                    "<div style='display:flex;align-items:center;gap:8px;margin:4px 0;'><input type='checkbox' disabled/><span>$1</span></div>",
                    RegexOptions.Multiline);
            }

            // Unordered lists
            html = Regex.Replace(html, "^- (.+)$", "<li style='margin-left:20px;'>$1</li>", RegexOptions.Multiline);
            html = Regex.Replace(html, @"^\* (.+)$", "<li style='margin-left:20px;'>$1</li>", RegexOptions.Multiline);

            // Tables
            if (EnableTables)
            {
                html = ProcessTables(html);
            }

            // Paragraphs
            var paragraphs = html.Split(new[] { "\r\n\r\n", "\n\n" }, StringSplitOptions.RemoveEmptyEntries);
            var result = new StringBuilder();
            foreach (var p in paragraphs)
            {
                var trimmed = p.Trim();
                if (string.IsNullOrEmpty(trimmed)) continue;

                // Don't wrap if already an HTML element
                result.AppendLine(trimmed.StartsWith("<")
                    ? trimmed
                    : $"<p style='margin:12px 0;line-height:1.6;'>{trimmed.Replace("\n", "<br/>")}</p>");
            }

            return result.ToString();
        }

        private string ProcessTables(string html)
        {
            var lines = html.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var result = new StringBuilder();
            var tableRows = new List<string>();
            bool inTable = false;

            foreach (var line in lines)
            {
                var trimmed = line.Trim();

                if (trimmed.Contains("|") && trimmed.Split('|').Length > 2)
                {
                    var cells = trimmed.Split('|')
                        .Select(c => c.Trim())
                        .Where(c => !string.IsNullOrEmpty(c))
                        .ToArray();

                    // Skip separator lines
                    if (cells.All(c => Regex.IsMatch(c, "^:?-+:?$")))
                        continue;

                    if (!inTable)
                    {
                        inTable = true;
                        tableRows.Clear();
                    }

                    bool isHeader = tableRows.Count == 0;
                    var tag = isHeader ? "th" : "td";
                    var cellStyle = isHeader
                        ? "border:1px solid #444;padding:8px 12px;background:#2d2d2d;font-weight:600;text-align:left;"
                        : "border:1px solid #444;padding:8px 12px;";

                    var row = new StringBuilder("<tr>");
                    foreach (var cell in cells)
                    {
                        row.Append($"<{tag} style='{cellStyle}'>{cell}</{tag}>");
                    }
                    row.Append("</tr>");
                    tableRows.Add(row.ToString());
                }
                else
                {
                    if (inTable && tableRows.Count > 0)
                    {
                        result.AppendLine($"<table style='width:100%;border-collapse:collapse;margin:16px 0;'>{string.Join("", tableRows)}</table>");
                        inTable = false;
                        tableRows.Clear();
                    }
                    result.AppendLine(trimmed);
                }
            }

            if (inTable && tableRows.Count > 0)
            {
                result.AppendLine($"<table style='width:100%;border-collapse:collapse;margin:16px 0;'>{string.Join("", tableRows)}</table>");
            }

            return result.ToString();
        }


    }
}
