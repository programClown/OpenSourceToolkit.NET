using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using OpenSourceToolkit.Converters;
using OpenSourceToolkit.NET.Localization;

namespace OpenSourceToolkit.NET.ViewModels.Tools
{
    public partial class JsonFormatterToolViewModel : ToolViewModel
    {
        public override int Id => 31;
        public override string Name => ToolkitLocalization.GetString("Tool_JsonFormatter_Name");
        public override string Description => ToolkitLocalization.GetString("Tool_JsonFormatter_Description");
        public override string IconKey => "JsonFormatterIcon";

        private string _inputText;
        public string InputText
        {
            get => _inputText;
            set
            {
                if (SetProperty(ref _inputText, value))
                {
                    OnPropertyChanged(nameof(InputSize));
                }
            }
        }

        public string InputSize => FormatSize(InputText?.Length ?? 0);

        private string _formatOutput;
        public string FormatOutput
        {
            get => _formatOutput;
            set
            {
                if (SetProperty(ref _formatOutput, value))
                {
                    OnPropertyChanged(nameof(FormatOutputSize));
                }
            }
        }
        public string FormatOutputSize => FormatSize(FormatOutput?.Length ?? 0);

        private string _minifyOutput;
        public string MinifyOutput
        {
            get => _minifyOutput;
            set
            {
                if (SetProperty(ref _minifyOutput, value))
                {
                    OnPropertyChanged(nameof(MinifyOutputSize));
                }
            }
        }
        public string MinifyOutputSize => FormatSize(MinifyOutput?.Length ?? 0);

        private string _convertOutput;
        public string ConvertOutput
        {
            get => _convertOutput;
            set
            {
                if (SetProperty(ref _convertOutput, value))
                {
                    OnPropertyChanged(nameof(ConvertOutputSize));
                }
            }
        }
        public string ConvertOutputSize => FormatSize(ConvertOutput?.Length ?? 0);

        private static string FormatSize(long bytes)
        {
            return $"{bytes} Bytes";
        }

        private string _inputFormat = "json";
        public string InputFormat
        {
            get => _inputFormat;
            set => SetProperty(ref _inputFormat, value);
        }

        private string _outputFormat = "json";
        public string OutputFormat
        {
            get => _outputFormat;
            set => SetProperty(ref _outputFormat, value);
        }

        private string _operation = "format";
        public string Operation
        {
            get => _operation;
            set
            {
                if (SetProperty(ref _operation, value))
                {
                    OnPropertyChanged(nameof(IsConvertMode));
                    OnPropertyChanged(nameof(IsFormatOrMinifyMode));
                    OnPropertyChanged(nameof(OperationIndex));
                    OnPropertyChanged(nameof(OperationDisplay));
                }
            }
        }

        public int OperationIndex
        {
            get => Operation == "format" ? 0 : Operation == "minify" ? 1 : 2;
            set
            {
                string newOp = value == 0 ? "format" : value == 1 ? "minify" : "convert";
                Operation = newOp;
            }
        }

        public string OperationDisplay
        {
            get
            {
                switch (Operation)
                {
                    case "convert": return "Convert";
                    case "minify": return "Minify";
                    default: return "Format";
                }
            }
        }

        public bool IsConvertMode => Operation == "convert";
        public bool IsFormatOrMinifyMode => Operation == "format" || Operation == "minify";

        private string _error;
        public string Error
        {
            get => _error;
            set => SetProperty(ref _error, value);
        }

        public ObservableCollection<string> Formats { get; } = new ObservableCollection<string> { "json", "xml", "yaml" };

        // We'll map indices or use string match in View for Tabs

        public ICommand ProcessCommand { get; }
        public ICommand ClearCommand { get; }
        public ICommand LoadFileCommand { get; }
        public ICommand FormatCommand { get; }
        public ICommand MinifyCommand { get; }
        public ICommand ConvertCommand { get; }

        public Func<System.Threading.Tasks.Task<string>> PickFileAction { get; set; }

        public JsonFormatterToolViewModel()
        {
            ProcessCommand = new RelayCommand(Process);
            ClearCommand = new RelayCommand(Clear);
            LoadFileCommand = new RelayCommand(async () => await LoadFileAsync());
            FormatCommand = new RelayCommand(DoFormat);
            MinifyCommand = new RelayCommand(DoMinify);
            ConvertCommand = new RelayCommand(DoConvert);
        }

        private async System.Threading.Tasks.Task LoadFileAsync()
        {
            if (PickFileAction != null)
            {
                var content = await PickFileAction();
                if (!string.IsNullOrEmpty(content))
                {
                    InputText = content;
                }
            }
        }

        private void DoFormat()
        {
            if (string.IsNullOrWhiteSpace(InputText))
            {
                Error = "Please enter some data to process";
                FormatOutput = string.Empty;
                return;
            }

            try
            {
                Error = string.Empty;
                switch (InputFormat)
                {
                    case "json":
                        FormatOutput = JsonXmlYamlConverter.FormatJson(InputText, false);
                        break;
                    case "xml":
                        FormatOutput = JsonXmlYamlConverter.FormatXml(InputText, false);
                        break;
                    case "yaml":
                        FormatOutput = JsonXmlYamlConverter.FormatYaml(InputText);
                        break;
                    default:
                        FormatOutput = JsonXmlYamlConverter.FormatJson(InputText, false);
                        break;
                }
            }
            catch (Exception ex)
            {
                Error = ex.Message;
                FormatOutput = string.Empty;
            }
        }

        private void DoMinify()
        {
            if (string.IsNullOrWhiteSpace(InputText))
            {
                Error = "Please enter some data to process";
                MinifyOutput = string.Empty;
                return;
            }

            try
            {
                Error = string.Empty;
                switch (InputFormat)
                {
                    case "json":
                        MinifyOutput = JsonXmlYamlConverter.FormatJson(InputText, true);
                        break;
                    case "xml":
                        MinifyOutput = JsonXmlYamlConverter.FormatXml(InputText, true);
                        break;
                    case "yaml":
                        MinifyOutput = JsonXmlYamlConverter.FormatYaml(InputText);
                        break;
                    default:
                        MinifyOutput = JsonXmlYamlConverter.FormatJson(InputText, true);
                        break;
                }
            }
            catch (Exception ex)
            {
                Error = ex.Message;
                MinifyOutput = string.Empty;
            }
        }

        private void DoConvert()
        {
            if (string.IsNullOrWhiteSpace(InputText))
            {
                Error = "Please enter some data to process";
                ConvertOutput = string.Empty;
                return;
            }

            try
            {
                Error = string.Empty;
                ConvertOutput = JsonXmlYamlConverter.Convert(InputText, InputFormat, OutputFormat);
            }
            catch (Exception ex)
            {
                Error = ex.Message;
                ConvertOutput = string.Empty;
            }
        }

        private void Process()
        {
            switch (Operation)
            {
                case "format":
                    DoFormat();
                    break;
                case "minify":
                    DoMinify();
                    break;
                case "convert":
                    DoConvert();
                    break;
            }
        }

        private void Clear()
        {
            InputText = string.Empty;
            FormatOutput = string.Empty;
            MinifyOutput = string.Empty;
            ConvertOutput = string.Empty;
            Error = string.Empty;
        }
    }
}
