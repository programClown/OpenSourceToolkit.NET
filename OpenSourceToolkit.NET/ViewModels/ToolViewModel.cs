using System;
using System.Globalization;
using System.Text.Json;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using OpenSourceToolkit.NET.Localization;
using OpenSourceToolkit.NET.Services;

namespace OpenSourceToolkit.NET.ViewModels
{
    public abstract class ToolViewModel : ViewModelBase
    {
        /// <summary>
        /// Static event raised when settings dialog closes. Tools can subscribe to refresh their state.
        /// </summary>
        public static event Action SettingsClosed;

        /// <summary>
        /// Raises the SettingsClosed event. Called by MainWindow after settings dialog closes.
        /// </summary>
        public static void RaiseSettingsClosed() => SettingsClosed?.Invoke();

        /// <summary>
        /// Unique numeric ID for this tool. Used for persistence (e.g., favorites).
        /// </summary>
        public abstract int Id { get; }

        public abstract string Name { get; }
        public virtual string Description => "";

        // Resource key for the icon in Icons.axaml (preferred)
        public virtual string IconKey => null;

        // Default to a generic "Tool" icon (Grid/Apps style)
        public virtual string IconPath => "M4 8h4V4H4v4zm6 12h4v-4h-4v4zm-6 0h4v-4H4v4zm0-6h4v-4H4v4zm6 0h4v-4h-4v4zm6-10v4h4V4h-4zm-6 4h4V4h-4zm6 6h4v-4h-4v4zm0 6h4v-4h-4v4z";

        public virtual void Cleanup() { }

        /// <summary>
        /// Event raised when favorite status changes.
        /// </summary>
        public event Action<ToolViewModel> FavoriteChanged;

        /// <summary>
        /// Whether this tool is marked as a favorite.
        /// </summary>
        public bool IsFavorite
        {
            get => AppSettings.Current.FavoriteToolIds.Contains(Id);
            set
            {
                var favorites = AppSettings.Current.FavoriteToolIds;
                if (value && !favorites.Contains(Id))
                {
                    favorites.Add(Id);
                    AppSettings.Save();
                    OnPropertyChanged();
                    FavoriteChanged?.Invoke(this);
                }
                else if (!value && favorites.Contains(Id))
                {
                    favorites.Remove(Id);
                    AppSettings.Save();
                    OnPropertyChanged();
                    FavoriteChanged?.Invoke(this);
                }
            }
        }

        public ICommand ToggleFavoriteCommand { get; }

        protected ToolViewModel()
        {
            ToggleFavoriteCommand = new RelayCommand(() => IsFavorite = !IsFavorite);
            
            // Subscribe to culture changes to update localized properties
            ToolkitLocalization.CultureChanged += OnCultureChanged;
        }

        private void OnCultureChanged(object sender, CultureInfo culture)
        {
            // Notify that Name and Description properties have changed
            OnPropertyChanged(nameof(Name));
            OnPropertyChanged(nameof(Description));
        }

        /// <summary>
        /// Unique key for storing this tool's settings. Defaults to Name.
        /// </summary>
        protected virtual string SettingsKey => Name;

        /// <summary>
        /// Gets a setting value for this tool.
        /// </summary>
        protected T GetSetting<T>(string key, T defaultValue = default)
        {
            var fullKey = $"{SettingsKey}.{key}";
            if (AppSettings.Current.ToolSettings.TryGetValue(fullKey, out var json))
            {
                try
                {
                    return JsonSerializer.Deserialize<T>(json);
                }
                catch
                {
                    return defaultValue;
                }
            }
            return defaultValue;
        }

        /// <summary>
        /// Sets a setting value for this tool and saves to disk.
        /// </summary>
        protected void SetSetting<T>(string key, T value)
        {
            var fullKey = $"{SettingsKey}.{key}";
            AppSettings.Current.ToolSettings[fullKey] = JsonSerializer.Serialize(value);
            AppSettings.Save();
        }

        /// <summary>
        /// Last folder path used by file pickers in this tool.
        /// </summary>
        public string LastFolderPath
        {
            get => GetSetting<string>("LastFolderPath");
            set => SetSetting("LastFolderPath", value);
        }
    }
}
