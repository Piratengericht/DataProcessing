using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Otrs;
using Alfresco;

namespace Otrs2Alfresco
{
    public class TextFileHandler : FileHandler
    {
        public TextFileHandler(
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
                data.FileName.EndsWith(".txt", StringComparison.InvariantCulture);
        }

        public override bool Handle(FileHandlerData data)
        {
            Context.Log.Info("Converting text file {0}", data.FileName);
            var textdata = TextEncoding.AutoConvertTextData(data.Data);
            var name = Helper.CreateName(data.Prefix, data.FileName, "pdf");
            var text = System.IO.File.ReadAllText("Templates/text.tex");
            var latex = new Latex(text);
            latex.Add("TEXTDOCUMENT", data.FileName);
            latex.Add("TEXTDATE", Helper.FormatDateTime(data.Date));
            latex.Add("TEXTBODY", textdata, LatexEscapeMode.MultiLine);
            var pdf = latex.Compile();

            if (pdf != null)
            {
                Handlers.Handle(
                    new FileHandlerData(
                        name,
                        data.Prefix,
                        data.Date,
                        pdf));
                return true;
            }
            else
            {
                Context.Log.Error("Eml could not be texed {0}", name);
                Context.Log.Error(latex.ErrorText);
                return false;
            }
        }
    }
}

