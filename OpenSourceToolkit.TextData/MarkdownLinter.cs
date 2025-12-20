using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace OpenSourceToolkit.TextData
{
    /// <summary>
    /// Represents a markdown lint violation.
    /// </summary>
    public class MarkdownLintViolation
    {
        public string RuleId { get; set; }
        public string RuleName { get; set; }
        public string Description { get; set; }
        public int LineNumber { get; set; }
        public string LineContent { get; set; }
        public bool CanAutoFix { get; set; }
    }

    /// <summary>
    /// A lightweight markdown linter implementing common markdownlint rules.
    /// </summary>
    public static class MarkdownLinter
    {
        // Shared regex patterns to avoid duplication
        private static readonly Regex HeadingPattern = new Regex(@"^#{1,6}\s", RegexOptions.Compiled);
        private static readonly Regex IndentedHeadingPattern = new Regex(@"^[ \t]+#{1,6}\s", RegexOptions.Compiled);
        private static readonly Regex UnorderedListPattern = new Regex(@"^\s*[-*+]\s", RegexOptions.Compiled);
        private static readonly Regex OrderedListPattern = new Regex(@"^\s*\d+\.\s", RegexOptions.Compiled);
        // MD004: Detect specific list markers
        private static readonly Regex AsteriskListPattern = new Regex(@"^\s*\*\s", RegexOptions.Compiled);
        private static readonly Regex PlusListPattern = new Regex(@"^\s*\+\s", RegexOptions.Compiled);
        private static readonly Regex TrailingSpacePattern = new Regex(@"[ \t]+$", RegexOptions.Compiled);
        private static readonly Regex MultipleSpaceBlockquotePattern = new Regex(@"^>+\s{2,}", RegexOptions.Compiled);
        private static readonly Regex UnorderedListMultiSpacePattern = new Regex(@"^\s*[-*+]\s{2,}", RegexOptions.Compiled);
        private static readonly Regex OrderedListMultiSpacePattern = new Regex(@"^\s*\d+\.\s{2,}", RegexOptions.Compiled);
        // MD046: Detect code fence markers anywhere on a line
        private static readonly Regex FencePattern = new Regex("`{3,}|~{3,}", RegexOptions.Compiled);
        
        // MD037: Detect spaces inside emphasis - patterns with capture groups to extract content
        // Pattern for leading space: *<space(s)>content* - captures the content after spaces
        private static readonly Regex SpaceInEmphasisAsteriskLeading = new Regex(@"\*[ \t]+([^ \t*\r\n]+)\*", RegexOptions.Compiled);
        // Pattern for trailing space: *content<space(s)>* - captures the content before spaces
        private static readonly Regex SpaceInEmphasisAsteriskTrailing = new Regex(@"\*([^ \t*\r\n]+)[ \t]+\*", RegexOptions.Compiled);
        // Pattern for both: *<space(s)>content<space(s)>* - captures the content
        private static readonly Regex SpaceInEmphasisAsteriskBoth = new Regex(@"\*[ \t]+([^ \t*\r\n]+)[ \t]+\*", RegexOptions.Compiled);
        // Same patterns for underscore emphasis
        private static readonly Regex SpaceInEmphasisUnderscoreLeading = new Regex(@"_[ \t]+([^ \t_\r\n]+)_", RegexOptions.Compiled);
        private static readonly Regex SpaceInEmphasisUnderscoreTrailing = new Regex(@"_([^ \t_\r\n]+)[ \t]+_", RegexOptions.Compiled);
        private static readonly Regex SpaceInEmphasisUnderscoreBoth = new Regex(@"_[ \t]+([^ \t_\r\n]+)[ \t]+_", RegexOptions.Compiled);
        
        // MD038: Match code spans with leading or trailing spaces
        // Pattern for leading space only: backtick, spaces (violation), non-space content, backtick
        private static readonly Regex SpaceInCodeSpanLeading = new Regex(@"`[ \t]+([^ \t`\r\n]+)`", RegexOptions.Compiled);
        // Pattern for trailing space only: backtick, non-space content, spaces (violation), backtick  
        private static readonly Regex SpaceInCodeSpanTrailing = new Regex(@"`([^ \t`\r\n]+)[ \t]+`", RegexOptions.Compiled);
        // Pattern for both leading AND trailing spaces: backtick, spaces, non-space content, spaces, backtick
        private static readonly Regex SpaceInCodeSpanBoth = new Regex(@"`[ \t]+([^ \t`\r\n]+)[ \t]+`", RegexOptions.Compiled);

        private static bool IsListItem(string line) => UnorderedListPattern.IsMatch(line) || OrderedListPattern.IsMatch(line);
        private static bool IsHeading(string line) => HeadingPattern.IsMatch(line.TrimStart());
        
        /// <summary>
        /// Checks if a match is a real spacing violation or a false positive from matching across separate spans.
        /// Key insight: when matching across spans like `foo` and `bar`, the first marker in the match
        /// is preceded (without whitespace) by the content of a previous span, which itself is preceded by a marker.
        /// In a real violation like "text ` code `", the first marker is preceded by whitespace.
        /// </summary>
        private static bool IsRealViolation(string line, Match match, char marker)
        {
            // Scan backwards from match position to check if this is a closing marker
            // (part of a previous span) rather than an opening marker
            for (int i = match.Index - 1; i >= 0; i--)
            {
                if (line[i] == marker)
                {
                    // Found another marker before hitting whitespace - this match's first marker 
                    // is a closing marker of a previous span (false positive)
                    return false;
                }
                if (char.IsWhiteSpace(line[i]))
                {
                    // Hit whitespace before finding another marker - this is an opening marker (real violation)
                    break;
                }
            }
            
            return true;
        }

        /// <summary>
        /// Checks if a line has an emphasis span with spacing violations (MD037).
        /// Uses position-based detection to filter out false positives from matching across separate spans.
        /// </summary>
        private static bool HasEmphasisSpacingViolation(string line)
        {
            // Check asterisk patterns
            var asteriskPatterns = new[] { SpaceInEmphasisAsteriskLeading, SpaceInEmphasisAsteriskTrailing, SpaceInEmphasisAsteriskBoth };
            foreach (var pattern in asteriskPatterns)
            {
                var matches = pattern.Matches(line);
                foreach (Match match in matches)
                {
                    if (IsRealViolation(line, match, '*'))
                        return true;
                }
            }
            
            // Check underscore patterns
            var underscorePatterns = new[] { SpaceInEmphasisUnderscoreLeading, SpaceInEmphasisUnderscoreTrailing, SpaceInEmphasisUnderscoreBoth };
            foreach (var pattern in underscorePatterns)
            {
                var matches = pattern.Matches(line);
                foreach (Match match in matches)
                {
                    if (IsRealViolation(line, match, '_'))
                        return true;
                }
            }
            
            return false;
        }

        /// <summary>
        /// Checks if a line has a code span with spacing violations (MD038).
        /// Uses position-based detection to filter out false positives from matching across separate spans.
        /// </summary>
        private static bool HasCodeSpanSpacingViolation(string line)
        {
            foreach (var pattern in new[] { SpaceInCodeSpanLeading, SpaceInCodeSpanTrailing, SpaceInCodeSpanBoth })
            {
                var matches = pattern.Matches(line);
                foreach (Match match in matches)
                {
                    if (IsRealViolation(line, match, '`'))
                        return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Analyzes markdown content and returns a list of violations.
        /// </summary>
        public static List<MarkdownLintViolation> Lint(string markdown)
        {
            var violations = new List<MarkdownLintViolation>();
            if (string.IsNullOrEmpty(markdown))
                return violations;

            var lines = markdown.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            var ctx = new LintContext(lines);

            // Single pass through all lines
            for (int i = 0; i < lines.Length; i++)
            {
                ctx.SetLine(i);
                CheckLineViolations(ctx, violations);
            }

            // File-level check
            if (lines.Length > 0 && !string.IsNullOrEmpty(lines[lines.Length - 1]))
            {
                violations.Add(CreateViolation("MD047", "single-trailing-newline",
                    "Files should end with a single newline character", lines.Length, lines[lines.Length - 1]));
            }

            return violations.OrderBy(v => v.LineNumber).ToList();
        }

        /// <summary>
        /// Context for linting - tracks state as we process lines.
        /// </summary>
        private class LintContext
        {
            public string[] Lines { get; }
            private bool[] InCodeBlock { get; }
            public bool[] IsOpeningFence { get; }
            public bool[] IsClosingFence { get; }
            public int Index { get; private set; }
            public string Line => Lines[Index];
            public int LineNumber => Index + 1;
            public int BlankLineCount { get; set; }

            public bool IsInCodeBlock => InCodeBlock[Index];
            public bool PrevInCodeBlock => Index > 0 && InCodeBlock[Index - 1];
            public bool NextInCodeBlock => Index < Lines.Length - 1 && InCodeBlock[Index + 1];
            public string PrevLine => Index > 0 ? Lines[Index - 1] : "";
            public string NextLine => Index < Lines.Length - 1 ? Lines[Index + 1] : "";

            /// <summary>
            /// Checks if an opening fence at the given index has a matching closing fence.
            /// </summary>
            public bool HasMatchingCloseFence(int openingIndex)
            {
                if (!IsOpeningFence[openingIndex]) return false;
                // Find the next closing fence after this opening
                for (int j = openingIndex + 1; j < Lines.Length; j++)
                {
                    if (IsClosingFence[j]) return true;
                    if (IsOpeningFence[j]) return false; // Another opening before closing = unmatched
                }
                return false; // No closing fence found
            }

            public LintContext(string[] lines)
            {
                Lines = lines;
                (InCodeBlock, IsOpeningFence, IsClosingFence) = BuildCodeBlockMap(lines);
            }

            public void SetLine(int index) => Index = index;

            private static (bool[] inCodeBlock, bool[] isOpening, bool[] isClosing) BuildCodeBlockMap(string[] lines)
            {
                var map = new bool[lines.Length];
                var opening = new bool[lines.Length];
                var closing = new bool[lines.Length];

                bool inside = false;
                char? fenceChar = null; // Tracks the current fence character (` or ~)
                int fenceLength = 0;    // Tracks how many fence characters started the block

                for (int i = 0; i < lines.Length; i++)
                {
                    var line = lines[i];
                    var trimmed = line.TrimStart();

                    if (!inside)
                    {
                        // Detect opening fence: must start at column 0 (after indentation)
                        if (trimmed.StartsWith("```") || trimmed.StartsWith("~~~"))
                        {
                            char currentChar = trimmed[0]; // ` or ~
                            int runLength = 0;
                            while (runLength < trimmed.Length && trimmed[runLength] == currentChar)
                                runLength++;

                            opening[i] = true;
                            inside = true;
                            fenceChar = currentChar;
                            fenceLength = runLength;
                            map[i] = true;
                        }
                        else
                        {
                            map[i] = false;
                        }
                    }
                    else
                    {
                        // Inside a fenced code block – look for closing fence of the same character
                        // Closing fence must be at the start of line (after optional whitespace) and contain only fence chars
                        // If we're inside a code block, fenceChar and fenceLength are guaranteed to be set
                        var pattern = new string(fenceChar.Value, fenceLength);
                        // Valid closing fence: starts with fence pattern (after optional whitespace), nothing else after
                        if (trimmed.StartsWith(pattern))
                        {
                            // Check that after the fence there's only whitespace (valid closing)
                            var afterFence = trimmed.Substring(fenceLength);
                            if (string.IsNullOrWhiteSpace(afterFence))
                            {
                                closing[i] = true;
                                inside = false;
                                fenceChar = null;
                                fenceLength = 0;
                                map[i] = true;
                            }
                            else
                            {
                                // Has content after fence - still inside code block (malformed but we'll flag it)
                                map[i] = true;
                            }
                        }
                        else
                        {
                            map[i] = true;
                        }
                    }
                }

                return (map, opening, closing);
            }
        }

        private static void CheckLineViolations(LintContext ctx, List<MarkdownLintViolation> violations)
        {
            var line = ctx.Line;
            var i = ctx.Index;

            // MD009: Trailing spaces (check all lines)
            if (TrailingSpacePattern.IsMatch(line))
                violations.Add(CreateViolation("MD009", "no-trailing-spaces", "Trailing spaces", ctx.LineNumber, line));

            // MD010: Hard tabs (check all lines)
            if (line.Contains("\t"))
                violations.Add(CreateViolation("MD010", "no-hard-tabs", "Hard tabs", ctx.LineNumber, line));

            // MD012: Multiple consecutive blank lines
            if (string.IsNullOrWhiteSpace(line))
            {
                ctx.BlankLineCount++;
                if (ctx.BlankLineCount > 1)
                    violations.Add(CreateViolation("MD012", "no-multiple-blanks", "Multiple consecutive blank lines", ctx.LineNumber, line));
            }
            else
            {
                ctx.BlankLineCount = 0;
            }

            // MD046: Malformed code fence detection - check for invalid fence syntax
            var fenceMatches = FencePattern.Matches(line);
            if (fenceMatches.Count > 0)
            {
                // Multiple fences on same line (e.g., "```text```xml") is always invalid
                if (fenceMatches.Count >= 2)
                {
                    violations.Add(CreateViolation("MD046", "code-block-style",
                        "Multiple code fences on same line - each fence must be on its own line", ctx.LineNumber, line));
                }
                else if (fenceMatches.Count == 1)
                {
                    var match = fenceMatches[0];
                    var beforeFence = line.Substring(0, match.Index).TrimStart();
                    var afterFence = line.Substring(match.Index + match.Length);

                    // Check if this looks like a closing fence (no content after, or only whitespace)
                    bool looksLikeClosing = string.IsNullOrWhiteSpace(afterFence) && string.IsNullOrEmpty(beforeFence.Trim());
                    // Check if this looks like an opening fence (optionally has language tag after)
                    // Language tag must be the ONLY content after fence (letters/digits, starting with letter)
                    var langMatch = Regex.Match(afterFence, "^([a-zA-Z][a-zA-Z0-9]*)");
                    bool hasValidLangTag = langMatch.Success && string.IsNullOrWhiteSpace(afterFence.Substring(langMatch.Length));
                    bool looksLikeOpening = string.IsNullOrEmpty(beforeFence) && (string.IsNullOrWhiteSpace(afterFence) || hasValidLangTag);

                    // Content before fence (not just whitespace) is invalid
                    if (!string.IsNullOrEmpty(beforeFence))
                    {
                        violations.Add(CreateViolation("MD046", "code-block-style",
                            "Code fence has content before it - fence must start at beginning of line", ctx.LineNumber, line));
                    }
                    // Content after fence that's not a valid language tag is invalid
                    else if (!looksLikeClosing && !looksLikeOpening && !string.IsNullOrWhiteSpace(afterFence))
                    {
                        violations.Add(CreateViolation("MD046", "code-block-style",
                            "Code fence has invalid content after it", ctx.LineNumber, line));
                    }
                }
            }

            // MD031: Fenced code blocks should be surrounded by blank lines (check before skipping code block content)
            // Only report if this fence is part of a properly paired open/close (has matching partner)
            // Check blank line before opening fence
            if (ctx.IsOpeningFence[i] && ctx.HasMatchingCloseFence(i) && i > 0 && !string.IsNullOrWhiteSpace(ctx.PrevLine))
                violations.Add(CreateViolation("MD031", "blanks-around-fences", "Fenced code blocks should be surrounded by blank lines [Above]", ctx.LineNumber, line));
            // Check blank line after closing fence
            if (ctx.IsClosingFence[i] && i < ctx.Lines.Length - 1 && !string.IsNullOrWhiteSpace(ctx.NextLine))
                violations.Add(CreateViolation("MD031", "blanks-around-fences", "Fenced code blocks should be surrounded by blank lines [Below]", ctx.LineNumber, line));

            // Skip remaining checks for code block lines
            if (ctx.IsInCodeBlock) return;

            // MD023: Headings must start at beginning of line
            if (IndentedHeadingPattern.IsMatch(line))
                violations.Add(CreateViolation("MD023", "heading-start-left", "Headings must start at the beginning of the line", ctx.LineNumber, line));

            // MD022: Headings should be surrounded by blank lines
            if (IsHeading(line))
            {
                if (i > 0 && !string.IsNullOrWhiteSpace(ctx.PrevLine))
                    violations.Add(CreateViolation("MD022", "blanks-around-headings", "Headings should be surrounded by blank lines [Above]", ctx.LineNumber, line));
                if (i < ctx.Lines.Length - 1 && !string.IsNullOrWhiteSpace(ctx.NextLine))
                    violations.Add(CreateViolation("MD022", "blanks-around-headings", "Headings should be surrounded by blank lines [Below]", ctx.LineNumber, line));
            }

            // MD027: Multiple spaces after blockquote symbol
            if (MultipleSpaceBlockquotePattern.IsMatch(line))
                violations.Add(CreateViolation("MD027", "no-multiple-space-blockquote", "Multiple spaces after blockquote symbol", ctx.LineNumber, line));

            // MD030: Multiple spaces after list markers
            if (UnorderedListMultiSpacePattern.IsMatch(line) || OrderedListMultiSpacePattern.IsMatch(line))
                violations.Add(CreateViolation("MD030", "list-marker-space", "Spaces after list markers", ctx.LineNumber, line));

            // MD004: Unordered list style (prefer dash over asterisk or plus)
            if (AsteriskListPattern.IsMatch(line))
                violations.Add(CreateViolation("MD004", "ul-style", "Unordered list style [Expected: dash; Actual: asterisk]", ctx.LineNumber, line));
            if (PlusListPattern.IsMatch(line))
                violations.Add(CreateViolation("MD004", "ul-style", "Unordered list style [Expected: dash; Actual: plus]", ctx.LineNumber, line));

            // MD032: Lists should be surrounded by blank lines
            if (IsListItem(line))
            {
                var prevIsListItem = !ctx.PrevInCodeBlock && IsListItem(ctx.PrevLine);
                var nextIsListItem = !ctx.NextInCodeBlock && IsListItem(ctx.NextLine);

                if (!prevIsListItem && i > 0 && !ctx.PrevInCodeBlock && !string.IsNullOrWhiteSpace(ctx.PrevLine))
                    violations.Add(CreateViolation("MD032", "blanks-around-lists", "Lists should be surrounded by blank lines", ctx.LineNumber, line));
                if (!nextIsListItem && i < ctx.Lines.Length - 1 && !ctx.NextInCodeBlock && !string.IsNullOrWhiteSpace(ctx.NextLine))
                    violations.Add(CreateViolation("MD032", "blanks-around-lists", "Lists should be surrounded by blank lines", ctx.LineNumber, line));
            }

            // MD037: Spaces inside emphasis markers (filters out false positives from adjacent spans)
            if (HasEmphasisSpacingViolation(line))
                violations.Add(CreateViolation("MD037", "no-space-in-emphasis", "Spaces inside emphasis markers", ctx.LineNumber, line));

            // MD038: Spaces inside code span elements (leading, trailing, or both)
            // Each pattern uses a capture group to extract the content, which we then verify contains code-like chars
            if (HasCodeSpanSpacingViolation(line))
                violations.Add(CreateViolation("MD038", "no-space-in-code", "Spaces inside code span elements", ctx.LineNumber, line));
        }

        private static MarkdownLintViolation CreateViolation(string ruleId, string ruleName, string description, int lineNumber, string lineContent)
        {
            return new MarkdownLintViolation
            {
                RuleId = ruleId,
                RuleName = ruleName,
                Description = description,
                LineNumber = lineNumber,
                LineContent = lineContent,
                CanAutoFix = true
            };
        }

        /// <summary>
        /// Fixes a single violation on a specific line.
        /// </summary>
        public static string FixSingleViolation(string markdown, MarkdownLintViolation violation)
        {
            if (string.IsNullOrEmpty(markdown) || violation == null)
                return markdown;

            var lines = markdown.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            int lineIndex = violation.LineNumber - 1;

            if (lineIndex < 0 || lineIndex >= lines.Length)
                return markdown;

            var line = lines[lineIndex];

            switch (violation.RuleId)
            {
                case "MD009": // Trailing spaces
                    lines[lineIndex] = TrailingSpacePattern.Replace(line, "");
                    return string.Join("\n", lines);
                case "MD010": // Hard tabs
                    lines[lineIndex] = line.Replace("\t", "    ");
                    return string.Join("\n", lines);
                case "MD023": // Indented heading
                    lines[lineIndex] = Regex.Replace(line, @"^[ \t]+(#{1,6}\s)", "$1");
                    return string.Join("\n", lines);
                case "MD027": // Multiple spaces after blockquote
                    lines[lineIndex] = Regex.Replace(line, @"^(>+)\s{2,}", "$1 ");
                    return string.Join("\n", lines);
                case "MD030": // Multiple spaces after list marker
                    {
                        var fixedLine = Regex.Replace(line, @"^(\s*[-*+])\s{2,}", "$1 ");
                        lines[lineIndex] = Regex.Replace(fixedLine, @"^(\s*\d+\.)\s{2,}", "$1 ");
                        return string.Join("\n", lines);
                    }
                case "MD004": // Unordered list style - convert asterisk/plus to dash
                    lines[lineIndex] = Regex.Replace(line, @"^(\s*)[*+](\s)", "$1-$2");
                    return string.Join("\n", lines);
                case "MD037": // Spaces inside emphasis - handle spaces on either or both sides
                    {
                        var fixedLine = Regex.Replace(line, @"\*\s+([^*]+?)\s+\*", "*$1*");
                        fixedLine = Regex.Replace(fixedLine, @"\*\s+([^*]+?)\*", "*$1*");
                        fixedLine = Regex.Replace(fixedLine, @"\*([^*]+?)\s+\*", "*$1*");
                        fixedLine = Regex.Replace(fixedLine, @"_\s+([^_]+?)\s+_", "_$1_");
                        fixedLine = Regex.Replace(fixedLine, @"_\s+([^_]+?)_", "_$1_");
                        lines[lineIndex] = Regex.Replace(fixedLine, @"_([^_]+?)\s+_", "_$1_");
                        return string.Join("\n", lines);
                    }
                case "MD038": // Spaces inside code span (handle leading, trailing, or both)
                    {
                        // Only fix code spans where content has no internal spaces (to avoid matching across spans)
                        // Fix both leading AND trailing spaces first: ` text ` -> `text`
                        var fixedLine = Regex.Replace(line, @"`[ \t]+([^ \t`\r\n]+)[ \t]+`", "`$1`");
                        // Remove leading spaces only: `  text` -> `text`
                        fixedLine = Regex.Replace(fixedLine, @"`[ \t]+([^ \t`\r\n]+)`", "`$1`");
                        // Remove trailing spaces only: `text  ` -> `text`
                        lines[lineIndex] = Regex.Replace(fixedLine, @"`([^ \t`\r\n]+)[ \t]+`", "`$1`");
                        return string.Join("\n", lines);
                    }
                case "MD012": // Multiple blank lines - remove this blank line
                    lines[lineIndex] = null; // Mark for removal
                    return string.Join("\n", lines.Where(l => l != null));
                case "MD046": // Malformed code fence - split into separate lines
                    var fenceMatches = FencePattern.Matches(line);
                    if (fenceMatches.Count >= 2)
                    {
                        // Multiple fences on same line - split at each fence
                        var result = new List<string>();
                        int lastEnd = 0;
                        foreach (Match m in fenceMatches)
                        {
                            // Content before this fence
                            if (m.Index > lastEnd)
                            {
                                var before = line.Substring(lastEnd, m.Index - lastEnd).Trim();
                                if (!string.IsNullOrEmpty(before))
                                {
                                    // Ensure separation from prior fence/content
                                    if (result.Count > 0 && !string.IsNullOrWhiteSpace(result[result.Count - 1]))
                                        result.Add("");
                                    result.Add(before);
                                    // If the split content is a structural element (heading/list), also ensure a blank after it
                                    if (IsHeading(before) || IsListItem(before))
                                        result.Add("");
                                }
                            }
                            // The fence itself (with any language tag for opening fences)
                            var afterFence = line.Substring(m.Index + m.Length);
                            // Treat as language tag ONLY if it's the sole token after the fence
                            var langMatch = Regex.Match(afterFence, "^([a-zA-Z][a-zA-Z0-9]*)");
                            bool langIsOnlyToken = langMatch.Success && string.IsNullOrWhiteSpace(afterFence.Substring(langMatch.Length));
                            // Add blank line before fence if there's content before
                            if (result.Count > 0 && !string.IsNullOrWhiteSpace(result[result.Count - 1]))
                                result.Add("");
                            // If fence is followed by a valid language tag, treat as opening fence
                            if (langIsOnlyToken)
                            {
                                result.Add(m.Value + langMatch.Value);
                                lastEnd = m.Index + m.Length + langMatch.Length;
                            }
                            else
                            {
                                result.Add(m.Value);
                                lastEnd = m.Index + m.Length;
                            }
                        }
                        // Content after last fence
                        if (lastEnd < line.Length)
                        {
                            var after = line.Substring(lastEnd).Trim();
                            if (!string.IsNullOrEmpty(after))
                            {
                                // Add blank line after closing fence before heading/list/content
                                result.Add("");
                                result.Add(after);
                            }
                        }
                        // Replace the line with multiple lines
                        var newLines = lines.Take(lineIndex).ToList();
                        newLines.AddRange(result);
                        newLines.AddRange(lines.Skip(lineIndex + 1));
                        return string.Join("\n", newLines);
                    }
                    else if (fenceMatches.Count == 1)
                    {
                        // Single fence with content before or after
                        var m = fenceMatches[0];
                        var before = line.Substring(0, m.Index).Trim();
                        var fence = m.Value;
                        var after = line.Substring(m.Index + m.Length);
                        // Check for language tag - valid only if it is the only token after the fence
                        var langMatch = Regex.Match(after, "^([a-zA-Z][a-zA-Z0-9]*)");
                        bool langIsOnlyToken = langMatch.Success && string.IsNullOrWhiteSpace(after.Substring(langMatch.Length));
                        string fenceWithLang = langIsOnlyToken ? fence + langMatch.Value : fence;
                        string remaining = langIsOnlyToken ? after.Substring(langMatch.Length).Trim() : after.Trim();

                        var result = new List<string>();
                        if (!string.IsNullOrEmpty(before))
                        {
                            result.Add(before);
                            result.Add(""); // Blank line before fence
                        }
                        result.Add(fenceWithLang);
                        if (!string.IsNullOrEmpty(remaining))
                        {
                            result.Add(""); // Blank line after fence
                            result.Add(remaining);
                        }

                        var newLines = lines.Take(lineIndex).ToList();
                        newLines.AddRange(result);
                        newLines.AddRange(lines.Skip(lineIndex + 1));
                        return string.Join("\n", newLines);
                    }
                    return markdown;
                case "MD022": // Blanks around headings
                    {
                        // Insert exactly one blank line above or below heading
                        var desc = violation.Description ?? string.Empty;
                        bool needsAbove = desc.IndexOf("[Above]", StringComparison.OrdinalIgnoreCase) >= 0;
                        bool needsBelow = desc.IndexOf("[Below]", StringComparison.OrdinalIgnoreCase) >= 0;

                        if (needsAbove)
                        {
                            if (lineIndex > 0 && !string.IsNullOrWhiteSpace(lines[lineIndex - 1]))
                            {
                                var newLines = lines.Take(lineIndex).ToList();
                                newLines.Add("");
                                newLines.AddRange(lines.Skip(lineIndex));
                                return string.Join("\n", newLines);
                            }
                            return markdown;
                        }

                        if (needsBelow)
                        {
                            if (lineIndex < lines.Length - 1 && !string.IsNullOrWhiteSpace(lines[lineIndex + 1]))
                            {
                                var newLines = lines.Take(lineIndex + 1).ToList();
                                newLines.Add("");
                                newLines.AddRange(lines.Skip(lineIndex + 1));
                                return string.Join("\n", newLines);
                            }
                        }

                        return markdown;
                    }
                case "MD031": // Blanks around fenced code blocks
                    {
                        // Insert exactly one blank line above opening fence or below closing fence
                        var desc = violation.Description ?? string.Empty;
                        bool needsAbove = desc.IndexOf("[Above]", StringComparison.OrdinalIgnoreCase) >= 0;
                        bool needsBelow = desc.IndexOf("[Below]", StringComparison.OrdinalIgnoreCase) >= 0;

                        if (needsAbove)
                        {
                            if (lineIndex > 0 && !string.IsNullOrWhiteSpace(lines[lineIndex - 1]))
                            {
                                var newLines = lines.Take(lineIndex).ToList();
                                newLines.Add("");
                                newLines.AddRange(lines.Skip(lineIndex));
                                return string.Join("\n", newLines);
                            }
                            return markdown;
                        }

                        if (needsBelow)
                        {
                            if (lineIndex < lines.Length - 1 && !string.IsNullOrWhiteSpace(lines[lineIndex + 1]))
                            {
                                var newLines = lines.Take(lineIndex + 1).ToList();
                                newLines.Add("");
                                newLines.AddRange(lines.Skip(lineIndex + 1));
                                return string.Join("\n", newLines);
                            }
                            return markdown;
                        }

                        // Fallback if direction is unknown: prefer adding below if needed, else above
                        if (lineIndex < lines.Length - 1 && !string.IsNullOrWhiteSpace(lines[lineIndex + 1]))
                        {
                            var newLines = lines.Take(lineIndex + 1).ToList();
                            newLines.Add("");
                            newLines.AddRange(lines.Skip(lineIndex + 1));
                            return string.Join("\n", newLines);
                        }
                        if (lineIndex > 0 && !string.IsNullOrWhiteSpace(lines[lineIndex - 1]))
                        {
                            var newLines = lines.Take(lineIndex).ToList();
                            newLines.Add("");
                            newLines.AddRange(lines.Skip(lineIndex));
                            return string.Join("\n", newLines);
                        }
                        return markdown;
                    }
                case "MD032": // Blanks around lists
                    {
                        // Insert exactly one blank line before or after list
                        // Check if we need blank above (previous line is not blank and not a list item)
                        if (lineIndex > 0 && !string.IsNullOrWhiteSpace(lines[lineIndex - 1]) && !IsListItem(lines[lineIndex - 1]))
                        {
                            var newLines = lines.Take(lineIndex).ToList();
                            newLines.Add("");
                            newLines.AddRange(lines.Skip(lineIndex));
                            return string.Join("\n", newLines);
                        }
                        // Check if we need blank below (next line is not blank and not a list item)
                        if (lineIndex < lines.Length - 1 && !string.IsNullOrWhiteSpace(lines[lineIndex + 1]) && !IsListItem(lines[lineIndex + 1]))
                        {
                            var newLines = lines.Take(lineIndex + 1).ToList();
                            newLines.Add("");
                            newLines.AddRange(lines.Skip(lineIndex + 1));
                            return string.Join("\n", newLines);
                        }
                        return markdown;
                    }
                case "MD047": // File should end with newline
                    if (!markdown.EndsWith("\n"))
                        return markdown + "\n";
                    return markdown;
                default:
                    return markdown;
            }
        }

        /// <summary>
        /// Attempts to auto-fix all violations by processing them bottom-to-top to avoid line number shifting.
        /// </summary>
        public static string AutoFix(string markdown)
        {
            if (string.IsNullOrEmpty(markdown))
                return markdown;

            var result = markdown;
            int iterations = 0;
            int initialViolations = Lint(result).Count;
            int maxIterations = initialViolations + 20;

            // Keep fixing until no more violations or max iterations reached
            while (iterations < maxIterations)
            {
                var violations = Lint(result);
                if (violations.Count == 0)
                    break;

                // Process bottom-to-top to avoid line number shifting
                var sortedViolations = violations.OrderByDescending(v => v.LineNumber).ToList();
                string before = result;

                foreach (var violation in sortedViolations)
                {
                    if (violation.CanAutoFix)
                        result = FixSingleViolation(result, violation);
                }

                // If no changes were made, stop
                if (result == before)
                    break;

                iterations++;
            }

            return result;
        }

        /// <summary>
        /// Attempts to auto-fix all violations using batch transformations (legacy method).
        /// </summary>
        public static string AutoFixBatch(string markdown)
        {
            if (string.IsNullOrEmpty(markdown))
                return markdown;

            var result = markdown;

            // MD046: Fix malformed code fences first (before other fixes)
            // Run in a loop until no more changes - splitting lines may reveal more issues
            string prev;
            int iterations = 0;
            do
            {
                prev = result;
                result = FixMalformedFences(result);
                iterations++;
            } while (result != prev && iterations < 10);

            // Regex-based fixes (can be applied globally)
            result = Regex.Replace(result, @"[ \t]+$", "", RegexOptions.Multiline);           // MD009: trailing spaces
            result = result.Replace("\t", "    ");                                             // MD010: hard tabs
            result = Regex.Replace(result, @"(\r?\n){3,}", "\n\n");                           // MD012: multiple blank lines
            result = Regex.Replace(result, @"^(>+)\s{2,}", "$1 ", RegexOptions.Multiline);    // MD027: blockquote spacing
            result = Regex.Replace(result, @"^(\s*)[*+](\s)", "$1-$2", RegexOptions.Multiline);      // MD004: unordered list style (dash)
            result = Regex.Replace(result, @"^(\s*[-*+])\s{2,}", "$1 ", RegexOptions.Multiline);   // MD030: list marker spacing
            result = Regex.Replace(result, @"^(\s*\d+\.)\s{2,}", "$1 ", RegexOptions.Multiline);   // MD030: ordered list spacing
            // MD037: emphasis spacing - handle spaces on either or both sides
            result = Regex.Replace(result, @"\*\s+([^*]+?)\s+\*", "*$1*");
            result = Regex.Replace(result, @"\*\s+([^*]+?)\*", "*$1*");
            result = Regex.Replace(result, @"\*([^*]+?)\s+\*", "*$1*");
            result = Regex.Replace(result, @"_\s+([^_]+?)\s+_", "_$1_");
            result = Regex.Replace(result, @"_\s+([^_]+?)_", "_$1_");
            result = Regex.Replace(result, @"_([^_]+?)\s+_", "_$1_");
            // MD038: code span spacing (only fix spans where content has no internal spaces)
            // Fix both leading AND trailing first, then each separately
            result = Regex.Replace(result, @"`[ \t]+([^ \t`\r\n]+)[ \t]+`", "`$1`");
            result = Regex.Replace(result, @"`[ \t]+([^ \t`\r\n]+)`", "`$1`");
            result = Regex.Replace(result, @"`([^ \t`\r\n]+)[ \t]+`", "`$1`");

            // Structure-aware fixes (single pass, respects code blocks)
            result = FixStructures(result);

            // MD047: file should end with newline
            if (!result.EndsWith("\n"))
                result += "\n";

            return result;
        }

        /// <summary>
        /// Fixes malformed code fences (MD046) - fences with content before/after or multiple fences on same line.
        /// </summary>
        private static string FixMalformedFences(string markdown)
        {
            var lines = markdown.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            var result = new List<string>();

            foreach (var line in lines)
            {
                var fenceMatches = FencePattern.Matches(line);

                if (fenceMatches.Count == 0)
                {
                    result.Add(line);
                    continue;
                }

                if (fenceMatches.Count >= 2)
                {
                    // Multiple fences on same line - split at each fence
                    int lastEnd = 0;
                    foreach (Match m in fenceMatches)
                    {
                        if (m.Index > lastEnd)
                        {
                            var before = line.Substring(lastEnd, m.Index - lastEnd).Trim();
                            if (!string.IsNullOrEmpty(before))
                            {
                                // Ensure separation from prior fence/content
                                if (result.Count > 0 && !string.IsNullOrWhiteSpace(result[result.Count - 1]))
                                    result.Add("");
                                result.Add(before);
                                // If the split content is a structural element (heading/list), also ensure a blank after it
                                if (IsHeading(before) || IsListItem(before))
                                    result.Add("");
                            }
                        }
                        var afterFence = line.Substring(m.Index + m.Length);
                        // Treat as language tag ONLY if it's the sole token after the fence
                        var langMatch = Regex.Match(afterFence, "^([a-zA-Z][a-zA-Z0-9]*)");
                        bool langIsOnlyToken = langMatch.Success && string.IsNullOrWhiteSpace(afterFence.Substring(langMatch.Length));
                        if (result.Count > 0 && !string.IsNullOrWhiteSpace(result[result.Count - 1]))
                            result.Add("");
                        // If fence is followed by a valid language tag, treat as opening fence
                        if (langIsOnlyToken)
                        {
                            result.Add(m.Value + langMatch.Value);
                            lastEnd = m.Index + m.Length + langMatch.Length;
                        }
                        else
                        {
                            result.Add(m.Value);
                            lastEnd = m.Index + m.Length;
                        }
                    }
                    if (lastEnd < line.Length)
                    {
                        var after = line.Substring(lastEnd).Trim();
                        if (!string.IsNullOrEmpty(after))
                        {
                            result.Add("");
                            result.Add(after);
                        }
                    }
                }
                else
                {
                    // Single fence - check for content before or after
                    var m = fenceMatches[0];
                    var before = line.Substring(0, m.Index).Trim();
                    var fence = m.Value;
                    var after = line.Substring(m.Index + m.Length);
                    // Treat as language tag ONLY if it's the sole token after the fence
                    var langMatch = Regex.Match(after, "^([a-zA-Z][a-zA-Z0-9]*)");
                    bool langIsOnlyToken = langMatch.Success && string.IsNullOrWhiteSpace(after.Substring(langMatch.Length));
                    string fenceWithLang = langIsOnlyToken ? fence + langMatch.Value : fence;
                    string remaining = langIsOnlyToken ? after.Substring(langMatch.Length).Trim() : after.Trim();

                    // Check if this is a valid fence line (no content before, only optional lang tag after)
                    bool isValidFence = string.IsNullOrEmpty(before) && string.IsNullOrEmpty(remaining);

                    if (isValidFence)
                    {
                        result.Add(line);
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(before))
                        {
                            result.Add(before);
                            result.Add("");
                        }
                        result.Add(fenceWithLang);
                        if (!string.IsNullOrEmpty(remaining))
                        {
                            result.Add("");
                            result.Add(remaining);
                        }
                    }
                }
            }

            return string.Join("\n", result);
        }

        /// <summary>
        /// Single-pass fix for MD022 (headings), MD023 (heading indent), MD031 (code fences), MD032 (lists).
        /// </summary>
        private static string FixStructures(string markdown)
        {
            var lines = markdown.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            var inCodeBlock = new bool[lines.Length];
            var isOpeningFence = new bool[lines.Length];
            var isClosingFence = new bool[lines.Length];
            bool inside = false;
            char? fenceChar = null;
            int fenceLength = 0;

            // Build code block map with explicit opening/closing fence tracking
            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                var trimmed = line.TrimStart();

                if (!inside)
                {
                    if (trimmed.StartsWith("```") || trimmed.StartsWith("~~~"))
                    {
                        char currentChar = trimmed[0];
                        int runLength = 0;
                        while (runLength < trimmed.Length && trimmed[runLength] == currentChar)
                            runLength++;

                        isOpeningFence[i] = true;
                        inside = true;
                        fenceChar = currentChar;
                        fenceLength = runLength;
                        inCodeBlock[i] = true;
                    }
                    else
                    {
                        inCodeBlock[i] = false;
                    }
                }
                else
                {
                    // If we're inside a code block, fenceChar and fenceLength are guaranteed to be set
                    var pattern = new string(fenceChar.Value, fenceLength);
                    // Valid closing fence: starts with fence pattern, nothing else after
                    if (trimmed.StartsWith(pattern) && string.IsNullOrWhiteSpace(trimmed.Substring(fenceLength)))
                    {
                        isClosingFence[i] = true;
                        inside = false;
                        fenceChar = null;
                        fenceLength = 0;
                        inCodeBlock[i] = true;
                    }
                    else
                    {
                        inCodeBlock[i] = true;
                    }
                }
            }

            var result = new List<string>();

            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                var prevInCode = i > 0 && inCodeBlock[i - 1];
                var nextInCode = i < lines.Length - 1 && inCodeBlock[i + 1];
                var prevLine = i > 0 ? lines[i - 1] : "";
                var nextLine = i < lines.Length - 1 ? lines[i + 1] : "";

                // MD031: blank line before opening code fence
                if (isOpeningFence[i] && i > 0 && !string.IsNullOrWhiteSpace(prevLine))
                {
                    if (result.Count > 0 && !string.IsNullOrWhiteSpace(result[result.Count - 1]))
                        result.Add("");
                }

                if (inCodeBlock[i])
                {
                    result.Add(line);
                    // MD031: blank line after closing code fence
                    if (isClosingFence[i] && i < lines.Length - 1 && !string.IsNullOrWhiteSpace(nextLine))
                        result.Add("");
                    continue;
                }

                // MD023: fix indented headings
                if (IndentedHeadingPattern.IsMatch(line))
                    line = Regex.Replace(line, @"^[ \t]+(#{1,6}\s)", "$1");

                var isHeading = IsHeading(line);
                var isListItem = IsListItem(line);
                var prevIsListItem = !prevInCode && IsListItem(prevLine);
                var prevIsHeading = !prevInCode && IsHeading(prevLine);
                var nextIsListItem = !nextInCode && IsListItem(nextLine);

                // Add blank before heading/list if needed
                bool needsBlankBeforeHeading = isHeading && i > 0 && !string.IsNullOrWhiteSpace(prevLine);
                bool needsBlankBeforeList = isListItem && !prevIsListItem && !prevIsHeading
                    && i > 0 && !prevInCode && !string.IsNullOrWhiteSpace(prevLine);
                bool needsBlankBefore = needsBlankBeforeHeading || needsBlankBeforeList;

                if (needsBlankBefore && result.Count > 0 && !string.IsNullOrWhiteSpace(result[result.Count - 1]))
                    result.Add("");

                result.Add(line);

                // Add blank after heading/list if needed
                // Note: headings always need blank after (even before code blocks)
                // Lists only need blank after if next line is not a list item
                bool needsBlankAfter = i < lines.Length - 1 && !string.IsNullOrWhiteSpace(nextLine)
                    && (isHeading || (isListItem && !nextIsListItem && !nextInCode));

                if (needsBlankAfter)
                    result.Add("");
            }

            return string.Join("\n", result);
        }
    }
}
