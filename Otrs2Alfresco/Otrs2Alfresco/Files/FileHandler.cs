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
        protected OtrsClient Otrs { get; private set; }

        protected ITargetCase Target { get; private set; }

        protected Config Config { get; private set; }

        protected FileHandlers Handlers { get; private set; }

        protected FileHandlerContext Context { get; private set; }

		public FileHandler(
            OtrsClient otrs,
            ITargetCase target,
            Config config,
            FileHandlers handlers,
            FileHandlerContext context)
		{
            Otrs = otrs;
            Target = target;
            Config = config;
            Handlers = handlers;
            Context = context;
		}

        public abstract bool CanHandle(FileHandlerData data);

        public abstract bool Handle(FileHandlerData data);

        protected void Upload(string name, byte[] pdf)
        {
            Context.Log.Notice("Uploading file {0}", name);
            Target.CreateFile(name, pdf);
        }
	}
}

