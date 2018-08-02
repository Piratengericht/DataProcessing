using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using Otrs;
using Alfresco;

namespace Otrs2Alfresco
{
    public class ZipFileHandler : FileHandler
    {
        public ZipFileHandler(
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
            return
                data.FileName.EndsWith(".zip", StringComparison.InvariantCulture);
        }

        public override bool Handle(FileHandlerData data)
        {
            Context.Log.Info("Extracting file {0}", data.FileName);

            using (var zip = new Zip())
            {
                if (!zip.Extract(data.Data))
                {
                    Context.Log.Error("Cannot extract file {0}", data.FileName);
                    Context.Log.Error(zip.ErrorText);
                    return false;
                }

                var counter = 1;

                foreach (var filePath in zip.Files)
                {
                    Handlers.Handle(
                        new FileHandlerData(
                            Path.GetFileName(filePath),
                            data.AddPrefix(counter),
                            data.Date,
                            File.ReadAllBytes(filePath)));
                    counter++;
                }
            }

            return true;
        }
    }
}

