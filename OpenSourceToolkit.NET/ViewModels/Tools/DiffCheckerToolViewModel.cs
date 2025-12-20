using CommunityToolkit.Mvvm.Input;
using OpenSourceToolkit.NET.Localization;
using OpenSourceToolkit.TextData;
using DiffPlex.DiffBuilder.Model;
using System.Windows.Input;

namespace OpenSourceToolkit.NET.ViewModels.Tools
{
    public class DiffCheckerToolViewModel : ToolViewModel
    {
        public override int Id => 33;
        public override string Name => ToolkitLocalization.GetString("Tool_DiffChecker_Name");
        public override string Description => ToolkitLocalization.GetString("Tool_DiffChecker_Description");
        public override string IconKey => "DiffCheckerIcon";

        private string _text1;
        public string Text1
        {
            get => _text1;
            set => SetProperty(ref _text1, value);
        }

        private string _text2;
        public string Text2
        {
            get => _text2;
            set => SetProperty(ref _text2, value);
        }

        private SideBySideDiffModel _diffModel;
        public SideBySideDiffModel DiffModel
        {
            get => _diffModel;
            set => SetProperty(ref _diffModel, value);
        }

        public ICommand CompareCommand { get; }

        private readonly DiffChecker _diffChecker;

        public DiffCheckerToolViewModel()
        {
            _diffChecker = new DiffChecker();
            CompareCommand = new RelayCommand(Compare);
        }

        private void Compare()
        {
            if (string.IsNullOrEmpty(Text1) && string.IsNullOrEmpty(Text2))
            {
                DiffModel = null;
                return;
            }

            DiffModel = _diffChecker.Compare(Text1 ?? string.Empty, Text2 ?? string.Empty);
        }
    }
}
