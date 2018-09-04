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
                data.FileName.EndsWith(".jpg", StringComparison.InvariantCulture) ||
                data.FileName.EndsWith(".jpeg", StringComparison.InvariantCulture) ||
                data.FileName.EndsWith(".png", StringComparison.InvariantCulture) ||
                data.FileName.EndsWith(".bmp", StringComparison.InvariantCulture);
        }

        public override bool Handle(FileHandlerData data)
        {
            Context.Log.Info("Converting image file {0}", data.FileName);
            var name = Helper.CreateName(data.Prefix, data.FileName, "pdf");
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
                Context.Log.Error("Image could not be texed {0}", name);
                Context.Log.Error(latex.ErrorText);
                Handlers.Handle(new FileHandlerData(name + ".tex", data.Prefix, data.Date, Encoding.UTF8.GetBytes(latex.TexDocument)));
            }

            return true;
        }
    }
}

