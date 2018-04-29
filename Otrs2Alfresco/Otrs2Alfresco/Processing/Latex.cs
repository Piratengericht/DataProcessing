using System;
using System.Diagnostics;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Otrs2Alfresco
{
    public enum LatexEscapeMode
    {
        SingleLine,
        MultiLine,
        Minimal,
    }

    public class Latex
    {
        private string _text;
        private Dictionary<string, string> _replaces;
        private Dictionary<string, byte[]> _files;

        public string ErrorText { get; private set; }

        public Latex(string text)
        {
            ErrorText = string.Empty;
            _text = text;
            _replaces = new Dictionary<string, string>();
            _files = new Dictionary<string, byte[]>();
        }

        public void Add(string find, string replace, LatexEscapeMode escape = LatexEscapeMode.SingleLine)
        {
            switch (escape)
            {
                case LatexEscapeMode.MultiLine:
                    replace = ConvertMultiline(replace);
                    break;
                case LatexEscapeMode.SingleLine:
                    replace = ConvertSingleLine(replace);
                    break;
                case LatexEscapeMode.Minimal:
                    replace = SanatizeLatex(replace);
                    break;
            }

            _replaces.Add(find, replace);
        }

        public void Add(string find, string fileName, byte[] data)
        {
            fileName = SanatizeFileName(fileName);
            _replaces.Add(find, fileName);
            _files.Add(fileName, data);
        }

        private string SanatizeFileName(string fileName)
        {
            return fileName.Replace(@"\", string.Empty)
                           .Replace("#", string.Empty)
                           .Replace("_", string.Empty)
                           .Replace("$", string.Empty);
        }

        private string ConvertParagraph(string text)
        {
            var builder = new StringBuilder();

            foreach (var line in text.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries))
            {
                var trimmed = line.Trim();

                builder.AppendLine(trimmed + @" \\");
            }

            return builder.ToString();
        }

        private string ConvertMultiline(string text)
        {
            // replace command chars
            text = ConvertSingleLine(text);

            text = text.Replace("\r\n", "\n")
                       .Replace("\r", "\n");

            var paras = new List<string>();

            foreach (var para in text.Split(new string[] { "\n\n" }, StringSplitOptions.RemoveEmptyEntries))
            {
                paras.Add(ConvertParagraph(para.Trim()));
            }

            return string.Join("\n\n", paras.ToArray());
        }

        private string ConvertSingleLine(string text)
        {
            text = text.Replace(@"\", @"\symbol{92}")
                       .Replace(@"&", @"\&")
                       .Replace("\"", @"\textquotedbl{}");

            text = Regex.Replace(text, @"^\[(.*)\](.*)$", "{[$1]}$2");
            text = Regex.Replace(text, @"^\[(.*)$", "{[}$1");

            return SanatizeLatex(text);
        }

        private string SanatizeLatex(string text)
        {
            return text.Replace("#", @"\#")
                       .Replace("_", @"\_")
                       .Replace("$", @"\$");
        }

        public string TexDocument
        {
            get
            {
                var text = _text;

                foreach (var r in _replaces)
                {
                    text = text.Replace(r.Key, r.Value);
                }

                return text;
            }
        }

        public byte[] Compile()
        {
            const string documentName = "document.tex";
            const string pdfName = "document.pdf";
            string tempFolder = Path.Combine("/tmp", DateTime.Now.Ticks.ToString());

            if (!Directory.Exists(tempFolder))
            {
                Directory.CreateDirectory(tempFolder);
            }

            try
            {
                foreach (var f in _files)
                {
                    File.WriteAllBytes(Path.Combine(tempFolder, f.Key), f.Value);
                }

                File.WriteAllText(Path.Combine(tempFolder, documentName), TexDocument);

                var start = new ProcessStartInfo(Binaries.Xelatex, documentName);
                start.UseShellExecute = false;
                start.WorkingDirectory = tempFolder;
                start.RedirectStandardError = true;
                start.RedirectStandardOutput = true;

                var process = Process.Start(start);

                process.WaitForExit(10000);

                if (!process.HasExited)
                {
                    process.Kill();
                    ErrorText = process.StandardOutput.ReadToEnd() + "\n" + process.StandardError.ReadToEnd();
                    return null;
                }

                if (process.ExitCode != 0)
                {
                    ErrorText = process.StandardOutput.ReadToEnd() + "\n" + process.StandardError.ReadToEnd();
                    return null;
                }

                var pdfPath = Path.Combine(tempFolder, pdfName);

                if (!File.Exists(pdfPath))
                {
                    throw new Exception("PDF not created");
                }

                return File.ReadAllBytes(pdfPath);
            }
            finally
            {
                if (Directory.Exists(tempFolder))
                {
                    Directory.Delete(tempFolder, true);
                }
            }
        }
    }
}
