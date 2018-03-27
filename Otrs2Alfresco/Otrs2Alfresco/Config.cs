using System;
using System.Xml.Linq;

namespace Otrs2Alfresco
{
    public class Config
    {
        private const string ConfigTag = "Config";
        private const string AlfrescoBaseUrlTag = "AlfrescoBaseUrl";
        private const string AlfrescoUsernameTag = "AlfrescoUsername";
        private const string AlfrescoPasswordTag = "AlfrescoPassword";
        private const string AlfrescoSitenameTag = "AlfrescoSitename";
        private const string OtrsBaseUrlTag = "OtrsBaseUrl";
        private const string OtrsUsernameTag = "OtrsUsername";
        private const string OtrsPasswordTag = "OtrsPassword";
        private const string TicketPrefixTag = "TicketPrefix";
        private const string TicketNumberRegexTag = "TicketNumberRegex";
        private const string TicketNumberFormatTag = "TicketNumberFormat";
        private const string AdminMailAddressTag = "AdminMailAddress";
        private const string MailServerHostTag = "MailServerHost";
        private const string MailServerPortTag = "MailServerPort";
        private const string SystemMailAddressTag = "SystemMailAddress";

        public string AlfrescoBaseUrl { get; private set; }

        public string AlfrescoUsername { get; private set; }

        public string AlfrescoPassword { get; private set; }

        public string AlfrescoSitename { get; private set; }

        public string OtrsBaseUrl { get; private set;  }

        public string OtrsUsername { get; private set; }

        public string OtrsPassword { get; private set; }

        public string TicketPrefix { get; private set; }

        public string TicketNumberRegex { get; private set; }

        public string TicketNumberFormat { get; private set; }

        public string AdminMailAddress { get; private set; }

        public string MailServerHost { get; private set; }

        public int MailServerPort { get; private set; }

        public string SystemMailAddress { get; private set; }

        public Config()
        {
            AlfrescoBaseUrl = string.Empty;
            AlfrescoUsername = string.Empty;
            AlfrescoPassword = string.Empty;
            AlfrescoSitename = string.Empty;
            OtrsBaseUrl = string.Empty;
            OtrsUsername = string.Empty;
            OtrsPassword = string.Empty;
            TicketPrefix = string.Empty;
            TicketNumberRegex = string.Empty;
            TicketNumberFormat = string.Empty;
            AdminMailAddress = string.Empty;
            MailServerHost = string.Empty;
            MailServerPort = 25;
            SystemMailAddress = string.Empty;
        }

        public static Config Load(string filename)
        {
            var config = new Config();
            var document = XDocument.Load(filename);
            var root = document.Root;

            config.AlfrescoBaseUrl = root.Element(AlfrescoBaseUrlTag).Value;
            config.AlfrescoUsername = root.Element(AlfrescoUsernameTag).Value;
            config.AlfrescoPassword = root.Element(AlfrescoPasswordTag).Value;
            config.AlfrescoSitename = root.Element(AlfrescoSitenameTag).Value;
            config.OtrsBaseUrl = root.Element(OtrsBaseUrlTag).Value;
            config.OtrsUsername = root.Element(OtrsUsernameTag).Value;
            config.OtrsPassword = root.Element(OtrsPasswordTag).Value;
            config.TicketPrefix = root.Element(TicketPrefixTag).Value;
            config.TicketNumberRegex = root.Element(TicketNumberRegexTag).Value;
            config.TicketNumberFormat = root.Element(TicketNumberFormatTag).Value;
            config.AdminMailAddress = root.Element(AdminMailAddressTag).Value;
            config.MailServerHost = root.Element(MailServerHostTag).Value;
            config.MailServerPort = int.Parse(root.Element(MailServerPortTag).Value);
            config.SystemMailAddress = root.Element(SystemMailAddressTag).Value;

            return config;
        }

        public void Save(string filename)
        {
            var document = new XDocument();
            var root = new XElement(ConfigTag);
            document.Add(root);

            root.Add(new XElement(AlfrescoBaseUrlTag, AlfrescoBaseUrl));
            root.Add(new XElement(AlfrescoUsernameTag, AlfrescoUsername));
            root.Add(new XElement(AlfrescoPasswordTag, AlfrescoPassword));
            root.Add(new XElement(AlfrescoSitenameTag, AlfrescoSitename));
            root.Add(new XElement(OtrsBaseUrlTag, OtrsBaseUrl));
            root.Add(new XElement(OtrsUsernameTag, OtrsUsername));
            root.Add(new XElement(OtrsPasswordTag, OtrsPassword));
            root.Add(new XElement(TicketPrefixTag, TicketPrefix));
            root.Add(new XElement(TicketNumberRegexTag, TicketNumberRegex));
            root.Add(new XElement(TicketNumberFormatTag, TicketNumberFormat));
            root.Add(new XElement(AdminMailAddressTag, TicketNumberFormat));
            root.Add(new XElement(MailServerHostTag, MailServerHost));
            root.Add(new XElement(MailServerPortTag, MailServerPort.ToString()));
            root.Add(new XElement(SystemMailAddressTag, SystemMailAddress));

            document.Save(filename);
        }
    }
}
