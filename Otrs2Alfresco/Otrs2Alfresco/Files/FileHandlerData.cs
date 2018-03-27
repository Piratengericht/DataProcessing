using System;

namespace Otrs2Alfresco
{
    public class FileHandlerData
    {
        public string Prefix { get; private set; }

        public DateTime Date { get; private set; }

        public string FileName { get; private set; }

        public byte[] Data { get; private set; }

        public string AddPrefix(int number)
        {
            return Prefix + "." + string.Format("{0:00}", number);
        }

        public FileHandlerData(
            string fileName,
            string prefix,
            DateTime date,
            byte[] data)
        {
            FileName = fileName;
            Prefix = prefix;
            Date = date;
            Data = data;
        }
    }
}
