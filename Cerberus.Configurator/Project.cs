using System.Windows.Controls;
using Microsoft.Localization.LocSolutions.Cerberus.Core;
using Microsoft.Localization.LocSolutions.Cerberus.Resources;
using System;

namespace Microsoft.Localization.LocSolutions.Cerberus.Configurator
{
    /// <summary>
    /// Represents a project object displayed in <seealso cref="ListViewItem"/>
    /// </summary>
    public class Project : DisplayableItem
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="project">Name of the project. Will be displayed as header of the <seealso cref="ListViewItem"/>.</param>
        /// <param name="configuration">Reference configuration allowing extraction of additional metadata.</param>
        public Project(string project, CheckConfiguration configuration)
            : base(project, ItemType.Project)
        {
        }

        /// <summary>
        /// Icon of a folder, representing a project.
        /// </summary>
        public override Image Icon
        {
            get { return ResourceHelper.GetImage(IconType.FolderPale, 24, 24); }
        }

        /// <summary>
        /// Tooltip for this project. Returns the name of the project.
        /// </summary>
        public override string OverridableTooltip
        {
            get { return Name; }
        }
    }
}
