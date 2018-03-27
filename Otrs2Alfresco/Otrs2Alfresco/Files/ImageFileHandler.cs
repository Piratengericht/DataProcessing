using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using Otrs;
using Alfresco;

namespace Otrs2Alfresco
{
    public class ImageFileHandler : FileHandler
    {
        public ImageFileHandler(
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
                data.FileName.EndsWith(".jpg") ||
                data.FileName.EndsWith(".jpeg") ||
                data.FileName.EndsWith(".png") ||
                data.FileName.EndsWith(".bmp");
        }

        public override bool Handle(FileHandlerData data)
        {
            Console.WriteLine("Converting image file " + data.FileName);
            var name = data.Prefix + " " + Helper.SanatizeName(data.FileName) + ".pdf";
            var text = System.IO.File.ReadAllText("Templates/gfx.tex");
            var latex = new Latex(text);
            latex.Add("GFXDOCUMENT", data.FileName);
            latex.Add("GFXDATE", Helper.FormatDateTime(data.Date));
            latex.Add("GFXIMAGE", data.FileName, data.Data);
            var pdf = latex.Compile();

            if (pdf != null)
            {
                Handlers.Handle(new FileHandlerData(name, data.Prefix, data.Date, pdf));
            }
            else
            {
                Console.WriteLine("Image could not be texed " + name);
                Handlers.Handle(new FileHandlerData(name + ".tex", data.Prefix, data.Date, Encoding.UTF8.GetBytes(latex.TexDocument)));
            }

            return true;
        }
    }
}

