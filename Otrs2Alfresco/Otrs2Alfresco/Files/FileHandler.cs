using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using Otrs;
using Alfresco;

namespace Otrs2Alfresco
{
    public abstract class FileHandler
	{
        protected AlfrescoClient Alfresco { get; private set; }

        protected OtrsClient Otrs { get; private set; }

        protected Config Config { get; private set; }

        protected FileHandlers Handlers { get; private set; }

        protected FileHandlerContext Context { get; private set; }

		public FileHandler(
            AlfrescoClient alfresco,
            OtrsClient otrs,
            Config config,
            FileHandlers handlers,
            FileHandlerContext context)
		{
            Alfresco = alfresco;
            Otrs = otrs;
            Config = config;
            Handlers = handlers;
            Context = context;
		}

        public abstract bool CanHandle(FileHandlerData data);

        public abstract bool Handle(FileHandlerData data);

        protected bool FileExists(string prefix)
        {
            return Context.NodesInCaseFolder
                .Any(file => file.Name.StartsWith(prefix));
        }

        protected void Upload(string name, byte[] pdf)
        {
            Console.WriteLine("Uploading file " + name);
            Alfresco.CreateFile(Context.CaseFolder.Id, name, pdf);
        }
	}
}

