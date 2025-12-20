using CommunityToolkit.Mvvm.Input;
using OpenSourceToolkit.NET.Localization;
using OpenSourceToolkit.TextData;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace OpenSourceToolkit.NET.ViewModels.Tools
{
    public class GuidFormatItem
    {
        public GuidFormat Format { get; set; }
        public string DisplayName { get; set; }
    }

    public partial class UuidToolViewModel : ToolViewModel
    {
        public override int Id => 1;
        public override string Name => ToolkitLocalization.GetString("Tool_Uuid_Name");
        public override string Description => ToolkitLocalization.GetString("Tool_Uuid_Description");
        public override string IconKey => "UuidIcon";

        public List<GuidFormatItem> AvailableFormats { get; } = new List<GuidFormatItem>
        {
            new GuidFormatItem { Format = GuidFormat.Registry, DisplayName = "Registry Format {xxxxxxxx-xxxx-...}" },
            new GuidFormatItem { Format = GuidFormat.CSharpAttribute, DisplayName = "[Guid(\"...\")]  (C#)" },
            new GuidFormatItem { Format = GuidFormat.VbNetAttribute, DisplayName = "<Guid(\"...\")>  (VB.NET)" },
            new GuidFormatItem { Format = GuidFormat.DefineGuid, DisplayName = "DEFINE_GUID(...)  (C/C++)" },
            new GuidFormatItem { Format = GuidFormat.ImplementOleCreate, DisplayName = "IMPLEMENT_OLECREATE(...)  (COM)" },
            new GuidFormatItem { Format = GuidFormat.StructGuid, DisplayName = "static const GUID = {...}  (C/C++)" },
            new GuidFormatItem { Format = GuidFormat.Plain, DisplayName = "Plain UUID" },
            new GuidFormatItem { Format = GuidFormat.Short, DisplayName = "Short UUID (Base64)" }
        };

        private GuidFormatItem _selectedFormat;
        public GuidFormatItem SelectedFormat
        {
            get => _selectedFormat;
            set => SetProperty(ref _selectedFormat, value);
        }

        private int _batchCount = 1;
        public int BatchCount
        {
            get => _batchCount;
            set => SetProperty(ref _batchCount, value);
        }

        private string _output;
        public string Output
        {
            get => _output;
            set => SetProperty(ref _output, value);
        }

        public ICommand GenerateCommand { get; }
        public ICommand CopyCommand { get; }

        public System.Action<string> CopyToClipboardAction { get; set; }

        public UuidToolViewModel()
        {
            _selectedFormat = AvailableFormats.First();
            GenerateCommand = new RelayCommand(Generate);
            CopyCommand = new RelayCommand(Copy, () => !string.IsNullOrEmpty(Output));
            Generate();
        }

        private void Generate()
        {
            var results = UuidGenerator.GenerateBatch(SelectedFormat.Format, BatchCount);
            Output = string.Join("\r\n\r\n", results);
            ((RelayCommand)CopyCommand).NotifyCanExecuteChanged();
        }

        private void Copy()
        {
            CopyToClipboardAction?.Invoke(Output);
        }
    }
}
