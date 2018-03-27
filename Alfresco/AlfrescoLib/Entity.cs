using System;
using Newtonsoft.Json.Linq;
using System.Globalization;

namespace Alfresco
{
    public class Entity
    {
    }

    public class Site : Entity
    {
        public string Id { get; private set; }

        public string Title { get; private set; }

        public Site(JObject data)
        {
            foreach (JProperty property in data.Properties())
            {
                var value = property.Value as JValue;

                switch (property.Name)
                {
                    case "id":
                        Id = value.Value<string>();
                        break;
                    case "title":
                        Title = value.Value<string>();
                        break;
                }
            }
        }
    }

    public class Node : Entity
    { 
        public string Id { get; private set; }

        public string Name { get; private set; }

        public string NodeType { get; private set; }

        public DateTime CreatedAt { get; private set; }

        public DateTime ModifiedAt { get; private set; }

        public bool IsFolder { get; private set;  }

        public bool IsFile { get; private set; }

        public long Size { get; private set;  }

        public string MimeType { get; private set; }

        public string Encoding { get; private set; }

        private void HandleContent(JObject data)
        {
            foreach (JProperty property in data.Properties())
            {
                var value = property.Value as JValue;

                switch (property.Name)
                {
                    case "mimeType":
                        MimeType = value.Value<string>();
                        break;
                    case "sizeInBytes":
                        Size = value.Value<long>();
                        break;
                    case "encoding":
                        Encoding = value.Value<string>();
                        break;
                }
            }
        }

        public Node(JObject data)
        {
            foreach (JProperty property in data.Properties())
            {
                var value = property.Value as JValue;

                switch (property.Name)
                {
                    case "id":
                        Id = value.Value<string>();
                        break;
                    case "createdAt":
                        CreatedAt = value.Value<DateTime>();
                        break;
                    case "modifiedAt":
                        ModifiedAt = value.Value<DateTime>();
                        break;
                    case "isFolder":
                        IsFolder = value.Value<bool>();
                        break;
                    case "isFile":
                        IsFile = value.Value<bool>();
                        break;
                    case "name":
                        Name = value.Value<string>();
                        break;
                    case "nodeType":
                        NodeType = value.Value<string>();
                        break;
                    case "content":
                        HandleContent(property.Value as JObject);
                        break;
                }}
        }
    }
}
