using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using Otrs;
using Alfresco;

namespace Otrs2Alfresco
{
    public class DefaultFileHandler : FileHandler
    {
        public DefaultFileHandler(
            AlfrescoClient alfresco,
            OtrsClient otrs,
            Config config,
            FileHandlers handlers,
            FileHandlerContext context)
            : base(alfresco, otrs, config, handlers, context)
        {
        }

        public override bool CanHandle(FileHandlerData data)
        {
           return true;
        }

        public override bool Handle(FileHandlerData data)
        {
            if (!FileExists(data.Prefix + " "))
            {
                var name = Helper.CreateName(data.Prefix, data.FileName, string.Empty);
                Upload(name, data.Data);
            }

            return true;
        }
    }
}

