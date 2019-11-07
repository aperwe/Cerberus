using System.Windows.Controls;
using System;
using System.Windows;

namespace Microsoft.Localization.LocSolutions.Cerberus.Configurator
{
    /// <summary>
    /// Custom <seealso cref="ListView"/> type of view that displays items in tiles.
    /// </summary>
    public class TileView : ViewBase
    {
        /// <summary>
        /// Undetermined meaning. Never initialized by us. Implemented based on book on WPF.
        /// </summary>
        public DataTemplate ItemTemplate { get; set; }

        /// <summary>
        /// Gets the object that is associated with the style for the view mode.
        /// <para/>Returns the style to use for the view mode. The default value is the style for the
        /// System.Windows.Controls.ListBox.
        /// </summary>
        protected override object DefaultStyleKey
        {
            get
            {
                return new ComponentResourceKey(GetType(), "TileView");
            }
        }
    }
}
