using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using Otrs;
using Alfresco;

namespace Otrs2Alfresco
{
    public class GpgFileHandler : FileHandler
    {
        public GpgFileHandler(
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
                data.FileName.EndsWith(".gpg") ||
                data.FileName.EndsWith(".asc");
        }

        public override bool Handle(FileHandlerData data)
        {
            Console.WriteLine("GPG decypting file " + data.FileName);

            using (var gpg = new Gpg())
            {
                if (!gpg.Decrypt(data.Data))
                {
                    Console.WriteLine("Cannot decrypt file " + data.FileName);
                    return false;
                }

                foreach (var file in gpg.Files)
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

