using System.Windows.Controls;
using Microsoft.Localization.LocSolutions.Cerberus.Resources;
using System;
using System.Windows.Media;
using System.Linq;
using Microsoft.Localization.LocSolutions.Cerberus.Core;

namespace Microsoft.Localization.LocSolutions.Cerberus.Configurator
{
    /// <summary>
    /// Represents a check object displayed in <seealso cref="ListViewItem"/> that shows a check icon.
    /// </summary>
    public class DisplayableCheck : DisplayableItem
    {
        /// <summary>
        /// Tooltip for this check.
        /// </summary>
        protected string GeneratedTooltip { get; set; }

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="check">Name of the check. Will be displayed as header of the <seealso cref="ListViewItem"/>.</param>
        /// <param name="configuration">Reference configuration allowing extraction of additional metadata.</param>
        public DisplayableCheck(string check, CheckConfiguration configuration) : base(check, ItemType.Check)
        {
            GenerateTooltip(check, configuration);
        }

        /// <summary>
        /// Generates tooltip for this check entry.
        /// </summary>
        /// <param name="check">Name of the check. Will be displayed as header of the <seealso cref="ListViewItem"/>.</param>
        /// <param name="configuration">Reference configuration allowing extraction of additional metadata.</param>
        protected virtual void GenerateTooltip(string check, CheckConfiguration configuration)
        {
        }

        /// <summary>
        /// Icon of a check.
        /// </summary>
        public override Image Icon
        {
            get { return ResourceHelper.GetImage(IconType.Check, 24, 24); }
        }

        /// <summary>
        /// Tooltip for this check. Returns list of projects for which this check is enabled.
        /// </summary>
        public override string OverridableTooltip
        {
            get
            {
                return GeneratedTooltip;
            }
        }

        /// <summary>
        /// Color to be used to display the name of the check in <seealso cref="ListViewItem"/>.
        /// </summary>
        public virtual object FontColor
        {
            get
            {
                return Colors.Black.ToString();
            }
        }
    }
}
