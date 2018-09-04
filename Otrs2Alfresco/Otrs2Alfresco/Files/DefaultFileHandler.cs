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
            OtrsClient otrs,
            ITargetCase target,
            Config config,
            FileHandlers handlers,
            FileHandlerContext context)
            : base(otrs, target, config, handlers, context)
        {
        }

        public override bool CanHandle(FileHandlerData data)
        {
           return true;
        }

        public override bool Handle(FileHandlerData data)
        {
            if (!Target.FileExists(data.Prefix + " "))
            {
                var name = data.Prefix + " " + Helper.SanatizeName(data.FileName);
                Upload(name, data.Data);
            }

            return true;
        }
    }
}

