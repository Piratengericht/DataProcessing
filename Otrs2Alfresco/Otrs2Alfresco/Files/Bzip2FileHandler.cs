using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using Otrs;
using Alfresco;

namespace Otrs2Alfresco
{
    public class Bzip2FileHandler : FileHandler
    {
        public Bzip2FileHandler(
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
                data.FileName.EndsWith (".bz2");
        }

        public override bool Handle(FileHandlerData data)
        {
            Context.Log.Info("Extracting file {0}", data.FileName);

            using (var bzip2 = new Bzip2())
            {
                if (!bzip2.Extract(data.Data))
                {
                    Context.Log.Error("Cannot extract file {0}", data.FileName);
                    Context.Log.Error(bzip2.ErrorText);
                    return false;
                }

                foreach (var file in bzip2.Files)
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

