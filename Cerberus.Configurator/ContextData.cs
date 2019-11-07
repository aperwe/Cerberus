using System;

namespace Microsoft.Localization.LocSolutions.Cerberus.Configurator
{
    /// <summary>
    /// Data holder for storing context about items (checkboxes) clicked in language or project lists.
    /// </summary>
    internal class ContextData
    {
        /// <summary>
        /// Name of project or language.
        /// </summary>
        internal string Element { get; set; }
        /// <summary>
        /// Indicator of what it is.
        /// </summary>
        internal ContextType ElementType { get; set; }
        public override string ToString()
        {
            return string.Format("{0} Name = {1}", ElementType, Element);
        }
    }
}
