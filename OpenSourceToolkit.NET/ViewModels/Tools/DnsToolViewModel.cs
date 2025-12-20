using CommunityToolkit.Mvvm.Input;
using DnsClient;
using OpenSourceToolkit.NET.Localization;
using OpenSourceToolkit.Networking;
using System;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace OpenSourceToolkit.NET.ViewModels.Tools
{
    public partial class DnsToolViewModel : ToolViewModel
    {
        public override int Id => 14;
        public override string Name => ToolkitLocalization.GetString("Tool_Dns_Name");
        public override string Description => ToolkitLocalization.GetString("Tool_Dns_Description");
        public override string IconKey => "DnsIcon";

        private string _domain = "google.com";
        public string Domain
        {
            get => _domain;
            set => SetProperty(ref _domain, value);
        }

        private string _output;
        public string Output
        {
            get => _output;
            set => SetProperty(ref _output, value);
        }

        public ICommand LookupCommand { get; }

        private readonly DnsLookupTool _dnsTool;

        public DnsToolViewModel()
        {
            _dnsTool = new DnsLookupTool();
            LookupCommand = new RelayCommand(async () => await Lookup());
        }

        private async Task Lookup()
        {
            if (string.IsNullOrWhiteSpace(Domain)) return;

            Output = "Querying...";
            try
            {
                var results = await _dnsTool.BatchQueryAsync(Domain);
                var sb = new StringBuilder();

                foreach (var recordType in results.Keys)
                {
                    sb.AppendLine($"--- {recordType} Records ---");
                    foreach (var record in results[recordType])
                    {
                        sb.AppendLine(record);
                    }
                    sb.AppendLine();
                }

                if (results.Count == 0)
                {
                    Output = "No records found.";
                }
                else
                {
                    Output = sb.ToString();
                }
            }
            catch (Exception ex)
            {
                Output = $"Error: {ex.Message}";
            }
        }
    }
}
