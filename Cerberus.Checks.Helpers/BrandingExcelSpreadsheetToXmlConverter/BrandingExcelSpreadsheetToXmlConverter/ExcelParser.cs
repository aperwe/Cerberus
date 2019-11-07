using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Office.Interop.Excel;
using System.Globalization;
using System.Xml.Linq;
using System.Xml.Schema;
using System.IO;
using System.Reflection;

namespace BrandingExcelSpreadsheetToXmlConverter
{
    class ExcelParser
    {
        List<BrandingNameEntry> brandingNameEntries = new List<BrandingNameEntry>();

        public void ParseExcelRange(Range range)
        {
            // read the range into a 2-dimensional array of object
            var table = (object[,])range.Value2;
            var firstRow = table.GetLowerBound(0);
            var lastRow = table.GetUpperBound(0);
            var firstColumn = table.GetLowerBound(1);
            var lastColumn = table.GetUpperBound(1);
            // extract column to culture mapping
            var columnToCultureName = new Dictionary<int, string>();
            for (int i = firstColumn; i <= lastColumn; i++)
            {
                columnToCultureName.Add(i, table[1, i] as string ?? String.Empty);
            }
            // verify there is no empty column headers
            var emptyColumnHeaders = columnToCultureName.Where(x => String.IsNullOrEmpty(x.Value)).Select(x=>x.Key.ToString()).ToArray();
            if (emptyColumnHeaders.Length > 0)
            {
                throw new InvalidOperationException("Empty column headers (first row) for columns: " + String.Join(",", emptyColumnHeaders));
            }
            // verify that culture names are unique
            var duplicateCultures = columnToCultureName.Values.Where(x => columnToCultureName.Values.Count(y => y.Equals(x, StringComparison.OrdinalIgnoreCase)) > 1).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
            if (duplicateCultures.Length > 0)
            {
                throw new InvalidOperationException(String.Format("Duplicate culture names in the table header (first row): {0}", String.Join(", ", duplicateCultures)));
            }
            var invalidCultureNames = columnToCultureName.Values.Where(
                c =>
                {
                    bool invalid = false;
                    try
                    {
                        var cultureTest = new CultureInfo(c);
                    }
                    catch (ArgumentException)
                    {
                        invalid = true;
                    }
                    return invalid;

                }
                ).ToArray();
            if (invalidCultureNames.Length > 0)
            {
                throw new InvalidOperationException("Invalid culture names used in table header (first row): " + String.Join(", ", invalidCultureNames));
            }
            // get the top lef t cell in the range so we can offset from here when calculating cells.
            var topLeftCell = (Range)range.Cells.get_Item(1, 1);

            // parse table into BrandingNameEntries
            brandingNameEntries.Clear();
            for (int i = firstRow + 1; i <= lastRow; i++)
            {
                // skip empty rows
                if (table[i, firstColumn] == null)
                    continue;
                var englishName = table[i, firstColumn].ToString();
                // skip rows that contain group names not actual product names
                if (englishName.Trim().StartsWith("#") || englishName.Trim().Equals(String.Empty))
                {
                    continue;
                }
                var englishNameCell = topLeftCell.get_Offset(i - firstRow, 0);
                var translations = new Dictionary<CultureInfo, BrandingName>();
                for (int j = firstColumn + 1; j <= lastColumn; j++)
                {
                    // skip empty translations
                    if (table[i, j] == null)
                    {
                        continue;
                    }
                    var translation = table[i, j].ToString();
                    // skip translations that contain only spaces
                    if (translation.Trim().Equals(String.Empty))
                    {
                        continue;
                    }
                   
                    var translationCell = topLeftCell.get_Offset(i - firstRow, j - firstColumn);
                    translations.Add(new CultureInfo(columnToCultureName[j]),
                        new BrandingName(
                            translation,
                            new ExcelLocation(translationCell),
                            false
                            ));
                }
                // finally, add the complete branding name entry for one row
                brandingNameEntries.Add(
                    new BrandingNameEntry(
                        new BrandingName(englishName, new ExcelLocation(englishNameCell), false),
                        translations
                    )
                );
            }

        }
        /// <summary>
        /// Checks that the working range is a valid range from which branding configuration can be read
        /// </summary>
        /// <returns></returns>
        public ValidationError[] ValidateRange()
        {
            IEnumerable<ValidationError> errors = new ValidationError[0];
            // check duplicate entries for English
            var duplicateEnglishBrandingNames =
                (
                from bne in brandingNameEntries
                select bne.English
                ).Duplicates((x, y) => x.Name.Equals(y.Name, StringComparison.OrdinalIgnoreCase));

            errors = errors.Union(
                from bn in duplicateEnglishBrandingNames
                select new ValidationError(
                    String.Format("Duplicate English branding name \"{0}\" at {1}", bn.Name, bn.Location),
                    bn.Location
                    )
                );

            // redundant english names with (R) removed
            // extract a list of duplicate branding names where the only difference between two strings is the ® character
            var rRemoved = brandingNameEntries.Where(bne => bne.English.Name.Contains("®")).Select(bne => bne.English.Name.Replace("®", String.Empty)).ToArray();
            var redundantEnglishBrandingNames = brandingNameEntries.Where(bne => rRemoved.Contains(bne.English.Name));

            errors = errors.Union(
                from bne in redundantEnglishBrandingNames
                select new ValidationError(
                    String.Format("Redundant branding entry \"{0}\" with ® removed at {1}", bne.English.Name, bne.English.Location),
                    bne.English.Location
                    )
                    );

            //redundant translations - translation values that are the same as English
            var redundantTranslations =
                from bne in brandingNameEntries
                from transEntry in bne.Translations
                where Helpers.BrandingNamesEqualIgnoreR(transEntry.Value, bne.English)
                select new ValidationError(
                    String.Format("Redundant translation \"{0}\" for culture {1} at {2}. Same as English.", transEntry.Value.Name, transEntry.Key, transEntry.Value.Location),
                    transEntry.Value.Location);

            errors = errors.Union(redundantTranslations);

            //potentially incorrect English names - vuales that contain no letters
            var containsALetter = new System.Text.RegularExpressions.Regex(@"\p{L}", System.Text.RegularExpressions.RegexOptions.IgnoreCase| System.Text.RegularExpressions.RegexOptions.Singleline);

            var incorrectEnglish =
                from bne in brandingNameEntries
                let bn = bne.English
                where !containsALetter.IsMatch(bn.Name)
                select new ValidationError(
                    String.Format("Incorrect English name - contains no letters: \"{0}\", at {1}", bn.Name, bn.Location),
                    bn.Location
                    );

            errors = errors.Union(incorrectEnglish);

            //potentially incorrect translations - translation values that contain no letters
            var incorrectTranslations =
                from bne in brandingNameEntries
                from trans in bne.Translations.Values
                where !containsALetter.IsMatch(trans.Name)
                select new ValidationError(
                    String.Format("Incorrect translation - contains no letters: \"{0}\", at {1}", trans.Name, trans.Location),
                    trans.Location
                    );

            errors = errors.Union(incorrectTranslations);
            return errors.ToArray();
        }

        internal void ExportToXML(string path)
        {
            var xDoc = XDocument.Parse(@"<?xml version=""1.0"" encoding=""UTF-8""?>
<!--This document contains a list of all official Branding/Product names and their approved translations that is used by several tools in the int'l production system, such
as Cerberus and Branding Token injector.-->
<BrandingNamesMatrix xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""http://tempuri.org/BrandingNamesMatrix.xsd"" xml:lang=""en-us"" />
");
            //var defaultNS = xDoc.Root.GetNamespaceOfPrefix("bn");//.GetPrefixOfNamespace(nameSpace);
            var defaultNS = xDoc.Root.GetDefaultNamespace();

            xDoc.Root.Add(
               from bne in brandingNameEntries
               from element in CreateElementsFromBrandingNameEntry(bne, defaultNS)
               select element
               );
            // validate xml
            // retrieve schema from embedded resource
            var asm = Assembly.GetExecutingAssembly();
            var xmlStream = asm.GetManifestResourceStream("BrandingExcelSpreadsheetToXmlConverter.BrandingNamesMatrix.xsd");
            var schema = System.Xml.Schema.XmlSchema.Read(xmlStream, null);
            XmlSchemaSet schemas = new XmlSchemaSet();

            //schemas.Add(defaultNS.NamespaceName, schema);
            schemas.Add(schema);
            var errors = new List<string>();
            xDoc.Validate(schemas, (o, e) =>
            {
                errors.Add(e.Message);
            });
            if (errors.Count > 0)
            {
                System.Windows.Forms.MessageBox.Show("Target XML invalidates schema. File was not saved. Details:\n" + String.Join("\n", errors.ToArray()));
            }
            else
            {
                xDoc.Save(path);
                System.Windows.Forms.MessageBox.Show("XML file saved successfully at:\n" + path);
            }
        }

        /// <summary>
        /// Create one or two XElements from a single branding name entry.
        /// If the English branding name contains a "®" character, we are going to create a duplicate
        /// XElement with all "®" removed from both English and translations.
        /// </summary>
        /// <param name="bne"></param>
        /// <returns></returns>
        private IEnumerable<XElement> CreateElementsFromBrandingNameEntry(BrandingNameEntry bne, XNamespace defaultNS)
        {
            var ret = new List<XElement>();
            ret.Add(
                new XElement(defaultNS + "BrandingName",
                        new XAttribute("Value", bne.English.Name),
                        from transEntry in bne.Translations
                        select
                            new XElement(defaultNS + "LocalizedBrandingName",
                                new XAttribute("Culture", transEntry.Key.Name),
                                new XAttribute("Translation", transEntry.Value.Name))
                )
            );
            if (bne.English.Name.Contains("®"))
            {
                ret.Add(
                    new XElement(defaultNS + "BrandingName",
                         new XAttribute("Value", bne.English.Name.Replace("®", String.Empty)),
                         from transEntry in bne.Translations
                         select
                             new XElement(defaultNS + "LocalizedBrandingName",
                                 new XAttribute("Culture", transEntry.Key.Name),
                                 new XAttribute("Translation", transEntry.Value.Name.Replace("®", String.Empty)))
                    )  
                );
            }
            return ret;
        }
    }
}
