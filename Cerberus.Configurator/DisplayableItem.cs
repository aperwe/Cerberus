using System.Windows.Controls;
using System;

namespace Microsoft.Localization.LocSolutions.Cerberus.Configurator
{
    /// <summary>
    /// Base class representing an item that can be displayed in ListView as <seealso cref="ListViewItem"/>.
    /// Currently it can be: Language, Language group, Check.
    /// </summary>
    public abstract class DisplayableItem
    {
        /// <summary>
        /// This is the item's name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Actual item type. Currently supported are: Language, Language group, Check.
        /// </summary>
        public ItemType ItemType { get; set; }

        /// <summary>
        /// Default constructor.
        /// </summary>
        protected DisplayableItem(string name, ItemType itemType)
        {
            Name = name;
            ItemType = itemType;
        }

        /// <summary>
        /// Implementors provide the icon for the specific <seealso cref="ListViewItem"/>.
        /// </summary>
        public abstract Image Icon { get; }

        /// <summary>
        /// Tooltip for the specific <seealso cref="ListViewItem"/>.
        /// </summary>
        public string Tooltip { get { return OverridableTooltip; } }

        /// <summary>
        /// Implementors provide their specific tooltip here.
        /// </summary>
        public abstract string OverridableTooltip { get; }
    }
}
