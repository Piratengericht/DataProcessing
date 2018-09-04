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
                data.FileName.EndsWith (".doc", StringComparison.InvariantCulture) ||
                data.FileName.EndsWith (".docx", StringComparison.InvariantCulture) ||
                data.FileName.EndsWith (".xls", StringComparison.InvariantCulture) ||
                data.FileName.EndsWith (".xlsx", StringComparison.InvariantCulture) ||
                data.FileName.EndsWith (".odt", StringComparison.InvariantCulture) ||
                data.FileName.EndsWith (".ods", StringComparison.InvariantCulture) ||
                data.FileName.EndsWith (".odg", StringComparison.InvariantCulture) ||
                data.FileName.EndsWith (".odp", StringComparison.InvariantCulture);
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

