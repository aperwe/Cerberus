using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

namespace BrandingExcelSpreadsheetToXmlConverter
{
    struct BrandingNameEntry
    {
        public BrandingNameEntry(BrandingName english, IDictionary<CultureInfo, BrandingName> translations)
        {
            English = english;
            Translations = translations;
        }
        public BrandingName English;
        public IDictionary<CultureInfo, BrandingName> Translations;
    }

    struct BrandingName
    {
        public BrandingName(string name, ILocation location, bool isRegex)
        {
            Name = name;
            Location = location;
            IsRegex = isRegex;
        }
        public string Name;
        public ILocation Location;
        public bool IsRegex;
        public static IEqualityComparer<BrandingName> ByNameComparer = new ByBrandingNameComparer();
        public class ByBrandingNameComparer : IEqualityComparer<BrandingName>
        {
            #region IEqualityComparer<BrandingName> Members

            public bool Equals(BrandingName x, BrandingName y)
            {
                return x.Name == y.Name;
            }

            public int GetHashCode(BrandingName obj)
            {
                return obj.Name.GetHashCode();
            }

            #endregion
        }

    }
   


}
