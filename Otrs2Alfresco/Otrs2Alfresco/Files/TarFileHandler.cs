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
            return Extensions.Any(e => data.FileName.EndsWith(e));
        }

        private string GetExtension(string fileName)
        {
            return Extensions.Where(e => fileName.EndsWith(e)).Single();
        }

        public override bool Handle(FileHandlerData data)
        {
            Console.WriteLine("Extracting file " + data.FileName);

            using (var tar = new Tar())
            {
                if (!tar.Extract(data.Data, GetExtension(data.FileName)))
                {
                    Console.WriteLine("Cannot extract file " + data.FileName);
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

