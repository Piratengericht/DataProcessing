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
            AlfrescoClient alfresco,
            OtrsClient otrs,
            Config config,
            FileHandlerContext context)
        {
            _handlers = new List<FileHandler>();
            _handlers.Add(new NullFileHandler(alfresco, otrs, config, this, context));
            _handlers.Add(new GpgFileHandler(alfresco, otrs, config, this, context));
            _handlers.Add(new TarFileHandler(alfresco, otrs, config, this, context));
            _handlers.Add(new ZipFileHandler(alfresco, otrs, config, this, context));
            _handlers.Add(new GzipFileHandler(alfresco, otrs, config, this, context));
            _handlers.Add(new Bzip2FileHandler(alfresco, otrs, config, this, context));
            _handlers.Add(new EmlFileHandler(alfresco, otrs, config, this, context));
            _handlers.Add(new ImageFileHandler(alfresco, otrs, config, this, context));
            _handlers.Add(new OfficeFileHandler(alfresco, otrs, config, this, context));
            _handlers.Add(new DefaultFileHandler(alfresco, otrs, config, this, context));
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

