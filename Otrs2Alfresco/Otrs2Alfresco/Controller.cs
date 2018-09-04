using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using Otrs;
using Alfresco;

namespace Otrs2Alfresco
{
    public class Controller
    {
        private Logger _log;
        private Config _config;
        private Mailer _mail;
        private OtrsClient _otrs;
        private ITargetApi _target;

        public Controller()
        {
            _log = new Logger();
            _config = LoadConfig();
            _mail = new Mailer(_log, _config);
        }

        public Cases CreateCases()
        {
            if ((_otrs == null) || (_target == null))
            {
                throw new InvalidOperationException();
            }

            return new Cases(_log, _config, _mail, _otrs, _target);
        }

        public void CheckPrerequisites()
        {
            foreach (var binary in Binaries.All)
            {
                if (File.Exists(binary))
                {
                    _log.Verbose("Prerequisite {0} : found", binary);
                }
                else
                {
                    _log.Warning("Prerequisite {0} : missing", binary);
                }
            }
        }

        public void Connect()
        {
            _otrs = new OtrsClient(_config.OtrsBaseUrl, _config.OtrsUsername, _config.OtrsPassword);
            _log.Info("OTRS URL is {0}", _config.OtrsBaseUrl);

            switch (_config.TargetApiType)
            {
                case TargetApiType.FileSystem:
                    _target = new FsApi(_log, _config);
                    _log.Info("Path is {0}", _config.FileSystemPath);
                    break;
                case TargetApiType.Alfresco:
                    _target = new AlfrescoApi(_log, _config);
                    _log.Info("Alfresco URL is {0}", _config.AlfrescoBaseUrl);
                    break;
                default:
                    throw new InvalidOperationException();
            }

            if (_log.HighestSeverity >= LogSeverity.Notice)
            {
                _mail.SendWarning(_log.ToText(LogSeverity.Verbose));
                _log.Clear();
            }
        }

        private IEnumerable<string> ConfigPaths
        {
            get
            {
                //yield return "/mnt/sdb/Security/PPS/O2A.config.xml";
                //yield return "/Security/PPS/O2A.config.xml";
                //yield return "/mnt/sdb/Security/PPDE/O2A.config.xml";
                yield return "/Security/PPDE/O2A.config.xml";
                yield return Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "config.xml");
            }
        }

        private Config LoadConfig()
        {
            foreach (var path in ConfigPaths)
            {
                if (File.Exists(path))
                {
                    _log.Info("Loading config file {0}", path);
                    return Config.Load(path);
                }
            }

            foreach (var path in ConfigPaths)
            {
                var folder = Path.GetDirectoryName(path);

                if (Directory.Exists(folder))
                {
                    var config = new Config();
                    config.Save(path);
                    throw _log.Critical("No config file cound. Created at {0}", path);
                }
            }

            throw _log.Critical("No valid config path found");
        }
    }
}
