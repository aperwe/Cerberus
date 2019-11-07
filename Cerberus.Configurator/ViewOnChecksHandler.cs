using System.Linq;
using System.Windows.Controls;
using Microsoft.Localization.LocSolutions.Cerberus.Core;
using Microsoft.Localization.LocSolutions.Cerberus.Resources;
using System.Windows;

namespace Microsoft.Localization.LocSolutions.Cerberus.Configurator
{
    /// <summary>
    /// Specific view on data that Cerberus supports. Shows Checks as root items.
    /// </summary>
    class ViewOnChecksHandler : ViewHandler
    {
        public ViewOnChecksHandler(Grid viewContainer, CheckConfiguration configuration) : base(viewContainer, configuration)
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
        /// Tree view created by this handler. This tree view is a child of the view container.
        /// </summary>
        private TreeView _treeView;

        private TagEditorViewHandler _tagEditorViewHandler;

        /// <summary>
        /// Node in checks view under which all checks (possibly filtered) are displayed.
        /// </summary>
        protected TreeViewItem _rootChecksNode;

        /// <summary>
        /// Reference of the tree view contained by this <see cref="ViewOnChecksHandler"/> handler. It contains the list of all checks under a root.
        /// </summary>
        internal TreeView ChecksTreeView { get { return _treeView; } }

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
                return "Use this view to see the list of all checks known to Cerberus and assign tags to checks.  Tags can be used to logically group checks. Once you are done with tagging, you can switch to 'Assignment of checks to enlistment items' to actually assign (or enable) checks to groups of enlistment elements.";
            }
        }

        /// <summary>
        /// Adds controls comprising the current view to the container of the view.
        /// The view will be visible immediately.
        /// </summary>
        internal override void Reveal()
        {
            _gridContainer = ViewContainer.Children.AddAndReference(UITheme.CreateGrid());
            Grid.SetRow(_gridContainer, 0); //In case we are contained in a grid which has more rows and columns
            Grid.SetColumn(_gridContainer, 0);

            #region Define as many rows as there are logical row items in your view
            _gridContainer.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            _gridContainer.RowDefinitions.Add(new RowDefinition { Height = new GridLength(0, GridUnitType.Auto) });
            #endregion


            _treeView = _gridContainer.AddGridItem(0, new TreeView());

            FillTreeWithEnlistmentData();


            #region Tag editor view
            if (ShowTagEditorView)
            {
                var tagEditorViewContainer = _gridContainer.AddGridItem(1, UITheme.CreateGrid());

                _tagEditorViewHandler = ViewFactory.CreateView(ViewType.TagEditorView, tagEditorViewContainer, Configuration) as TagEditorViewHandler;
                _tagEditorViewHandler.ApplyTagToSelectionEvent += ApplyTagToSelection;
                _tagEditorViewHandler.Reveal();
            }
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
            UpdateChecks(_rootChecksNode);
        }

        /// <summary>
        /// Populates the tree with tree view items that reflect the desired data items from the enlistment database.
        /// This view implementation looks at checks.
        /// </summary>
        private void FillTreeWithEnlistmentData()
        {
            #region Root node first

            var itemPanel = new StackPanel {Orientation = Orientation.Horizontal};
            itemPanel.Children.Add(ResourceHelper.GetImage(IconType.Database, 16, 16));
            itemPanel.Children.Add(new Label {Content = "Checks", Name = "Root"});
            _rootChecksNode = new TreeViewItem {Header = itemPanel, IsExpanded = true};
            _treeView.Items.Add(_rootChecksNode);

            #endregion

            UpdateChecks(_rootChecksNode);
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
                    var checkPanel = new StackPanel {Orientation = Orientation.Horizontal};
                    checkPanel.Children.Add(ResourceHelper.GetImage(IconType.Check, 16, 16));
                    checkPanel.Children.Add(new CheckBox {Name = "CheckCheckBox"});
                    checkPanel.Children.Add(new Label {Content = check, Name = "CheckName"});

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
                        checkNode = new TreeViewItem {Header = checkPanel};
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
                _treeView.EndInit();
            }
        }

        /// <summary>
        /// Applies the currently entered (existing or new) tag to the selection, that is, to these checks
        /// that have checkboxes selected.
        /// </summary>
        /// <param name="tag">New or existing tag to be applied to selection.</param>
        void ApplyTagToSelection(string tag)
        {
            var checks = from checkItem in _rootChecksNode.Items.ToList<TreeViewItem>()
                            let checkPanel = checkItem.Header as StackPanel
                            where checkPanel.Children.ToList<CheckBox>().Any(cb => cb.IsChecked.Value)
                            let checkNameLabel = checkPanel.Children.ToList<Label>().First()
                            select checkNameLabel.Content.ToString();
            Configuration.ApplyTagToChecks(tag, checks);
            UpdateViewToMatchData();
        }
    }
}