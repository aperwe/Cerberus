using System.Linq;
using System.Windows.Controls;
using Microsoft.Localization.LocSolutions.Cerberus.Core;
using Microsoft.Localization.LocSolutions.Cerberus.Resources;
using System.Windows;

namespace Microsoft.Localization.LocSolutions.Cerberus.Configurator
{
    /// <summary>
    /// This view shows to the user what enlistment items each check is configured on.
    /// Check is the data selector, and languages and projects on which the selected check is enabled, are shown in secondary view (list or tree).
    /// </summary>
    class ConfigurationSummaryViewHandler : ViewHandler
    {
        public ConfigurationSummaryViewHandler(Grid viewContainer, CheckConfiguration configuration) : base(viewContainer, configuration)
        {
        }

        private TreeView _treeView;

        /// <summary>
        /// The root node, under which checks are placed with their child language.
        /// </summary>
        private TreeViewItem _rootNode;

        /// <summary>
        /// This is a view description.
        /// </summary>
        public override string ViewDescription
        {
            get
            {
                return "Use this view to see — for each check — which enlistment items the check has been enabled on.";
            }
        }

        /// <summary>
        /// Adds controls comprising the current view to the container of the view.
        /// The view will be visible immediately.
        /// </summary>
        internal override void Reveal()
        {
            ViewContainer.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            _treeView = ViewContainer.AddGridItem(0, new TreeView());

            FillTreeWithEnlistmentData();
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
            UpdateChecks(_rootNode);
        }

        /// <summary>
        /// Populates the tree with tree view items that reflect the desired data items from the enlistment database.
        /// This view implementation looks at checks.
        /// </summary>
        private void FillTreeWithEnlistmentData()
        {
            #region Root node first

            var itemPanel = new StackPanel { Orientation = Orientation.Horizontal };
            itemPanel.Children.Add(ResourceHelper.GetImage(IconType.Database, 16, 16));
            itemPanel.Children.Add(new Label { Content = "Check enablement", Name = "Root" });
            _rootNode = new TreeViewItem { Header = itemPanel, IsExpanded = true };
            _treeView.Items.Add(_rootNode);

            #endregion

            UpdateChecks(_rootNode);
        }

        /// <summary>
        /// Adds tree view items for each check.
        /// </summary>
        /// <param name="parentNode">Node under which checks are to be added.</param>
        private void UpdateChecks(ItemsControl parentNode)
        {
            _treeView.BeginInit();
            try
            {
                foreach (var check in Configuration.GetAllChecks())
                {
                    var checkPanel = new StackPanel { Orientation = Orientation.Horizontal };
                    checkPanel.Children.Add(ResourceHelper.GetImage(IconType.Check, 16, 16));
                    checkPanel.Children.Add(new CheckBox { Name = "CheckCheckBox" });
                    checkPanel.Children.Add(new Label { Content = check, Name = "CheckName" });

                    AddOptionalTagIconWithTooltip(checkPanel, Configuration.GetCheckTags(check));

                    //See if there already is a UI item for this check
                    var existingPanels = parentNode.Items.ToList<TreeViewItem>()
                        .Where(child => (child.Header as StackPanel).Children.ToList<Label>().First().Content.ToString().Equals(check));

                    TreeViewItem checkNode;
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
                        checkPanel.Children.ToList<CheckBox>().First().IsChecked =
                            oldPanel.Children.ToList<CheckBox>().First().IsChecked;
                    }

                    AddCheckEnablementInfo(checkNode, check);

                }
            }
            finally
            {
                _treeView.EndInit();
            }
        }

        /// <summary>
        /// Examines the database to see if there are any languages, for which this check is enabled.
        /// If so, it adds the language node as child to the specified <paramref name="parentNode"/>.
        /// </summary>
        /// <param name="parentNode">Parent node (of a check), under which to locate information about languages where this check is enabled.</param>
        /// <param name="check">Name of the check.</param>
        private void AddCheckEnablementInfo(ItemsControl parentNode, string check)
        {
            foreach (var language in Configuration.GetLanguagesWithCheckEnabled(check))
            {
                var languageItem = UpdateOrCreateLanguageChildItem(language, parentNode, true);
                var languagePanel = languageItem.Header as StackPanel;
                var icon = languagePanel.Children.AddAndReference(ResourceHelper.GetImage(IconType.Search, 16, 16));
                icon.ToolTip = string.Format("Enabled checks: {0}", string.Join(", ", Configuration.GetChecksEnabledForLanguage(language).ToArray()));
                var removeIcon = languagePanel.Children.AddAndReference(ResourceHelper.GetImage(IconType.EditRemove, 16, 16));
                removeIcon.ToolTip = string.Format("Click here to turn off this {0} check for {1} language", check, language);
                removeIcon.MouseLeftButtonUp += delegate
                                                    {
                                                        //If this icon is clicked, remove the association from the database and remove the language from view.
                                                        Configuration.DisableCheckForLanguage(check, language);
                                                        parentNode.Items.Remove(languageItem);
                                                    };
            }
            AddCheckEnablementInfoForProjects(parentNode, check);
        }

        /// <summary>
        /// For the specified language, makes sure that a tree view item under the specified parent node exists.
        /// If the child tree view item doesn't exist, this method creates one.
        /// If the child tree view item already exists, it updates its header with up-to-date information from the database.
        /// </summary>
        /// <param name="language">Language, for which you want to ensure there is an up-to-date tree view item that is child of the specified <paramref name="parentNode"/>.</param>
        /// <param name="parentNode">Parent node (of a check), under which to create or update a tree view item specific to the given language.</param>
        /// <param name="replaceExistingPanel">Indicates whether the language panel should be replaced if one already exists. When adding check enablement info for project items, this parameter should be set to false to prevent removal of any additional controls (icons) on the language panel.</param>
        /// <returns>Reference to the already-existing, or newly created, tree view item with information about the specified language.</returns>
        private TreeViewItem UpdateOrCreateLanguageChildItem(string language, ItemsControl parentNode, bool replaceExistingPanel)
        {
            var languagePanel = new StackPanel {Orientation = Orientation.Horizontal};
            languagePanel.Children.Add(ResourceHelper.GetImage(IconType.EarthGlobe, 16, 16));
            languagePanel.Children.Add(new CheckBox {Name = "LanguageCheckBox"});
            languagePanel.Children.Add(new Label {Content = language, Name = "LanguageName"});

            AddOptionalTagIconWithTooltip(languagePanel, Configuration.GetLanguageTags(language));

            //See if there already is a UI item for this language
            var existingPanels = parentNode.Items.ToList<TreeViewItem>()
                .Where(child => (child.Header as StackPanel).Children.ToList<Label>().First().Content.ToString().Equals(language));

            TreeViewItem languageNode;
            if (existingPanels.Count() == 0)
            {
                languageNode = new TreeViewItem {Header = languagePanel};
                parentNode.Items.Add(languageNode);
            }
            else
            {
                //Replace existing panel with the new panel.
                languageNode = existingPanels.First();
                if (replaceExistingPanel)
                {
                    var oldPanel = languageNode.Header as StackPanel;
                    languageNode.Header = languagePanel;
                    //Update check check box status
                    languagePanel.Children.ToList<CheckBox>().First().IsChecked = oldPanel.Children.ToList<CheckBox>().First().IsChecked;
                }
            }
            return languageNode;
        }

        /// <summary>
        /// For the specified project, makes sure that a tree view item under the specified parent node exists.
        /// If the child tree view item doesn't exist, this method creates one.
        /// If the child tree view item already exists, it updates its header with up-to-date information from the database.
        /// </summary>
        /// <param name="project">Language, for which you want to ensure there is an up-to-date tree view item that is child of the specified <paramref name="parentNode"/>.</param>
        /// <param name="parentNode">Parent node (of a language), under which to create or update a tree view item specific to the given project.</param>
        /// <returns>Reference to the already-existing, or newly created, tree view item with information about the specified project.</returns>
        private TreeViewItem UpdateOrCreateProjectChildItem(string project, ItemsControl parentNode)
        {
            var projectPanel = new StackPanel { Orientation = Orientation.Horizontal };
            projectPanel.Children.Add(ResourceHelper.GetImage(IconType.FolderPale, 16, 16));
            projectPanel.Children.Add(new CheckBox { Name = "ProjectCheckBox" });
            projectPanel.Children.Add(new Label { Content = project, Name = "ProjectName" });

            AddOptionalTagIconWithTooltip(projectPanel, Configuration.GetProjectTags(project));

            //See if there already is a UI item for this project
            var existingPanels = parentNode.Items.ToList<TreeViewItem>()
                .Where(child => (child.Header as StackPanel).Children.ToList<Label>().First().Content.ToString().Equals(project));

            TreeViewItem projectNode;
            if (existingPanels.Count() == 0)
            {
                projectNode = new TreeViewItem { Header = projectPanel };
                parentNode.Items.Add(projectNode);
            }
            else
            {
                //Replace existing panel with the new panel.
                projectNode = existingPanels.First();
                var oldPanel = projectNode.Header as StackPanel;
                projectNode.Header = projectPanel;
                //Update check check box status
                projectPanel.Children.ToList<CheckBox>().First().IsChecked = oldPanel.Children.ToList<CheckBox>().First().IsChecked;
            }
            return projectNode;
        }

        /// <summary>
        /// Examines the database to see if there are any projects, for which this check is enabled.
        /// If so, it makes sure a language node is added as child to the specified <paramref name="parentNode"/>,
        /// and then it adds a project node as a child to that language node.
        /// </summary>
        /// <param name="parentNode">Parent node (of a check), under which to locate information about projects (and their parent languages) where this check is enabled.</param>
        /// <param name="check">Name of the check.</param>
        private void AddCheckEnablementInfoForProjects(ItemsControl parentNode, string check)
        {
            foreach (var language in Configuration.GetAllLanguages())
            {
                foreach (var project in Configuration.GetProjectsWithCheckEnabled(check))
                {
                    var languageItem = UpdateOrCreateLanguageChildItem(language, parentNode, false);
                    var projectItem = UpdateOrCreateProjectChildItem(project, languageItem);
                    var projectPanel = projectItem.Header as StackPanel;
                    var icon = projectPanel.Children.AddAndReference(ResourceHelper.GetImage(IconType.Search, 16, 16));
                    icon.ToolTip = string.Format("Enabled checks: {0}", string.Join(", ", Configuration.GetChecksEnabledForProject(project).ToArray()));
                }
            }
        }
    }
}