using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Avalonia.Layout;
using System;
using System.IO;
using System.Linq;

namespace OpenSourceToolkit.NET.Controls
{
    public class FileDroppedEventArgs : EventArgs
    {
        public string Path { get; set; }
        public string Name { get; set; }
    }

    public partial class DragDropTextBox : UserControl
    {
        public static readonly StyledProperty<string> TextProperty =
            AvaloniaProperty.Register<DragDropTextBox, string>(nameof(Text), defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

        public string Text
        {
            get => GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public static readonly StyledProperty<string> WatermarkProperty =
            AvaloniaProperty.Register<DragDropTextBox, string>(nameof(Watermark));

        public string Watermark
        {
            get => GetValue(WatermarkProperty);
            set => SetValue(WatermarkProperty, value);
        }

        public static readonly StyledProperty<bool> AcceptsReturnProperty =
            AvaloniaProperty.Register<DragDropTextBox, bool>(nameof(AcceptsReturn), defaultValue: true);

        public bool AcceptsReturn
        {
            get => GetValue(AcceptsReturnProperty);
            set => SetValue(AcceptsReturnProperty, value);
        }

        public static readonly StyledProperty<TextWrapping> TextWrappingProperty =
            AvaloniaProperty.Register<DragDropTextBox, TextWrapping>(nameof(TextWrapping), defaultValue: TextWrapping.Wrap);

        public TextWrapping TextWrapping
        {
            get => GetValue(TextWrappingProperty);
            set => SetValue(TextWrappingProperty, value);
        }

        public static readonly new StyledProperty<FontFamily> FontFamilyProperty =
            AvaloniaProperty.Register<DragDropTextBox, FontFamily>(nameof(FontFamily), defaultValue: new FontFamily("Consolas,Monospace"));

        public new FontFamily FontFamily
        {
            get => GetValue(FontFamilyProperty);
            set => SetValue(FontFamilyProperty, value);
        }

        public static readonly new StyledProperty<double> FontSizeProperty =
            AvaloniaProperty.Register<DragDropTextBox, double>(nameof(FontSize), defaultValue: 13d);

        public new double FontSize
        {
            get => GetValue(FontSizeProperty);
            set => SetValue(FontSizeProperty, value);
        }

        public static readonly new StyledProperty<VerticalAlignment> VerticalContentAlignmentProperty =
            AvaloniaProperty.Register<DragDropTextBox, VerticalAlignment>(nameof(VerticalContentAlignment), defaultValue: VerticalAlignment.Top);

        public new VerticalAlignment VerticalContentAlignment
        {
            get => GetValue(VerticalContentAlignmentProperty);
            set => SetValue(VerticalContentAlignmentProperty, value);
        }

        public static readonly StyledProperty<Thickness> TextPaddingProperty =
            AvaloniaProperty.Register<DragDropTextBox, Thickness>(nameof(TextPadding), defaultValue: new Thickness(8));

        public Thickness TextPadding
        {
            get => GetValue(TextPaddingProperty);
            set => SetValue(TextPaddingProperty, value);
        }

        public static readonly StyledProperty<double> MinimumEditorHeightProperty =
            AvaloniaProperty.Register<DragDropTextBox, double>(nameof(MinimumEditorHeight), defaultValue: 140d);

        public double MinimumEditorHeight
        {
            get => GetValue(MinimumEditorHeightProperty);
            set => SetValue(MinimumEditorHeightProperty, value);
        }

        public static readonly StyledProperty<bool> IsDragOverProperty =
            AvaloniaProperty.Register<DragDropTextBox, bool>(nameof(IsDragOver));

        public bool IsDragOver
        {
            get => GetValue(IsDragOverProperty);
            set => SetValue(IsDragOverProperty, value);
        }

        public static readonly StyledProperty<string> DropTitleProperty =
            AvaloniaProperty.Register<DragDropTextBox, string>(nameof(DropTitle), "Drop file or text");

        public string DropTitle
        {
            get => GetValue(DropTitleProperty);
            set => SetValue(DropTitleProperty, value);
        }

        public static readonly StyledProperty<string> DropHintProperty =
            AvaloniaProperty.Register<DragDropTextBox, string>(nameof(DropHint), "We will read the file content or dropped text for you.");

        public string DropHint
        {
            get => GetValue(DropHintProperty);
            set => SetValue(DropHintProperty, value);
        }

        public static readonly StyledProperty<bool> ShowLineNumbersProperty =
            AvaloniaProperty.Register<DragDropTextBox, bool>(nameof(ShowLineNumbers), defaultValue: false);

        public bool ShowLineNumbers
        {
            get => GetValue(ShowLineNumbersProperty);
            set => SetValue(ShowLineNumbersProperty, value);
        }

        public TextBox InnerTextBox => PART_TextBox;
        public LineNumberTextBox LineNumberEditor => PART_LineNumberTextBox;

        public event EventHandler<FileDroppedEventArgs> FileDropped;

        public DragDropTextBox()
        {
            InitializeComponent();

            // Default to top-aligned content so multi-line inputs start at the top without requiring consumer overrides.
            VerticalContentAlignment = VerticalAlignment.Top;

            DragDrop.SetAllowDrop(this, true);
            DragDrop.SetAllowDrop(InnerTextBox, true);
            DragDrop.SetAllowDrop(LineNumberEditor, true);
            if (LineNumberEditor?.InnerTextBox != null)
            {
                DragDrop.SetAllowDrop(LineNumberEditor.InnerTextBox, true);
            }

            AddHandler(DragDrop.DragEnterEvent, OnDragEnter);
            AddHandler(DragDrop.DragOverEvent, OnDragOver);
            AddHandler(DragDrop.DragLeaveEvent, OnDragLeave);
            AddHandler(DragDrop.DropEvent, OnDrop);
        }

        private void OnDragEnter(object sender, DragEventArgs e)
        {
            UpdateDragState(e);
        }

        private void OnDragOver(object sender, DragEventArgs e)
        {
            UpdateDragState(e);
        }

        private void UpdateDragState(DragEventArgs e)
        {
            if (HasSupportedData(e))
            {
                e.DragEffects = DragDropEffects.Copy;
                IsDragOver = true;
            }
            else
            {
                e.DragEffects = DragDropEffects.None;
                IsDragOver = false;
            }
            e.Handled = true;
        }

        private void OnDragLeave(object sender, DragEventArgs e)
        {
            IsDragOver = false;
        }

        private async void OnDrop(object sender, DragEventArgs e)
        {
            IsDragOver = false;
            if (!HasSupportedData(e))
                return;

            e.Handled = true;

            var dataTransfer = e.DataTransfer;
            if (dataTransfer == null)
                return;

            if (dataTransfer.Contains(DataFormat.File))
            {
                var file = dataTransfer.TryGetFile() as IStorageFile
                    ?? (dataTransfer.TryGetFiles() ?? Array.Empty<IStorageItem>()).OfType<IStorageFile>().FirstOrDefault();

                if (file != null)
                {
                    var content = await ReadFileAsync(file);
                    if (content != null)
                    {
                        Text = content;
                        var localPath = file.TryGetLocalPath();
                        FileDropped?.Invoke(this, new FileDroppedEventArgs
                        {
                            Path = localPath,
                            Name = file.Name
                        });
                    }
                }
            }
            else if (dataTransfer.Contains(DataFormat.Text))
            {
                var text = dataTransfer.TryGetText();
                if (!string.IsNullOrEmpty(text))
                {
                    Text = text;
                }
            }
        }

        private async System.Threading.Tasks.Task<string> ReadFileAsync(IStorageFile file)
        {
            try
            {
                using (var stream = await file.OpenReadAsync())
                using (var reader = new StreamReader(stream))
                {
                    return await reader.ReadToEndAsync();
                }
            }
            catch
            {
                return null;
            }
        }

        private bool HasSupportedData(DragEventArgs e)
        {
            var dataTransfer = e.DataTransfer;
            if (dataTransfer == null)
                return false;

            return dataTransfer.Contains(DataFormat.File) || dataTransfer.Contains(DataFormat.Text);
        }

        /// <summary>
        /// Scrolls to the specified line number and centers it in the view.
        /// </summary>
        public void ScrollToLineCenter(int lineNumber)
        {
            if (ShowLineNumbers && LineNumberEditor != null)
            {
                LineNumberEditor.ScrollToLineCenter(lineNumber);
            }
            else if (InnerTextBox != null)
            {
                // For regular TextBox, just set caret - centering requires more work
                var text = Text ?? string.Empty;
                var lines = text.Split('\n');
                int charIndex = 0;
                for (int i = 0; i < lineNumber - 1 && i < lines.Length; i++)
                {
                    charIndex += lines[i].Length + 1;
                }
                InnerTextBox.CaretIndex = Math.Min(charIndex, text.Length);
                InnerTextBox.Focus();
            }
        }
    }
}
