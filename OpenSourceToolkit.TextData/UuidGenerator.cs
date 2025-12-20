using System;
using System.Collections.Generic;

namespace OpenSourceToolkit.TextData
{
    public enum GuidFormat
    {
        Registry,
        CSharpAttribute,
        VbNetAttribute,
        DefineGuid,
        ImplementOleCreate,
        StructGuid,
        Plain,
        Short
    }

    public class UuidGenerator
    {
        public static Guid Generate()
        {
            return Guid.NewGuid();
        }

        public static string Format(Guid guid, GuidFormat format)
        {
            var bytes = guid.ToByteArray();
            var upper = guid.ToString("D").ToUpperInvariant();

            switch (format)
            {
                case GuidFormat.Registry:
                    return $"{{{upper}}}";

                case GuidFormat.CSharpAttribute:
                    return $"[Guid(\"{upper}\")]";

                case GuidFormat.VbNetAttribute:
                    return $"<Guid(\"{upper}\")>";

                case GuidFormat.DefineGuid:
                    return $"DEFINE_GUID(<<name>>, 0x{bytes[3]:x2}{bytes[2]:x2}{bytes[1]:x2}{bytes[0]:x2}, " +
                           $"0x{bytes[5]:x2}{bytes[4]:x2}, 0x{bytes[7]:x2}{bytes[6]:x2}, " +
                           $"0x{bytes[8]:x2}, 0x{bytes[9]:x2}, 0x{bytes[10]:x2}, 0x{bytes[11]:x2}, " +
                           $"0x{bytes[12]:x2}, 0x{bytes[13]:x2}, 0x{bytes[14]:x2}, 0x{bytes[15]:x2});";

                case GuidFormat.ImplementOleCreate:
                    return $"// {{{upper}}}\r\n" +
                           $"IMPLEMENT_OLECREATE(<<class>>, <<external_name>>, " +
                           $"0x{bytes[3]:x2}{bytes[2]:x2}{bytes[1]:x2}{bytes[0]:x2}, " +
                           $"0x{bytes[5]:x2}{bytes[4]:x2}, 0x{bytes[7]:x2}{bytes[6]:x2}, " +
                           $"0x{bytes[8]:x2}, 0x{bytes[9]:x2}, 0x{bytes[10]:x2}, 0x{bytes[11]:x2}, " +
                           $"0x{bytes[12]:x2}, 0x{bytes[13]:x2}, 0x{bytes[14]:x2}, 0x{bytes[15]:x2});";

                case GuidFormat.StructGuid:
                    return $"// {{{upper}}}\r\n" +
                           $"static const GUID <<name>> = {{ 0x{bytes[3]:x2}{bytes[2]:x2}{bytes[1]:x2}{bytes[0]:x2}, " +
                           $"0x{bytes[5]:x2}{bytes[4]:x2}, 0x{bytes[7]:x2}{bytes[6]:x2}, " +
                           $"{{ 0x{bytes[8]:x2}, 0x{bytes[9]:x2}, 0x{bytes[10]:x2}, 0x{bytes[11]:x2}, " +
                           $"0x{bytes[12]:x2}, 0x{bytes[13]:x2}, 0x{bytes[14]:x2}, 0x{bytes[15]:x2} }} }};";

                case GuidFormat.Plain:
                    return guid.ToString("D").ToLowerInvariant();

                case GuidFormat.Short:
                    return Convert.ToBase64String(bytes)
                        .Replace("/", "_")
                        .Replace("+", "-")
                        .Substring(0, 22);

                default:
                    return guid.ToString("D").ToLowerInvariant();
            }
        }

        public static string GenerateFormatted(GuidFormat format)
        {
            return Format(Generate(), format);
        }

        public static List<string> GenerateBatch(GuidFormat format, int count)
        {
            count = Math.Max(1, Math.Min(count, 100));
            var results = new List<string>(count);
            for (int i = 0; i < count; i++)
            {
                results.Add(GenerateFormatted(format));
            }
            return results;
        }
    }
}
