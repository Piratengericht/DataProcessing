using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using System.Text.RegularExpressions;
using Otrs;
using Alfresco;

namespace Otrs2Alfresco
{
    public class Cases
    {
        private Logger _log;
        private Config _config;
        private Mailer _mail;
        private AlfrescoClient _alfresco;
        private OtrsClient _otrs;
        private DateTime _lastUpdate;

        public Cases()
        {
            _log = new Logger();
        }

        private IEnumerable<string> ConfigPaths
        {
            get
            {
                //yield return "/Security/PPS/O2A.config.xml";
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

        public void Connect()
        {
            _config = LoadConfig();
            _mail = new Mailer(_log, _config);

            _otrs = new OtrsClient(_config.OtrsBaseUrl, _config.OtrsUsername, _config.OtrsPassword);
            _log.Info("OTRS URL is {0}", _config.OtrsBaseUrl);

            _alfresco = new AlfrescoClient(_config.AlfrescoBaseUrl, _config.AlfrescoUsername, _config.AlfrescoPassword);
            _log.Info("Alfresco URL is {0}", _config.AlfrescoBaseUrl);

            if (_log.HighestSeverity >= LogSeverity.Notice)
            {
                _mail.SendWarning(_log.ToText(LogSeverity.Verbose));
                _log.Clear();
            }
        }

        private bool IsCase(Ticket ticket)
        {
            return Regex.IsMatch(ticket.Number, _config.TicketNumberRegex);
        }

        public void FullSync()
        {
            if (_config == null || _otrs == null || _alfresco == null)
            {
                throw new InvalidOperationException("Not ready");
            }

            _lastUpdate = new DateTime(2000, 1, 1);
            _log.Info("Full sync");

            var library = _alfresco.GetNode("Sites", _config.AlfrescoSitename, "documentLibrary");

            var ticketIds = _otrs.SearchTickets().ToList();
            var ticketCounter = 1;

            foreach (var ticketId in ticketIds)
            {
                var ticket = _otrs.GetTicket(ticketId);
                _log.Info("Checking ticket {0} / {1}: {2}", ticketCounter, ticketIds.Count, ticket.Number);

                if (IsCase(ticket))
                {
                    var c = new Case(_log, _otrs, _alfresco, _config, ticket, library);
                    c.Sync();
                }

                if (_lastUpdate < ticket.Changed.Date)
                {
                    _lastUpdate = ticket.Changed.Date;
                }

                ticketCounter++;
            }

            if (_log.HighestSeverity >= LogSeverity.Notice)
            {
                _mail.SendWarning(_log.ToText(LogSeverity.Verbose));
                _log.Clear();
            }
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

        public void IncrementalSync()
        {
            if (_config == null || _otrs == null || _alfresco == null)
            {
                throw new InvalidOperationException("Not ready");
            }

            _log.Info("Incremental sync");

            var library = _alfresco.GetNode("Sites", _config.AlfrescoSitename, "documentLibrary");

            var ticketIds = _otrs.SearchTickets(SearchCriteria.TicketLastChangeTimeNewerDate(_lastUpdate.AddMinutes(-3))).ToList();
            var ticketCounter = 1;

            foreach (var ticketId in ticketIds)
            {
                var ticket = _otrs.GetTicket(ticketId);
                _log.Info("Checking ticket {0} / {1}: {2}", ticketCounter, ticketIds.Count, ticket.Number);

                if (IsCase(ticket))
                {
                    var c = new Case(_log, _otrs, _alfresco, _config, ticket, library);
                    c.Sync();
                }

                if (_lastUpdate < ticket.Changed.Date)
                {
                    _lastUpdate = ticket.Changed.Date;
                }

                ticketCounter++;
            }

            if (_log.HighestSeverity >= LogSeverity.Notice)
            {
                _mail.SendWarning(_log.ToText(LogSeverity.Verbose));
                _log.Clear();
            }
        }
    }
}
