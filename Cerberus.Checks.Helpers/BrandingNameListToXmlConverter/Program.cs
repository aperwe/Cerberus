using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.IO;
using System.Xml.Schema;

namespace BrandingNameToXmlExtractor
{
    class Program
    {
        static void Main(string[] args)
        {
            var names = File.ReadAllLines("listofenbrandingnames.txt");
            // target xml document for branding check configuration
            var xDoc = XDocument.Parse(@"<?xml version=""1.0"" encoding=""UTF-8""?>
<?mso-infoPathSolution solutionVersion=""1.0.0.21"" productVersion=""14.0.0"" PIVersion=""1.0.0.0"" href=""file:///C:\Cerberus\Cerberus.Checks.Helpers\BrandingNameListToXmlConverter\BrandingNamesMatrixForm.xsn"" name=""urn:schemas-microsoft-com:office:infopath:BrandingMasterListForm:"" ?>
<?mso-application progid=""InfoPath.Document"" versionProgid=""InfoPath.Document.3""?>
<bn:BrandingNamesMatrix xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:bn=""http://tempuri.org/BrandingNamesMatrix.xsd"" xmlns:my=""http://schemas.microsoft.com/office/infopath/2003/myXSD/2009-06-26T22:42:01"" xmlns:xd=""http://schemas.microsoft.com/office/infopath/2003"" xml:lang=""en-us"" />
");
            var defaultNS = xDoc.Root.GetDefaultNamespace();
            
            // extract a list of duplicate branding names where the only difference between two strings is the ® character
            var rRemoved = names.Select(n => n.Replace("®", String.Empty)).ToArray();
            var duplicateEntries = rRemoved.Where(n => rRemoved.Count(x => x.Equals(n, StringComparison.Ordinal)) > 1).ToArray();
            if (duplicateEntries.Length > 0)
            {
                Console.WriteLine("Duplicate entries detected (differing only by the ® character):");
                foreach (var dupe in duplicateEntries)
                {
                    Console.WriteLine(dupe);
                }
                System.Windows.Forms.MessageBox.Show("Duplicate entries detected (differing only by the ® character). Check console output. Exiting...");
                return;
            }
            foreach (var name in names)
            {
                var bNameNode = new XElement(XName.Get("BrandingName", defaultNS.NamespaceName),
                    new XAttribute("Value", name));
                xDoc.Root.Add(bNameNode);
            }
            // validate xml
            XmlSchemaSet schemas = new XmlSchemaSet();
            schemas.Add(defaultNS.ToString(), "BrandingNamesMatrix.xsd");
            bool errors = false;
            xDoc.Validate(schemas, (o, e) =>
            {
                Console.WriteLine("{0}", e.Message);
                errors = true;
            });
            if (errors)
            {
                System.Windows.Forms.MessageBox.Show("Target XML invalidates schema. Check console output. File was not saved");
            }
            else
            {
                xDoc.Save("BrandingNamesMatrix.xml");
            }
        }
    }
}
