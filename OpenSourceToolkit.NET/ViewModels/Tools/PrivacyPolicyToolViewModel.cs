using CommunityToolkit.Mvvm.Input;
using OpenSourceToolkit.NET.Localization;
using OpenSourceToolkit.TextData;
using System.Windows.Input;

namespace OpenSourceToolkit.NET.ViewModels.Tools
{
    public partial class PrivacyPolicyToolViewModel : ToolViewModel
    {
        public override int Id => 4;
        public override string Name => ToolkitLocalization.GetString("Tool_PrivacyPolicy_Name");
        public override string Description => ToolkitLocalization.GetString("Tool_PrivacyPolicy_Description");
        public override string IconKey => "PrivacyPolicyIcon";

        private readonly PrivacyPolicyOptions _options = new PrivacyPolicyOptions();

        public PrivacyPolicyToolViewModel()
        {
            GenerateCommand = new RelayCommand(Generate);
            LoadCompanyInfo();
        }

        private void LoadCompanyInfo()
        {
            _options.CompanyName = GetSetting("CompanyName", _options.CompanyName);
            _options.WebsiteUrl = GetSetting("WebsiteUrl", _options.WebsiteUrl);
            _options.ContactEmail = GetSetting("ContactEmail", _options.ContactEmail);
        }

        public string CompanyName
        {
            get => _options.CompanyName;
            set
            {
                if (_options.CompanyName != value)
                {
                    _options.CompanyName = value;
                    SetSetting("CompanyName", value);
                    OnPropertyChanged();
                }
            }
        }

        public string WebsiteUrl
        {
            get => _options.WebsiteUrl;
            set
            {
                if (_options.WebsiteUrl != value)
                {
                    _options.WebsiteUrl = value;
                    SetSetting("WebsiteUrl", value);
                    OnPropertyChanged();
                }
            }
        }

        public string ContactEmail
        {
            get => _options.ContactEmail;
            set
            {
                if (_options.ContactEmail != value)
                {
                    _options.ContactEmail = value;
                    SetSetting("ContactEmail", value);
                    OnPropertyChanged();
                }
            }
        }

        public bool CollectsCookies
        {
            get => _options.CollectsCookies;
            set { _options.CollectsCookies = value; OnPropertyChanged(); }
        }

        public bool CollectsAnalytics
        {
            get => _options.CollectsAnalytics;
            set { _options.CollectsAnalytics = value; OnPropertyChanged(); }
        }

        public bool CollectsPersonalData
        {
            get => _options.CollectsPersonalData;
            set { _options.CollectsPersonalData = value; OnPropertyChanged(); }
        }

        public bool HasThirdPartyServices
        {
            get => _options.HasThirdPartyServices;
            set { _options.HasThirdPartyServices = value; OnPropertyChanged(); }
        }

        public bool HasUserAccounts
        {
            get => _options.HasUserAccounts;
            set { _options.HasUserAccounts = value; OnPropertyChanged(); }
        }

        public bool IncludeGdprSection
        {
            get => _options.IncludeGdprSection;
            set { _options.IncludeGdprSection = value; OnPropertyChanged(); }
        }

        public bool IncludeCcpaSection
        {
            get => _options.IncludeCcpaSection;
            set { _options.IncludeCcpaSection = value; OnPropertyChanged(); }
        }

        public bool OutputAsMarkdown
        {
            get => _options.OutputFormat == PrivacyPolicyFormat.Markdown;
            set { _options.OutputFormat = value ? PrivacyPolicyFormat.Markdown : PrivacyPolicyFormat.PlainText; OnPropertyChanged(); }
        }

        private string _output;
        public string Output
        {
            get => _output;
            set => SetProperty(ref _output, value);
        }

        public ICommand GenerateCommand { get; }

        private void Generate()
        {
            Output = PrivacyPolicyGenerator.Generate(_options);
        }
    }
}
