using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BrandingExcelSpreadsheetToXmlConverter
{
    static class Helpers
    {
        public static IEnumerable<T> Duplicates<T>(this IEnumerable<T> collection, IEqualityComparer<T> comparer)
        {
            return collection.Where(x => collection.Count(y => comparer.Equals(x,y)) > 1);
        }
        public static IEnumerable<T> Duplicates<T>(this IEnumerable<T> collection, Func<T, T,bool> equals)
        {
            return collection.Where(x => collection.Count(y => equals(x, y)) > 1);
        }
        /// <summary>
        /// Ordinal ignore case comparison of values of BrandingNames
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static bool BrandingNamesEqual(BrandingName x, BrandingName y)
        {
            return x.Name.Equals(y.Name, StringComparison.OrdinalIgnoreCase);
        }
        /// <summary>
        /// Ordinal ignore case comparison of values of BrandingNames.
        /// Removes (R) from both branding names before comparison
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static bool BrandingNamesEqualIgnoreR(BrandingName x, BrandingName y)
        {
            return x.Name.Replace("®", String.Empty).Equals(y.Name.Replace("®", String.Empty), StringComparison.OrdinalIgnoreCase);
        }
    }
}
