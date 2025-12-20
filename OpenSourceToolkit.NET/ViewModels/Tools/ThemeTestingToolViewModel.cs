using OpenSourceToolkit.NET.Localization;

namespace OpenSourceToolkit.NET.ViewModels.Tools
{
    /// <summary>
    /// Theme Testing tool for previewing all Semi Avalonia themed controls.
    /// </summary>
    public partial class ThemeTestingToolViewModel : ToolViewModel
    {
        public override int Id => 40;
        public override string Name => ToolkitLocalization.GetString("Tool_ThemeTesting_Name");
        public override string Description => ToolkitLocalization.GetString("Tool_ThemeTesting_Description");
        public override string IconKey => "PaletteIcon";

        public ThemeTestingToolViewModel()
        {
        }
    }
}
