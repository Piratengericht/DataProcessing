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
                data.FileName.EndsWith (".gz");
        }

        public override bool Handle(FileHandlerData data)
        {
            Console.WriteLine("Extracting file " + data.FileName);

            using (var gzip = new Gzip())
            {
                if (!gzip.Extract(data.Data))
                {
                    Console.WriteLine("Cannot extract file " + data.FileName);
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

