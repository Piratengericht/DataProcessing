using System;
using System.IO;
using System.Threading;

namespace Otrs2Alfresco
{
    public static class MainClass
    {
        private static void CheckPrerequisites()
        {
            foreach (var binary in Binaries.All)
            {
                if (File.Exists(binary))
                {
                    Console.WriteLine("{0} : found", binary);
                }
                else
                { 
                    Console.WriteLine("{0} : missing", binary);
                }
            }
        }

        private static void RunSync()
        { 
            var cases = new Cases();
            cases.Connect();
            cases.FullSync();

            while (true)
            {
                for (int i = 0; i < 10; i++)
                {
                    Thread.Sleep(1000);
                }

                cases.IncrementalSync();
            }
        }

        public static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                RunSync();
                return;
            }

            switch (args[0])
            {
                case "run":
                    RunSync();
                    break;
                case "prerequisites":
                    CheckPrerequisites();
                    break;
            }
        }
    }
}
