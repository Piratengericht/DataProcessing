using System;
using System.Linq;
using System.Collections.Generic;
using Alfresco;

namespace Otrs2Alfresco
{
    public class AlfrescoCase : ITargetCase
    { 
        private Logger _log;
        private Config _config;
        private AlfrescoClient _alfresco;
        private Node _caseFolder;
        private List<Node> _nodesInCaseFolder;

        public AlfrescoCase(Logger log, Config config, AlfrescoClient alfresco, Node alfrescoLibraryNode, string caseName)
        {
            _log = log;
            _config = config;
            _alfresco = alfresco;

            _caseFolder = _alfresco.GetNodes(alfrescoLibraryNode.Id)
                       .Where(n => n.Name == caseName)
               .SingleOrDefault();

            if (_caseFolder == null)
            {
                _log.Info("Creating folder {0}", caseName);
                _caseFolder = _alfresco.CreateFolder(alfrescoLibraryNode.Id, caseName);
            }

            _nodesInCaseFolder = _alfresco.GetNodes(_caseFolder.Id).ToList();

            foreach (var node in _nodesInCaseFolder)
            {
                Console.WriteLine(node.Name);
            }

            var documents = _alfresco.GetNodes(_caseFolder.Id);

        }

        public bool FileExists(string prefix)
        {
            return _nodesInCaseFolder
                .Any(file => file.Name.StartsWith(prefix, StringComparison.InvariantCulture));
        }

        public void CreateFile(string filename, byte[] filedata)
        {
            _alfresco.CreateFile(_caseFolder.Id, filename, filedata);
        }
    }

    public class AlfrescoApi : ITargetApi
    {
        private Logger _log;
        private Config _config;
        private AlfrescoClient _alfresco;
        private Node _alfrescoLibraryNode;

        public AlfrescoApi(Logger log, Config config)
        {
            _log = log;
            _config = config;
            _alfresco = new AlfrescoClient(_config.AlfrescoBaseUrl, _config.AlfrescoUsername, _config.AlfrescoPassword);
            _alfrescoLibraryNode = _alfresco.GetNode("Sites", _config.AlfrescoSitename, "documentLibrary");
        }

        public ITargetCase OpenOrCreateCase(string caseName)
        {
            return new AlfrescoCase(_log, _config, _alfresco, _alfrescoLibraryNode, caseName);
        }
    }
}
