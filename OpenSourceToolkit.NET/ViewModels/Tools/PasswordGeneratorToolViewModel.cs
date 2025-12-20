using CommunityToolkit.Mvvm.Input;
using OpenSourceToolkit.NET.Localization;
using OpenSourceToolkit.Security;
using System.Windows.Input;

namespace OpenSourceToolkit.NET.ViewModels.Tools
{
    public class PasswordGeneratorToolViewModel : ToolViewModel
    {
        public override int Id => 29;
        public override string Name => ToolkitLocalization.GetString("Tool_PasswordGenerator_Name");
        public override string Description => ToolkitLocalization.GetString("Tool_PasswordGenerator_Description");
        public override string IconKey => "PasswordIcon";

        private int _length = 16;
        public int Length
        {
            get => _length;
            set => SetProperty(ref _length, value);
        }

        private bool _includeUppercase = true;
        public bool IncludeUppercase
        {
            get => _includeUppercase;
            set => SetProperty(ref _includeUppercase, value);
        }

        private bool _includeLowercase = true;
        public bool IncludeLowercase
        {
            get => _includeLowercase;
            set => SetProperty(ref _includeLowercase, value);
        }

        private bool _includeNumbers = true;
        public bool IncludeNumbers
        {
            get => _includeNumbers;
            set => SetProperty(ref _includeNumbers, value);
        }

        private bool _includeSymbols = true;
        public bool IncludeSymbols
        {
            get => _includeSymbols;
            set => SetProperty(ref _includeSymbols, value);
        }

        private bool _excludeSimilar = true;
        public bool ExcludeSimilar
        {
            get => _excludeSimilar;
            set => SetProperty(ref _excludeSimilar, value);
        }

        private bool _excludeAmbiguous = true;
        public bool ExcludeAmbiguous
        {
            get => _excludeAmbiguous;
            set => SetProperty(ref _excludeAmbiguous, value);
        }

        private string _password;
        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        public ICommand GenerateCommand { get; }

        public PasswordGeneratorToolViewModel()
        {
            GenerateCommand = new RelayCommand(Generate);
            Generate(); // Generate one on init
        }

        private void Generate()
        {
            var options = new PasswordOptions
            {
                Length = Length,
                IncludeUppercase = IncludeUppercase,
                IncludeLowercase = IncludeLowercase,
                IncludeNumbers = IncludeNumbers,
                IncludeSymbols = IncludeSymbols,
                ExcludeSimilar = ExcludeSimilar,
                ExcludeAmbiguous = ExcludeAmbiguous,
                // Defaults for mins to ensure strength if selected
                MinUppercase = IncludeUppercase ? 1 : 0,
                MinLowercase = IncludeLowercase ? 1 : 0,
                MinNumbers = IncludeNumbers ? 1 : 0,
                MinSymbols = IncludeSymbols ? 1 : 0
            };

            // Prevent empty charsets crash
            if (!options.IncludeUppercase && !options.IncludeLowercase &&
                !options.IncludeNumbers && !options.IncludeSymbols)
            {
                Password = "Select at least one character type.";
                return;
            }

            Password = PasswordGenerator.Generate(options);
        }
    }
}
