using System;
using MailKit;
using MailKit.Net;
using MailKit.Net.Smtp;
using MimeKit;

namespace Otrs2Alfresco
{
    public class Mailer
    {
        private const string ErrorSubject = "Error in O2A";
        private const string WarningSubject = "Warning in O2A";

        private Logger _log;
        private Config _config;

        public Mailer(Logger log, Config config)
        {
            _log = log;
            _config = config;
        }

        public void SendError(Exception exception)
        {
            Send(_config.AdminMailAddress, ErrorSubject, exception.ToString());
        }

        public void SendWarning(string body)
        {
            Send(_config.AdminMailAddress, WarningSubject, body);
        }

        public void SendAdmin(string subject, string body)
        {
            Send(_config.AdminMailAddress, subject, body);
        }

        public void Send(string to, string subject, string body)
        {
            return;
            _log.Verbose("Sending message to {0}", to);

            try
            {
                var client = new SmtpClient();
                client.SslProtocols = System.Security.Authentication.SslProtocols.None;
                client.Connect(_config.MailServerHost, _config.MailServerPort);
                _log.Verbose("Connected to mail server {0}:{1}", _config.MailServerHost, _config.MailServerPort);

                var message = new MimeMessage();
                message.From.Add(InternetAddress.Parse(_config.SystemMailAddress));
                message.To.Add(InternetAddress.Parse(to));
                message.Subject = subject;
                message.Body = new TextPart("plain") { Text = body };
                client.Send(message);

                _log.Info("Message sent to {0}", to);
            }
            catch (Exception exception)
            { 
                _log.Error("Error sending mail to {0}", to);
                _log.Error(exception.ToString());
            }
        }
    }
}
