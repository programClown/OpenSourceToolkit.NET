using CommunityToolkit.Mvvm.Input;
using OpenSourceToolkit.NET.Localization;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Input;

namespace OpenSourceToolkit.NET.ViewModels.Tools
{
    public class RegexTesterToolViewModel : ToolViewModel
    {
        public override int Id => 26;
        public override string Name => ToolkitLocalization.GetString("Tool_RegexTester_Name");
        public override string Description => ToolkitLocalization.GetString("Tool_RegexTester_Description");
        public override string IconKey => "RegexIcon";

        private string _pattern = "^[0-9]+$";
        public string Pattern
        {
            get => _pattern;
            set => SetProperty(ref _pattern, value);
        }

        private string _input = "12345";
        public string Input
        {
            get => _input;
            set => SetProperty(ref _input, value);
        }

        private string _replacementText = "";
        public string ReplacementText
        {
            get => _replacementText;
            set => SetProperty(ref _replacementText, value);
        }

        private string _replaceResult;
        public string ReplaceResult
        {
            get => _replaceResult;
            set => SetProperty(ref _replaceResult, value);
        }

        // Options
        private bool _ignoreCase;
        public bool IgnoreCase
        {
            get => _ignoreCase;
            set => SetProperty(ref _ignoreCase, value);
        }

        private bool _multiline;
        public bool Multiline
        {
            get => _multiline;
            set => SetProperty(ref _multiline, value);
        }

        private bool _singleline;
        public bool Singleline
        {
            get => _singleline;
            set => SetProperty(ref _singleline, value);
        }

        private string _errorMessage;
        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public ObservableCollection<MatchItem> Matches { get; } = new ObservableCollection<MatchItem>();
        public ObservableCollection<RegexExample> Examples { get; } = new ObservableCollection<RegexExample>();

        private RegexExample _selectedExample;
        public RegexExample SelectedExample
        {
            get => _selectedExample;
            set
            {
                if (SetProperty(ref _selectedExample, value) && value != null)
                {
                    Pattern = value.Pattern;
                    Input = value.Input;
                    // Auto-enable Multiline if input contains newlines and pattern uses ^ or $
                    if (value.Input.Contains('\n') && (value.Pattern.Contains("^") || value.Pattern.Contains("$")))
                    {
                        Multiline = true;
                    }
                    ErrorMessage = null;
                    Matches.Clear();
                    ReplaceResult = string.Empty;
                }
            }
        }

        public ICommand TestCommand { get; }

        public RegexTesterToolViewModel()
        {
            TestCommand = new RelayCommand(Test);
            InitializeExamples();
        }

        private void InitializeExamples()
        {
            Examples.Add(new RegexExample
            {
                Name = "Email Address",
                Pattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
                Input = "test.email@example.com\ninvalid.email@com",
                Description = "Validates standard email formats."
            });

            Examples.Add(new RegexExample
            {
                Name = "IPv4 Address",
                Pattern = @"^(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$",
                Input = "192.168.1.1\n256.0.0.1\n127.0.0.1",
                Description = "Matches valid IPv4 addresses."
            });

            Examples.Add(new RegexExample
            {
                Name = "Date (YYYY-MM-DD)",
                Pattern = @"^\d{4}-(?:0[1-9]|1[0-2])-(?:0[1-9]|[12]\d|3[01])$",
                Input = "2023-12-31\n2023-13-01\n2024-02-29",
                Description = "Matches dates in YYYY-MM-DD format."
            });

            Examples.Add(new RegexExample
            {
                Name = "URL/Website",
                Pattern = @"^(https?:\/\/)?([\da-z\.-]+)\.([a-z\.]{2,6})([\/\w \.-]*)*\/?$",
                Input = "https://www.google.com\nhttp://example.org/path",
                Description = "Validates standard URLs."
            });

            Examples.Add(new RegexExample
            {
                Name = "Password Strength",
                Pattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
                Input = "Password123!\nweakpassword",
                Description = "Min 8 chars, 1 uppercase, 1 lowercase, 1 number, 1 special char."
            });

            Examples.Add(new RegexExample
            {
                Name = "HTML Tags",
                Pattern = @"<([a-z]+)([^<]+)*(?:>(.*)<\/\1>|\s+\/>)",
                Input = "<div class=\"test\">Content</div>\n<br />",
                Description = "Matches simple HTML tags and content."
            });

            Examples.Add(new RegexExample
            {
                Name = "Phone Number (US)",
                Pattern = @"^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$",
                Input = "123-456-7890\n(123) 456-7890\n123.456.7890",
                Description = "Matches US phone number formats."
            });

            Examples.Add(new RegexExample
            {
                Name = "Hex Color Code",
                Pattern = @"^#?([a-fA-F0-9]{6}|[a-fA-F0-9]{3})$",
                Input = "#FFFFFF\n#000\n#G00",
                Description = "Matches 3 or 6 digit hex color codes."
            });

            Examples.Add(new RegexExample
            {
                Name = "Credit Card (Simple)",
                Pattern = @"^(?:4[0-9]{12}(?:[0-9]{3})?|5[1-5][0-9]{14}|3[47][0-9]{13}|6(?:011|5[0-9]{2})[0-9]{12})$",
                Input = "4111111111111111\n5500000000000000",
                Description = "Matches format of Visa, MasterCard, Amex, Discover."
            });

            Examples.Add(new RegexExample
            {
                Name = "JSON Property",
                Pattern = @"""(\w+)""\s*:\s*""?([^""]+)""?",
                Input = "{ \"name\": \"John\", \"age\": 30 }",
                Description = "Captures JSON property keys and values."
            });
        }

        private void Test()
        {
            ErrorMessage = null;
            Matches.Clear();
            ReplaceResult = string.Empty;

            if (string.IsNullOrEmpty(Pattern)) return;

            try
            {
                var options = RegexOptions.None;
                if (IgnoreCase) options |= RegexOptions.IgnoreCase;
                if (Multiline) options |= RegexOptions.Multiline;
                if (Singleline) options |= RegexOptions.Singleline;

                var regex = new Regex(Pattern, options);
                var matches = regex.Matches(Input ?? "");

                foreach (Match match in matches)
                {
                    var groups = match.Groups.Cast<Group>()
                        .Select((g, i) => new GroupItem { Index = i, Value = g.Value, Success = g.Success })
                        .ToList();

                    Matches.Add(new MatchItem
                    {
                        Index = match.Index,
                        Length = match.Length,
                        Value = match.Value,
                        Groups = new ObservableCollection<GroupItem>(groups)
                    });
                }

                if (!string.IsNullOrEmpty(ReplacementText))
                {
                    ReplaceResult = regex.Replace(Input ?? "", ReplacementText);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error: {ex.Message}";
            }
        }
    }

    public class MatchItem
    {
        public int Index { get; set; }
        public int Length { get; set; }
        public string Value { get; set; }
        public ObservableCollection<GroupItem> Groups { get; set; }
    }

    public class GroupItem
    {
        public int Index { get; set; }
        public string Value { get; set; }
        public bool Success { get; set; }
        public string Display => $"[{Index}]: {Value}";
    }

    public class RegexExample
    {
        public string Name { get; set; }
        public string Pattern { get; set; }
        public string Input { get; set; }
        public string Description { get; set; }
    }
}
