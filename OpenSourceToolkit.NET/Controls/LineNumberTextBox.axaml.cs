using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.VisualTree;
using System;

namespace OpenSourceToolkit.NET.Controls
{
    public partial class LineNumberTextBox : UserControl
    {
        public static readonly StyledProperty<string> TextProperty =
            AvaloniaProperty.Register<LineNumberTextBox, string>(nameof(Text), defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

        public string Text
        {
            get => GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public static readonly StyledProperty<string> WatermarkProperty =
            AvaloniaProperty.Register<LineNumberTextBox, string>(nameof(Watermark));

        public string Watermark
        {
            get => GetValue(WatermarkProperty);
            set => SetValue(WatermarkProperty, value);
        }

        public static readonly StyledProperty<bool> AcceptsReturnProperty =
            AvaloniaProperty.Register<LineNumberTextBox, bool>(nameof(AcceptsReturn), defaultValue: true);

        public bool AcceptsReturn
        {
            get => GetValue(AcceptsReturnProperty);
            set => SetValue(AcceptsReturnProperty, value);
        }

        public static readonly StyledProperty<TextWrapping> TextWrappingProperty =
            AvaloniaProperty.Register<LineNumberTextBox, TextWrapping>(nameof(TextWrapping), defaultValue: TextWrapping.NoWrap);

        public TextWrapping TextWrapping
        {
            get => GetValue(TextWrappingProperty);
            set => SetValue(TextWrappingProperty, value);
        }

        public static readonly new StyledProperty<FontFamily> FontFamilyProperty =
            AvaloniaProperty.Register<LineNumberTextBox, FontFamily>(nameof(FontFamily), defaultValue: new FontFamily("Consolas,Monospace"));

        public new FontFamily FontFamily
        {
            get => GetValue(FontFamilyProperty);
            set => SetValue(FontFamilyProperty, value);
        }

        public static readonly new StyledProperty<double> FontSizeProperty =
            AvaloniaProperty.Register<LineNumberTextBox, double>(nameof(FontSize), defaultValue: 13d);

        public new double FontSize
        {
            get => GetValue(FontSizeProperty);
            set => SetValue(FontSizeProperty, value);
        }

        public static readonly new StyledProperty<VerticalAlignment> VerticalContentAlignmentProperty =
            AvaloniaProperty.Register<LineNumberTextBox, VerticalAlignment>(nameof(VerticalContentAlignment), defaultValue: VerticalAlignment.Top);

        public new VerticalAlignment VerticalContentAlignment
        {
            get => GetValue(VerticalContentAlignmentProperty);
            set => SetValue(VerticalContentAlignmentProperty, value);
        }

        public static readonly StyledProperty<double> MinimumEditorHeightProperty =
            AvaloniaProperty.Register<LineNumberTextBox, double>(nameof(MinimumEditorHeight), defaultValue: 140d);

        public double MinimumEditorHeight
        {
            get => GetValue(MinimumEditorHeightProperty);
            set => SetValue(MinimumEditorHeightProperty, value);
        }

        public static readonly StyledProperty<bool> ShowLineNumbersProperty =
            AvaloniaProperty.Register<LineNumberTextBox, bool>(nameof(ShowLineNumbers), defaultValue: true);

        public bool ShowLineNumbers
        {
            get => GetValue(ShowLineNumbersProperty);
            set => SetValue(ShowLineNumbersProperty, value);
        }

        public static readonly DirectProperty<LineNumberTextBox, string> LineNumbersTextProperty =
            AvaloniaProperty.RegisterDirect<LineNumberTextBox, string>(
                nameof(LineNumbersText),
                o => o.LineNumbersText);

        private string _lineNumbersText = "1";
        public string LineNumbersText
        {
            get => _lineNumbersText;
            private set => SetAndRaise(LineNumbersTextProperty, ref _lineNumbersText, value);
        }

        public TextBox InnerTextBox => PART_TextBox;

        private ScrollViewer _textBoxScrollViewer;
        private int _lastLineCount = 1;
        private bool _scrollSyncSetup;

        public LineNumberTextBox()
        {
            InitializeComponent();
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);

            if (change.Property == TextProperty)
            {
                UpdateLineNumbers();
            }
        }

        protected override void OnAttachedToVisualTree(Avalonia.VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);
            UpdateLineNumbers();

            // Defer scroll sync setup to allow TextBox template to be applied
            Avalonia.Threading.Dispatcher.UIThread.Post(() => SetupScrollSync(), Avalonia.Threading.DispatcherPriority.Loaded);
        }

        protected override void OnDetachedFromVisualTree(Avalonia.VisualTreeAttachmentEventArgs e)
        {
            base.OnDetachedFromVisualTree(e);
            CleanupScrollSync();
        }

        private void CleanupScrollSync()
        {
            if (_textBoxScrollViewer != null)
            {
                _textBoxScrollViewer.ScrollChanged -= OnTextBoxScrollChanged;
                _textBoxScrollViewer.PropertyChanged -= OnScrollViewerPropertyChanged;
            }
            _scrollSyncSetup = false;
        }

        private void SetupScrollSync()
        {
            if (_scrollSyncSetup || InnerTextBox == null)
                return;

            _textBoxScrollViewer = FindScrollViewer(InnerTextBox);
            if (_textBoxScrollViewer != null)
            {
                _textBoxScrollViewer.ScrollChanged += OnTextBoxScrollChanged;
                _textBoxScrollViewer.PropertyChanged += OnScrollViewerPropertyChanged;
                _scrollSyncSetup = true;

                // Initial sync
                SyncLineNumberScroll();
            }
        }

        private ScrollViewer FindScrollViewer(Visual visual)
        {
            if (visual is ScrollViewer sv)
                return sv;

            foreach (var child in visual.GetVisualChildren())
            {
                var result = FindScrollViewer(child);
                if (result != null)
                    return result;
            }
            return null;
        }

        private void OnScrollViewerPropertyChanged(object sender, AvaloniaPropertyChangedEventArgs e)
        {
            if (e.Property == ScrollViewer.OffsetProperty)
            {
                SyncLineNumberScroll();
            }
        }

        private void OnTextBoxScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            SyncLineNumberScroll();
        }

        private void SyncLineNumberScroll()
        {
            if (_textBoxScrollViewer != null)
            {
                PART_LineNumberScroller.Offset = new Vector(PART_LineNumberScroller.Offset.X, _textBoxScrollViewer.Offset.Y);
            }
        }

        private void UpdateLineNumbers()
        {
            var text = Text ?? string.Empty;
            var lineCount = 1;
            foreach (char c in text)
            {
                if (c == '\n')
                    lineCount++;
            }

            if (lineCount == _lastLineCount)
                return;

            _lastLineCount = lineCount;

            var maxDigits = lineCount.ToString().Length;
            var sb = new System.Text.StringBuilder();
            for (int i = 1; i <= lineCount; i++)
            {
                if (i > 1)
                    sb.Append('\n');
                sb.Append(i.ToString().PadLeft(maxDigits));
            }
            LineNumbersText = sb.ToString();
        }

        public int CaretIndex
        {
            get => InnerTextBox?.CaretIndex ?? 0;
            set
            {
                if (InnerTextBox != null)
                    InnerTextBox.CaretIndex = value;
            }
        }

        public int SelectionStart
        {
            get => InnerTextBox?.SelectionStart ?? 0;
            set
            {
                if (InnerTextBox != null)
                    InnerTextBox.SelectionStart = value;
            }
        }

        public int SelectionEnd
        {
            get => InnerTextBox?.SelectionEnd ?? 0;
            set
            {
                if (InnerTextBox != null)
                    InnerTextBox.SelectionEnd = value;
            }
        }

        public void Focus()
        {
            InnerTextBox?.Focus();
        }

        /// <summary>
        /// Scrolls to the specified line number and centers it in the view.
        /// </summary>
        public void ScrollToLineCenter(int lineNumber)
        {
            if (InnerTextBox == null || string.IsNullOrEmpty(Text))
                return;

            // Calculate character index for the target line
            var lines = Text.Split('\n');
            int charIndex = 0;
            for (int i = 0; i < lineNumber - 1 && i < lines.Length; i++)
            {
                charIndex += lines[i].Length + 1;
            }

            InnerTextBox.CaretIndex = Math.Min(charIndex, Text.Length);
            InnerTextBox.Focus();

            int targetLine = lineNumber;
            int totalLines = lines.Length;

            // Defer scroll adjustment - use Background priority to run after caret scroll completes
            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                if (_textBoxScrollViewer == null)
                    _textBoxScrollViewer = FindScrollViewer(InnerTextBox);

                if (_textBoxScrollViewer == null)
                    return;

                // Calculate line height from total extent and line count
                double extentHeight = _textBoxScrollViewer.Extent.Height;
                double viewportHeight = _textBoxScrollViewer.Viewport.Height;
                double lineHeight = totalLines > 1 ? extentHeight / totalLines : FontSize * 1.5;

                // Target line Y position (0-indexed)
                double targetLineY = (targetLine - 1) * lineHeight;

                // Calculate offset to center the line in the viewport
                double centeredOffset = targetLineY - (viewportHeight / 2) + (lineHeight / 2);

                // Clamp to valid scroll range
                double maxOffset = Math.Max(0, extentHeight - viewportHeight);
                centeredOffset = Math.Max(0, Math.Min(centeredOffset, maxOffset));

                _textBoxScrollViewer.Offset = new Vector(_textBoxScrollViewer.Offset.X, centeredOffset);
            }, Avalonia.Threading.DispatcherPriority.Background);
        }
    }
}
