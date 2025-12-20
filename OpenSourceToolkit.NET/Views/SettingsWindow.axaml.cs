using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using OpenSourceToolkit.NET.Services;
using OpenSourceToolkit.NET.ViewModels;

namespace OpenSourceToolkit.NET.Views
{
    public partial class SettingsWindow : Window
    {
        private SettingsViewModel _viewModel;

        public SettingsWindow()
        {
            AvaloniaXamlLoader.Load(this);
            _viewModel = new SettingsViewModel();
            _viewModel.PromptSaveChangesAction = PromptSaveChangesAsync;
#if DEBUG
            _viewModel.ShowDebugExceptionAction = ShowDebugException;
#endif
            DataContext = _viewModel;
        }

        private async Task<bool?> PromptSaveChangesAsync(string message)
        {
            var dialog = new Window
            {
                Title = "Unsaved Changes",
                Width = 350,
                Height = 150,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                CanResize = false
            };

            bool? result = null;
            var saveBtn = new Button { Content = "Save", Width = 80 };
            var discardBtn = new Button { Content = "Discard", Width = 80 };
            var cancelBtn = new Button { Content = "Cancel", Width = 80 };

            saveBtn.Click += (s, e) => { result = true; dialog.Close(); };
            discardBtn.Click += (s, e) => { result = false; dialog.Close(); };
            cancelBtn.Click += (s, e) => { result = null; dialog.Close(); };

            dialog.Content = new StackPanel
            {
                Margin = new Thickness(20),
                Spacing = 16,
                Children =
                {
                    new TextBlock { Text = message, TextWrapping = Avalonia.Media.TextWrapping.Wrap },
                    new StackPanel
                    {
                        Orientation = Avalonia.Layout.Orientation.Horizontal,
                        Spacing = 8,
                        HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
                        Children = { saveBtn, discardBtn, cancelBtn }
                    }
                }
            };

            await dialog.ShowDialog(this);
            return result;
        }

#if DEBUG
        private async void ShowDebugException(Exception ex)
        {
            var dialog = new Window
            {
                Title = "Debug: Exception Details",
                Width = 600,
                Height = 400,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Content = new ScrollViewer
                {
                    Content = new TextBox
                    {
                        Text = ex.ToString(),
                        IsReadOnly = true,
                        AcceptsReturn = true,
                        TextWrapping = Avalonia.Media.TextWrapping.Wrap
                    }
                }
            };
            await dialog.ShowDialog(this);
        }
#endif

        protected override void OnOpened(EventArgs e)
        {
            base.OnOpened(e);
            AdjustHeightToScreen();
        }

        /// <summary>
        /// Shrinks window height if it exceeds the screen's working area (excluding taskbar).
        /// Only shrinks, never enlarges the window.
        /// </summary>
        private void AdjustHeightToScreen()
        {
            var screen = Screens.ScreenFromWindow(this);
            if (screen == null) return;

            // Get working area (screen minus taskbar)
            var workingArea = screen.WorkingArea;
            var scaling = screen.Scaling;

            // Convert to DIPs (device-independent pixels)
            var maxHeight = workingArea.Height / scaling;

            // Leave some margin (20px top + 20px bottom)
            var availableHeight = maxHeight - 40;

            // Only shrink if window is too tall (never enlarge)
            if (Height > availableHeight)
            {
                // Respect MinHeight constraint
                Height = Math.Max(availableHeight, MinHeight);

                // Re-center vertically within working area
                var workingAreaTop = workingArea.Y / scaling;
                Position = new PixelPoint(Position.X, (int)((workingAreaTop + 20) * scaling));
            }
        }

        private async void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel != null && !await _viewModel.CanCloseAsync())
                return;

            AppSettings.Save();
            Close();
        }

        protected override async void OnClosing(WindowClosingEventArgs e)
        {
            if (_viewModel != null && _viewModel.HasUnsavedConnectionChanges)
            {
                e.Cancel = true;
                if (await _viewModel.CanCloseAsync())
                {
                    AppSettings.Save();
                    Close();
                }
                return;
            }
            base.OnClosing(e);
        }
    }
}
