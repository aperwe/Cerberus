using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml.Schema;

namespace BrandingExcelSpreadsheetToXmlConverter
{
    class BrandingNameManager
    {

        readonly List<BrandingNameEntry> brandingNameEntries = new List<BrandingNameEntry>();
        public void AddBrandingNameEntries(IEnumerable<BrandingNameEntry> entries)
        {
            brandingNameEntries.AddRange(entries);
        }
        public void ClearBrandingNameEntries()
        {
            brandingNameEntries.Clear();
        }
        public ValidationError[] ValidateBrandingNames()
        {
            IEnumerable<ValidationError> retVal = null;
            // check for duplicate English entries
            var englishBrandingNames = brandingNameEntries.Select(bne => bne.English).ToArray();

            var duplicateNames = englishBrandingNames.Except(englishBrandingNames.Distinct(BrandingName.ByNameComparer));

            retVal = duplicateNames.Select(bn => 
                new ValidationError(
                    String.Format("Duplicate English branding name: {0} at {1}", bn.Name, bn.Location.Description),
                    bn.Location)
                    );

            // check for translations that are the same as English, i.e. redundant
            var redundantTranslations =
                from bne in brandingNameEntries
                from trans in bne.Translations
                where trans.Value.Name == bne.English.Name
                select new ValidationError(
                    String.Format("Translation for culture {0} is same as English {1}. Should be removed.", trans.Key, bne.English.Name),
                    trans.Value.Location
                    );

            retVal = retVal.Union(redundantTranslations);
            
            // check that any regular expression branding names compile correctly
            var allBrandingNames =
                englishBrandingNames.Union(
                    from bne in brandingNameEntries
                    from trans in bne.Translations
                    select trans.Value
                );
             
            var invalidRegexes = 
                from bn in allBrandingNames
                where bn.IsRegex && IsInvalidRegex(bn.Name)
                select new ValidationError(
                    String.Format("Branding Name {0} contains incorrect regular expression at {2}", bn.Name, bn.Location),
                    bn.Location
                    );

            retVal = retVal.Union(invalidRegexes);
              
            return retVal.ToArray();
        }

        static bool IsInvalidRegex(string s)
        {
            bool isInvalid = false;
            try
            {
                var tryRegex = new System.Text.RegularExpressions.Regex(s);
            }
            catch (ArgumentException)
            {
                isInvalid = true;
            }
            return isInvalid;
        }
        public void ExportToXml()
        {
            var xDoc = XDocument.Parse(@"<?xml version=""1.0"" encoding=""UTF-8""?>
<?mso-infoPathSolution solutionVersion=""1.0.0.21"" productVersion=""14.0.0"" PIVersion=""1.0.0.0"" href=""file:///C:\Cerberus\Cerberus.Checks.Helpers\BrandingNameListToXmlConverter\BrandingNamesMatrixForm.xsn"" name=""urn:schemas-microsoft-com:office:infopath:BrandingMasterListForm:"" ?>
<?mso-application progid=""InfoPath.Document"" versionProgid=""InfoPath.Document.3""?>
<bn:BrandingNamesMatrix xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:bn=""http://tempuri.org/BrandingNamesMatrix.xsd"" xmlns:my=""http://schemas.microsoft.com/office/infopath/2003/myXSD/2009-06-26T22:42:01"" xmlns:xd=""http://schemas.microsoft.com/office/infopath/2003"" xml:lang=""en-us"" />
");
            var defaultNS = xDoc.Root.GetDefaultNamespace();

            xDoc.Root.Add(
                (
                from entry in brandingNameEntries
                select new XElement(
                    "BrandingName" + defaultNS.NamespaceName,
                    new XAttribute("Value", entry.English),
                        from trans in entry.Translations
                        select new XElement(
                            "LocalizedBrandingName" + defaultNS.NamespaceName,
                            new XAttribute("Culture", trans.Key.Name),
                            new XAttribute("Translation", trans.Value.Name),
                            trans.Value.IsRegex ? new XAttribute("RegularExpression", trans.Value.Name) : null
                            )
                    )
                ).ToArray()
                );
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

