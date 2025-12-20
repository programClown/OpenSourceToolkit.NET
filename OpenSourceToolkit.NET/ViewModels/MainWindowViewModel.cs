using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows.Input;
using Avalonia.Data.Converters;
using CommunityToolkit.Mvvm.Input;
using OpenSourceToolkit.NET.Localization;
using OpenSourceToolkit.NET.Services;
using OpenSourceToolkit.NET.ViewModels.Tools;

namespace OpenSourceToolkit.NET.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public static IValueConverter SidebarWidthConverter { get; } = Converters.SidebarWidthConverter.Instance;
        public static IValueConverter ToggleIconConverter { get; } = Converters.ToggleIconConverter.Instance;
        public static IValueConverter FavoriteIconConverter { get; } = Converters.FavoriteIconConverter.Instance;
        public static IValueConverter FavoriteTooltipConverter { get; } = Converters.FavoriteTooltipConverter.Instance;

        // Star icon paths for favorites
        public const string StarFilledPath = "M12,17.27L18.18,21L16.54,13.97L22,9.24L14.81,8.62L12,2L9.19,8.62L2,9.24L7.45,13.97L5.82,21L12,17.27Z";

        private readonly List<ToolViewModel> _allTools;
        private readonly List<ToolGroup> _allGroups;
        private readonly HomeViewModel _homeViewModel;
        private ToolGroup _favoritesGroup;

        public ObservableCollection<ToolGroup> Groups { get; }

        public bool HasFavorites => _favoritesGroup?.Tools.Count > 0;

        private ToolViewModel _currentTool;
        public ToolViewModel CurrentTool
        {
            get => _currentTool;
            set => SetProperty(ref _currentTool, value);
        }

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    FilterTools();
                    OnPropertyChanged(nameof(IsSearchActive));
                }
            }
        }

        public bool IsSearchActive => !string.IsNullOrWhiteSpace(SearchText);

        private bool _isSidebarExpanded = true;
        public bool IsSidebarExpanded
        {
            get => _isSidebarExpanded;
            set => SetProperty(ref _isSidebarExpanded, value);
        }

        public ICommand ClearSearchCommand { get; }
        public ICommand ToggleSidebarCommand { get; }
        public ICommand OpenSettingsCommand { get; }
        public ICommand SelectToolCommand { get; }

        #region Localized Strings

        public string SearchToolsPlaceholder => ToolkitLocalization.GetString("Sidebar_SearchTools");
        public string HomeTooltip => ToolkitLocalization.GetString("Sidebar_Home");
        public string ToggleSidebarTooltip => ToolkitLocalization.GetString("Sidebar_ToggleSidebar");
        public string ThemeLabel => ToolkitLocalization.GetString("Sidebar_Theme");
        public string SettingsLabel => ToolkitLocalization.GetString("Sidebar_Settings");

        private void OnCultureChanged(object sender, CultureInfo culture)
        {
            // Refresh all localized property bindings
            OnPropertyChanged(nameof(SearchToolsPlaceholder));
            OnPropertyChanged(nameof(HomeTooltip));
            OnPropertyChanged(nameof(ToggleSidebarTooltip));
            OnPropertyChanged(nameof(ThemeLabel));
            OnPropertyChanged(nameof(SettingsLabel));
        }

        #endregion

        public MainWindowViewModel()
        {
            ClearSearchCommand = new RelayCommand(() => SearchText = string.Empty);
            ToggleSidebarCommand = new RelayCommand(() => IsSidebarExpanded = !IsSidebarExpanded);
            OpenSettingsCommand = new RelayCommand(OpenSettings);
            SelectToolCommand = new RelayCommand<ToolViewModel>(SelectTool);

            // Subscribe to localization changes
            ToolkitLocalization.CultureChanged += OnCultureChanged;

            // Initialize all tool view models
            var toolList = new List<ToolViewModel>
            {
                new UuidToolViewModel(),
                new LoremIpsumToolViewModel(),
                new MockDataToolViewModel(),
                new PrivacyPolicyToolViewModel(),
                new QrCodeToolViewModel(),
                new TextCaseToolViewModel(),
                new TimestampToolViewModel(),
                new HashToolViewModel(),
                new HmacToolViewModel(),
                new JwtToolViewModel(),
                new Base64ToolViewModel(),
                new ColorToolViewModel(),
                new UptimeToolViewModel(),
                new DnsToolViewModel(),
                new IpLocationToolViewModel(),
                new CronToolViewModel(),
                new FolderAnalyzerToolViewModel(),
                new AsciiArtToolViewModel(),
                new HardwareToolViewModel(),
                new PdfToolViewModel(),
                new FinancialCalculatorToolViewModel(),
                new IpCalculatorToolViewModel(),
                new ApiTesterToolViewModel(),
                new NextJsImageDecoderToolViewModel(),
                new AudioNoiseReductionToolViewModel(),
                new RegexTesterToolViewModel(),
                new VCardGeneratorToolViewModel(),
                new EthConverterToolViewModel(),
                new PasswordGeneratorToolViewModel(),
                new ClipboardImageSaverToolViewModel(),
                new JsonFormatterToolViewModel(),
                new ImageConverterToolViewModel(),
                new DiffCheckerToolViewModel(),
                new KeyboardTesterToolViewModel(),
                new SpeedTestToolViewModel(),
                new StopwatchTimerToolViewModel(),
                new SqlFormatterToolViewModel(),
                new MarkdownEditorToolViewModel(),
                new FontsViewerToolViewModel(),
                new ThemeTestingToolViewModel(),
                new ScientificCalculatorToolViewModel()
            };

            _allTools = new List<ToolViewModel>(toolList);

            // Add Home at the very top - pass allTools so it can show favorites as Quick Actions
            _homeViewModel = new HomeViewModel(NavigateToTool, _allTools);

            // Create groups
            _allGroups = new List<ToolGroup>
            {
                CreateGroup("Media & Files", "M14,2H6A2,2 0 0,0 4,4V20A2,2 0 0,0 6,22H18A2,2 0 0,0 20,20V8L14,2M18,20H6V4H13V9H18V20Z",
                    typeof(ImageConverterToolViewModel),
                    typeof(FolderAnalyzerToolViewModel),
                    typeof(AsciiArtToolViewModel),
                    typeof(PdfToolViewModel),
                    typeof(ClipboardImageSaverToolViewModel),
                    typeof(AudioNoiseReductionToolViewModel),
                    typeof(FontsViewerToolViewModel)),

                CreateGroup("Generators", "M19,13H13V19H11V13H5V11H11V5H13V11H19V13Z",
                    typeof(UuidToolViewModel),
                    typeof(LoremIpsumToolViewModel),
                    typeof(MockDataToolViewModel),
                    typeof(PrivacyPolicyToolViewModel),
                    typeof(QrCodeToolViewModel),
                    typeof(PasswordGeneratorToolViewModel),
                    typeof(VCardGeneratorToolViewModel)),

                CreateGroup("Converters", "M16,4L9,8.04V15.96L16,20L23,15.96V8.04M16,6.31L19.8,8.5L16,10.69L12.21,8.5M11,10.11L15,12.42V17.11L11,14.81M21,14.81L17,17.11V12.42L21,10.11V14.81M3,13H7V15H3M1,10H7V12H1M5,7H7V9H5",
                    typeof(TextCaseToolViewModel),
                    typeof(TimestampToolViewModel),
                    typeof(Base64ToolViewModel),
                    typeof(ColorToolViewModel),
                    typeof(EthConverterToolViewModel),
                    typeof(JsonFormatterToolViewModel)),

                CreateGroup("Security", "M12,1L3,5V11C3,16.55 6.84,21.74 12,23C17.16,21.74 21,16.55 21,11V5L12,1Z",
                    typeof(HashToolViewModel),
                    typeof(HmacToolViewModel),
                    typeof(JwtToolViewModel)),

                CreateGroup("Networking", "M12,21L15.6,16.2C14.6,15.45 13.35,15 12,15C10.65,15 9.4,15.45 8.4,16.2L12,21M12,3C7.95,3 4.21,4.34 1.2,6.6L3,9C5.5,7.12 8.62,6 12,6C15.38,6 18.5,7.12 21,9L22.8,6.6C19.79,4.34 16.05,3 12,3M12,9C9.3,9 6.81,9.89 4.8,11.4L6.6,13.8C8.1,12.67 9.97,12 12,12C14.03,12 15.9,12.67 17.4,13.8L19.2,11.4C17.19,9.89 14.7,9 12,9Z",
                    typeof(UptimeToolViewModel),
                    typeof(DnsToolViewModel),
                    typeof(IpLocationToolViewModel),
                    typeof(IpCalculatorToolViewModel),
                    typeof(SpeedTestToolViewModel)),

                CreateGroup("Development", "M8,3A2,2 0 0,0 6,5V9A2,2 0 0,1 4,11H3V13H4A2,2 0 0,1 6,15V19A2,2 0 0,0 8,21H10V19H8V14A2,2 0 0,0 6,12A2,2 0 0,0 8,10V5H10V3M16,3A2,2 0 0,1 18,5V9A2,2 0 0,0 20,11H21V13H20A2,2 0 0,0 18,15V19A2,2 0 0,1 16,21H14V19H16V14A2,2 0 0,1 18,12A2,2 0 0,1 16,10V5H14V3H16Z",
                    typeof(CronToolViewModel),
                    typeof(ApiTesterToolViewModel),
                    typeof(NextJsImageDecoderToolViewModel),
                    typeof(RegexTesterToolViewModel),
                    typeof(DiffCheckerToolViewModel),
                    typeof(SqlFormatterToolViewModel),
                    typeof(MarkdownEditorToolViewModel),
                    typeof(ThemeTestingToolViewModel)),

                CreateGroup("Hardware", "M4,6H20V16H4M20,18A2,2 0 0,0 22,16V6C22,4.89 21.1,4 20,4H4C2.89,4 2,4.89 2,6V16A2,2 0 0,0 4,18H0V20H24V18H20Z",
                    typeof(HardwareToolViewModel),
                    typeof(KeyboardTesterToolViewModel),
                    typeof(StopwatchTimerToolViewModel)),

                CreateGroup("Math", "M19,3H5C3.89,3 3,3.89 3,5V19A2,2 0 0,0 5,21H19A2,2 0 0,0 21,19V5A2,2 0 0,0 19,3M19,19H5V5H19V19M13,7.5H15V10.5H18V12.5H15V15.5H13V12.5H10V10.5H13V7.5M8,17L9.5,17L11,15L12.5,17L14,17L11.8,14.3L14,11.5H12.5L11,13.3L9.5,11.5H8L10.2,14.3L8,17Z",
                    typeof(ScientificCalculatorToolViewModel)),

                CreateGroup("Finance", "M5,6H23V18H5V6M14,9A3,3 0 0,1 17,12A3,3 0 0,1 14,15A3,3 0 0,1 11,12A3,3 0 0,1 14,9M9,8A2,2 0 0,1 7,10V14A2,2 0 0,1 9,16H19A2,2 0 0,1 21,14V10A2,2 0 0,1 19,8H9M1,10H3V20H19V22H1V10Z",
                    typeof(FinancialCalculatorToolViewModel))
            };

            Groups = new ObservableCollection<ToolGroup>(_allGroups);
            CurrentTool = _homeViewModel;

            // Subscribe to favorite changes on all tools
            foreach (var tool in _allTools)
            {
                tool.FavoriteChanged += OnToolFavoriteChanged;
            }

            // Build initial favorites group
            RebuildFavoritesGroup();
        }

        private void OnToolFavoriteChanged(ToolViewModel tool)
        {
            RebuildFavoritesGroup();
            // Also refresh Quick Actions on Home page
            _homeViewModel.RefreshQuickActions();
        }

        private void RebuildFavoritesGroup()
        {
            // Remove old favorites group if present
            if (_favoritesGroup != null && Groups.Contains(_favoritesGroup))
            {
                Groups.Remove(_favoritesGroup);
            }

            RebuildFavoritesGroupInternal();
        }

        public void GoHome()
        {
            CurrentTool = _homeViewModel;
            SearchText = string.Empty;
            OnPropertyChanged(nameof(ShowToolHeader));
        }

        /// <summary>
        /// Returns true if a tool header should be shown (false for Home page).
        /// </summary>
        public bool ShowToolHeader => CurrentTool != null && CurrentTool != _homeViewModel;

        /// <summary>
        /// Navigates to a tool by its ViewModel type.
        /// </summary>
        public void NavigateToToolByType(Type toolType)
        {
            var tool = _allTools.FirstOrDefault(t => t.GetType() == toolType);
            if (tool != null)
            {
                CurrentTool = tool;
                OnPropertyChanged(nameof(ShowToolHeader));
            }
        }

        private ToolGroup CreateGroup(string name, string iconPath, params Type[] toolTypes)
        {
            var group = new ToolGroup(name, iconPath);
            foreach (var type in toolTypes)
            {
                var tool = _allTools.FirstOrDefault(t => t.GetType() == type);
                if (tool != null)
                {
                    group.Tools.Add(tool);
                }
            }
            // Sort tools within group alphabetically
            var sorted = group.Tools.OrderBy(t => t.Name).ToList();
            group.Tools.Clear();
            foreach (var tool in sorted)
            {
                group.Tools.Add(tool);
            }
            return group;
        }

        private void FilterTools()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                // Restore all groups with all tools
                Groups.Clear();

                // Add favorites group first if any
                RebuildFavoritesGroupInternal();

                foreach (var group in _allGroups)
                {
                    Groups.Add(group);
                }
            }
            else
            {
                // Filter and show only matching tools
                Groups.Clear();

                // Filter favorites too
                var favoriteIds = AppSettings.Current.FavoriteToolIds;
                var filteredFavorites = _allTools.Where(t =>
                    favoriteIds.Contains(t.Id) &&
                    t.Name.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) >= 0
                ).OrderBy(t => t.Name).ToList();

                if (filteredFavorites.Count > 0)
                {
                    var favGroup = new ToolGroup("Favorites", StarFilledPath) { IsExpanded = true };
                    foreach (var tool in filteredFavorites)
                    {
                        favGroup.Tools.Add(tool);
                    }
                    Groups.Add(favGroup);
                }

                foreach (var group in _allGroups)
                {
                    var filteredTools = group.Tools.Where(t =>
                        t.Name.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) >= 0
                    ).ToList();

                    if (filteredTools.Count > 0)
                    {
                        var filteredGroup = new ToolGroup(group.Name, group.IconPath) { IsExpanded = true };
                        foreach (var tool in filteredTools)
                        {
                            filteredGroup.Tools.Add(tool);
                        }
                        Groups.Add(filteredGroup);
                    }
                }
            }
        }

        private void RebuildFavoritesGroupInternal()
        {
            var favoriteIds = AppSettings.Current.FavoriteToolIds;
            var favoriteTools = _allTools.Where(t => favoriteIds.Contains(t.Id)).OrderBy(t => t.Name).ToList();

            if (favoriteTools.Count > 0)
            {
                _favoritesGroup = new ToolGroup("Favorites", StarFilledPath);
                foreach (var tool in favoriteTools)
                {
                    _favoritesGroup.Tools.Add(tool);
                }
                Groups.Insert(0, _favoritesGroup);
            }
            else
            {
                _favoritesGroup = null;
            }

            OnPropertyChanged(nameof(HasFavorites));
        }

        private void NavigateToTool(Type toolType)
        {
            var tool = _allTools.FirstOrDefault(t => t.GetType() == toolType);
            if (tool != null)
            {
                CurrentTool = tool;
                SearchText = string.Empty;
            }
        }

        private void SelectTool(ToolViewModel tool)
        {
            if (tool != null)
            {
                CurrentTool = tool;
            }
        }

        private void OpenSettings()
        {
            // Settings will be handled via an event or the View code-behind
            // For now, this is a placeholder - the View will handle opening the dialog
            OnPropertyChanged(nameof(OpenSettingsCommand));
        }
    }
}
