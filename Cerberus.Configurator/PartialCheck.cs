using System.Collections.Generic;
using System.Linq;
using Microsoft.Localization.LocSolutions.Cerberus.Core;
using System;
using System.Windows.Media;
using System.Windows.Controls;

namespace Microsoft.Localization.LocSolutions.Cerberus.Configurator
{
    /// <summary>
    /// A variant of <see cref="LanguagePivotedCheck"/> that represents a check that is only partially enabled for the set of enlistment elements.
    /// For example for language groups, a partial check represents a check that is enabled for some, but not all of that language group's languages.
    /// </summary>
    public class PartialCheck : LanguagePivotedCheck
    {
        private string tooltip;

        /// <summary>
        /// Constructs an instance of partial check with necessary metadata.
        /// </summary>
        /// <param name="check">Check name to describe.</param>
        /// <param name="configuration">Reference configuration allowing extraction of additional metadata.</param>
        /// <param name="tagLanguages">Complete list of languages against which we are determining the 'completeness' of this check.</param>
        public PartialCheck(string check, CheckConfiguration configuration, IEnumerable<string> tagLanguages)
            : base(check, configuration)
        {
            tooltip = string.Format("Enabled for: {0}.", string.Join(", ", configuration.GetLanguagesWithCheckEnabled(check).Intersect(tagLanguages).ToArray()));
        }

        /// <summary>
        /// Color to be used to display the name of the check in <seealso cref="ListViewItem"/>.
        /// </summary>
        public override object FontColor
        {
            get
            {
                return Colors.Gray.ToString();
            }
        }

        /// <summary>
        /// Shows languages for which this check is enabled.
        /// </summary>
        public override string OverridableTooltip
        {
            get
            {
                return tooltip;
            }
        }
    }
}
