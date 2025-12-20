using CommunityToolkit.Mvvm.Input;
using OpenSourceToolkit.Calculators;
using OpenSourceToolkit.NET.Localization;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace OpenSourceToolkit.NET.ViewModels.Tools
{
    public class PaperRollItem
    {
        public string Expression { get; set; }
        public string Result { get; set; }

        public PaperRollItem() { }

        public PaperRollItem(string expression, string result)
        {
            Expression = expression;
            Result = result;
        }
    }

    public class ScientificCalculatorToolViewModel : ToolViewModel
    {
        public override int Id => 1100; // Pick a unique ID
        public override string Name => ToolkitLocalization.GetString("Tool_ScientificCalculator_Name");
        public override string Description => ToolkitLocalization.GetString("Tool_ScientificCalculator_Description");
        public override string IconKey => "CalculatorVariant"; // Assuming this icon key exists or use a generic one like "Calculator"

        private string _display = "0";
        public string Display
        {
            get => _display;
            set => SetProperty(ref _display, value);
        }

        private string _expression = "";
        public string Expression
        {
            get => _expression;
            set => SetProperty(ref _expression, value);
        }

        private double _memory;
        public double Memory
        {
            get => _memory;
            set
            {
                if (SetProperty(ref _memory, value))
                {
                    OnPropertyChanged(nameof(IsMemoryStored));
                }
            }
        }

        public bool IsMemoryStored => Memory != 0;

        private ObservableCollection<PaperRollItem> _history = new ObservableCollection<PaperRollItem>();
        public ObservableCollection<PaperRollItem> History
        {
            get => _history;
            set => SetProperty(ref _history, value);
        }

        private bool _isResultShown;
        private bool _waitingForOperand;

        public ICommand ClearCommand { get; }
        public ICommand ClearEntryCommand { get; }
        public ICommand BackspaceCommand { get; }
        public ICommand DigitCommand { get; }
        public ICommand OperatorCommand { get; }
        public ICommand FunctionCommand { get; }
        public ICommand EqualsCommand { get; }
        public ICommand MemoryClearCommand { get; }
        public ICommand MemoryReadCommand { get; }
        public ICommand MemoryAddCommand { get; }
        public ICommand MemorySubtractCommand { get; }
        public ICommand ClearHistoryCommand { get; }
        public ICommand UndoCommand { get; }
        public ICommand CopyDisplayCommand { get; }
        public ICommand CopyTapeCommand { get; }

        public System.Func<string, System.Threading.Tasks.Task> CopyToClipboardAction { get; set; }

        public ScientificCalculatorToolViewModel()
        {
            ClearCommand = new RelayCommand(Clear);
            ClearEntryCommand = new RelayCommand(ClearEntry);
            BackspaceCommand = new RelayCommand(Backspace);
            DigitCommand = new RelayCommand<string>(Digit);
            OperatorCommand = new RelayCommand<string>(Operator);
            FunctionCommand = new RelayCommand<string>(Function);
            EqualsCommand = new RelayCommand(Calculate);
            MemoryClearCommand = new RelayCommand(MemoryClear);
            MemoryReadCommand = new RelayCommand(MemoryRead);
            MemoryAddCommand = new RelayCommand(MemoryAdd);
            MemorySubtractCommand = new RelayCommand(MemorySubtract);
            ClearHistoryCommand = new RelayCommand(() => History.Clear());
            UndoCommand = new RelayCommand(Undo);
            CopyDisplayCommand = new RelayCommand(CopyDisplay);
            CopyTapeCommand = new RelayCommand(CopyTape);

            // Load history
            var savedHistory = GetSetting<List<PaperRollItem>>("History");
            if (savedHistory != null && savedHistory.Count > 0)
            {
                History = new ObservableCollection<PaperRollItem>(savedHistory);
                // Restore state from last tape entry
                Display = History.Last().Result;
                _isResultShown = true;
            }

            History.CollectionChanged += (s, e) => SaveHistory();
        }

        private void SaveHistory()
        {
            SetSetting("History", History.ToList());
        }

        private void Clear()
        {
            Display = "0";
            Expression = "";
            _isResultShown = false;
            _waitingForOperand = false;
        }

        private void ClearEntry()
        {
            Display = "0";
            _isResultShown = false;
            _waitingForOperand = false;
        }

        private void Backspace()
        {
            if (_isResultShown)
            {
                Clear();
                return;
            }

            Display = Display.Length > 1 ? Display.Substring(0, Display.Length - 1) : "0";
        }

        private void Digit(string digit)
        {
            _waitingForOperand = false;

            if (_isResultShown)
            {
                Display = digit;
                _isResultShown = false;
            }
            else
            {
                if (Display == "0" && digit != ".")
                    Display = digit;
                else if (digit == "." && Display.Contains("."))
                {
                    // Ignore double decimal
                }
                else
                    Display += digit;
            }
        }

        private void Operator(string op)
        {
            if (_isResultShown)
            {
                Expression = Display + " " + op + " ";
                _isResultShown = false;
                _waitingForOperand = true;
                Display = "0";
            }
            else
            {
                if (_waitingForOperand && !string.IsNullOrEmpty(Expression))
                {
                    Expression = Expression.TrimEnd();
                    var lastSep = Expression.LastIndexOf(' ');
                    if (lastSep != -1)
                    {
                        Expression = Expression.Substring(0, lastSep + 1) + op + " ";
                    }
                    else
                    {
                        // Should not happen if format is valid, but fallback to append
                         Expression += Display + " " + op + " ";
                         Display = "0";
                         _waitingForOperand = true;
                    }
                }
                else
                {
                    Expression += Display + " " + op + " ";
                    Display = "0";
                    _waitingForOperand = true;
                }
            }
        }

        private void Function(string func)
        {
            _waitingForOperand = false;

            if (_isResultShown)
            {
                Display = func + "(" + Display + ")";
                _isResultShown = false; // It's now an editable expression part
            }
            else
            {
                if (Display == "0")
                    Display = func + "(";
                else
                    Display = func + "(" + Display + ")";
            }
        }

        private void Calculate()
        {
            var fullExpression = Expression + Display;

            // Auto-close parentheses
            int openParens = 0;
            foreach (char c in fullExpression) if (c == '(') openParens++;
            foreach (char c in fullExpression) if (c == ')') openParens--;
            while (openParens > 0)
            {
                fullExpression += ")";
                openParens--;
            }

            double result = ScientificCalculator.Evaluate(fullExpression);

            if (double.IsNaN(result))
            {
                Display = "Error";
                _isResultShown = true;
            }
            else
            {
                History.Add(new PaperRollItem(fullExpression, result.ToString(System.Globalization.CultureInfo.InvariantCulture)));
                Display = result.ToString(System.Globalization.CultureInfo.InvariantCulture);
                Expression = "";
                _isResultShown = true;
            }
        }

        private void MemoryClear()
        {
            Memory = 0;
        }

        private void MemoryRead()
        {
            Display = Memory.ToString(System.Globalization.CultureInfo.InvariantCulture);
            _isResultShown = true;
            _waitingForOperand = false;
        }

        private void MemoryAdd()
        {
            if (double.TryParse(Display, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double val))
            {
                Memory += val;
                _isResultShown = true;
            }
        }

        private void MemorySubtract()
        {
            if (double.TryParse(Display, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double val))
            {
                Memory -= val;
                _isResultShown = true;
            }
        }

        private void Undo()
        {
            if (History.Count <= 0) return;
            History.RemoveAt(History.Count - 1);

            Display = History.Count > 0 ? History.Last().Result : "0";

            Expression = "";
            _isResultShown = true;
            _waitingForOperand = false;
        }

        private void CopyDisplay()
        {
            if (!string.IsNullOrEmpty(Display))
            {
                CopyToClipboardAction?.Invoke(Display);
            }
        }

        private void CopyTape()
        {
            if (History.Count <= 0) return;
            var sb = new System.Text.StringBuilder();
            foreach (var item in History)
            {
                sb.AppendLine($"{item.Expression} = {item.Result}");
            }
            CopyToClipboardAction?.Invoke(sb.ToString());
        }
    }
}
