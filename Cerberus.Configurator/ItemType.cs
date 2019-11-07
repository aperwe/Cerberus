using System;

namespace Microsoft.Localization.LocSolutions.Cerberus.Configurator
{
    /// <summary>
    /// Type of item that is displayed in a view handler's list view as ListViewItem.
    /// </summary>
    public enum ItemType
    {
        /// <summary>
        /// Language group (or tag) is shown in list view.
        /// </summary>
        Tag,

        /// <summary>
        /// Language is shown in list view.
        /// </summary>
        Language,

        /// <summary>
        /// Check is shown in list view.
        /// </summary>
        Check,

        /// <summary>
        /// Project is shown in list view.
        /// </summary>
        Project
    }
}
