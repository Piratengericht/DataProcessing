using System;
using System.IO;

namespace Otrs2Alfresco
{
    public class FsCase : ITargetCase
    {
        private Logger _log;
        private Config _config;
        private string _path;

        public FsCase(Logger log, Config config, string path)
        {
            _log = log;
            _config = config;
            _path = path;
        }

        public void CreateFile(string filename, byte[] filedata)
        {
            var filePath = Path.Combine(_path, filename);
            File.WriteAllBytes(filePath, filedata);
        }

        public bool FileExists(string prefix)
        {
            return Directory.GetFiles(_path, prefix + "*", SearchOption.TopDirectoryOnly).Length > 0;
        }
    }

    public class FsApi : ITargetApi
    {
        private Logger _log;
        private Config _config;
        private string _path;

        public FsApi(Logger log, Config config)
        {
            _log = log;
            _config = config;
            _path = _config.FileSystemPath;
        }

        public ITargetCase OpenOrCreateCase(string caseName)
        {
            var root = new DirectoryInfo(_path);
            foreach (var dir in root.GetDirectories("*", SearchOption.AllDirectories))
            {
                if (Path.GetFileName(dir.FullName) == caseName)
                {
                    return new FsCase(_log, _config, dir.FullName);
                }
            }

            var caseDir = Path.Combine(_path, caseName);
            Directory.CreateDirectory(caseDir);
            return new FsCase(_log, _config, caseDir);
        }
    }
}
