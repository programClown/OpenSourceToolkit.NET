using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using OpenSourceToolkit.NET.ViewModels.Tools;
using System;
using System.Collections.Generic;
using System.IO;

namespace OpenSourceToolkit.NET.Views.Tools
{
    public partial class ClipboardImageSaverToolView : ToolViewBase
    {
        public ClipboardImageSaverToolView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        protected override void OnDataContextChanged(EventArgs e)
        {
            base.OnDataContextChanged(e);
            if (DataContext is ClipboardImageSaverToolViewModel vm)
            {
                vm.SaveImageAction = SaveImageToFile;
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
             base.OnKeyDown(e);
             if (e.KeyModifiers == KeyModifiers.Control && e.Key == Key.V)
             {
                 Paste_Click(this, new RoutedEventArgs());
                 e.Handled = true;
             }
        }

        private void Paste_Click(object sender, RoutedEventArgs e)
        {
            // Try System.Windows.Forms.Clipboard first as it's reliable for images on Windows net472
            if (System.Windows.Forms.Clipboard.ContainsImage())
            {
                try
                {
                    using (var image = System.Windows.Forms.Clipboard.GetImage())
                    {
                        if (image != null)
                        {
                            using (var ms = new MemoryStream())
                            {
                                // Save as PNG to preserve quality in transit
                                image.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                                var bytes = ms.ToArray();
                                (DataContext as ClipboardImageSaverToolViewModel)?.AddImageFromClipboard(bytes);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("WinForms Clipboard Error: " + ex.Message);
                }
            }
            else if (System.Windows.Forms.Clipboard.ContainsFileDropList())
            {
                 var files = System.Windows.Forms.Clipboard.GetFileDropList();
                 if (files.Count > 0)
                 {
                     try
                     {
                         var file = files[0];
                         var ext = Path.GetExtension(file).ToLower();
                         if (ext == ".png" || ext == ".jpg" || ext == ".jpeg" || ext == ".bmp" || ext == ".gif" || ext == ".webp")
                         {
                             var bytes = File.ReadAllBytes(file);
                             (DataContext as ClipboardImageSaverToolViewModel)?.AddImageFromClipboard(bytes);
                         }
                     }
                     catch (Exception ex)
                     {
                         Console.WriteLine("File Clipboard Error: " + ex.Message);
                     }
                 }
            }
        }

        private async void SaveImageToFile(byte[] data, string format)
        {
            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel == null) return;

            var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "Save Image",
                DefaultExtension = format,
                FileTypeChoices = new List<FilePickerFileType>
                {
                    new FilePickerFileType($"{format.ToUpper()} Image")
                    {
                        Patterns = new[] { $"*.{format}" }
                    }
                },
                SuggestedStartLocation = await GetStartFolderAsync()
            });

            if (file != null)
            {
                SaveLastFolder(file.Path.LocalPath);
                using (var stream = await file.OpenWriteAsync())
                {
                    await stream.WriteAsync(data, 0, data.Length);
                }
            }
        }
    }
}
