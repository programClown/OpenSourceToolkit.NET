using System.Collections.ObjectModel;
using Flowery.Controls;

namespace OpenSourceToolkit.NET.Data;

/// <summary>
/// Provides OpenSourceToolkit-specific sidebar categories and languages.
/// Matches the existing tool groups: Favorites, Media & Files, Generators, Converters,
/// Security, Networking, Development, Hardware, Math, Finance.
/// </summary>
public static class ToolkitSidebarData
{
    /// <summary>
    /// Creates the default categories for the toolkit sidebar.
    /// Uses localization keys that will be resolved by ToolkitLocalization.
    /// </summary>
    public static ObservableCollection<SidebarCategory> CreateCategories()
    {
        return new ObservableCollection<SidebarCategory>
        {
            // Home stays at top
            new SidebarCategory
            {
                Name = "Sidebar_Home",
                IconKey = "DaisyIconHome",
                Items = new ObservableCollection<SidebarItem>
                {
                    new SidebarItem { Id = "welcome", Name = "Sidebar_Home", TabHeader = "Sidebar_Home" },
                    new ToolkitThemeSelectorItem { Id = "theme", Name = "Sidebar_Theme", TabHeader = "Sidebar_Home" },
                    new ToolkitLanguageSelectorItem { Id = "language", Name = "Sidebar_Language", TabHeader = "Sidebar_Home" }
                }
            },
            // Favorites - dynamically populated by MainWindow
            new SidebarCategory
            {
                Name = "Group_Favorites",
                IconKey = "DaisyIconStar",
                Items = new ObservableCollection<SidebarItem>() // Populated dynamically
            },
            // Media & Files
            new SidebarCategory
            {
                Name = "Group_MediaFiles",
                IconKey = "DaisyIconMediaFiles",
                Items = new ObservableCollection<SidebarItem>
                {
                    new SidebarItem { Id = "image-converter", Name = "Tool_ImageEditor_Name", TabHeader = "Group_MediaFiles" },
                    new SidebarItem { Id = "folder-analyzer", Name = "Tool_FolderAnalyzer_Name", TabHeader = "Group_MediaFiles" },
                    new SidebarItem { Id = "ascii-art", Name = "Tool_AsciiArt_Name", TabHeader = "Group_MediaFiles" },
                    new SidebarItem { Id = "pdf-tools", Name = "Tool_Pdf_Name", TabHeader = "Group_MediaFiles" },
                    new SidebarItem { Id = "clipboard-image", Name = "Tool_ClipboardImageSaver_Name", TabHeader = "Group_MediaFiles" },
                    new SidebarItem { Id = "audio-noise", Name = "Tool_AudioNoiseReduction_Name", TabHeader = "Group_MediaFiles" },
                    new SidebarItem { Id = "fonts-viewer", Name = "Tool_FontsViewer_Name", TabHeader = "Group_MediaFiles" }
                }
            },
            // Generators
            new SidebarCategory
            {
                Name = "Group_Generators",
                IconKey = "DaisyIconGenerator",
                Items = new ObservableCollection<SidebarItem>
                {
                    new SidebarItem { Id = "uuid", Name = "Tool_Uuid_Name", TabHeader = "Group_Generators" },
                    new SidebarItem { Id = "lorem-ipsum", Name = "Tool_LoremIpsum_Name", TabHeader = "Group_Generators" },
                    new SidebarItem { Id = "mock-data", Name = "Tool_MockData_Name", TabHeader = "Group_Generators" },
                    new SidebarItem { Id = "privacy-policy", Name = "Tool_PrivacyPolicy_Name", TabHeader = "Group_Generators" },
                    new SidebarItem { Id = "qr-code", Name = "Tool_QrCode_Name", TabHeader = "Group_Generators" },
                    new SidebarItem { Id = "password", Name = "Tool_PasswordGenerator_Name", TabHeader = "Group_Generators" },
                    new SidebarItem { Id = "vcard", Name = "Tool_VCardGenerator_Name", TabHeader = "Group_Generators" }
                }
            },
            // Converters
            new SidebarCategory
            {
                Name = "Group_Converters",
                IconKey = "DaisyIconConverter",
                Items = new ObservableCollection<SidebarItem>
                {
                    new SidebarItem { Id = "text-case", Name = "Tool_TextCase_Name", TabHeader = "Group_Converters" },
                    new SidebarItem { Id = "timestamp", Name = "Tool_Timestamp_Name", TabHeader = "Group_Converters" },
                    new SidebarItem { Id = "base64", Name = "Tool_Base64_Name", TabHeader = "Group_Converters" },
                    new SidebarItem { Id = "color", Name = "Tool_Color_Name", TabHeader = "Group_Converters" },
                    new SidebarItem { Id = "eth-converter", Name = "Tool_EthConverter_Name", TabHeader = "Group_Converters" },
                    new SidebarItem { Id = "json-formatter", Name = "Tool_JsonFormatter_Name", TabHeader = "Group_Converters" }
                }
            },
            // Security
            new SidebarCategory
            {
                Name = "Group_Security",
                IconKey = "DaisyIconSecurity",
                Items = new ObservableCollection<SidebarItem>
                {
                    new SidebarItem { Id = "hash", Name = "Tool_Hash_Name", TabHeader = "Group_Security" },
                    new SidebarItem { Id = "hmac", Name = "Tool_Hmac_Name", TabHeader = "Group_Security" },
                    new SidebarItem { Id = "jwt", Name = "Tool_Jwt_Name", TabHeader = "Group_Security" }
                }
            },
            // Networking
            new SidebarCategory
            {
                Name = "Group_Networking",
                IconKey = "DaisyIconNetworking",
                Items = new ObservableCollection<SidebarItem>
                {
                    new SidebarItem { Id = "uptime", Name = "Tool_Uptime_Name", TabHeader = "Group_Networking" },
                    new SidebarItem { Id = "dns", Name = "Tool_Dns_Name", TabHeader = "Group_Networking" },
                    new SidebarItem { Id = "ip-location", Name = "Tool_IpLocation_Name", TabHeader = "Group_Networking" },
                    new SidebarItem { Id = "ip-calculator", Name = "Tool_IpCalculator_Name", TabHeader = "Group_Networking" },
                    new SidebarItem { Id = "speed-test", Name = "Tool_SpeedTest_Name", TabHeader = "Group_Networking" }
                }
            },
            // Development
            new SidebarCategory
            {
                Name = "Group_Development",
                IconKey = "DaisyIconDevelopment",
                Items = new ObservableCollection<SidebarItem>
                {
                    new SidebarItem { Id = "cron", Name = "Tool_Cron_Name", TabHeader = "Group_Development" },
                    new SidebarItem { Id = "api-tester", Name = "Tool_ApiTester_Name", TabHeader = "Group_Development" },
                    new SidebarItem { Id = "nextjs-image", Name = "Tool_NextJsImageDecoder_Name", TabHeader = "Group_Development" },
                    new SidebarItem { Id = "regex", Name = "Tool_RegexTester_Name", TabHeader = "Group_Development" },
                    new SidebarItem { Id = "diff-checker", Name = "Tool_DiffChecker_Name", TabHeader = "Group_Development" },
                    new SidebarItem { Id = "sql-formatter", Name = "Tool_SqlFormatter_Name", TabHeader = "Group_Development" },
                    new SidebarItem { Id = "markdown-editor", Name = "Tool_MarkdownEditor_Name", TabHeader = "Group_Development" },
                    new SidebarItem { Id = "theme-testing", Name = "Tool_ThemeTesting_Name", TabHeader = "Group_Development" }
                }
            },
            // Hardware
            new SidebarCategory
            {
                Name = "Group_Hardware",
                IconKey = "DaisyIconHardware",
                Items = new ObservableCollection<SidebarItem>
                {
                    new SidebarItem { Id = "hardware", Name = "Tool_Hardware_Name", TabHeader = "Group_Hardware" },
                    new SidebarItem { Id = "keyboard-tester", Name = "Tool_KeyboardTester_Name", TabHeader = "Group_Hardware" },
                    new SidebarItem { Id = "stopwatch-timer", Name = "Tool_StopwatchTimer_Name", TabHeader = "Group_Hardware" }
                }
            },
            // Math
            new SidebarCategory
            {
                Name = "Group_Math",
                IconKey = "DaisyIconMath",
                Items = new ObservableCollection<SidebarItem>
                {
                    new SidebarItem { Id = "calculator", Name = "Tool_ScientificCalculator_Name", TabHeader = "Group_Math" }
                }
            },
            // Finance
            new SidebarCategory
            {
                Name = "Group_Finance",
                IconKey = "DaisyIconFinance",
                Items = new ObservableCollection<SidebarItem>
                {
                    new SidebarItem { Id = "financial-calculator", Name = "Tool_FinancialCalculator_Name", TabHeader = "Group_Finance" }
                }
            },
            // Settings at bottom
            new SidebarCategory
            {
                Name = "Sidebar_Settings",
                IconKey = "DaisyIconSettings",
                Items = new ObservableCollection<SidebarItem>
                {
                    new ToolkitSettingsItem { Id = "settings", Name = "Sidebar_Settings", TabHeader = "Sidebar_Settings" }
                }
            }
        };
    }

    /// <summary>
    /// Creates the available languages for the toolkit.
    /// Uses ToolkitLocalization.SupportedLanguages as the single source of truth.
    /// </summary>
    public static ObservableCollection<SidebarLanguage> CreateLanguages()
    {
        var languages = new ObservableCollection<SidebarLanguage>();
        foreach (var (code, displayName) in Localization.ToolkitLocalization.SupportedLanguages)
        {
            languages.Add(new SidebarLanguage { Code = code, DisplayName = displayName });
        }
        return languages;
    }
}

/// <summary>
/// Toolkit-specific sidebar item for theme selection.
/// Extends the library's SidebarThemeSelectorItem so the existing template works.
/// </summary>
public class ToolkitThemeSelectorItem : SidebarThemeSelectorItem
{
}

/// <summary>
/// Toolkit-specific sidebar item for language selection.
/// Extends the library's SidebarLanguageSelectorItem so the existing template works.
/// </summary>
public class ToolkitLanguageSelectorItem : SidebarLanguageSelectorItem
{
}

/// <summary>
/// Toolkit-specific sidebar item for settings.
/// Opening settings requires special handling (dialog window).
/// </summary>
public class ToolkitSettingsItem : SidebarItem
{
}

/// <summary>
/// Toolkit-specific sidebar item for tool entries.
/// Links to a tool by its numeric ID and supports favorites functionality.
/// </summary>
public class ToolkitToolSidebarItem : SidebarItem
{
    /// <summary>
    /// The numeric ID of the tool this item represents.
    /// Used to look up the tool's favorite state.
    /// </summary>
    public int ToolId { get; set; }

    public ToolkitToolSidebarItem()
    {
        // Tool items show the favorite star icon
        ShowFavoriteIcon = true;
    }
}
