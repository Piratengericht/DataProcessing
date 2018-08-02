using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using Otrs;
using Alfresco;

namespace Otrs2Alfresco
{
    public class TarFileHandler : FileHandler
    {
        public TarFileHandler(
            AlfrescoClient alfresco,
            OtrsClient otrs,
            Config config,
            FileHandlers handlers,
            FileHandlerContext context)
            : base(alfresco, otrs, config, handlers, context)
        {
        }

        private IEnumerable<string> Extensions
        {
            get
            {
                yield return ".tar.gz";
                yield return ".tar.bz2";
                yield return ".tar";
            }
        }

        public override bool CanHandle(FileHandlerData data)
        {
            return Extensions.Any(e => data.FileName.EndsWith(e, StringComparison.InvariantCulture));
        }

        private string GetExtension(string fileName)
        {
            return Extensions.Where(e => fileName.EndsWith(e, StringComparison.InvariantCulture)).Single();
        }

        public override bool Handle(FileHandlerData data)
        {
            Context.Log.Info("Extracting file {0}", data.FileName);

            using (var tar = new Tar())
            {
                if (!tar.Extract(data.Data, GetExtension(data.FileName)))
                {
                    Context.Log.Error("Cannot extract file {0}", data.FileName);
                    Context.Log.Error(tar.ErrorText);
                    return false;
                }

                var counter = 1;

                foreach (var filename in tar.Files)
                {
                    Handlers.Handle(
                        new FileHandlerData(
                            Path.GetFileName(filename),
                            data.AddPrefix(counter),
                            data.Date,
                            File.ReadAllBytes(filename)));
                    counter++;
                }
            }

            return true;
        }
    }
}

