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
                data.FileName.EndsWith(".eml", StringComparison.InvariantCulture);
        }

        private string ToString(InternetAddressList addresses)
        {
            return Latex.TableMultiline(addresses.Select(a => a.ToString()));
        }

        private static IEnumerable<string> GetAttachementNames(MimeMessage message)
        {
            foreach (MimePart attachment in message.Attachments)
            {
                yield return attachment.FileName;
            }
        }

        private void HandleMessage(MimeMessage message, string prefix)
        {
            if (!Target.FileExists(prefix + " "))
            {
                var name = Helper.CreateName(prefix, message.Subject, "pdf");
                var text = System.IO.File.ReadAllText("Templates/mail.tex");
                var latex = new Latex(text);
                latex.Add("MAILDATE", Helper.FormatDateTime(message.Date.Date));
                latex.Add("MAILFROM", ToString(message.From));
                latex.Add("MAILTO", ToString(message.To), LatexEscapeMode.Minimal);
                latex.Add("MAILCC", ToString(message.Cc), LatexEscapeMode.Minimal);
                latex.Add("MAILSUBJECT", message.Subject);
                latex.Add("MAILATTACHMENTS", Latex.TableMultiline(GetAttachementNames(message)), LatexEscapeMode.Minimal);
                latex.Add("MAILBODY", message.TextBody ?? message.Body.ToString(), LatexEscapeMode.MultiLine);
                var pdf = latex.Compile();

                if (pdf != null)
                {
                    Context.Log.Notice("Uploading file {0}", name);
	                Target.CreateFile(name, pdf);
                }
                else
                {
                    Context.Log.Error("Eml could not be texed {0}", name);
                    Context.Log.Error(latex.ErrorText);
                    Context.Log.Notice("Uploading file {0}", name + ".tex");
    	            Target.CreateFile(name + ".tex", Encoding.UTF8.GetBytes(latex.TexDocument));
                }
            }
        }

        public override bool Handle(FileHandlerData data)
        {
            using (var stream = new MemoryStream(data.Data))
            {
                var message = MimeMessage.Load(stream);
                HandleMessage(message, data.Prefix);
                var counter = 1;

                foreach (MimePart attachement in message.Attachments)
                {
                    using (var file = new MemoryStream())
                    {
                        attachement.Content.DecodeTo(file);
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

