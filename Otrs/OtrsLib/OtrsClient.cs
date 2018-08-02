using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;

namespace Otrs
{
    public class Attribute
    {
        public string Name { get; private set; }

        public string Value { get; private set; }

        public Attribute(string name, string value)
        {
            Name = name;
            Value = value;
        }

        public void AddTo(JObject request)
        {
            request.Add(Name, Value);
        }

        public static Attribute Limit(int limit)
        {
            return new Attribute("Limit", limit.ToString());
        }
    }

    public static class SearchCriteria
    {
        public static Attribute TicketLastChangeTimeNewerDate(DateTime date)
        {
            return new Attribute("TicketLastChangeTimeNewerDate", date.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture));
        }

        public static Attribute TicketLastChangeTimeOlderDate(DateTime date)
        {
            return new Attribute("TicketLastChangeTimeOlderDate", date.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture));
        }

        public static Attribute TicketChangeTimeNewerDate(DateTime date)
        {
            return new Attribute("TicketChangeTimeNewerDate", date.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture));
        }

        public static Attribute TicketChangeTimeOlderDate(DateTime date)
        {
            return new Attribute("TicketChangeTimeOlderDate", date.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture));
        }

        public static Attribute Queue(string queue)
        {
            return new Attribute("Queues", queue);
        }

        public static Attribute State(string state)
        {
            return new Attribute("States", state);
        }
    }

    public class OtrsClient
    {
        private string _baseUrl;
        private string _userName;
        private string _password;

        public OtrsClient(string baseUrl, string userName, string password)
        {
            _baseUrl = baseUrl;
            _userName = userName;
            _password = password;
        }

        public IEnumerable<Ticket> GetTickets(IEnumerable<int> ids)
        {
            if (ids.Count() > 0)
            {
                var uri = _baseUrl + "/nph-genericinterface.pl/Webservice/REST/TicketGetAll";

                var request = new JObject(
                              new JProperty("UserLogin", new JValue(_userName)),
                              new JProperty("Password", new JValue(_password)),
                              new JProperty("AllArticles", new JValue("1")),
                              new JProperty("Attachments", new JValue("1")));

                foreach (var id in ids)
                {
                    request.Add("TicketID", new JValue(id.ToString()));
                }

                var response = Execute(uri, request);

                var list = response.Property("Ticket").Value as JArray;

                foreach (var obj in list.Values<JObject>())
                {
                    yield return new Ticket(obj);
                }
            }
        }

        public Ticket GetTicket(int id)
        {
            var uri = _baseUrl + "/nph-genericinterface.pl/Webservice/REST/TicketGetAll";

            var request = new JObject(
                              new JProperty("UserLogin", new JValue(_userName)),
                              new JProperty("Password", new JValue(_password)),
                              new JProperty("TicketID", new JValue(id.ToString())),
                              new JProperty("AllArticles", new JValue("1")),
                              new JProperty("Attachments", new JValue("1")));
            
            var response = Execute(uri, request);

            var list = response.Property("Ticket").Value as JArray;

            return new Ticket(list.Values<JObject>().First());
        }

        public IEnumerable<int> SearchTickets(params Attribute[] attributes)
        {
            var uri = _baseUrl + "/nph-genericinterface.pl/Webservice/REST/TicketSearch";

            var request = new JObject(
                              new JProperty("UserLogin", new JValue(_userName)),
                              new JProperty("Password", new JValue(_password)));
            
            foreach (var attribute in attributes)
            {
                attribute.AddTo(request);
            }

            var response = Execute(uri, request);

            if (response.Properties().Count() > 0)
            {
                var list = response.Property("TicketID").Value as JArray;

                foreach (JValue value in list)
                {
                    yield return value.Value<int>();
                }
            }
        }

        private JObject Execute(string uri, JObject data)
        {
            var request = new HttpRequestMessage(HttpMethod.Put, uri);
            request.Content = new StringContent(data.ToString());
            var client = new HttpClient();
            var response = client.SendAsync(request).Result;

            if ((int)response.StatusCode != 200) 
            {
                throw new Exception("OTRS response status code " + ((int)response.StatusCode).ToString());
            }

            return JObject.Parse(response.Content.ReadAsStringAsync().Result);
        }
    }
}
