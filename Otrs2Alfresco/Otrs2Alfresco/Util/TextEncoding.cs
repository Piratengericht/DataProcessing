using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;

namespace Otrs2Alfresco
{
    public static class TextEncoding
    {
        private static IEnumerable<Encoding> Encodings
        {
            get
            {
                yield return Encoding.UTF8;
                yield return Encoding.GetEncoding(1252);
                yield return Encoding.UTF7;
                yield return Encoding.ASCII;
            }
        }

        private static bool IsGoodChar(char c)
        {
            const string addons = "äöüéàèêïë\n\r";

            if (char.IsLetterOrDigit(c) ||
                char.IsPunctuation(c) ||
                char.IsWhiteSpace(c))
            {
                return true;
            }
            else if (addons.Contains(c))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private static double QuoteOfGoodChars(string text)
        {
            return 1d / (double)text.Length *
                   (double)text.Count(t => IsGoodChar(t)); 
        }

        private static double Preference(Encoding encoding, byte[] data)
        {
            var french = "éèà";
            var german = "äöü";
            var text = encoding.GetString(data);

            if (text.Any(t => german.Contains(t)) &&
                text.Any(t => french.Contains(t)))
            {
                return 100d - QuoteOfGoodChars(text);
            }
            else if (text.Any(t => german.Contains(t)))
            {
                return 200d - QuoteOfGoodChars(text);
            }
            else if (text.Any(t => french.Contains(t)))
            {
                return 300d - QuoteOfGoodChars(text);
            }
            else
            {
                return 900d - QuoteOfGoodChars(text);
            }
        }

        public static string AutoConvertTextData(byte[] data)
        {
            var list = new List<Tuple<double, Encoding>>();

            foreach (var e in Encodings)
            {
                list.Add(new Tuple<double, Encoding>(Preference(e, data), e));
            }

            var encoding = list.OrderBy(i => i.Item1).Select(i => i.Item2).First();
            return encoding.GetString(data);
        }
    }
}
