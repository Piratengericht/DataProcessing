﻿using System;
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
                data.FileName.EndsWith(".gpg", StringComparison.InvariantCulture) ||
                data.FileName.EndsWith(".asc", StringComparison.InvariantCulture);
        }

        public override bool Handle(FileHandlerData data)
        {
            Context.Log.Info("GPG decypting file {0}", data.FileName);

            using (var gpg = new Gpg())
            {
                if (!gpg.Decrypt(data.Data))
                {
                    Context.Log.Info("Cannot decrypt file {0}", data.FileName);
                    Context.Log.Error(gpg.ErrorText);
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

