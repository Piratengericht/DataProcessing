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
        private Config _config;
        private AlfrescoClient _alfresco;
        private OtrsClient _otrs;
        private DateTime _lastUpdate;

        public Cases()
        {
        }

        private IEnumerable<string> ConfigPaths
        {
            get
            {
                yield return "/mnt/sdb/Security/PPS/O2A.config.xml";
                yield return "/Security/PPS/O2A.config.xml";
                //yield return "/mnt/sdb/Security/PPDE/O2A.config.xml";
                //yield return "/Security/PPDE/O2A.config.xml";
                yield return Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "config.xml");
            }
        }

        private Config LoadConfig()
        {
            foreach (var path in ConfigPaths)
            {
                if (File.Exists(path))
                {
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
                    throw new IOException("No config file found");
                }
            }

            throw new IOException("No valid config path found");
        }

        public void Connect()
        {
            _config = LoadConfig();

            _otrs = new OtrsClient(_config.OtrsBaseUrl, _config.OtrsUsername, _config.OtrsPassword);
            Console.WriteLine("OTRS URL is " + _config.OtrsBaseUrl);

            _alfresco = new AlfrescoClient(_config.AlfrescoBaseUrl, _config.AlfrescoUsername, _config.AlfrescoPassword);
            Console.WriteLine("Alfresco URL is " + _config.AlfrescoBaseUrl);
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
            Console.WriteLine("Full sync");

            var library = _alfresco.GetNode("Sites", _config.AlfrescoSitename, "documentLibrary");

            var ticketIds = _otrs.SearchTickets().ToList();
            var ticketCounter = 1;

            foreach (var ticketId in ticketIds)
            {
                var ticket = _otrs.GetTicket(ticketId);
                Console.WriteLine("Checking ticket {0} / {1}: {2}", ticketCounter, ticketIds.Count, ticket.Number);

                if (IsCase(ticket))
                {
                    var c = new Case(_otrs, _alfresco, _config, ticket, library);
                    c.Sync();
                }

                if (_lastUpdate < ticket.Changed.Date)
                {
                    _lastUpdate = ticket.Changed.Date;
                }

                ticketCounter++;
            }
        }

        public void IncrementalSync()
        {
            if (_config == null || _otrs == null || _alfresco == null)
            {
                throw new InvalidOperationException("Not ready");
            }

            Console.WriteLine("Incremental sync");

            var library = _alfresco.GetNode("Sites", _config.AlfrescoSitename, "documentLibrary");

            var ticketIds = _otrs.SearchTickets(SearchCriteria.TicketLastChangeTimeNewerDate(_lastUpdate.AddMinutes(-3))).ToList();
            var ticketCounter = 1;

            foreach (var ticketId in ticketIds)
            {
                var ticket = _otrs.GetTicket(ticketId);
                Console.WriteLine("Checking ticket {0} / {1}: {2}", ticketCounter, ticketIds.Count, ticket.Number);

                if (IsCase(ticket))
                {
                    var c = new Case(_otrs, _alfresco, _config, ticket, library);
                    c.Sync();
                }

                if (_lastUpdate < ticket.Changed.Date)
                {
                    _lastUpdate = ticket.Changed.Date;
                }

                ticketCounter++;
            }
        }
    }
}
