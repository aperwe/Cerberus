using System.Windows.Controls;
using Microsoft.Localization.LocSolutions.Cerberus.Resources;
using System;
using System.Windows.Media;
using System.Linq;
using Microsoft.Localization.LocSolutions.Cerberus.Core;

namespace Microsoft.Localization.LocSolutions.Cerberus.Configurator
{
    /// <summary>
    /// Represents a check object displayed in <seealso cref="ListViewItem"/> that is pivoted around languages.
    /// This means it presents tooltip containing information about projects for which this check is enabled.
    /// </summary>
    public class LanguagePivotedCheck : DisplayableCheck
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="check">Name of the check. Will be displayed as header of the <seealso cref="ListViewItem"/>.</param>
        /// <param name="configuration">Reference configuration allowing extraction of additional metadata.</param>
        public LanguagePivotedCheck(string check, CheckConfiguration configuration) : base(check, configuration) { }

        /// <summary>
        /// Generates tooltip for language-pivoted check entry.
        /// </summary>
        /// <param name="check">Name of the check. Will be displayed as header of the <seealso cref="ListViewItem"/>.</param>
        /// <param name="configuration">Reference configuration allowing extraction of additional metadata.</param>
        protected override void GenerateTooltip(string check, CheckConfiguration configuration)
        {
            var activeProjects = configuration.GetProjectsWithCheckEnabled(check).ToArray();
            var allProjects = configuration.GetAllProjects().Count();
            if (activeProjects.Count() == 0)
            {
                GeneratedTooltip = "Not enabled for any project.";
            }
            else if (activeProjects.Count() == allProjects)
            {
                GeneratedTooltip = "Enabled for all projects.";
            }
            else
            {
                GeneratedTooltip = string.Format("Enabled for these projects: {0}.", string.Join(", ", activeProjects));
            }
        }
    }
}
