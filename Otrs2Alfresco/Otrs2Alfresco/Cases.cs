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
        private OtrsClient _otrs;
        private ITargetApi _target;
        private DateTime _lastUpdate;

        public Cases(Logger log, Config config, Mailer mail, OtrsClient otrs, ITargetApi target)
        {
            _log = log;
            _config = config;
            _mail = mail;
            _otrs = otrs;
            _target = target;
        }

        private bool IsCase(Ticket ticket)
        {
            return Regex.IsMatch(ticket.Number, _config.TicketNumberRegex);
        }

        public void FullSync()
        {
            if (_config == null || _otrs == null || _target == null)
            {
                throw new InvalidOperationException("Not ready");
            }

            _lastUpdate = new DateTime(2000, 1, 1);
            _log.Info("Full sync");

            var ticketIds = _otrs.SearchTickets().ToList();
            var ticketCounter = 1;

            foreach (var ticketId in ticketIds)
            {
                var ticket = _otrs.GetTicket(ticketId);
                _log.Info("Checking ticket {0} / {1}: {2}", ticketCounter, ticketIds.Count, ticket.Number);

                if (IsCase(ticket))
                {
                    var c = new Case(_log, _otrs, _target, _config, ticket);
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

        public void IncrementalSync()
        {
            if (_config == null || _otrs == null || _target == null)
            {
                throw new InvalidOperationException("Not ready");
            }

            _log.Info("Incremental sync");

            var ticketIds = _otrs.SearchTickets(SearchCriteria.TicketLastChangeTimeNewerDate(_lastUpdate.AddMinutes(-3))).ToList();
            var ticketCounter = 1;

            foreach (var ticketId in ticketIds)
            {
                var ticket = _otrs.GetTicket(ticketId);
                _log.Info("Checking ticket {0} / {1}: {2}", ticketCounter, ticketIds.Count, ticket.Number);

                if (IsCase(ticket))
                {
                    var c = new Case(_log, _otrs, _target, _config, ticket);
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
