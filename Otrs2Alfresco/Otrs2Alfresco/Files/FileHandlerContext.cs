using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using Otrs;
using Alfresco;

namespace Otrs2Alfresco
{
    public class FileHandlerContext
    {
        public Logger Log { get; private set; }

        public Ticket Ticket { get; private set; }

        public Article Article { get; private set; }

        public Node CaseFolder { get; private set; }

        public List<Node> NodesInCaseFolder { get; private set; }

        public FileHandlerContext(
            Logger log,
            Ticket ticket,
            Article article,
            Node caseFolder,
            List<Node> nodesInCaseFolder)
        {
            Log = log;
            Ticket = ticket;
            Article = article;
            CaseFolder = caseFolder;
            NodesInCaseFolder = nodesInCaseFolder;
        }
    }
}

