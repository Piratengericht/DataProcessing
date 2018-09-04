using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Otrs;

namespace Otrs2Alfresco
{
    public class Case
    {
        private Logger _log;
        private OtrsClient _otrs;
        private ITargetApi _target;
        private ITargetCase _targetCase;
        private Config _config;
        private Ticket _ticket;

        public Case(
            Logger log,
            OtrsClient otrs,
            ITargetApi target,
            Config config,
            Ticket ticket)
        {
            _log = log;
            _otrs = otrs;
            _target = target;
            _config = config;
            _ticket = ticket;
        }

        private void UploadArticle(Article article, string prefix)
        {
            if (!_targetCase.FileExists(prefix + " "))
            {
                var name = Helper.CreateName(prefix, article.Subject, "pdf");
                _log.Info("Handling file {0}", name);
                var text = System.IO.File.ReadAllText("Templates/mail.tex");
                var latex = new Latex(text);
                latex.Add("MAILDATE", Helper.FormatDateTime(article.CreateTime));
                latex.Add("MAILFROM", article.From ?? string.Empty, LatexEscapeMode.Minimal);
                latex.Add("MAILTO", Latex.TableMultiline(article.To ?? string.Empty), LatexEscapeMode.Minimal);
                latex.Add("MAILCC", Latex.TableMultiline(article.CC ?? string.Empty), LatexEscapeMode.Minimal);
                latex.Add("MAILSUBJECT", article.Subject ?? string.Empty);
                latex.Add("MAILATTACHMENTS", Latex.TableMultiline(article.Attachements.Select(a => a.Filename)), LatexEscapeMode.Minimal);
                latex.Add("MAILBODY", article.Body, LatexEscapeMode.MultiLine);
                var pdf = latex.Compile();

                if (pdf != null)
                {
                    _log.Notice("Uploading file {0}", name);
                    _targetCase.CreateFile(name, pdf);
                }
                else
                {
                    _log.Error("Article could not be texed {0}", name);
                    _log.Error(latex.ErrorText);
                    _log.Notice("Uploading file {0}", name + ".tex");
                    _targetCase.CreateFile(name + ".tex", Encoding.UTF8.GetBytes(latex.TexDocument));
                }
            }

            int attachementNumber = 1;

            foreach (var attachement in article.Attachements)
            {
                UploadAttachement(article, attachement, prefix, attachementNumber);
                attachementNumber++;
            }
        }

        private void UploadAttachement(Article article, Attachement attachement, string prefix, int number)
        {
            string attachmentPrefix = prefix + "." + string.Format("{0:00}", number);

            if (!_targetCase.FileExists(attachmentPrefix + " "))
            {
                _log.Info("Handling file {0} {1}", attachmentPrefix, attachement.Filename);
                var handlers = new FileHandlers(_otrs, _targetCase, _config, new FileHandlerContext(_log, _ticket, article));
                handlers.Handle(
                    new FileHandlerData(
                        attachement.Filename,
                        attachmentPrefix,
                        article.CreateTime,
                        Convert.FromBase64String(attachement.Content)));
            }
        }

        private object[] GetGroups(Match match)
        {
            var values = new List<object>();

            for (int i = 1; i < match.Length; i++)
            {
                var value = match.Groups[i].Value;
                int number = 0;

                if (int.TryParse(value, out number))
                {
                    values.Add(number);
                }
                else
                {
                    values.Add(value);
                }
            }

            return values.ToArray();
        }

        private string CaseName
        {
            get
            {
                return _config.TicketPrefix +
                              Regex.Replace(_ticket.Number, _config.TicketNumberRegex,
                                            m => string.Format(_config.TicketNumberFormat, GetGroups(m)));
            }
        }

        public void Sync()
        {
            _log.Info("Checking case {0}", _ticket.Number);

            _targetCase = _target.OpenOrCreateCase(CaseName);
            foreach (var article in _ticket.Articles)
            {
                var prefix = string.Format("{0:000}", article.Number);
                UploadArticle(article, prefix);
            }
        }
    }
}
