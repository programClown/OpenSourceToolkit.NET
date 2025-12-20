using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Platform.Storage;
using OpenSourceToolkit.NET.Controls;
using OpenSourceToolkit.NET.ViewModels.Tools;
using System;
using System.Threading.Tasks;

namespace OpenSourceToolkit.NET.Views.Tools
{
    public partial class MarkdownEditorToolView : UserControl
    {
        public MarkdownEditorToolView()
        {
            InitializeComponent();
            KeyDown += OnKeyDown;
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            var vm = DataContext as MarkdownEditorToolViewModel;
            if (vm == null)
                return;

            // Ctrl+Z = Undo, Ctrl+Y = Redo, Ctrl++/- = Font size
            if (e.KeyModifiers == KeyModifiers.Control)
            {
                if (e.Key == Key.Z && vm.UndoCommand.CanExecute(null))
                {
                    vm.UndoCommand.Execute(null);
                    e.Handled = true;
                }
                else if (e.Key == Key.Y && vm.RedoCommand.CanExecute(null))
                {
                    vm.RedoCommand.Execute(null);
                    e.Handled = true;
                }
                else if ((e.Key == Key.OemPlus || e.Key == Key.Add) && vm.IncreaseFontSizeCommand.CanExecute(null))
                {
                    vm.IncreaseFontSizeCommand.Execute(null);
                    e.Handled = true;
                }
                else if ((e.Key == Key.OemMinus || e.Key == Key.Subtract) && vm.DecreaseFontSizeCommand.CanExecute(null))
                {
                    vm.DecreaseFontSizeCommand.Execute(null);
                    e.Handled = true;
                }
            }
        }

        protected override void OnDataContextChanged(EventArgs e)
        {
            base.OnDataContextChanged(e);
            if (DataContext is MarkdownEditorToolViewModel vm)
            {
                vm.OpenFileAction = OpenMarkdownFileAsync;
                vm.SaveFileAction = suggestedFileName => SaveMarkdownFileAsync(suggestedFileName);
                vm.ScrollToLineAction = ScrollEditorToLine;
                vm.InsertAtCursorAction = InsertAtCursor;
            }
        }

        protected override void OnLoaded(Avalonia.Interactivity.RoutedEventArgs e)
        {
            base.OnLoaded(e);
            // EditorTextBox is source-generated from x:Name in AXAML
        }

        private void ScrollEditorToLine(int lineNumber)
        {
            // EditorTextBox is source-generated from x:Name in AXAML
            EditorTextBox.ScrollToLineCenter(lineNumber);
        }

        /// <summary>
        /// Inserts text at the current cursor position, replacing any selection.
        /// Places cursor between prefix and suffix for easy editing.
        /// </summary>
        private void InsertAtCursor(string prefix, string suffix, string placeholder)
        {
            var vm = DataContext as MarkdownEditorToolViewModel;
            if (vm == null)
                return;

            // Get the active inner TextBox from source-generated EditorTextBox
            var innerTextBox = EditorTextBox.ShowLineNumbers
                ? EditorTextBox.LineNumberEditor?.InnerTextBox
                : EditorTextBox.InnerTextBox;

            if (innerTextBox == null)
                return;

            var text = innerTextBox.Text ?? "";
            var selStart = innerTextBox.SelectionStart;
            var selLength = innerTextBox.SelectionEnd - innerTextBox.SelectionStart;

            // Get selected text or use placeholder
            var selectedText = selLength > 0 ? text.Substring(selStart, selLength) : placeholder;
            var insertText = prefix + selectedText + suffix;

            // Build new text
            var before = text.Substring(0, selStart);
            var after = text.Substring(selStart + selLength);
            var newText = before + insertText + after;

            // Calculate new caret position (after prefix, before suffix, with placeholder selected)
            var newCaretPos = selStart + prefix.Length;
            var newSelLength = selectedText.Length;

            // Update via ViewModel to maintain undo history
            vm.Markdown = newText;

            // Restore cursor position and select the placeholder/selected text
            innerTextBox.CaretIndex = newCaretPos;
            innerTextBox.SelectionStart = newCaretPos;
            innerTextBox.SelectionEnd = newCaretPos + newSelLength;
            innerTextBox.Focus();
        }

        private async Task<string> OpenMarkdownFileAsync()
        {
            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel == null)
                return null;

            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Open Markdown File",
                AllowMultiple = false,
                FileTypeFilter = new[]
                {
                    new FilePickerFileType("Markdown Files") { Patterns = new[] { "*.md" } }
                }
            });

            if (files != null && files.Count > 0)
            {
                return files[0].TryGetLocalPath();
            }

            return null;
        }

        private async Task<string> SaveMarkdownFileAsync(string suggestedFileName)
        {
            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel == null)
                return null;

            var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "Save Markdown File",
                SuggestedFileName = suggestedFileName ?? "document.md",
                FileTypeChoices = new[]
                {
                    new FilePickerFileType("Markdown Files") { Patterns = new[] { "*.md" } }
                }
            });

            return file?.TryGetLocalPath();
        }
    }
}
