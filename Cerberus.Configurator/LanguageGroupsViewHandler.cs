using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Localization.LocSolutions.Cerberus.Core;
using Microsoft.Localization.LocSolutions.Cerberus.Resources;
using System.Collections.Generic;

namespace Microsoft.Localization.LocSolutions.Cerberus.Configurator
{
    /// <summary>
    /// Specific view on data that Cerberus supports. Shows Languages as root items, under them shows projects.
    /// </summary>
    internal class LanguageGroupsViewHandler : ViewHandler
    {

        public LanguageGroupsViewHandler(Grid viewContainer, CheckConfiguration configuration) : base(viewContainer, configuration)
        {
        }

        /// <summary>
        /// List view presented on the left side, containing defined groups of languages.
        /// </summary>
        public ListView LanguageGroupListView { get; set; }

        public Grid LeftSideView { get; set; }

        public Grid RightSideView { get; set; }

        public ListView LanguagesList { get; set; }

        public CheckBox CheckBoxAllLanguages { get; set; }

        public Grid TAGVIEWCONTAINER { get; set; }

        public TagEditorViewHandler TAGEDITORVIEW { get; set; }

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
        /// Called by the container (calling code) in order to show the view to the user.
        /// The base implementation of this method contains code to initialize common elements of each view
        /// and then it calls view-specific control construction.
        /// </summary>
        public override void Show()
        {
            ViewContainer.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); //Left view (language groups)
            ViewContainer.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) }); //Right view (languages)
            ViewContainer.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            base.Show();
        }
        /// <summary>
        /// Adds controls comprising the current view to the container of the view.
        /// The view will be visible immediately.
        /// </summary>
        internal override void Reveal()
        {
            LeftSideView = ViewContainer.AddGridItem(0, 0, UITheme.CreateGrid());

            LeftSideView.RowDefinitions.Add(new RowDefinition { Height = new GridLength(0, GridUnitType.Auto) }); //Language groups label
            LeftSideView.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }); //Language groups control
            LeftSideView.RowDefinitions.Add(new RowDefinition { Height = new GridLength(0, GridUnitType.Auto) }); //Language groups label
            LeftSideView.AddGridItem(0, new Label { Content = "Select a language group to configure" });
            LanguageGroupListView = LeftSideView.AddGridItem(1, new ListView());
            LanguageGroupListView.SelectionChanged += LanguageGroupListSelectionChanged;
            TAGVIEWCONTAINER = LeftSideView.AddGridItem(2, 0, UITheme.CreateGrid());
            TAGEDITORVIEW = (TagEditorViewHandler)ViewFactory.CreateView(ViewType.TagEditorView, TAGVIEWCONTAINER, Configuration);
            TAGEDITORVIEW.Reveal();
            TAGEDITORVIEW.SaveTagEvent += savedTag => UpdateViewToMatchData();

            RightSideView = ViewContainer.AddGridItem(0, 1, UITheme.CreateGrid());
            IsRightSideEnabled = false;

            RightSideView.RowDefinitions.Add(new RowDefinition { Height = new GridLength(0, GridUnitType.Auto) }); //Languages label
            RightSideView.RowDefinitions.Add(new RowDefinition { Height = new GridLength(2, GridUnitType.Star) }); //Languages control

            AddLanguagesHeader(RightSideView, 0);
            LanguagesList = RightSideView.AddGridItem(1, new ListView());
            AddContent(LanguagesList, Configuration.GetAllLanguages(), ContextType.Language);

            FillWithConfigurationData();

        }

        /// <summary>
        /// Embeds the header over languages control (tree view) in the specified row.
        /// </summary>
        /// <param name="container">Container grid that will contain the header.</param>
        /// <param name="containerRow">Row in the parent container, where the header should be placed.</param>
        private void AddLanguagesHeader(Grid container, int containerRow)
        {
            var x = container.AddGridItem(containerRow, UITheme.CreateGrid());
            x.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0, GridUnitType.Auto) });
            x.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            var label = x.Children.AddAndReference(new Label { Content = "Languages belonging to the selected language group" });
            Grid.SetColumn(label, 0);

            CheckBoxAllLanguages = x.Children.AddAndReference(new CheckBox { Content = "All", IsThreeState = true, VerticalAlignment = VerticalAlignment.Center });
            Grid.SetColumn(CheckBoxAllLanguages, 1);
            CheckBoxAllLanguages.Click += CheckBoxAllLanguagesClicked;
        }

        /// <summary>
        /// Fills the specified list control with languages from configuration database.
        /// </summary>
        private void AddContent(ListView listView, IEnumerable<string> collection, ContextType elementType)
        {
            listView.Items.Clear();
            listView.View = new TileView();
            foreach (var element in collection)
            {
                var item = new ListViewItem();
                var panel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(4), Focusable = false };

                var checkBox = panel.Children.AddAndReference(new CheckBox { Content = element, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(4), Tag = new ContextData { Element = element, ElementType = elementType }, MinWidth = 90 });
                checkBox.Click += CheckBoxClicked;

                item.Content = panel;
                item.Tag = element;
                item.Focusable = false;

                listView.Items.Add(item);
            }
        }

        /// <summary>
        /// Fills the view after it has been constructed with the appropriate contents of the configuration database.
        /// </summary>
        private void FillWithConfigurationData()
        {
            LanguageGroupListView.Items.Clear();
            foreach (var check in Configuration.GetAllTags())
            {
                var checkItem = new ListViewItem();
                var panel = new StackPanel { Orientation = Orientation.Horizontal };
                panel.Children.Add(ResourceHelper.GetImage(IconType.Tag, 24, 24));
                panel.Children.Add(new Label { Content = check });
                checkItem.Content = panel;
                checkItem.Tag = check;
                LanguageGroupListView.Items.Add(checkItem);
            }
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
            FillWithConfigurationData();
        }

        /// <summary>
        /// Reacts to user clicking 'All languages' checkbox. This realizes a macro function to select/unselect all languages.
        /// <para/>
        /// Unchecked => Checked
        /// Checked => Unchecked
        /// Grayed => Unchecked
        /// </summary>
        void CheckBoxAllLanguagesClicked(object sender, RoutedEventArgs e)
        {
            if (LanguageGroupListView.SelectedItems.Count != 1) return;
            var tag = ((ListViewItem)LanguageGroupListView.SelectedItem).Tag.ToString();
            switch (CheckBoxAllLanguages.IsChecked)
            {
                //Null is coming from 'true', so fall it back to 'false'
                case null:
                    CheckBoxAllLanguages.IsChecked = false;
                    Configuration.ApplyTagToLanguages(tag, Configuration.GetAllLanguages(), false);
                    break;
                case false:
                    Configuration.ApplyTagToLanguages(tag, Configuration.GetAllLanguages(), false);
                    break;
                case true:
                    Configuration.ApplyTagToLanguages(tag, Configuration.GetAllLanguages(), true);
                    break;
            }
            UpdateStatesOfLanguageCheckBoxes(tag);
        }

        #region Event handlers

        /// <summary>
        /// Responds to the user clicking on a language.
        /// </summary>
        void CheckBoxClicked(object sender, RoutedEventArgs e)
        {
            var cb = (CheckBox)sender;
            var context = (ContextData)cb.Tag;
            if (LanguageGroupListView.SelectedItems.Count != 1) return;
            var tag = ((ListViewItem)LanguageGroupListView.SelectedItem).Tag.ToString();
            switch (context.ElementType)
            {
                case ContextType.Language:
                    Configuration.ApplyTagToLanguages(tag, new[] { context.Element }, cb.IsChecked.Value);
                    UpdateCheckBoxState(CheckBoxAllLanguages, Configuration.GetTagLanguages(tag).Count(), Configuration.GetAllLanguages().Count());
                    break;
            }
        }

        /// <summary>
        /// Reacts to selection change in the list of language groups.
        /// </summary>
        void LanguageGroupListSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            IsRightSideEnabled = LanguageGroupListView.SelectedItems.Count == 1; //Enable the right-hand side view if there is exactly one language group selected.
            if (LanguageGroupListView.SelectedItems.Count != 1) return; //Don't do anything else if there is not exactly one language group selected.

            var tag = ((ListViewItem)LanguageGroupListView.SelectedItem).Tag.ToString();

            #region Languages
            UpdateStatesOfLanguageCheckBoxes(tag);
            #endregion
        }


        #endregion

        /// <summary>
        /// Call this method to update the states of all language checkboxes based on current configuration.
        /// </summary>
        /// <param name="tag">Name of the language group for which enablement states are to be updated.</param>
        private void UpdateStatesOfLanguageCheckBoxes(string tag)
        {
            var associatedLanguages = Configuration.GetTagLanguages(tag).ToList(); //Enable checkboxes for these languages. Disable for anything else
            foreach (ListViewItem lvi in LanguagesList.Items)
            {
                var panel = (StackPanel)lvi.Content;
                var checkBox = panel.Children.ToList<CheckBox>().First();
                var language = lvi.Tag.ToString();
                checkBox.IsChecked = associatedLanguages.Contains(language);
            }
            //Update the 'All' checkbox
            UpdateCheckBoxState(CheckBoxAllLanguages, associatedLanguages.Count, Configuration.GetAllLanguages().Count());
        }

        /// <summary>
        /// Updates a three-state checkbox based on the relation of counts provided.
        /// If <paramref name="currentCount"/> is greater than 0 but less than <paramref name="maxCount"/>, the checkbox is assigned the grayed (third) state.
        /// </summary>
        /// <param name="checkBox">Checkbox to update state of.</param>
        /// <param name="currentCount">Count of elements in one list.</param>
        /// <param name="maxCount">Count of elements in super list (maximum size of the list).</param>
        private void UpdateCheckBoxState(CheckBox checkBox, int currentCount, int maxCount)
        {
            if (currentCount == maxCount)
            {
                checkBox.IsChecked = true;
                return;
            }
            if (currentCount == 0)
            {
                checkBox.IsChecked = false;
                return;
            }
            //Some items are selected
            checkBox.IsChecked = null;
        }

        /// <summary>
        /// Enables or disables the right side of the view depending on whether a check is selected in the left view.
        /// </summary>
        private bool IsRightSideEnabled { set { RightSideView.IsEnabled = value; } }
    }
}