using System;
using System.IO;
using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;
using System.Threading;

namespace Otrs2Alfresco
{
    public class Gzip : IDisposable
    {
        private const string DataFile = "data.gz";
        private string _tempFolder;

        public string ErrorText { get; private set; }

        public Gzip()
        {
            ErrorText = string.Empty;
            _tempFolder = Path.Combine("/tmp/gzip", DateTime.Now.Ticks.ToString());

            if (!Directory.Exists(_tempFolder))
            {
                Directory.CreateDirectory(_tempFolder);
            }
        }

        public bool Extract(byte[] data)
        {
            var dataPath = Path.Combine(_tempFolder, DataFile);

            File.WriteAllBytes(dataPath, data);

            var start = new ProcessStartInfo(Binaries.Gzip, DataFile);
            start.UseShellExecute = false;
            start.WorkingDirectory = _tempFolder;
            start.RedirectStandardError = true;
            start.RedirectStandardOutput = true;

            var process = Process.Start(start);
            var startTime = DateTime.Now;

            while (!process.HasExited)
            {
                if (DateTime.Now.Subtract(startTime).TotalSeconds > 2d)
                {
                    break;
                }
                else
                {
                    Thread.Sleep(100);
                }
            }

            if (process.ExitCode == 0)
            {
                File.Delete(dataPath);
                return true;
            }
            else
            {
                ErrorText = process.StandardOutput.ReadToEnd() + "\n" + process.StandardError.ReadToEnd();
                return false;
            }
        }

        public void Dispose()
        {
            if (Directory.Exists(_tempFolder))
            {
                Directory.Delete(_tempFolder, true);
            }
        }

        public IEnumerable<string> Files
        {
            get
            {
                var directory = new DirectoryInfo(_tempFolder);

                foreach (var file in directory.GetFiles("*", SearchOption.AllDirectories))
                {
                    yield return file.FullName;
                }
            }
        }
    }
}
