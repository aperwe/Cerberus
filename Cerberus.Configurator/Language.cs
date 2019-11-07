using System.Windows.Controls;
using Microsoft.Localization.LocSolutions.Cerberus.Core;
using Microsoft.Localization.LocSolutions.Cerberus.Resources;
using System;

namespace Microsoft.Localization.LocSolutions.Cerberus.Configurator
{
    /// <summary>
    /// Represents a language object displayed in <seealso cref="ListViewItem"/>
    /// </summary>
    public class Language : DisplayableItem
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="language">Name of the language. Will be displayed as header of the <seealso cref="ListViewItem"/>.</param>
        /// <param name="configuration">Reference configuration allowing extraction of additional metadata.</param>
        public Language(string language, CheckConfiguration configuration)
            : base(language, ItemType.Language)
        {
        }

        /// <summary>
        /// Icon of a globe, representing a language.
        /// </summary>
        public override Image Icon
        {
            get { return ResourceHelper.GetImage(IconType.EarthGlobe, 24, 24); }
        }

        /// <summary>
        /// Tooltip for this language. Returns the name of this language.
        /// </summary>
        public override string OverridableTooltip
        {
            get { return Name; }
        }
    }
}
