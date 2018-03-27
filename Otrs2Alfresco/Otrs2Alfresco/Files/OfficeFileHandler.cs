using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using Otrs;
using Alfresco;

namespace Otrs2Alfresco
{
    public class OfficeFileHandler : FileHandler
    {
        public OfficeFileHandler(
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
                data.FileName.EndsWith (".doc") ||
                data.FileName.EndsWith (".docx") ||
                data.FileName.EndsWith (".xls") ||
                data.FileName.EndsWith (".xlsx") ||
                data.FileName.EndsWith (".odt") ||
                data.FileName.EndsWith (".ods") ||
                data.FileName.EndsWith (".odg") ||
                data.FileName.EndsWith (".odp");
        }

        public override bool Handle(FileHandlerData data)
        {
            Context.Log.Info("Converting office file {0}", data.FileName);

            using (var office = new Office())
            {
                if (!office.Convert(data.Data, Path.GetExtension(data.FileName)))
                {
                    Context.Log.Error("Cannot convert file {0}", data.FileName);
                    Context.Log.Error(office.ErrorText);
                    return false;
                }

                foreach (var file in office.Files)
                {
                    var newFileName = Path.GetFileName(file);
                    var newData = File.ReadAllBytes(file);
                    Handlers.Handle(new FileHandlerData(newFileName, data.Prefix, data.Date, newData));
                }
            }

            return true;
        }
    }
}

