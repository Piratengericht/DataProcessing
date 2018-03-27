using System;
using System.IO;
using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;

namespace Otrs2Alfresco
{
    public class Gpg : IDisposable
    {
        private const string DataFile = "data.gpg";
        private const string DataFilePlain = "data";
        private string _tempFolder;

        public string ErrorText { get; private set; }

        public Gpg()
        {
            ErrorText = string.Empty;
            _tempFolder = Path.Combine("/tmp/gpg", DateTime.Now.Ticks.ToString());

            if (!Directory.Exists(_tempFolder))
            {
                Directory.CreateDirectory(_tempFolder);
            }
        }

        public bool Decrypt(byte[] data)
        {
            var dataPath = Path.Combine(_tempFolder, DataFile);

            File.WriteAllBytes(dataPath, data);

            var start = new ProcessStartInfo(Binaries.Gpg2, "-o " + DataFilePlain + "-d " + DataFile);
            start.UseShellExecute = false;
            start.WorkingDirectory = _tempFolder;
            start.RedirectStandardError = true;
            start.RedirectStandardOutput = true;

            var process = Process.Start(start);

            process.WaitForExit(2000);

            if (!process.HasExited)
            {
                process.Kill();
                return false;
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
