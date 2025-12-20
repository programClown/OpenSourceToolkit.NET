namespace OpenSourceToolkit.NET.ViewModels.Tools.ImageConverter.Models
{
    /// <summary>
    /// Represents an ICO size preset for multi-size icon generation.
    /// </summary>
    public class IcoSizePreset
    {
        public string Name { get; }
        public int[] Sizes { get; }

        public IcoSizePreset(string name, int[] sizes)
        {
            Name = name;
            Sizes = sizes;
        }

        public override string ToString() => Name;
    }
}
