using CommunityToolkit.Mvvm.Input;
using OpenSourceToolkit.NET.Localization;
using OpenSourceToolkit.TextData;
using System.Linq;
using System.Text.Json;
using System.Windows.Input;

namespace OpenSourceToolkit.NET.ViewModels.Tools
{
    /// <summary>
    /// UI wrapper around MockDataType for display purposes (adds FlagKey for XAML).
    /// </summary>
    public class DataTypeItem
    {
        public string Key { get; set; }
        public string Name { get; set; }
        public string FlagKey { get; set; }
    }

    public partial class MockDataToolViewModel : ToolViewModel
    {
        public override int Id => 3;
        public override string Name => ToolkitLocalization.GetString("Tool_MockData_Name");
        public override string Description => ToolkitLocalization.GetString("Tool_MockData_Description");
        public override string IconKey => "MockDataIcon";

        private int _count = 5;
        public int Count
        {
            get => _count;
            set => SetProperty(ref _count, value);
        }

        private DataTypeItem _dataType;
        public DataTypeItem DataType
        {
            get => _dataType;
            set => SetProperty(ref _dataType, value);
        }

        public DataTypeItem[] DataTypes { get; }

        private string _output;
        public string Output
        {
            get => _output;
            set => SetProperty(ref _output, value);
        }

        public ICommand GenerateCommand { get; }

        public MockDataToolViewModel()
        {
            DataTypes = MockDataService.AvailableTypes
                .Select(t => new DataTypeItem { Key = t.Key, Name = t.DisplayName, FlagKey = t.FlagKey })
                .ToArray();
            _dataType = DataTypes[0];
            GenerateCommand = new RelayCommand(Generate);
        }

        private void Generate()
        {
            var key = DataType?.Key ?? "Users";
            var data = MockDataService.Generate(key, Count);
            var options = new JsonSerializerOptions { WriteIndented = true };
            Output = JsonSerializer.Serialize(data, options);
        }
    }
}
