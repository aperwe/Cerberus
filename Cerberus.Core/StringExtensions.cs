using System;
using System.Linq;

namespace Microsoft.Localization.LocSolutions.Cerberus.Core
{
    /// <summary>
    /// Convenience methods to operate on strings.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Returns true, if the string contains the specified <paramref name="text"/> without caring for string case (InvariantCulture).
        /// </summary>
        /// <param name="me">String</param>
        /// <param name="text">Returns true if this text is contained within the given string.</param>
        public static bool ContainsCaseInsensitive(this string me, string text)
        {
            if (me == null) return false;
            return me.ToLowerInvariant().Contains(text.ToLowerInvariant());
        }
    }
}