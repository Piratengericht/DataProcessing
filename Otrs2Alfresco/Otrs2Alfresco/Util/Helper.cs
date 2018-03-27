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
            return input.Replace("*", ".")
                .Replace("/", ".")
                .Replace(":", ".");
        }
    }
}

