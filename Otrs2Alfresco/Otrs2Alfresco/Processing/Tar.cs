using System;
using System.IO;
using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;

namespace Otrs2Alfresco
{
    public class Tar : IDisposable
    {
        private const string DataFile = "data";
        private string _tempFolder;

        public string ErrorText { get; private set; }

		public Tar()
        {
            ErrorText = string.Empty;
            _tempFolder = Path.Combine("/tmp/untar", DateTime.Now.Ticks.ToString());

            if (!Directory.Exists(_tempFolder))
            {
                Directory.CreateDirectory(_tempFolder);
            }
        }

		public bool Extract(byte[] data, string suffix)
        {
			var dataPath = Path.Combine(_tempFolder, DataFile + suffix);

			File.WriteAllBytes(dataPath, data);

            var start = new ProcessStartInfo(Binaries.Tar, "-xf " + DataFile + suffix);
            start.UseShellExecute = false;
            start.WorkingDirectory = _tempFolder;
            start.RedirectStandardError = true;
            start.RedirectStandardOutput = true;

            var process = Process.Start(start);

            process.WaitForExit();

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
