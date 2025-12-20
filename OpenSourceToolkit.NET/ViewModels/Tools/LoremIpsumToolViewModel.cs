using CommunityToolkit.Mvvm.Input;
using OpenSourceToolkit.NET.Localization;
using OpenSourceToolkit.TextData;
using System.Windows.Input;

namespace OpenSourceToolkit.NET.ViewModels.Tools
{
    public partial class LoremIpsumToolViewModel : ToolViewModel
    {
        public override int Id => 2;
        public override string Name => ToolkitLocalization.GetString("Tool_LoremIpsum_Name");
        public override string Description => ToolkitLocalization.GetString("Tool_LoremIpsum_Description");
        public override string IconKey => "LoremIpsumIcon";

        private int _count = 5;
        public int Count
        {
            get => _count;
            set => SetProperty(ref _count, value);
        }

        private string _output;
        public string Output
        {
            get => _output;
            set => SetProperty(ref _output, value);
        }

        public ICommand GenerateWordsCommand { get; }
        public ICommand GenerateSentencesCommand { get; }
        public ICommand GenerateParagraphsCommand { get; }

        private readonly LoremIpsumGenerator _generator;

        public LoremIpsumToolViewModel()
        {
            _generator = new LoremIpsumGenerator();
            GenerateWordsCommand = new RelayCommand(() => Output = _generator.GenerateWords(Count));
            GenerateSentencesCommand = new RelayCommand(() => Output = _generator.GenerateSentences(Count));
            GenerateParagraphsCommand = new RelayCommand(() => Output = _generator.GenerateParagraphs(Count));
        }
    }
}
