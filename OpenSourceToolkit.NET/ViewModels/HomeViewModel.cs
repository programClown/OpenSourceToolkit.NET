using Avalonia.Media;
using CommunityToolkit.Mvvm.Input;
using OpenSourceToolkit.NET.Localization;
using OpenSourceToolkit.NET.Services;
using OpenSourceToolkit.NET.ViewModels.Tools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Input;

namespace OpenSourceToolkit.NET.ViewModels
{
    public class QuickActionItem
    {
        public string Title { get; }
        public string Description { get; }
        public string IconKey { get; }
        public string IconPath { get; }
        public SolidColorBrush IconBrush { get; }
        public Type ToolType { get; }

        public QuickActionItem(string title, string description, string iconKey, string iconPath, string iconColor, Type toolType)
        {
            Title = title;
            Description = description;
            IconKey = iconKey;
            IconPath = iconPath;
            IconBrush = new SolidColorBrush(Color.Parse(iconColor));
            ToolType = toolType;
        }
    }

    public partial class HomeViewModel : ToolViewModel
    {
        public override int Id => 0;
        public override string Name => ToolkitLocalization.GetString("Sidebar_Home");
        public override string Description => "";
        public override string IconPath => "M10 20v-6h4v6h5v-8h3L12 3 2 12h3v8z";

        #region Localized Strings

        public string AppTitle => ToolkitLocalization.GetString("App_Title");
        public string AppSubtitle => ToolkitLocalization.GetString("App_Subtitle");
        public string NoFavoritesTitle => ToolkitLocalization.GetString("Home_NoFavorites");
        public string NoFavoritesHint => ToolkitLocalization.GetString("Home_NoFavoritesHint");
        public string ViewOnGitHub => ToolkitLocalization.GetString("Home_ViewSourceOnGitHub");

        private void OnCultureChanged(object sender, CultureInfo culture)
        {
            OnPropertyChanged(nameof(AppTitle));
            OnPropertyChanged(nameof(AppSubtitle));
            OnPropertyChanged(nameof(NoFavoritesTitle));
            OnPropertyChanged(nameof(NoFavoritesHint));
            OnPropertyChanged(nameof(ViewOnGitHub));
            OnPropertyChanged(nameof(Name));

            // Refresh favorites to get newly translated tool names/descriptions
            RefreshQuickActions();
        }

        #endregion

        public ICommand NavigateCommand { get; }
        public ICommand OpenRepoCommand { get; }

        public ObservableCollection<QuickActionItem> QuickActions { get; }

        private readonly Action<Type> _navigateCallback;
        private readonly List<ToolViewModel> _allTools;

        // Color mapping for tools (matching web app's Tailwind colors)
        private static readonly Dictionary<int, string> ToolColors = new Dictionary<int, string>
        {
            { 1, "#3b82f6" },   // UUID - blue
            { 2, "#f59e0b" },   // Lorem Ipsum - amber
            { 3, "#10b981" },   // Mock Data - emerald
            { 4, "#8b5cf6" },   // Privacy Policy - violet
            { 5, "#475569" },   // QR Code - slate
            { 6, "#14b8a6" },   // Text Case - teal
            { 7, "#22c55e" },   // Timestamp - green
            { 8, "#10b981" },   // Hash - emerald
            { 9, "#64748b" },   // HMAC - slate
            { 10, "#ef4444" },  // JWT - red
            { 11, "#a855f7" },  // Base64 - purple
            { 12, "#ec4899" },  // Color - pink
            { 13, "#2563eb" },  // Uptime - blue
            { 14, "#06b6d4" },  // DNS - cyan
            { 15, "#6366f1" },  // IP Location - indigo
            { 16, "#a855f7" },  // Cron - purple
            { 17, "#3b82f6" },  // Folder Analyzer - blue
            { 18, "#9333ea" },  // ASCII Art - purple
            { 19, "#6366f1" },  // Hardware - indigo
            { 20, "#dc2626" },  // PDF - red
            { 21, "#10b981" },  // Financial Calc - emerald
            { 22, "#3b82f6" },  // IP Calculator - blue
            { 23, "#8b5cf6" },  // API Tester - violet
            { 24, "#10b981" },  // Next.js Image - emerald
            { 25, "#0d9488" },  // Audio Noise - teal
            { 26, "#22c55e" },  // Regex - green
            { 27, "#6366f1" },  // VCard - indigo
            { 28, "#3b82f6" },  // ETH Converter - blue
            { 29, "#ef4444" },  // Password - red
            { 30, "#f43f5e" },  // Clipboard Image - rose
            { 31, "#f97316" },  // JSON Formatter - orange
            { 32, "#f97316" },  // Image Converter - orange
            { 33, "#a855f7" },  // Diff Checker - purple
            { 34, "#6366f1" }   // Keyboard - indigo
        };

        public HomeViewModel(Action<Type> navigateCallback, List<ToolViewModel> allTools)
        {
            _navigateCallback = navigateCallback;
            _allTools = allTools;
            NavigateCommand = new RelayCommand<Type>(navigateCallback);
            OpenRepoCommand = new RelayCommand(OpenRepo);
            QuickActions = new ObservableCollection<QuickActionItem>();

            // Subscribe to localization changes
            ToolkitLocalization.CultureChanged += OnCultureChanged;

            RefreshQuickActions();
        }

        private void OpenRepo()
        {
            const string url = "https://github.com/tobitege/OpenSourceToolkit.NET";
            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Process.Start("xdg-open", url);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Process.Start("open", url);
                }
            }
            catch
            {
                // Silently ignore if browser cannot be opened
            }
        }

        public void RefreshQuickActions()
        {
            QuickActions.Clear();

            var favoriteIds = AppSettings.Current.FavoriteToolIds;
            var favoriteTools = _allTools
                .Where(t => favoriteIds.Contains(t.Id))
                .OrderBy(t => favoriteIds.IndexOf(t.Id)) // Keep order from settings
                .ToList();

            foreach (var tool in favoriteTools)
            {
                var color = ToolColors.TryGetValue(tool.Id, out var c) ? c : "#6366f1";
                QuickActions.Add(new QuickActionItem(
                    tool.Name,
                    tool.Description,
                    tool.IconKey,
                    tool.IconPath,
                    color,
                    tool.GetType()
                ));
            }
        }
    }
}
