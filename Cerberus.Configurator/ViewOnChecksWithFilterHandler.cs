using Microsoft.Localization.LocSolutions.Cerberus.Core;
using Microsoft.Localization.LocSolutions.Cerberus.Resources;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System;

namespace Microsoft.Localization.LocSolutions.Cerberus.Configurator
{
    /// <summary>
    /// Specific view on data that Cerberus supports. Shows Checks as root items and allows filtering by check name or any tags associated with a check.
    /// </summary>
    class ViewOnChecksWithFilterHandler : ViewOnChecksHandler
    {
        public ViewOnChecksWithFilterHandler(Grid viewContainer, CheckConfiguration configuration) : base(viewContainer, configuration) { }

        /// <summary>
        /// A field that allows filtering of checks.
        /// </summary>
        private TextBox _checkFilterTextBox;

        /// <summary>
        /// Reference to the view's filter text box that contains the check filter.
        /// </summary>
        internal TextBox CheckFilterTextBox { get { return _checkFilterTextBox; } }

        /// <summary>
        /// Node in checks view under which all checks (possibly filtered) are displayed.
        /// </summary>
        internal TreeViewItem RootChecksNode { get { return _rootChecksNode; } }

        internal override void Reveal()
        {
            base.Reveal();
            ViewContainer.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }); //Base checks view occupies this
            ViewContainer.RowDefinitions.Add(new RowDefinition { Height = new GridLength(0, GridUnitType.Auto) });
            var checkFilterPanel = ViewContainer.Children.AddAndReference(new StackPanel { Orientation = Orientation.Horizontal });
            Grid.SetRow(checkFilterPanel, 1);
            checkFilterPanel.Children.Add(new Label { Content = "Filter:" });
            _checkFilterTextBox = checkFilterPanel.Children.AddAndReference(new TextBox { MinWidth = 120 });
            _checkFilterTextBox.TextChanged += ReactionToCheckFilterChange;
            checkFilterPanel.Children.AddAndReference(UITheme.CreateButton("Clear filter")).Click += delegate { _checkFilterTextBox.Clear(); };
        }

        /// <summary>
        /// This method responds to the change in check filter. Checks visible in the check tree view should be
        /// filtered based on the contents of <see cref="_checkFilterTextBox"/> text.
        /// </summary>
        private void ReactionToCheckFilterChange(object sender, TextChangedEventArgs e)
        {
            AddOrUpdateChecks(_rootChecksNode, _checkFilterTextBox.Text);
        }

        /// <summary>
        /// Adds tree view items for each check. Optional filter is applied. If <paramref name="filterText"/> parameter is null or empty, no filtering is applied.
        /// If <paramref name="filterText"/> is not empty, then it is broken into chunks (each separated by space) and each chunk is used
        /// to see whether any check property {name, tags}, case-insensitively, matches the filter. If all filter items match any of the check properties, then a check is shown.
        /// Otherwise the check is hidden from the list.
        /// </summary>
        /// <param name="parentNode">Node under which checks are to be added.</param>
        /// <param name="filterText">Filter string, entered in a text box. When empty (or only whitespaces), no filtering is applied, otherwise, each keyword (separated by whitespaces) is used to filter on anything about a check.</param>
        private void AddOrUpdateChecks(ItemsControl parentNode, string filterText)
        {
            try
            {
                parentNode.BeginInit();

                foreach (var check in Configuration.GetAllChecks())
                {
                    #region Enable filter

                    if (!CheckMatchesFilter(check, filterText))
                    {
                        //If the check doesn't match the filter, remove if from the list if it was added.
                        var existingUI = parentNode.Items.ToList<TreeViewItem>()
                            .Where(child => (child.Header as StackPanel).Children.ToList<Label>().First().Content.ToString().Equals(check));
                        if (existingUI.Count() == 0) continue;
                        parentNode.Items.Remove(existingUI.First());
                        continue;
                    }

                    #endregion

                    var checkPanel = new StackPanel { Orientation = Orientation.Horizontal };
                    checkPanel.Children.Add(ResourceHelper.GetImage(IconType.Check, 16, 16));
                    checkPanel.Children.Add(new CheckBox { Name = "CheckCheckBox" });
                    checkPanel.Children.Add(new Label { Content = check, Name = "CheckName" });

                    AddOptionalTagIconWithTooltip(checkPanel, Configuration.GetCheckTags(check));

                    #region Enable removal of tags when the user selects context menu

                    var checkTagRemoval = checkPanel.Children.ToList<Image>()
                        .Where(i => !ReferenceEquals(null, i.Tag))
                        .Where(i => i.Tag.ToString() == "RemoveTag");
                    if (checkTagRemoval.Count() == 1)
                    {
                        var removalIcon = checkTagRemoval.First();
                        var menuItems = removalIcon.ContextMenu;

                        foreach (var item in menuItems.Items.ToList<MenuItem>())
                        {
                            var currentItem = item;
                            var currentCheck = check;
                            RoutedEventHandler mbeh = delegate { Configuration.RemoveTagFomChecks(currentItem.Tag.ToString(), new[] { currentCheck }); UpdateViewToMatchData(); };
                            item.Click += mbeh;
                        }
                    }

                    #endregion

                    //See if there already is a UI item for this check
                    var existingPanels = parentNode.Items.ToList<TreeViewItem>()
                        .Where(child => (child.Header as StackPanel).Children.ToList<Label>().First().Content.ToString().Equals(check));

                    TreeViewItem checkNode = null;
                    if (existingPanels.Count() == 0)
                    {
                        checkNode = new TreeViewItem { Header = checkPanel };
                        parentNode.Items.Add(checkNode);
                    }
                    else
                    {
                        //Replace existing panel with the new panel.
                        checkNode = existingPanels.First();
                        var oldPanel = checkNode.Header as StackPanel;
                        checkNode.Header = checkPanel;
                        //Update check check box status
                        checkPanel.Children.ToList<CheckBox>().First().IsChecked = oldPanel.Children.ToList<CheckBox>().First().IsChecked;
                    }
                }
            }
            finally
            {
                parentNode.EndInit();
            }
        }

        /// <summary>
        /// Returns true if any of check's properties match the filter.
        /// Also returns true if the filter is null or empty.
        /// </summary>
        /// <param name="check">Name of the check to see if it matches the specified filter.</param>
        /// <param name="filterText">Filter text (may be null or empty string - in which case filtering is disabled and true is returned) to see if any of check's properties match the filter.</param>
        /// <returns>True if the specified check matches the filter. False otherwise.</returns>
        private bool CheckMatchesFilter(string check, string filterText)
        {
            if (!string.IsNullOrEmpty(filterText))
            {
                var filter = filterText.Trim();
                if (!string.IsNullOrEmpty(filter))
                {
                    var filterItems = filter.Split(' ');
                    if (!filterItems.All(f => AnyCheckPropertyMatchesFilter(check, f))) return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Determines if any property of the check matches the specific filter item.
        /// </summary>
        /// <param name="check">Check name</param>
        /// <param name="filter">Filter string to compare all check's properties against</param>
        /// <returns>If any property of the specified check matches the filter item, then true is returned. False is returned when none of known check properties matches the filter item.</returns>
        private bool AnyCheckPropertyMatchesFilter(string check, string filter)
        {
            if (check.ContainsCaseInsensitive(filter)) return true;
            if (Configuration.GetCheckTags(check).Any(tag => tag.ContainsCaseInsensitive(filter))) return true;
            return false;
        }

    }
}
