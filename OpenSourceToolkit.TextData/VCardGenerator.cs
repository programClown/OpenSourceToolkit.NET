using System;
using System.Text;

namespace OpenSourceToolkit.TextData
{
    public static class VCardGenerator
    {
        public static string Generate(string firstName, string lastName, string email = null, string phone = null, string org = null)
        {
            var options = new VCardOptions
            {
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                Phone = phone,
                Organization = org
            };
            return Generate(options);
        }

        public static string Generate(VCardOptions options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            var sb = new StringBuilder();
            sb.AppendLine("BEGIN:VCARD");
            sb.AppendLine("VERSION:3.0");
            sb.AppendLine($"N:{options.LastName};{options.FirstName};;;");
            sb.AppendLine($"FN:{options.FirstName} {options.LastName}");

            if (!string.IsNullOrEmpty(options.Organization))
                sb.AppendLine($"ORG:{options.Organization}");

            if (!string.IsNullOrEmpty(options.Title))
                sb.AppendLine($"TITLE:{options.Title}");

            if (!string.IsNullOrEmpty(options.Phone))
                sb.AppendLine($"TEL;TYPE=CELL:{options.Phone}");

            if (!string.IsNullOrEmpty(options.Email))
                sb.AppendLine($"EMAIL:{options.Email}");

            if (!string.IsNullOrEmpty(options.Website))
                sb.AppendLine($"URL:{options.Website}");

            if (HasAddress(options))
            {
                sb.AppendLine($"ADR;TYPE=WORK:;;{options.Street ?? ""};{options.City ?? ""};{options.State ?? ""};{options.PostalCode ?? ""};{options.Country ?? ""}");
            }

            if (!string.IsNullOrEmpty(options.Note))
                sb.AppendLine($"NOTE:{options.Note}");

            sb.AppendLine("END:VCARD");
            return sb.ToString();
        }

        private static bool HasAddress(VCardOptions options)
        {
            return !string.IsNullOrEmpty(options.Street) ||
                   !string.IsNullOrEmpty(options.City) ||
                   !string.IsNullOrEmpty(options.State) ||
                   !string.IsNullOrEmpty(options.PostalCode) ||
                   !string.IsNullOrEmpty(options.Country);
        }
    }
}
