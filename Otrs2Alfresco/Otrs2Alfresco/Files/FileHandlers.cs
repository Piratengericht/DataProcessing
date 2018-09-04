using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using Otrs;
using Alfresco;

namespace Otrs2Alfresco
{
    public class FileHandlers
    {
        private List<FileHandler> _handlers;

        public FileHandlers(
            OtrsClient otrs,
            ITargetCase target,
            Config config,
            FileHandlerContext context)
        {
            _handlers = new List<FileHandler>();
            _handlers.Add(new NullFileHandler(otrs, target, config, this, context));
            _handlers.Add(new GpgFileHandler(otrs, target, config, this, context));
            _handlers.Add(new TarFileHandler(otrs, target, config, this, context));
            _handlers.Add(new ZipFileHandler(otrs, target, config, this, context));
            _handlers.Add(new GzipFileHandler(otrs, target, config, this, context));
            _handlers.Add(new Bzip2FileHandler(otrs, target, config, this, context));
            _handlers.Add(new EmlFileHandler(otrs, target, config, this, context));
            _handlers.Add(new ImageFileHandler(otrs, target, config, this, context));
            _handlers.Add(new OfficeFileHandler(otrs, target, config, this, context));
            _handlers.Add(new DefaultFileHandler(otrs, target, config, this, context));
        }

        public void Handle(FileHandlerData data)
        {
            foreach (var handler in _handlers)
            {
                if (handler.CanHandle(data))
                {
                    if (handler.Handle(data))
                    {
                        return;
                    }
                }
            }

            throw new Exception("No handler for file " + data.FileName);
        }
    }
}

