using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using Otrs;
using Alfresco;
using MimeKit;

namespace Otrs2Alfresco
{
    public class EmlFileHandler : FileHandler
    {
        public EmlFileHandler(
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
                data.FileName.EndsWith(".eml");
        }

        private string ToString(InternetAddressList addresses)
        {
            return string.Join("; ", addresses.Select(a => a.ToString()).ToString());
        }

        private void HandleMessage(MimeMessage message, string prefix)
        { 
            var name = prefix + " " + Helper.SanatizeName(message.Subject);
            Context.Log.Info("Uploading file {0}", name);
            var text = System.IO.File.ReadAllText("Templates/mail.tex");
            var latex = new Latex(text);
            latex.Add("MAILDATE", Helper.FormatDateTime(message.Date.Date));
            latex.Add("MAILFROM", ToString(message.From));
            latex.Add("MAILTO", ToString(message.To));
            latex.Add("MAILCC", ToString(message.Cc));
            latex.Add("MAILSUBJECT", message.Subject);
            latex.Add("MAILATTACHMENTS", string.Join(" \\\\\n \\> ", message.Attachments.Select(a => a.ContentId).ToArray()), LatexEscapeMode.Minimal);
            latex.Add("MAILBODY", message.TextBody ?? message.Body.ToString(), LatexEscapeMode.MultiLine);
            var pdf = latex.Compile();
        
            if (pdf != null)
            {
                Alfresco.CreateFile(Context.CaseFolder.Id, name, pdf);
            }
            else
            {
                Context.Log.Error("Eml could not be texed {0}", name);
                Context.Log.Error(latex.ErrorText);
                Alfresco.CreateFile(Context.CaseFolder.Id, name + ".tex", Encoding.UTF8.GetBytes(latex.TexDocument));
            }
        }

        public override bool Handle(FileHandlerData data)
        {
            using (var stream = new MemoryStream(data.Data))
            {
                var message = MimeMessage.Load(stream);
                HandleMessage(message, data.Prefix);
                var counter = 1;

                foreach (var attachement in message.Attachments)
                {
                    using (var file = new MemoryStream())
                    {
                        attachement.WriteTo(file, true);
                        Handlers.Handle(
                            new FileHandlerData(
                                attachement.ContentDisposition.FileName,
                                data.AddPrefix(counter),
                                message.Date.Date,
                                file.ToArray()));
                    }

                    counter++;
                }
            }

            return true;
        }
    }
}

