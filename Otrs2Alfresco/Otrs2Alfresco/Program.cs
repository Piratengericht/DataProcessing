using System;
using System.IO;
using System.Threading;

namespace Otrs2Alfresco
{
    public static class MainClass
    {
        private static void RunSync()
        {
            var controller = new Controller();
            controller.CheckPrerequisites();
            controller.Connect();

            var cases = controller.CreateCases();
            cases.FullSync();

            while (true)
            {
                for (int i = 0; i < 600; i++)
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
            }
        }
    }
}
