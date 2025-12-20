using CommunityToolkit.Mvvm.Input;
using OpenSourceToolkit.Converters;
using OpenSourceToolkit.NET.Localization;
using System.Windows.Input;

namespace OpenSourceToolkit.NET.ViewModels.Tools
{
    public partial class TextCaseToolViewModel : ToolViewModel
    {
        public override int Id => 6;
        public override string Name => ToolkitLocalization.GetString("Tool_TextCase_Name");
        public override string Description => ToolkitLocalization.GetString("Tool_TextCase_Description");
        public override string IconKey => "TextCaseIcon";

        private string _input;
        public string Input
        {
            get => _input;
            set
            {
                if (SetProperty(ref _input, value))
                {
                    UpdateOutputs();
                }
            }
        }

        private string _upper;
        public string Upper
        {
            get => _upper;
            set => SetProperty(ref _upper, value);
        }

        private string _lower;
        public string Lower
        {
            get => _lower;
            set => SetProperty(ref _lower, value);
        }

        private string _title;
        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        private string _sentence;
        public string Sentence
        {
            get => _sentence;
            set => SetProperty(ref _sentence, value);
        }

        private void UpdateOutputs()
        {
            Upper = TextCaseConverter.ToUpperCase(Input);
            Lower = TextCaseConverter.ToLowerCase(Input);
            Title = TextCaseConverter.ToTitleCase(Input);
            Sentence = TextCaseConverter.ToSentenceCase(Input);
        }
    }
}
