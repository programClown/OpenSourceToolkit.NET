#nullable enable
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using System.Collections.Specialized;
using OpenSourceToolkit.NET.ViewModels.Tools;

namespace OpenSourceToolkit.NET.Views.Tools
{
    public partial class ScientificCalculatorToolView : UserControl
    {
        private ScientificCalculatorToolViewModel? _vm;

        public ScientificCalculatorToolView()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
            Focusable = true;
            KeyDown += OnKeyDown;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        protected override void OnAttachedToVisualTree(Avalonia.VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);
            Focus();
        }

        private void OnDataContextChanged(object? sender, System.EventArgs e)
        {
            if (DataContext is ScientificCalculatorToolViewModel vm)
            {
                _vm = vm;
                vm.CopyToClipboardAction = async text =>
                {
                    var topLevel = TopLevel.GetTopLevel(this);
                    if (topLevel?.Clipboard != null)
                    {
                        await topLevel.Clipboard.SetTextAsync(text);
                    }
                };
                vm.History.CollectionChanged += OnHistoryChanged;
            }
        }

        private void OnKeyDown(object? sender, KeyEventArgs e)
        {
            if (_vm == null) return;

            switch (e.Key)
            {
                // Digits
                case Key.D0 or Key.NumPad0:
                    _vm.DigitCommand.Execute("0");
                    e.Handled = true;
                    break;
                case Key.D1 or Key.NumPad1:
                    _vm.DigitCommand.Execute("1");
                    e.Handled = true;
                    break;
                case Key.D2 or Key.NumPad2:
                    _vm.DigitCommand.Execute("2");
                    e.Handled = true;
                    break;
                case Key.D3 or Key.NumPad3:
                    _vm.DigitCommand.Execute("3");
                    e.Handled = true;
                    break;
                case Key.D4 or Key.NumPad4:
                    _vm.DigitCommand.Execute("4");
                    e.Handled = true;
                    break;
                case Key.D5 or Key.NumPad5:
                    _vm.DigitCommand.Execute("5");
                    e.Handled = true;
                    break;
                case Key.D6 or Key.NumPad6:
                    _vm.DigitCommand.Execute("6");
                    e.Handled = true;
                    break;
                case Key.D7 or Key.NumPad7:
                    _vm.DigitCommand.Execute("7");
                    e.Handled = true;
                    break;
                case Key.D8 or Key.NumPad8:
                    if (e.KeyModifiers.HasFlag(KeyModifiers.Shift))
                        _vm.OperatorCommand.Execute("*"); // Shift+8 = *
                    else
                        _vm.DigitCommand.Execute("8");
                    e.Handled = true;
                    break;
                case Key.D9 or Key.NumPad9:
                    _vm.DigitCommand.Execute("9");
                    e.Handled = true;
                    break;

                // Operators
                case Key.Add:
                    _vm.OperatorCommand.Execute("+");
                    e.Handled = true;
                    break;
                case Key.Subtract or Key.OemMinus:
                    _vm.OperatorCommand.Execute("-");
                    e.Handled = true;
                    break;
                case Key.Multiply:
                    _vm.OperatorCommand.Execute("*");
                    e.Handled = true;
                    break;
                case Key.Divide or Key.OemQuestion:
                    _vm.OperatorCommand.Execute("/");
                    e.Handled = true;
                    break;

                // Decimal point
                case Key.Decimal or Key.OemPeriod:
                    _vm.DigitCommand.Execute(".");
                    e.Handled = true;
                    break;

                // Equals / Enter
                case Key.Enter or Key.Return:
                    _vm.EqualsCommand.Execute(null);
                    e.Handled = true;
                    break;

                // Backspace
                case Key.Back:
                    _vm.BackspaceCommand.Execute(null);
                    e.Handled = true;
                    break;

                // Delete / Clear
                case Key.Delete:
                    _vm.ClearCommand.Execute(null);
                    e.Handled = true;
                    break;

                // Escape = Clear
                case Key.Escape:
                    _vm.ClearCommand.Execute(null);
                    e.Handled = true;
                    break;
            }
        }

        private void OnHistoryChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                // Use FindControl since source generation doesn't work with manual InitializeComponent
                var scrollViewer = this.FindControl<ScrollViewer>("RollScrollViewer");
                scrollViewer?.ScrollToEnd();
            }
        }
    }
}
