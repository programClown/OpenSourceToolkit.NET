using CommunityToolkit.Mvvm.Input;
using OpenSourceToolkit.NET.Localization;
using OpenSourceToolkit.TextData;
using System.Linq;
using System.Windows.Input;

namespace OpenSourceToolkit.NET.ViewModels.Tools
{
    public class VCardGeneratorToolViewModel : ToolViewModel
    {
        public override int Id => 27;
        public override string Name => ToolkitLocalization.GetString("Tool_VCardGenerator_Name");
        public override string Description => ToolkitLocalization.GetString("Tool_VCardGenerator_Description");
        public override string IconKey => "VCardIcon";

        private bool _useTestData;
        public bool UseTestData
        {
            get => _useTestData;
            set => SetProperty(ref _useTestData, value);
        }

        public string FirstName
        {
            get => GetSetting<string>(nameof(FirstName)) ?? "John";
            set { SetSetting(nameof(FirstName), value); OnPropertyChanged(); }
        }

        public string LastName
        {
            get => GetSetting<string>(nameof(LastName)) ?? "Doe";
            set { SetSetting(nameof(LastName), value); OnPropertyChanged(); }
        }

        public string Email
        {
            get => GetSetting<string>(nameof(Email)) ?? "john.doe@example.com";
            set { SetSetting(nameof(Email), value); OnPropertyChanged(); }
        }

        public string Phone
        {
            get => GetSetting<string>(nameof(Phone));
            set { SetSetting(nameof(Phone), value); OnPropertyChanged(); }
        }

        public string Organization
        {
            get => GetSetting<string>(nameof(Organization));
            set { SetSetting(nameof(Organization), value); OnPropertyChanged(); }
        }

        public string Title
        {
            get => GetSetting<string>(nameof(Title));
            set { SetSetting(nameof(Title), value); OnPropertyChanged(); }
        }

        public string Website
        {
            get => GetSetting<string>(nameof(Website));
            set { SetSetting(nameof(Website), value); OnPropertyChanged(); }
        }

        public string Street
        {
            get => GetSetting<string>(nameof(Street));
            set { SetSetting(nameof(Street), value); OnPropertyChanged(); }
        }

        public string City
        {
            get => GetSetting<string>(nameof(City));
            set { SetSetting(nameof(City), value); OnPropertyChanged(); }
        }

        public string State
        {
            get => GetSetting<string>(nameof(State));
            set { SetSetting(nameof(State), value); OnPropertyChanged(); }
        }

        public string PostalCode
        {
            get => GetSetting<string>(nameof(PostalCode));
            set { SetSetting(nameof(PostalCode), value); OnPropertyChanged(); }
        }

        public string Country
        {
            get => GetSetting<string>(nameof(Country));
            set { SetSetting(nameof(Country), value); OnPropertyChanged(); }
        }

        public string Note
        {
            get => GetSetting<string>(nameof(Note));
            set { SetSetting(nameof(Note), value); OnPropertyChanged(); }
        }

        private string _output;
        public string Output
        {
            get => _output;
            set => SetProperty(ref _output, value);
        }

        public ICommand GenerateCommand { get; }

        public VCardGeneratorToolViewModel()
        {
            GenerateCommand = new RelayCommand(Generate);
        }

        private void Generate()
        {
            VCardOptions options;

            if (UseTestData)
            {
                options = GenerateTestDataOptions();
            }
            else
            {
                options = new VCardOptions
                {
                    FirstName = FirstName,
                    LastName = LastName,
                    Email = Email,
                    Phone = Phone,
                    Organization = Organization,
                    Title = Title,
                    Website = Website,
                    Street = Street,
                    City = City,
                    State = State,
                    PostalCode = PostalCode,
                    Country = Country,
                    Note = Note
                };
            }

            Output = VCardGenerator.Generate(options);
        }

        private VCardOptions GenerateTestDataOptions()
        {
            var users = MockDataService.Generate("Users", 1).ToList();
            var companies = MockDataService.Generate("Companies", 1).ToList();
            var addresses = MockDataService.Generate("Addresses_US", 1).ToList();

            dynamic user = users.FirstOrDefault();
            dynamic company = companies.FirstOrDefault();
            dynamic address = addresses.FirstOrDefault();

            return new VCardOptions
            {
                FirstName = user?.FirstName ?? "Test",
                LastName = user?.LastName ?? "User",
                Email = user?.Email,
                Phone = user?.Phone,
                Organization = company?.Name,
                Title = "Employee",
                Website = company?.Website,
                Street = address?.Street,
                City = address?.City,
                State = address?.State,
                PostalCode = address?.ZipCode,
                Country = address?.Country,
                Note = "Generated with test data"
            };
        }
    }
}
