using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Xml;
using System.Xml.Linq;
using YamlDotNet.Serialization;

namespace OpenSourceToolkit.Converters
{
    public static class JsonXmlYamlConverter
    {
        public static string FormatJson(string json, bool minify)
        {
            if (string.IsNullOrWhiteSpace(json)) return string.Empty;
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = !minify };
                var jsonElement = JsonSerializer.Deserialize<JsonElement>(json);
                return JsonSerializer.Serialize(jsonElement, options);
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"Invalid JSON: {ex.Message}", ex);
            }
        }

        public static string FormatXml(string xml, bool minify)
        {
            if (string.IsNullOrWhiteSpace(xml)) return string.Empty;
            try
            {
                var doc = XDocument.Parse(xml);
                var settings = new XmlWriterSettings
                {
                    Indent = !minify,
                    OmitXmlDeclaration = false,
                    NewLineOnAttributes = false
                };

                using (var stringWriter = new StringWriter())
                using (var xmlWriter = XmlWriter.Create(stringWriter, settings))
                {
                    doc.WriteTo(xmlWriter);
                    xmlWriter.Flush();
                    return stringWriter.ToString();
                }
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"Invalid XML: {ex.Message}", ex);
            }
        }

        public static string FormatYaml(string yaml)
        {
            if (string.IsNullOrWhiteSpace(yaml)) return string.Empty;
            try
            {
                var deserializer = new DeserializerBuilder().Build();
                var yamlObject = deserializer.Deserialize(new StringReader(yaml));

                var serializer = new SerializerBuilder().Build();
                return serializer.Serialize(yamlObject);
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"Invalid YAML: {ex.Message}", ex);
            }
        }

        public static string Convert(string input, string fromFormat, string toFormat)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;

            object data = null;

            // Parse Input
            if (fromFormat.Equals("json", StringComparison.OrdinalIgnoreCase))
            {
                var jsonElement = JsonSerializer.Deserialize<JsonElement>(input);
                data = ConvertJsonElementToNative(jsonElement);
            }
            else if (fromFormat.Equals("yaml", StringComparison.OrdinalIgnoreCase))
            {
                var deserializer = new DeserializerBuilder().Build();
                data = deserializer.Deserialize<object>(new StringReader(input));
            }
            else if (fromFormat.Equals("xml", StringComparison.OrdinalIgnoreCase))
            {
                throw new NotSupportedException("XML parsing not fully implemented in this demo");
            }
            else
            {
                throw new NotSupportedException($"Unsupported input format: {fromFormat}");
            }

            // Convert to Output
            if (toFormat.Equals("json", StringComparison.OrdinalIgnoreCase))
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                return JsonSerializer.Serialize(data, options);
            }
            else if (toFormat.Equals("yaml", StringComparison.OrdinalIgnoreCase))
            {
                var serializer = new SerializerBuilder().Build();
                return serializer.Serialize(data);
            }
            else if (toFormat.Equals("xml", StringComparison.OrdinalIgnoreCase))
            {
                return JsonToXml(data);
            }

            return input;
        }

        private static object ConvertJsonElementToNative(JsonElement element)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.Object:
                    var dict = new Dictionary<string, object>();
                    foreach (var prop in element.EnumerateObject())
                    {
                        dict[prop.Name] = ConvertJsonElementToNative(prop.Value);
                    }
                    return dict;
                case JsonValueKind.Array:
                    var list = new List<object>();
                    foreach (var item in element.EnumerateArray())
                    {
                        list.Add(ConvertJsonElementToNative(item));
                    }
                    return list;
                case JsonValueKind.String:
                    return element.GetString();
                case JsonValueKind.Number:
                    if (element.TryGetInt64(out long l)) return l;
                    return element.GetDouble();
                case JsonValueKind.True:
                    return true;
                case JsonValueKind.False:
                    return false;
                case JsonValueKind.Null:
                    return null;
                default:
                    return element.ToString();
            }
        }

        private static string JsonToXml(object data, string rootName = "root")
        {
             return $"<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n<{rootName}>{Xmlify(data)}</{rootName}>";
        }

        private static string Xmlify(object data)
        {
            if (data == null) return string.Empty;

            if (data is List<object> list)
            {
                var sb = new System.Text.StringBuilder();
                foreach (var item in list)
                {
                    sb.Append(Xmlify(item));
                }
                return sb.ToString();
            }
            else if (data is Dictionary<object, object> dictObj) // YamlDotNet deserializes to object keys
            {
                 var sb = new System.Text.StringBuilder();
                 foreach (var kvp in dictObj)
                 {
                     var key = kvp.Key.ToString();
                     sb.Append($"<{key}>{Xmlify(kvp.Value)}</{key}>");
                 }
                 return sb.ToString();
            }
            else if (data is Dictionary<string, object> dict)
            {
                var sb = new System.Text.StringBuilder();
                foreach (var kvp in dict)
                {
                    sb.Append($"<{kvp.Key}>{Xmlify(kvp.Value)}</{kvp.Key}>");
                }
                return sb.ToString();
            }
            else
            {
                return System.Security.SecurityElement.Escape(data.ToString());
            }
        }
    }
}
