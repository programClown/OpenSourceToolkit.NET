using System;
using System.Collections.ObjectModel;
using System.Globalization;
using OpenSourceToolkit.NET.Localization;
using OpenSourceToolkit.NET.Services;

namespace OpenSourceToolkit.NET.ViewModels
{
    public class ToolGroup : ViewModelBase
    {
        /// <summary>
        /// Internal name key used for settings persistence and localization lookup.
        /// </summary>
        public string Name { get; }
        public string IconPath { get; }
        public ObservableCollection<ToolViewModel> Tools { get; }

        /// <summary>
        /// Localized display name for the sidebar. Falls back to Name if no translation exists.
        /// </summary>
        public string DisplayName
        {
            get
            {
                // Convert name to resource key format: "Media & Files" -> "MediaFiles"
                var resourceKey = "Group_" + Name.Replace(" ", "").Replace("&", "");
                return ToolkitLocalization.GetString(resourceKey) ?? Name;
            }
        }

        private bool _isExpanded = true;
        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                if (SetProperty(ref _isExpanded, value))
                {
                    AppSettings.Current.GroupExpandedStates[Name] = value;
                    AppSettings.Save();
                }
            }
        }

        public ToolGroup(string name, string iconPath)
        {
            Name = name;
            IconPath = iconPath;
            Tools = new ObservableCollection<ToolViewModel>();

            // Load saved expanded state
            if (AppSettings.Current.GroupExpandedStates.TryGetValue(name, out var savedState))
            {
                _isExpanded = savedState;
            }

            // Subscribe to culture changes to update DisplayName
            ToolkitLocalization.CultureChanged += OnCultureChanged;
        }

        private void OnCultureChanged(object sender, CultureInfo culture)
        {
            OnPropertyChanged(nameof(DisplayName));
        }
    }
}
