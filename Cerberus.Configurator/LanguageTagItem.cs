using System.Linq;
using System.Windows.Controls;
using Microsoft.Localization.LocSolutions.Cerberus.Core;
using Microsoft.Localization.LocSolutions.Cerberus.Resources;
using System;

namespace Microsoft.Localization.LocSolutions.Cerberus.Configurator
{
    /// <summary>
    /// Class representing a Language tag (or group) that can be displayed in ListView as <seealso cref="ListViewItem"/>.
    /// </summary>
    public class LanguageTagItem : DisplayableItem
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="tag">Language tag name. This will be displayed in the <seealso cref="ListViewItem"/>.</param>
        /// <param name="configuration">Reference configuration allowing extraction of additional metadata.</param>
        public LanguageTagItem(string tag, CheckConfiguration configuration)
            : base(tag, ItemType.Tag)
        {
            var tagLanguages = configuration.GetTagLanguages(tag).ToArray();
            var allLanguages = configuration.GetAllLanguages().Count();
            if (tagLanguages.Count() == 0)
            {
                tooltip = "This group contains no languages.";
            }
            else if (tagLanguages.Count() == allLanguages)
            {
                tooltip = "All languages.";
            }
            else
            {
                tooltip = string.Format("Languages: {0}.", string.Join(", ", tagLanguages));
            }
        }

        /// <summary>
        /// Icon of a tag.
        /// </summary>
        public override Image Icon
        {
            get { return ResourceHelper.GetImage(IconType.Tag, 24, 24); }
        }

        private string tooltip;

        /// <summary>
        /// Tooltip for this language tag. Shows languages that have this tag.
        /// </summary>
        public override string OverridableTooltip
        {
            get { return tooltip; }
        }
    }
}
