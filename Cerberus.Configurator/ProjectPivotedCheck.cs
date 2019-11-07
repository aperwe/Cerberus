using System.Windows.Controls;
using Microsoft.Localization.LocSolutions.Cerberus.Resources;
using System;
using System.Windows.Media;
using System.Linq;
using Microsoft.Localization.LocSolutions.Cerberus.Core;

namespace Microsoft.Localization.LocSolutions.Cerberus.Configurator
{
    /// <summary>
    /// Represents a check object displayed in <seealso cref="ListViewItem"/> that is pivoted around projects.
    /// This means it presents tooltip containing information about languages for which this check is enabled.
    /// </summary>
    public class ProjectPivotedCheck : DisplayableCheck
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="check">Name of the check. Will be displayed as header of the <seealso cref="ListViewItem"></seealso>.</param>
        /// <param name="configuration">Reference configuration allowing extraction of additional metadata.</param>
        public ProjectPivotedCheck(string check, CheckConfiguration configuration) : base(check, configuration) { }

        /// <summary>
        /// Generates tooltip for project-pivoted check entry.
        /// </summary>
        /// <param name="check">Name of the check. Will be displayed as header of the <seealso cref="ListViewItem"></seealso>.</param>
        /// <param name="configuration">Reference configuration allowing extraction of additional metadata.</param>
        protected override void GenerateTooltip(string check, CheckConfiguration configuration)
        {
            var activeLanguages = configuration.GetLanguagesWithCheckEnabled(check).ToArray();
            var allLanguages = configuration.GetAllLanguages().Count();
            if (activeLanguages.Count() == 0)
            {
                GeneratedTooltip = "Not enabled for any language.";
            }
            else if (activeLanguages.Count() == allLanguages)
            {
                GeneratedTooltip = "Enabled for all languages.";
            }
            else
            {
                GeneratedTooltip = string.Format("Enabled for these languages: {0}.", string.Join(", ", activeLanguages));
            }
        }
    }
}
