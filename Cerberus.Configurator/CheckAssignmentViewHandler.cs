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
    /// Specific view on data that Cerberus supports.
    /// Shows Checks in one list, and allows to assign them to Languages as root items, under them shows projects.
    /// </summary>
    internal class CheckAssignmentViewHandler : ViewHandler
    {
        public CheckAssignmentViewHandler(Grid container, CheckConfiguration configuration) : base(container, configuration) { }

        /// <summary>
        /// Tree view of checks creted by this handler. This tree view is a child of the view container.
        /// This view contains all checks.
        /// </summary>
        private TreeView _checksTree { get { return (_checksViewHandler as ViewOnChecksHandler).ChecksTreeView; } }

        /// <summary>
        /// Node in checks view under which all checks (possibly filtered) are displayed.
        /// </summary>
        private TreeViewItem _rootChecksNode { get { return _checksViewHandler.RootChecksNode; } }

        /// <summary>
        /// Separate view on enlistment. Provided by <see cref="ViewOnEnlistmentHandler"/> object (referenced as <see cref="_enlistmentViewHandler"/>).
        /// </summary>
        private TreeView _enlistementTree { get { return _enlistmentViewHandler.EnlistmentTreeView; } }

        /// <summary>
        /// Handler that manages the left-hand-side on checks
        /// </summary>
        private ViewOnChecksWithFilterHandler _checksViewHandler;

        /// <summary>
        /// Handler that manages the right-hand-side view on enlistment.
        /// </summary>
        private ViewOnEnlistmentHandler _enlistmentViewHandler;

        /// <summary>
        /// Reference to the check filtering text box created by <see cref="ViewOnChecksWithFilterHandler"/> object.
        /// </summary>
        private TextBox _checkFilterTextBox { get { return _checksViewHandler.CheckFilterTextBox; } }

        /// <summary>
        /// This is a view description provided to the container (calling code).
        /// The calling code places the view description in an appropriate location, so that the user can read it.
        /// </summary>
        public override string ViewDescription
        {
            get
            {
                return "This is the most important view in Cerberus. Use it to enable checks on languages and projects. When you are finished with enabling checks on enlistment items, switch to 'Configuration summary' view to see what items each check has been enabled on.";
            }
        }

        /// <summary>
        /// Adds controls comprising the current view to the container of the view.
        /// The view will be visible immediately.
        /// </summary>
        internal override void Reveal()
        {
            #region Define as many rows as there are logical row items in your view
            ViewContainer.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            ViewContainer.RowDefinitions.Add(new RowDefinition { Height = new GridLength(0, GridUnitType.Auto) });

            ViewContainer.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) }); //Checks
            ViewContainer.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) }); //Enlistment
            #endregion


            var checkViewContainer = ViewContainer.AddGridItem(0, UITheme.CreateGrid());
            Grid.SetColumn(checkViewContainer, 0);
            checkViewContainer.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }); //Checks view (tree and buttons)
            checkViewContainer.RowDefinitions.Add(new RowDefinition { Height = new GridLength(0, GridUnitType.Auto) }); //Check filter section

            _checksViewHandler = ViewFactory.CreateView(ViewType.ViewOnChecksWithFilter, checkViewContainer, Configuration) as ViewOnChecksWithFilterHandler;
            _checksViewHandler.ShowTagEditorView = false;
            _checksViewHandler.Reveal();

            var rowOfButtonsToSelectOrUnselectAllChecks = checkViewContainer.Children.AddAndReference(new StackPanel { Orientation = Orientation.Horizontal });
            Grid.SetRow(rowOfButtonsToSelectOrUnselectAllChecks, 1);
            Grid.SetColumn(rowOfButtonsToSelectOrUnselectAllChecks, 0);
            var selectAllFilteredChecks = rowOfButtonsToSelectOrUnselectAllChecks.Children.AddAndReference(UITheme.CreateButton("Select all"));
            selectAllFilteredChecks.Click += ReactionToClickingOnSelectAllFilteredChecks;
            var unselectAllFilteredChecks = rowOfButtonsToSelectOrUnselectAllChecks.Children.AddAndReference(UITheme.CreateButton("Clear selection"));
            unselectAllFilteredChecks.Click += ReactionToClickingOnUnselectFilteredChecks;

            #region Right side with the view on enlistment.

            var containerOfEnlistmentView = ViewContainer.AddGridItem(0, UITheme.CreateGrid());
            Grid.SetColumn(containerOfEnlistmentView, 1);
            _enlistmentViewHandler = ViewFactory.CreateView(ViewType.ViewOnEnlistment, containerOfEnlistmentView, Configuration) as ViewOnEnlistmentHandler;
            _enlistmentViewHandler.ShowTagEditorView = false;
            _enlistmentViewHandler.Reveal();

            #endregion

            #region Final 'Apply' button. It has to be in a panel or else it will stretch across the entire grid's column.
            var panelForApplyButton = ViewContainer.AddGridItem(1, new StackPanel { Orientation = Orientation.Horizontal });
            var applyButton = panelForApplyButton.Children.AddAndReference(UITheme.CreateButton("Apply to enlistment selection"));
            applyButton.Click += ReactionToApplyButtonClick;
            #endregion

        }

        /// <summary>
        /// Implement this method to respond to data changes in the <see cref="ViewHandler.Configuration"/>.
        /// Whatever events update your data, in response to to such events, your code should call into this method
        /// to ensure that the view reflects what's in data tables.
        /// <para/>For example, if the user adds a new tag on a view item, call this method. Implementation of this method
        /// should reread relevant data entries and update UI elements.
        /// </summary>
        public override void UpdateViewToMatchData()
        {
            _checksViewHandler.UpdateViewToMatchData();
            _enlistmentViewHandler.UpdateViewToMatchData();
        }

        /// <summary>
        /// Reacts to user clicking on 'Apply to enlistment selection' button.
        /// This should create an association between each of the selected checks and each of the langugaes or projects that are in the enlistment.
        /// </summary>
        void ReactionToApplyButtonClick(object sender, RoutedEventArgs e)
        {
            var checkItems =  _rootChecksNode.Items.Cast<TreeViewItem>();
            var anyDataModified = false; //Performance optimization. If no changes are made to data source, skip the call to expensive method: UpdateViewToMatchData().
            foreach (var checkItem in checkItems)
            {
                var checkPanel = checkItem.Header as StackPanel;
                var checkEnablementCheckBox = checkPanel.Children.ToList<CheckBox>().First();
                if (!checkEnablementCheckBox.IsChecked.Value) continue;
                var enabledCheckName = checkPanel.Children.ToList<Label>().First().Content.ToString();
                var rootUnderWhichLanguagesArePresent = _enlistementTree.Items.ToList<TreeViewItem>().First();
                var languages = rootUnderWhichLanguagesArePresent.Items.ToList<TreeViewItem>();

                #region Here we enable the check on selected languages
                foreach (var languageTreeViewItem in languages)
                {
                    var lHeader = languageTreeViewItem.Header as StackPanel;
                    if (!lHeader.Children.ToList<CheckBox>().First().IsChecked.Value) continue;

                    EnableCheckOnLanguage(enabledCheckName, lHeader.Children.ToList<Label>().First().Content.ToString());
                    anyDataModified = true;
                }

                #endregion

                #region Here we enable the check on selected projects, we skip duplicates by using Distinct() call.
                var distinctProjects = languages.SelectMany(langItem => langItem.Items.ToList<TreeViewItem>()).Distinct(new CustomProjectTreeViewItemComparer());

                foreach (var projectTreeViewItem in distinctProjects)
                {
                    var pHeader = (StackPanel)projectTreeViewItem.Header;
                    if (!pHeader.Children.ToList<CheckBox>().First().IsChecked.Value) continue;
                    EnableCheckOnProject(enabledCheckName, pHeader.Children.ToList<Label>().First().Content.ToString());
                    anyDataModified = true;
                }
                #endregion
            }
            if (anyDataModified) UpdateViewToMatchData();
        }
        
        /// <summary>
        /// When a button is clicked, this method enables the specified check on the specified language.
        /// </summary>
        private void EnableCheckOnLanguage(string checkName, string langugage)
        {
            Configuration.EnableCheckForLanguage(checkName, langugage);
        }

        /// <summary>
        /// When a button is clicked, this method enables the specified check on the specified language.
        /// </summary>
        private void EnableCheckOnProject(string checkName, string project)
        {
            Configuration.EnableCheckForProject(checkName, project);
        }

        /// <summary>
        /// This method reacts to clicking on 'Select all' button that should select all checks currently visible in the view.
        /// </summary>
        void ReactionToClickingOnSelectAllFilteredChecks(object sender, RoutedEventArgs e)
        {
            SetCheckEnabledState(true);
        }

        /// <summary>
        /// This method reacts to clicking on 'Clear selection' button that should unselect all checks currently visible in the view.
        /// </summary>
        void ReactionToClickingOnUnselectFilteredChecks(object sender, RoutedEventArgs e)
        {
            SetCheckEnabledState(false);
        }

        /// <summary>
        /// Checks or unchecks all checks currently visible dependent on <paramref name="isChecked"/> state.
        /// </summary>
        /// <param name="isChecked">If true, all visible checks will be enabled (checked). Otherwise, all visible checks will be disabled (unchecked).</param>
        private void SetCheckEnabledState(bool isChecked)
        {
            var checkItems = _rootChecksNode.Items.Cast<TreeViewItem>();
            foreach (var checkItem in checkItems)
            {
                var checkPanel = checkItem.Header as StackPanel;
                var checkEnablementCheckBox = checkPanel.Children.ToList<CheckBox>().First();
                checkEnablementCheckBox.IsChecked = isChecked;
            }
        }

    }
}