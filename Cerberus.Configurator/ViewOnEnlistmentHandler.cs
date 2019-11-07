using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Localization.LocSolutions.Cerberus.Core;
using Microsoft.Localization.LocSolutions.Cerberus.Resources;

namespace Microsoft.Localization.LocSolutions.Cerberus.Configurator
{
    /// <summary>
    /// Specific view on data that Cerberus supports. Shows Languages as root items, under them shows projects.
    /// </summary>
    internal class ViewOnEnlistmentHandler : EnlistmentAbstractViewHandler
    {

        public ViewOnEnlistmentHandler(Grid viewContainer, CheckConfiguration configuration) : base(viewContainer, configuration)
        {
            ShowTagEditorView = true; //Set the default behavior
        }

        /// <summary>
        /// This grid is the only child of the <see cref="ViewHandler.ViewContainer"/>.
        /// It has 1 column (implicit, default).
        /// It has 2 rows.
        /// The first row contains the tree view and takes all the height remaining after laying out the other controls in the second row.
        /// The second row contains the TagEditor view and takes any required height to display its contents.
        /// </summary>
        private Grid _gridContainer;

        /// <summary>
        /// A flag that indicates whether the tag management piece should be added to this view.
        /// Note: Setting this value has only effect before a call to <see cref="Reveal"/>. So set this flag, if needed, after constructing this object
        /// and before calling <see cref="Reveal"/>.
        /// Default value: true.
        /// </summary>
        internal bool ShowTagEditorView { get; set; }

        /// <summary>
        /// Describes the view to the user.
        /// </summary>
        public override string ViewDescription
        {
            get
            {
                return "Use this view to browse through the structure of enlistment and assign tags to enlistment elements.  If you want to work with project tags, you might want to switch to 'View on projects'.  Tags can be used to logically group enlistment elements (such as groups of languages or groups of project).  Once you are done with tagging, you can switch to 'Assignment of checks to enlistment items' to actually assign (or enable) checks to groups of enlistment elements.";
            }
        }

        /// <summary>
        /// Adds controls comprising the current view to the container of the view.
        /// The view will be visible immediately.
        /// </summary>
        internal override void Reveal()
        {
            _gridContainer = ViewContainer.Children.AddAndReference(UITheme.CreateGrid());
            
            #region Define as many rows as there are logical row items in your view
            _gridContainer.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            _gridContainer.RowDefinitions.Add(new RowDefinition { Height = new GridLength(0, GridUnitType.Auto) });
            _gridContainer.RowDefinitions.Add(new RowDefinition { Height = new GridLength(0, GridUnitType.Auto) });
            _gridContainer.RowDefinitions.Add(new RowDefinition { Height = new GridLength(0, GridUnitType.Auto) });
            #endregion


            _treeView = _gridContainer.AddGridItem(0, new TreeView());


            #region Macro buttons

            var macroContainer = _gridContainer.AddGridItem(1, new StackPanel { Orientation = Orientation.Horizontal });
            macroContainer.Children.AddAndReference(UITheme.CreateButton("Select languages")).Click += delegate { ToggleLanguages(true); };
            macroContainer.Children.AddAndReference(UITheme.CreateButton("Select projects")).Click += delegate { ToggleProjects(true); };
            macroContainer.Children.AddAndReference(UITheme.CreateButton("Unselect languages")).Click += delegate { ToggleLanguages(false); };

            macroContainer.Children.AddAndReference(UITheme.CreateButton("Unselect projects")).Click += delegate { ToggleProjects(false); };
            macroContainer.Children.AddAndReference(UITheme.CreateButton("Unselect all")).Click += delegate { ToggleLanguages(false); ToggleProjects(false); };

            #endregion

            FillTreeWithEnlistmentData(_treeView);

            #region Tag editor view
            if (ShowTagEditorView)
            {
                var tagEditorViewContainer = _gridContainer.AddGridItem(2, UITheme.CreateGrid());

                _tagEditorViewHandler = new TagEditorViewHandler(tagEditorViewContainer, Configuration);
                _tagEditorViewHandler.ApplyTagToSelectionEvent += ApplyTagToSelection;
                _tagEditorViewHandler.Reveal();
            }
            #endregion

            #region Filter
            var enlistmentFilterPanel = _gridContainer.AddGridItem(3, new StackPanel { Orientation = Orientation.Horizontal });
            enlistmentFilterPanel.Children.Add(new Label { Content = "Filter:" });
            _FilterTextBox = enlistmentFilterPanel.Children.AddAndReference(new TextBox { MinWidth = 120, MaxWidth = 120 });
            _FilterTextBox.TextChanged += delegate { UpdateViewToMatchData(); };
            enlistmentFilterPanel.Children.AddAndReference(UITheme.CreateButton("Clear filter")).Click += delegate { _FilterTextBox.Clear(); };

            #endregion

        }

        /// <summary>
        /// Sets checkboxes on all languages in the view to the specified value.
        /// </summary>
        /// <param name="enable">Intended checkbox status. If true, checkboxes will be checked. If false, checkboxes will be unchecked.</param>
        private void ToggleLanguages(bool enable)
        {
            var languagePanels = _rootNode.Items.ToList<TreeViewItem>().Select(item => item.Header as StackPanel);
            var checkboxes = languagePanels.Select(panel => panel.Children.ToList<CheckBox>().First());
            checkboxes.ToList().ForEach(cb => cb.IsChecked = enable);
        }

        /// <summary>
        /// Sets checkboxes on all projects under all languages in the view to the specified value.
        /// </summary>
        /// <param name="enable">Intended checkbox status. If true, checkboxes will be checked. If false, checkboxes will be unchecked.</param>
        private void ToggleProjects(bool enable)
        {
            var projectPanels = _rootNode.Items.ToList<TreeViewItem>().SelectMany(lang => lang.Items.ToList<TreeViewItem>()).Select(item => item.Header as StackPanel);
            var checkboxes = projectPanels.Select(panel => panel.Children.ToList<CheckBox>().First());
            checkboxes.ToList().ForEach(cb => cb.IsChecked = enable);
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
            UpdateLanguages(_rootNode, _FilterTextBox.Text);
        }

        /// <summary>
        /// Populates the tree with tree view items that reflect the desired data items from the enlistment database.
        /// This view implementation looks at languages and projects beneath each language.
        /// </summary>
        private void FillTreeWithEnlistmentData(ItemsControl view)
        {
            #region Root node first

            var itemPanel = new StackPanel {Orientation = Orientation.Horizontal};
            itemPanel.Children.Add(ResourceHelper.GetImage(IconType.Database, 16, 16));
            itemPanel.Children.Add(new Label {Content = "Enlistment", Name = "Root"});
            _rootNode = new TreeViewItem {Header = itemPanel, IsExpanded = true};
            view.Items.Add(_rootNode);

            #endregion

            UpdateLanguages(_rootNode, string.Empty);
        }

        /// <summary>
        /// Adds tree view items for each language, if the item does not exist yet.
        /// If the item does exist, it only updates its contents to match the database.
        /// </summary>
        /// <param name="parentNode">Node under which languages are to be added or updated.</param>
        /// <param name="filterText">Filter string, entered in a text box. When empty (or only whitespaces), no filtering is applied, otherwise, each keyword (separated by whitespaces) is used to filter on anything about a check or language.</param>
        private void UpdateLanguages(ItemsControl parentNode, string filterText)
        {
            _treeView.BeginInit();
            try
            {
                foreach (var language in Configuration.GetAllLanguages())
                {
                    #region Enable filter

                    if (!LanguageMatchesFilter(language, filterText))
                    {
                        //If the check doesn't match the filter, remove if from the list if it was added.
                        var existingUI = parentNode.Items.ToList<TreeViewItem>()
                            .Where(child => (child.Header as StackPanel).Children.ToList<Label>().First().Content.ToString().Equals(language));
                        if (existingUI.Count() == 0) continue;
                        parentNode.Items.Remove(existingUI.First());
                        continue;
                    }

                    #endregion

                    var languagePanel = new StackPanel { Orientation = Orientation.Horizontal };
                    languagePanel.Children.Add(ResourceHelper.GetImage(IconType.EarthGlobe, 16, 16));
                    languagePanel.Children.Add(new CheckBox {Name = "LanguageCheckBox"});
                    languagePanel.Children.Add(new Label {Content = language, Name = "LanguageName"});

                    AddOptionalTagIconWithTooltip(languagePanel, Configuration.GetLanguageTags(language));

                    #region Enable removal of tags when the user selects context menu

                    var languageTagRemoval = languagePanel.Children.ToList<Image>()
                        .Where(i => !ReferenceEquals(null, i.Tag))
                        .Where(i => i.Tag.ToString() == "RemoveTag");
                    if (languageTagRemoval.Count() == 1)
                    {
                        var removalIcon = languageTagRemoval.First();
                        var menuItems = removalIcon.ContextMenu;

                        foreach (var item in menuItems.Items.ToList<MenuItem>())
                        {
                            var currentItem = item;
                            var currentLanguage = language;
                            RoutedEventHandler mbeh = delegate { Configuration.RemoveTagFomLanguages(currentItem.Tag.ToString(), new[] {currentLanguage}); UpdateViewToMatchData(); };
                            item.Click += mbeh;
                        }
                    }

                    #endregion


                    //See if there already is a UI item for this language
                    var existingPanels = parentNode.Items.ToList<TreeViewItem>()
                        .Where(child => (child.Header as StackPanel).Children.ToList<Label>().First().Content.ToString().Equals(language));

                    TreeViewItem languageNode = null;
                    if (existingPanels.Count() == 0)
                    {
                        languageNode = new TreeViewItem {Header = languagePanel};
                        parentNode.Items.Add(languageNode);
                    }
                    else
                    {
                        //Replace existing panel with the new panel.
                        languageNode = existingPanels.First();
                        var oldPanel = languageNode.Header as StackPanel;
                        languageNode.Header = languagePanel;
                        //Update language check box status
                        languagePanel.Children.ToList<CheckBox>().First().IsChecked = oldPanel.Children.ToList<CheckBox>().First().IsChecked;
                    }
                    UpdateProjects(languageNode, filterText);
                }
            }
            finally
            {
                _treeView.EndInit();
            }
        }

        /// <summary>
        /// Adds tree view items for each project, if the item does not exist yet.
        /// If the item does exist, it only updates its contents to match the database.
        /// </summary>
        /// <param name="parentNode">Node (of the parent language) under which projects are to be added or updated.</param>
        /// <param name="filterText">Filter string, entered in a text box. When empty (or only whitespaces), no filtering is applied, otherwise, each keyword (separated by whitespaces) is used to filter on anything about a project.</param>
        private void UpdateProjects(ItemsControl parentNode, string filterText)
        {
            foreach (var project in Configuration.GetAllProjects())
            {
                #region Enable filter

                if (!ProjectMatchesFilter(project, filterText))
                {
                    //If the check doesn't match the filter, remove if from the list if it was added.
                    var existingUI = parentNode.Items.ToList<TreeViewItem>()
                        .Where(child => (child.Header as StackPanel).Children.ToList<Label>().First().Content.ToString().Equals(project));
                    if (existingUI.Count() == 0) continue;
                    parentNode.Items.Remove(existingUI.First());
                    continue;
                }

                #endregion

                var projectPanel = new StackPanel { Orientation = Orientation.Horizontal };
                projectPanel.Children.Add(ResourceHelper.GetImage(IconType.FolderPale, 16, 16));
                projectPanel.Children.Add(new CheckBox { Name = "ProjectCheckBox" });
                projectPanel.Children.Add(new Label { Content = project, Name = "ProjectName" });

                AddOptionalTagIconWithTooltip(projectPanel, Configuration.GetProjectTags(project));

                #region Enable removal of tags when the user selects context menu

                var projectTagRemoval = projectPanel.Children.ToList<Image>()
                    .Where(i => !ReferenceEquals(null, i.Tag))
                    .Where(i => i.Tag.ToString() == "RemoveTag");
                if (projectTagRemoval.Count() == 1)
                {
                    var removalIcon = projectTagRemoval.First();
                    var menuItems = removalIcon.ContextMenu;

                    foreach (var item in menuItems.Items.ToList<MenuItem>())
                    {
                        var currentItem = item;
                        var currentProject = project;
                        RoutedEventHandler mbeh = delegate { Configuration.RemoveTagFomProjects(currentItem.Tag.ToString(), new[] { currentProject }); UpdateViewToMatchData(); };
                        item.Click += mbeh;
                    }
                }

                #endregion

                //See if there already is a UI item for this language
                var existingPanels = parentNode.Items.ToList<TreeViewItem>().Where(child =>
                        (child.Header as StackPanel).Children.ToList<Label>().First().Content.ToString().Equals(project));

                TreeViewItem projectNode = null;
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
                    //Update language check box status
                    projectPanel.Children.ToList<CheckBox>().First().IsChecked = oldPanel.Children.ToList<CheckBox>().First().IsChecked;
                }
            }
        }

        /// <summary>
        /// Applies the currently entered (existing or new) tag to the selection, that is, to these languages and projects
        /// that have checkboxes selected.
        /// </summary>
        /// <param name="tag">New or existing tag to be applied to selection.</param>
        void ApplyTagToSelection(string tag)
        {
            var languages = from languageItem in _rootNode.Items.ToList<TreeViewItem>()
                            let languagePanel = languageItem.Header as StackPanel
                            where languagePanel.Children.ToList<CheckBox>().Any(cb => cb.IsChecked.Value)
                            let languageNameLabel = languagePanel.Children.ToList<Label>().First()
                            select languageNameLabel.Content.ToString();
            Configuration.ApplyTagToLanguages(tag, languages);

            var projects = from languageItem in _rootNode.Items.ToList<TreeViewItem>()
                           let projectList = languageItem.Items.ToList<TreeViewItem>()
                           from projectItem in projectList
                           let projectPanel = projectItem.Header as StackPanel
                           where projectPanel.Children.ToList<CheckBox>().Any(cb => cb.IsChecked.Value)
                           let projectNameLabel = projectPanel.Children.ToList<Label>().First()
                           select projectNameLabel.Content.ToString();
            Configuration.ApplyTagToProjects(tag, projects);
            UpdateViewToMatchData();
        }

    }
}