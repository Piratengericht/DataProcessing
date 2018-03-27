using System;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using System.Text;
using System.Linq;
using System.Collections.Generic;

namespace Alfresco
{
    public class AlfrescoClient
    {
        private string _baseUrl;

        private string _username;

        private string _password;

        private string ApiUrl
        {
            get
            {
                return _baseUrl + "/alfresco/api/-default-/public/alfresco/versions/1/";
            }
        }

        public AlfrescoClient(string baseUrl, string username, string password)
        {
            _baseUrl = baseUrl;
            _username = username;
            _password = password;
        }

        private void AddAuthHeader(HttpRequestMessage request)
        {
            var credentials = _username + ":" + _password;
            var authData = Convert.ToBase64String(Encoding.UTF8.GetBytes(credentials));

            request.Headers.Add("Authorization", "Basic " + authData);
        }

        private JObject Execute(HttpMethod method, string urlSuffix, JObject body)
        {
            return Execute(method, urlSuffix, body.ToString());
        }

        private bool IsSuccess(HttpStatusCode statusCode)
        {
            return ((int)statusCode >= 200) && ((int)statusCode <= 299);
        }

        private JObject Execute(HttpMethod method, string urlSuffix, string body)
        {
            var client = new HttpClient();

            var url = ApiUrl + urlSuffix;
            var request = new HttpRequestMessage(method, url);
            AddAuthHeader(request);
            request.Content = new StringContent(body);

            var response = client.SendAsync(request).Result;

            if (!IsSuccess(response.StatusCode))
            {
                throw new Exception("HTTP error: " + (int)response.StatusCode);
            }

            return JObject.Parse(response.Content.ReadAsStringAsync().Result);
        }

        private JObject Execute(HttpMethod method, string urlSuffix, byte[] body = null)
        { 
            var client = new HttpClient();

            var url = ApiUrl + urlSuffix;
            var request = new HttpRequestMessage(method, url);
            AddAuthHeader(request);

            if (body != null)
            {
                request.Content = new ByteArrayContent(body);
            }

            var response = client.SendAsync(request).Result;

            if (!IsSuccess(response.StatusCode))
            {
                throw new Exception("HTTP error: " + (int)response.StatusCode);
            }

            return JObject.Parse(response.Content.ReadAsStringAsync().Result);
        }

        public Node GetNode(string nodeId)
        {
            var data = Execute(HttpMethod.Get, "nodes/" + nodeId);
            var entry = data.Property("entry").Value as JObject;
            return new Node(entry);
        }

        public Node GetNode(params string[] path)
        {
            var currentNode = GetNode("-root-");
            var currentList = GetNodes("-root-").ToList();

            foreach (var element in path)
            {
                var next = currentList.Where(n => n.Name == element).SingleOrDefault();

                if (next == null)
                {
                    throw new Exception("Path not found.");
                }
                else
                {
                    currentNode = next;
                    currentList = GetNodes(currentNode.Id).ToList();
                }
            }

            return currentNode;
        }

        public IEnumerable<Node> GetNodes(string nodeId)
        {
            var data = Execute(HttpMethod.Get, "nodes/" + nodeId + "/children");
            var list = data.Property("list");
            var entries = ((JObject)list.Value).Property("entries");

            foreach (JObject preEntry in entries.Values())
            {
                JObject entry = (JObject)preEntry.Property("entry").Value;
                yield return new Node(entry);
            }
        }

        public IEnumerable<Site> GetSites()
        {
            var data = Execute(HttpMethod.Get, "sites");

            var list = data.Property("list");
            var entries = ((JObject)list.Value).Property("entries");

            foreach (JObject preEntry in entries.Values())
            {
                JObject entry = (JObject)preEntry.Property("entry").Value;
                yield return new Site(entry);
            }
        }

        public Node CreateFolder(string parentNodeId, string name)
        { 
            var body = new JObject(
                new JProperty("name", new JValue(name)),
                new JProperty("nodeType", new JValue("cm:folder")));

            var data = Execute(HttpMethod.Post, "nodes/" + parentNodeId + "/children", body);

            var entry = data.Property("entry").Value as JObject;
            return new Node(entry);
        }

        public Node CreateFile(string parentNodeId, string name, byte[] data)
        {
            var body = new JObject(
                new JProperty("name", new JValue(name)),
                new JProperty("nodeType", new JValue("cm:content")));

            var reply = Execute(HttpMethod.Post, "nodes/" + parentNodeId + "/children", body);

            var entry = reply.Property("entry").Value as JObject;
            var file = new Node(entry);

            var reply2 = Execute(HttpMethod.Put, "nodes/" + file.Id + "/content", data);

            var entry2 = reply.Property("entry").Value as JObject;
            return new Node(entry2);
        }
    }
}
