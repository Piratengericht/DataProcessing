using System;
using System.Linq;
using System.Collections.Generic;

namespace Otrs2Alfresco
{
    public static class Binaries
    {
        public const string Xelatex = "/usr/bin/xelatex";
        public const string Soffice = "/usr/bin/soffice";
        public const string Unzip = "/usr/bin/unzip";
        public const string Tar = "/bin/tar";
        public const string Gzip = "/bin/gzip";
        public const string Bzip2 = "/bin/bzip2";
        public const string Gpg2 = "/usr/bin/gpg2";

        public static IEnumerable<string> All
        {
            get
            {
                yield return Xelatex;
                yield return Soffice;
                yield return Unzip;
                yield return Tar;
                yield return Gzip;
            }
        }
    }
}
