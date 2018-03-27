using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Collections.Generic;

namespace Otrs
{
    public static class MainClass
    {
        public static void Main(string[] args)
        {
            var client = new OtrsClient("https://otrs.piratengericht.ch/otrs", "exception", "secret");

            var date = DateTime.UtcNow;
            Console.WriteLine(date);

            while (true)
            {
                var tids = client.SearchTickets(SearchCriteria.TicketLastChangeTimeNewerDate(date));
                var ts = client.GetTickets(tids);

                if (ts.Count() > 0)
                {
                    Console.WriteLine();
                }

                foreach (var t in ts)
                {
                    Console.WriteLine(t.Id + " " + t.State + " " + t.Changed.ToString());
                   
                    if (t.Changed > date)
                    {
                        date = t.Changed.AddSeconds(1);
                        Console.WriteLine(date);
                    }
                }

                Console.Write(".");
                Thread.Sleep(1000);
            }

            //Console.WriteLine(client.SearchTickets(SearchCriteria.State("new")).Count().ToString());
            //Console.WriteLine(client.SearchTickets(Attribute.Limit(5)).Count().ToString());
            //Console.WriteLine(client.SearchTickets(SearchCriteria.Queue("Junk")).Count().ToString());
            //Console.WriteLine(client.SearchTickets(SearchCriteria.TicketLastChangeTimeNewerDate(new DateTime(2018, 1, 1))).Count().ToString());
            return;

            foreach (var id in client.SearchTickets())
            {
                var t = client.GetTicket(id);

                Console.WriteLine(t.Id.ToString());
                Console.WriteLine(t.Title);
                Console.WriteLine(t.Queue);
                Console.WriteLine(t.Created.ToString());

                foreach (var a in t.Articles)
                {
                    Console.WriteLine("  " + a.Id.ToString());
                    Console.WriteLine("  " + a.CreateTime.ToString());
                    Console.WriteLine("  " + a.ChangeTime.ToString());
                    Console.WriteLine("  " + a.From);
                    Console.WriteLine("  " + a.To);
                    Console.WriteLine("  " + a.CC);
                    Console.WriteLine("  " + a.Subject);
                    Console.WriteLine("  " + a.Body);

                    foreach (var x in a.Attachements)
                    {
                        Console.WriteLine("    " + x.Id.ToString());
                        Console.WriteLine("    " + x.Disposition);
                        Console.WriteLine("    " + x.Filename);
                        Console.WriteLine("    " + x.ContentType);
                    }
                }

                return;
                //Console.WriteLine(id);
            }
        }
    }
}
