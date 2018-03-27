using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;

namespace Otrs
{
    public enum State
    {
        Invalid,
        New,
        Open,
        ClosedSuccessful,
        ClosedUnsuccessful,
        PendingReminder,
        PendingAutoCloseUnsuccessful,
        PendingAutoCloseSuccessful,
    }

    public static class StateExtension
    {
        public static State Parse(string text)
        {
            switch (text)
            {
                case "new":
                    return State.New;
                case "open":
                    return State.Open;
                case "closed successful":
                    return State.ClosedSuccessful;
                case "closed unsuccessful":
                    return State.ClosedUnsuccessful;
                case "pending reminder":
                    return State.PendingReminder;
                case "pending auto close+":
                    return State.PendingAutoCloseSuccessful;
                case "pending auto close-":
                    return State.PendingAutoCloseUnsuccessful;
                default:
                    Console.WriteLine(text);
                    return State.Invalid;
            }
        }
    }

    public class Ticket
    {
        public int Id { get; private set; }

        public string Title { get; private set; }

        public string Number { get; private set; }

        public string Queue { get; private set; }

        public DateTime Created { get; private set; }

        public DateTime Changed { get; private set; }

        public State State { get; private set; }

        public List<Article> Articles { get; private set; }

        public Ticket(JObject data)
        {
            Articles = new List<Article>();

            foreach (JProperty property in data.Properties())
            {
                var value = (property.Value as JValue);

                switch (property.Name)
                {
                    case "TicketID":
                        Id = value.Value<int>();
                        break;
                    case "Title":
                        Title = value.Value<string>();
                        break;
                    case "Queue":
                        Queue = value.Value<string>();
                        break;
                    case "TicketNumber":
                        Number = value.Value<string>();
                        break;
                    case "Created":
                        Created = DateTime.ParseExact(value.Value<string>(), "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                        break;
                    case "Changed":
                        Changed = DateTime.ParseExact(value.Value<string>(), "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                        break;
                    case "State":
                        State = StateExtension.Parse(value.Value<string>());
                        break;
                    case "Article":
                        var list = (property.Value as JArray);

                        foreach (JObject sub in list)
                        {
                            Articles.Add(new Article(sub));
                        }
                        break;
                }
            }
        }
    }

    public class Article
    {
        public int Id { get; private set; }

        public int Number { get; private set; }

        public string MimeType { get; private set; }

        public string ContentCharset { get; private set; }

        public string From { get; private set; }

        public string To { get; private set; }

        public string CC { get; private set; }

        public string Bcc { get; private set; }

        public string ReplyTo { get; private set; }

        public string Subject { get; private set; }

        public string Body { get; private set; }

        public DateTime CreateTime { get; private set; }

        public DateTime ChangeTime { get; private set; }

        public List<Attachement> Attachements { get; private set; }

        public Article(JObject data)
        {
            Attachements = new List<Attachement>();

            foreach (JProperty property in data.Properties())
            {
                var value = (property.Value as JValue);

                switch (property.Name)
                {
                    case "ArticleID":
                        Id = value.Value<int>();
                        break;
                    case "ArticleNumber":
                        Number = value.Value<int>();
                        break;
                    case "MimeType":
                        MimeType = value.Value<string>();
                        break;
                    case "ContentCharset":
                        ContentCharset = value.Value<string>();
                        break;
                    case "From":
                        From = value.Value<string>();
                        break;
                    case "To":
                        To = value.Value<string>();
                        break;
                    case "CC":
                        CC = value.Value<string>();
                        break;
                    case "Bcc":
                        Bcc = value.Value<string>();
                        break;
                    case "ReplyTo":
                        ReplyTo = value.Value<string>();
                        break;
                    case "Subject":
                        Subject = value.Value<string>();
                        break;
                    case "Body":
                        Body = value.Value<string>();
                        break;
                    case "ChangeTime":
                        ChangeTime = DateTime.ParseExact(value.Value<string>(), "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                        break;
                    case "CreateTime":
                        CreateTime = DateTime.ParseExact(value.Value<string>(), "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                        break;
                    case "Attachment":
                        var list = (property.Value as JArray);

                        foreach (JObject sub in list)
                        {
                            Attachements.Add(new Attachement(sub));
                        }
                        break;

                }
            }
        }
    }

    public class Attachement
    {
        public int Id { get; private set; }

        public string Disposition { get; private set; }

        public string ContentType { get; private set; }

        public string Filename { get; private set; }

        public string Content { get; private set; }

        public Attachement(JObject data)
        {
            foreach (JProperty property in data.Properties())
            {
                var value = (property.Value as JValue);

                switch (property.Name)
                {
                    case "FileId":
                        Id = value.Value<int>();
                        break;
                    case "Filename":
                        Filename = value.Value<string>();
                        break;
                    case "Content":
                        Content = value.Value<string>();
                        break;
                    case "ContentType":
                        ContentType = value.Value<string>();
                        break;
                    case "Disposition":
                        Disposition = value.Value<string>();
                        break;
                }
            }
        }
    }
}

