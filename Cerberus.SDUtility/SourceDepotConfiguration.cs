using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.XPath;

namespace PreBuild
{
    sealed class SourceDepotConfiguration
    {
        public string Name { get; private set; }
        public string Address { get; private set; }
        public string Root { get; private set; }

        public SourceDepotConfiguration(string name)
        {
            var xPathDocument = new XPathDocument("PreBuild.xml");
            var navigator = xPathDocument.CreateNavigator();
            var nsmgr = new XmlNamespaceManager(navigator.NameTable);
            var xPathQuery = string.Format("cfm:Root/cfm:SourceDepot[@name='{0}']", name);
            nsmgr.AddNamespace("cfm", "");
            XPathExpression query = navigator.Compile(xPathQuery);
            query.SetContext(nsmgr);
            var node = navigator.SelectSingleNode(query);
            this.Name = node.GetAttribute("name", "");
            this.Address = node.GetAttribute("address", "");
            this.Root = node.GetAttribute("root", "");
        }
    }
}
