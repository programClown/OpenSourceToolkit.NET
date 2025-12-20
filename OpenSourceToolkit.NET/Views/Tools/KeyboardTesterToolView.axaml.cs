using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using OpenSourceToolkit.NET.ViewModels.Tools;

namespace OpenSourceToolkit.NET.Views.Tools
{
    public partial class KeyboardTesterToolView : UserControl
    {
        public KeyboardTesterToolView()
        {
            InitializeComponent();

            // Ensure we can focus to receive key events
            Focusable = true;

            // Handle pointer press to focus automatically when clicked
            PointerPressed += (s, e) => Focus();

            KeyDown += OnKeyDown;
            KeyUp += OnKeyUp;
            TextInput += OnTextInput;
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (DataContext is KeyboardTesterToolViewModel vm)
            {
                vm.HandleKeyEvent(e, "KeyDown");
            }
        }

        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            if (DataContext is KeyboardTesterToolViewModel vm)
            {
                vm.HandleKeyEvent(e, "KeyUp");
            }
        }

        private void OnTextInput(object sender, TextInputEventArgs e)
        {
            // TextInput provides the actual typed character (case-sensitive, locale-aware)
            // This handles German umlauts (ä, ö, ü, ß) and other special characters
            if (DataContext is KeyboardTesterToolViewModel vm && !string.IsNullOrEmpty(e.Text))
            {
                vm.HandleTextInput(e.Text);
            }
        }

        protected override void OnAttachedToVisualTree(global::Avalonia.VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);
            Focus();
        }

        public void OnStartMonitoringClick(object sender, RoutedEventArgs e)
        {
            // After button click, immediately move focus back to the UserControl
            // so keystrokes are captured without needing an extra click
            Focus();
        }

        public void OnStartTypingTestClick(object sender, RoutedEventArgs e)
        {
            // Post focus to run after the command has executed and the TextBox is enabled
            Dispatcher.UIThread.Post(() => TypingInputBox.Focus(), DispatcherPriority.Background);
        }
    }
}
