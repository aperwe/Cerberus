using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Localization.LocSolutions.Cerberus.Core;
using Microsoft.Localization.LocSolutions.Cerberus.Resources;

namespace Microsoft.Localization.LocSolutions.Cerberus.Configurator
{
    /// <summary>
    /// Custom comparer to determine whether two TreeViewItems actually pertain to the same project (name) and whether their enablement state is equal.
    /// </summary>
    class CustomProjectTreeViewItemComparer : IEqualityComparer<TreeViewItem>
    {
        #region IEqualityComparer<TreeViewItem> Members

        /// <summary>
        /// Returns true if the first string labels on both TreeViewItems are the same
        /// AND if the first checkboxes have the same enablement status.
        /// </summary>
        /// <param name="x">Left item to compare.</param>
        /// <param name="y">Right item to compare.</param>
        /// <returns></returns>
        public bool Equals(TreeViewItem x, TreeViewItem y)
        {
            var xPanel = (StackPanel)x.Header;
            var yPanel = (StackPanel)y.Header;

            var xName = xPanel.Children.ToList<Label>().First().Content.ToString();
            var yName = yPanel.Children.ToList<Label>().First().Content.ToString();
            if (xName != yName) return false;

            var xEnableState = xPanel.Children.ToList<CheckBox>().First().IsChecked;
            var yEnabledState = yPanel.Children.ToList<CheckBox>().First().IsChecked;
            if (xEnableState != yEnabledState) return false;
            return true;
        }

        /// <summary>
        /// Gets hashcode for the item.
        /// </summary>
        public int GetHashCode(TreeViewItem item)
        {
            var x = ((StackPanel)item.Header).Children.ToList<Label>().First().Content.ToString().Length;
            return x;
        }

        #endregion
    }
}
