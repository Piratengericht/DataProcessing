using System;
using System.Text;

namespace Alfresco
{
    public static class MainClass
    {
        public static void Main(string[] args)
        {
            var client = new AlfrescoClient("https://alfresco.piratengericht.ch/", "exception", "secret");

            var pger = client.GetNode("Sites", "piratengericht", "documentLibrary");
            Console.WriteLine(pger.Id + " " + pger.Name);

            /*
            var data = System.IO.File.ReadAllBytes("/home/stefan/Downloads/A-3763_2011.pdf");

            var file = client.CreateFile(pger.Id, "A-3763_2011.pdf", data);
            Console.WriteLine(file.Id + " " + file.Name);
            */

            /*
            var n = client.GetNode("-root-");
            Console.WriteLine(n.Id + " " + n.Name);

            foreach (var x in client.GetNodes("-root-"))
            {
                Console.WriteLine("  " + x.Id + " " + x.Name);

                foreach (var y in client.GetNodes(x.Id))
                {
                    Console.WriteLine("    " + y.Id + " " + y.Name);

                    foreach (var z in client.GetNodes(y.Id))
                    {
                        Console.WriteLine("    " + z.Id + " " + z.Name);
                    }
                }
            }
            */

            /*
            foreach (var site in client.GetSites())
            {
                Console.WriteLine(site.Id + " " + site.Title);
            }
            */
        }
    }
}
