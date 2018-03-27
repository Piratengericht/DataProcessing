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
                data.FileName.EndsWith(".txt");
        }

        public override bool Handle(FileHandlerData data)
        {
            Console.WriteLine("Converting text file " + data.FileName);
            var textdata = TextEncoding.AutoConvertTextData(data.Data);
            var name = data.Prefix + " " + Helper.SanatizeName(data.FileName) + ".pdf";
            var text = System.IO.File.ReadAllText("Templates/text.tex");
            var latex = new Latex(text);
            latex.Add("TEXTDOCUMENT", data.FileName);
            latex.Add("TEXTDATE", Helper.FormatDateTime(data.Date));
            latex.Add("TEXTBODY", textdata);
            var pdf = latex.Compile();
            Handlers.Handle(
                new FileHandlerData(
                    name,
                    data.Prefix,
                    data.Date,
                    pdf));

            return true;
        }

        private object AutoConvertTextData(byte[] data)
        {
            throw new NotImplementedException();
        }
    }
}

