using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Otrs;
using Alfresco;

namespace Otrs2Alfresco
{
    public class Case
    {
        private Logger _log;
        private OtrsClient _otrs;
        private Alfresco.AlfrescoClient _alfresco;
        private Config _config;
        private Ticket _ticket;
        private Node _alfrescoLibraryNode;
        private Node _caseFolder;
        private List<Node> _nodesInCaseFolder;

        public Case(
            Logger log,
            OtrsClient otrs,
            AlfrescoClient alfresco,
            Config config,
            Ticket ticket,
            Node alfrescoLibraryNode)
        {
            _log = log;
            _otrs = otrs;
            _alfresco = alfresco;
            _config = config;
            _ticket = ticket;
            _alfrescoLibraryNode = alfrescoLibraryNode;
        }

        private bool FileExists(string prefix)
        {
            return _nodesInCaseFolder
                .Any(file => file.Name.StartsWith(prefix, StringComparison.InvariantCulture));
        }

        private void UploadArticle(Article article, string prefix)
        {
            if (!FileExists(prefix + " "))
            {
                var name = Helper.CreateName(prefix, article.Subject, "pdf");
                _log.Notice("Uploading file {0}", name);
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
                    _alfresco.CreateFile(_caseFolder.Id, name, pdf);
                }
                else
                {
                    _log.Error("Article could not be texed {0}", name);
                    _log.Error(latex.ErrorText);
                    _alfresco.CreateFile(_caseFolder.Id, name + ".tex", Encoding.UTF8.GetBytes(latex.TexDocument));
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
            var handlers = new FileHandlers(_alfresco, _otrs, _config, new FileHandlerContext(_log, _ticket, article, _caseFolder, _nodesInCaseFolder));
            handlers.Handle(
                new FileHandlerData(
                    attachement.Filename,
                    attachmentPrefix, 
                    article.CreateTime,
                    Convert.FromBase64String(attachement.Content)));
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

            _caseFolder = _alfresco.GetNodes(_alfrescoLibraryNode.Id)
                                   .Where(n => n.Name == CaseName)
                                   .SingleOrDefault();

            if (_caseFolder == null)
            {
                _log.Info("Creating folder {0}", CaseName);
                _caseFolder = _alfresco.CreateFolder(_alfrescoLibraryNode.Id, CaseName);
            }

            _nodesInCaseFolder = _alfresco.GetNodes(_caseFolder.Id).ToList();

            foreach (var article in _ticket.Articles)
            {
                var prefix = string.Format("{0:000}", article.Number);

                if (!FileExists(prefix))
                {
                    UploadArticle(article, prefix);
                }
            }
        }
    }
}
