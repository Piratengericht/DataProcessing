using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using Otrs;
using Alfresco;

namespace Otrs2Alfresco
{
    public class GzipFileHandler : FileHandler
    {
        public GzipFileHandler(
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
            return
                data.FileName.EndsWith (".gz", StringComparison.InvariantCulture);
        }

        public override bool Handle(FileHandlerData data)
        {
            Context.Log.Info("Extracting file {0}", data.FileName);

            using (var gzip = new Gzip())
            {
                if (!gzip.Extract(data.Data))
                {
                    Context.Log.Error("Cannot extract file {0}", data.FileName);
                    Context.Log.Error(gzip.ErrorText);
                    return false;
                }

                foreach (var file in gzip.Files)
                {
                    var newFileName = Path.GetFileName(file);
                    var newData = File.ReadAllBytes(file);
                    Handlers.Handle(
                        new FileHandlerData(
                            newFileName, 
                            data.Prefix,
                            data.Date,
                            newData));
                }
            }

            return true;
        }
    }
}

