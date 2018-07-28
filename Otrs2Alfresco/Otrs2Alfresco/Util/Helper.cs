using System;
using System.Globalization;

namespace Otrs2Alfresco
{
    public static class Helper
    {
        public static string FormatDateTime(DateTime dateTime)
        {
            return dateTime.ToString("dd.MM.yyyy HH:mm:ss", CultureInfo.InvariantCulture);
        }

        public static string SanatizeName(string input)
        {
            input = input.Trim();

            input = input.Replace("*", ".")
                .Replace("/", ".")
                .Replace(":", ".");

            if (input.EndsWith(".", StringComparison.InvariantCulture))
            {
                input = input.Substring(0, input.Length - 1);
            }

            if (input == string.Empty)
            {   
                input = "_";
            }

            return input;
        }
    }
}

