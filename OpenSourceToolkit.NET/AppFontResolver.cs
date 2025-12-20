using Avalonia.Platform;
using PdfSharp.Fonts;
using System;
using System.IO;
using System.Reflection;

namespace OpenSourceToolkit.NET
{
    public class AppFontResolver : IFontResolver
    {
        public FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic)
        {
            // Map everything to Roboto to ensure consistent rendering
            // This handles "Arial", "Roboto", or any other font request by returning our bundled font
            return new FontResolverInfo("Roboto");
        }

        public byte[] GetFont(string faceName)
        {
            if (faceName == "Roboto")
            {
                try
                {
                    // Try loading from Avalonia Resources (embedded in assembly)
                    var uri = new Uri("avares://OpenSourceToolkit.NET/Assets/Fonts/Roboto-Regular.ttf");
                    if (AssetLoader.Exists(uri))
                    {
                        using (var stream = AssetLoader.Open(uri))
                        using (var ms = new MemoryStream())
                        {
                            stream.CopyTo(ms);
                            return ms.ToArray();
                        }
                    }
                }
                catch (Exception)
                {
                    // Fallback to file system if resource load fails
                }

                // Fallback: Try to read from the Assets folder relative to execution
                // This is useful if assets are copied to output directory but not embedded
                try
                {
                    var basePath = AppDomain.CurrentDomain.BaseDirectory;
                    var fontPath = Path.Combine(basePath, "Assets", "Fonts", "Roboto-Regular.ttf");

                    if (File.Exists(fontPath))
                    {
                        return File.ReadAllBytes(fontPath);
                    }
                }
                catch { }
            }
            return null;
        }
    }
}
