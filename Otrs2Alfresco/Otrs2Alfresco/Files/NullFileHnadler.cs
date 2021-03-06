﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using Otrs;
using Alfresco;
using System.Text;

namespace Otrs2Alfresco
{
    public class NullFileHandler : FileHandler
    {
        public NullFileHandler(
            OtrsClient otrs,
            ITargetCase target,
            Config config,
            FileHandlers handlers,
            FileHandlerContext context)
            : base(otrs, target, config, handlers, context)
        {
        }

        private bool CanHandleData(FileHandlerData data)
        {
            if (data.FileName.EndsWith(".asc", StringComparison.InvariantCulture))
            {
                var text = Encoding.ASCII.GetString(data.Data);

                return text.Contains("-----BEGIN PGP PUBLIC KEY BLOCK-----") ||
                       text.Contains("-----BEGIN PGP SIGNATURE-----");
            }
            else
            {
                return false;
            }
        }

        public override bool CanHandle(FileHandlerData data)
        {
            if (data.FileName.EndsWith(".vcf", StringComparison.InvariantCulture) ||
                data.FileName.EndsWith(".sig", StringComparison.InvariantCulture) ||
                data.FileName.StartsWith("sig.gpg", StringComparison.InvariantCulture))
            {
                return true;
            }
            else if (CanHandleData(data))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public override bool Handle(FileHandlerData data)
        {
            // Do nothing with that file.
            Context.Log.Info("Ignoring file {0}", data.FileName);
            return true;
        }
    }
}

