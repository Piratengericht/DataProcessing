using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Otrs2Alfresco
{
    public static class Helper
    {
        private const int MaxAlfrescoNameLength = 128;

        public static string FormatDateTime(DateTime dateTime)
        {
            return dateTime.ToString("dd.MM.yyyy HH:mm:ss", CultureInfo.InvariantCulture);
        }

        public static string CreateName(string prefix, string name, string suffix)
        {
            if (prefix.IsNullOrEmpty())
            {
                prefix = string.Empty;
            }
            else if (name.Trim().StartsWith(prefix.Trim(), StringComparison.InvariantCulture))
            {
                prefix = string.Empty;
            }
            else
            {
                prefix = prefix.Trim() + " ";
            }

            if (suffix.IsNullOrEmpty())
            {
                suffix = string.Empty;
            }
            else
            {
                suffix = "." + suffix.Trim();
            }

            var maxLength = MaxAlfrescoNameLength - (prefix.Length + suffix.Length + 1);
            name = SanatizeName(name, maxLength);

            return prefix + name + suffix;
        }

        public static string SanatizeName(string input, int maxLength)
        {
            input = input.Trim();

            input = Regex.Replace(input, @"\[.*?\]", "").Trim();

            input = input.Replace("UNCHECKED", "")
                         .Replace("Re: ", "Re ")
                         .Replace("RE: ", "Re ")
                         .Replace("Aw: ", "Aw ")
                         .Replace("AW: ", "Aw ")
                         .Replace("Fwd: ", "Fwd ")
                         .Replace("FWD: ", "Fwd ")
                         .Replace("Wg: ", "Wg ")
                         .Replace("WG: ", "Wg ")
                         .Replace("*", "")
                         .Replace("/", "-")
                         .Replace(":", "")
                         .Replace("\t", " ")
                         .Replace("  ", " ")
                         .Replace("  ", " ")
                         .Replace("  ", " ");

            if (input.EndsWith(".", StringComparison.InvariantCulture))
            {
                input = input.Substring(0, input.Length - 1);
            }

            if (input == string.Empty)
            {   
                input = "Kein Titel";
            }

            if (input.Length > maxLength)
            {
                input = input.Substring(0, maxLength);
            }

            return input;
        }
    }
}

