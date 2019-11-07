using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using Microsoft.Localization.LocSolutions.Cerberus.Configurator.Themes;
using Microsoft.Localization.LocSolutions.Cerberus.Core;
using Microsoft.Localization.LocSolutions.Cerberus.Resources;
using System;
using System.Windows;
using System.Windows.Input;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;

namespace Microsoft.Localization.LocSolutions.Cerberus.Configurator
{
    internal class MainCheckConfigurationViewHandler : ViewHandler
    {
        /// <summary>
        /// List that displays checks in the "Main Check Configuration View".
        /// It is located on the left side of the view.
        /// </summary>
        protected ListView CheckList { get; set; }

        /// <summary>
        /// Inner grid that contains the left side of this view (list of checks).
        /// </summary>
        private Grid LeftSideView { get; set; }

        /// <summary>
        /// Inner grid that contains the right side of this view (lists of languages and projects).
        /// </summary>
        private Grid RightSideView { get; set; }

        /// <summary>
        /// Check description is contained in this box.
        /// </summary>
        private TextBox DescriptionBlock { get; set; }

        /// <summary>
        /// Check owner information is contained in this box.
        /// </summary>
        private TextBox OwnersBlock;

        /// <summary>
        /// This is a list in the right-hand side of the view that contains all language groups with checkboxes.
        /// </summary>
        private ListView LanguageGroupsList { get; set; }

        /// <summary>
        /// This is a list in the right-hand side of the view that contains all languages with checkboxes.
        /// </summary>
        private ListView LanguagesList { get; set; }

        /// <summary>
        /// This is a list in the right-hand side of the view that contains all projects with checkboxes.
        /// </summary>
        private ListView ProjectsList { get; set; }

        /// <summary>
        /// This is a three-state checkbox that performs aggregations on selection of all languages.
        /// <para/>If all languages are selected, this checkbox is checked.
        /// <para/>If no languages are selected, this checkbox is unchecked.
        /// <para/>If some - but not all - languages are selected, this checkbox is grayed (thrird state).
        /// <para/>If this checkbox is clicked it changes as follows:
        /// Unchecked => Checked
        /// Checked => Unchecked
        /// Grayed => Unchecked
        /// </summary>
        public CheckBox CheckBoxAllLanguages { get; set; }

        /// <summary>
        /// This is a three-state checkbox that performs aggregations on selection of all projects.
        /// <para/>If all projects are selected, this checkbox is checked.
        /// <para/>If no projects are selected, this checkbox is unchecked.
        /// <para/>If some - but not all - projects are selected, this checkbox is grayed (thrird state).
        /// <para/>If this checkbox is clicked it changes as follows:
        /// Unchecked => Checked
        /// Checked => Unchecked
        /// Grayed => Unchecked
        /// </summary>
        public CheckBox CheckBoxAllProjects { get; set; }

        internal MainCheckConfigurationViewHandler(Grid viewContainer, CheckConfiguration configuration) : base(viewContainer, configuration) { }

        /// <summary>
        /// This is a view description provided by the view implementation to the container (calling code).
        /// The calling code places the view description in an appropriate location, so that the user can read it.
        /// </summary>
        public override string ViewDescription
        {
            get
            {
                return "Use this view to manage assignment of checks to languages and projects that are defined in Cerberus configuration database. When a check is enabled on a language and a set of projects, Cerberus executor will run this check for these projects in the selected language. NOTE: If a check is selected for some projects but not for any language, effectively this check will not run on anything.";
            }
        }

        public override string ResourceName { get { return "mainCheckConfiguration"; } }

        /// <summary>
        /// Adds controls comprising the current view to the container of the view.
        /// <para></para>The view will be visible immediately.
        /// <para></para>This internal method is called from the publicly available Show() method.
        /// <para></para>Client code (container that uses this view), should not call this method directly, but it should call <see cref="Show"></see> instead.
        /// </summary>
        internal override void Reveal()
        {
            CheckList = GetViewElement<ListView>("checkList");
            CheckList.SelectionChanged += CheckListSelectionChanged;

            RightSideView = GetViewElement<Grid>("rightSideView");
            IsRightSideEnabled = false;

            LanguageGroupsList = GetViewElement<ListView>("languagesGroupsList");

            CheckBoxAllLanguages = GetViewElement<CheckBox>("checkBoxAllLanguages");
            CheckBoxAllLanguages.Click += CheckBoxAllLanguagesClicked;
            LanguagesList = GetViewElement<ListView>("languagesList");

            CheckBoxAllProjects = GetViewElement<CheckBox>("checkBoxAllProjects");
            CheckBoxAllProjects.Click += CheckBoxAllProjectsClicked;
            ProjectsList = GetViewElement<ListView>("projectsList");

            DescriptionBlock = GetViewElement<TextBox>("descriptionBlock");

            OwnersBlock = GetViewElement<TextBox>("ownersBlock");

            #region Populate controls in the right-hand side view
            AddLanguageGroups(LanguageGroupsList);
            AddContent(LanguagesList, Configuration.GetAllLanguages().ToArray(), ContextType.Language);
            AddContent(ProjectsList, Configuration.GetAllProjects().ToArray(), ContextType.Project);
            #endregion

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
            if (CheckList.SelectedItems.Count != 1) return;
            var checkName = ((ListViewItem)CheckList.SelectedItem).Tag.ToString();
            switch (CheckBoxAllLanguages.IsChecked)
            {
                //Null is coming from 'true', so fall it back to 'false'
                case null:
                    CheckBoxAllLanguages.IsChecked = false;
                    Configuration.EnableCheckForAllLanguages(checkName, false);
                    break;
                case false:
                    Configuration.EnableCheckForAllLanguages(checkName, false);
                    break;
                case true:
                    Configuration.EnableCheckForAllLanguages(checkName, true);
                    break;
            }
            UpdateStatesOfLanguageCheckBoxes(checkName);
            UpdateStatesOfLanguageGroupCheckBoxes(checkName);
        }

        /// <summary>
        /// Reacts to user clicking 'All projects' checkbox. This realizes a macro function to select/unselect all projects.
        /// <para/>
        /// Unchecked => Checked
        /// Checked => Unchecked
        /// Grayed => Unchecked
        /// </summary>
        void CheckBoxAllProjectsClicked(object sender, RoutedEventArgs e)
        {
            if (CheckList.SelectedItems.Count != 1) return;
            var checkName = ((ListViewItem)CheckList.SelectedItem).Tag.ToString();
            switch (CheckBoxAllProjects.IsChecked)
            {
                //Null is coming from 'true', so fall it back to 'false'
                case null:
                    CheckBoxAllProjects.IsChecked = false;
                    Configuration.EnableCheckForAllProjects(checkName, false);
                    break;
                case false:
                    Configuration.EnableCheckForAllProjects(checkName, false);
                    break;
                case true:
                    Configuration.EnableCheckForAllProjects(checkName, true);
                    break;
            }
            UpdateStatesOfProjectCheckBoxes(checkName);
        }

        #region Event handlers
        /// <summary>
        /// Responds to the user clicking on a language group, language or project checkbox.
        /// </summary>
        void CheckBoxClicked(object sender, RoutedEventArgs e)
        {
            var cb = (CheckBox)sender;
            var context = (ContextData)cb.Tag;
            if (CheckList.SelectedItems.Count != 1) return;
            var checkName = ((ListViewItem)CheckList.SelectedItem).Tag.ToString();
            switch (context.ElementType)
            {
                case ContextType.LanguageGroup: //Control the whole group of languages
                    Array.ForEach(Configuration.GetTagLanguages(context.Element).ToArray(),
                        language =>
                            Configuration.EnableCheckForLanguage(checkName, language, cb.IsChecked.Value)
                            );
                    UpdateStatesOfLanguageGroupCheckBoxes(checkName);
                    UpdateStatesOfLanguageCheckBoxes(checkName);
                    break;
                case ContextType.Language:
                    Configuration.EnableCheckForLanguage(checkName, context.Element, cb.IsChecked.Value);
                    UpdateCheckBoxState(CheckBoxAllLanguages, Configuration.GetLanguagesWithCheckEnabled(checkName).Count(), Configuration.GetAllLanguages().Count());
                    UpdateStatesOfLanguageGroupCheckBoxes(checkName);
                    break;
                case ContextType.Project:
                    Configuration.EnableCheckForProject(checkName, context.Element, cb.IsChecked.Value);
                    UpdateCheckBoxState(CheckBoxAllProjects, Configuration.GetProjectsWithCheckEnabled(checkName).Count(), Configuration.GetAllProjects().Count());
                    break;
            }
        }

        /// <summary>
        /// Reacts to selection change in the list of checks.
        /// </summary>
        void CheckListSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            IsRightSideEnabled = CheckList.SelectedItems.Count == 1; //Enable the right-hand side view if there is exactly one check selected.
            if (CheckList.SelectedItems.Count != 1) return; //Don't do anything else if there is not exactly one check selected.

            var checkName = ((DisplayableCheck)CheckList.SelectedItem).Name;

            #region Language groups
            UpdateStatesOfLanguageGroupCheckBoxes(checkName);
            #endregion

            #region Languages
            UpdateStatesOfLanguageCheckBoxes(checkName);
            #endregion

            #region Projects
            UpdateStatesOfProjectCheckBoxes(checkName);
            #endregion

            #region Check description and owners
            DescriptionBlock.TextChanged -= DescriptionChanged;
            DescriptionBlock.Text = Configuration.GetCheckDescription(checkName);
            DescriptionBlock.TextChanged += DescriptionChanged;
            OwnersBlock.TextChanged -= OwnersChanged;
            OwnersBlock.Text = Configuration.GetCheckOwners(checkName);
            OwnersBlock.TextChanged += OwnersChanged;
            #endregion
        }

        /// <summary>
        /// Call this method to update the states of all language group checkboxes based on current configuration.
        /// </summary>
        /// <param name="checkName">Name of the check for which enablement states are to be updated.</param>
        private void UpdateStatesOfLanguageGroupCheckBoxes(string checkName)
        {
            LanguageGroupsCollection.ActiveCheck = checkName;
            return;
        }

        /// <summary>
        /// Call this method to update the states of all language checkboxes based on current configuration.
        /// </summary>
        /// <param name="checkName">Name of the check for which enablement states are to be updated.</param>
        private void UpdateStatesOfLanguageCheckBoxes(string checkName)
        {
            var enabledLanguages = Configuration.GetLanguagesWithCheckEnabled(checkName).ToList(); //Enable checkboxes for these languages. Disable for anything else
            foreach (ListViewItem lvi in LanguagesList.Items)
            {
                var panel = (StackPanel)lvi.Content;
                var checkBox = panel.Children.ToList<CheckBox>().First();
                var language = lvi.Tag.ToString();
                checkBox.IsChecked = enabledLanguages.Contains(language);
            }
            //Update the 'All' checkbox
            UpdateCheckBoxState(CheckBoxAllLanguages, enabledLanguages.Count, Configuration.GetAllLanguages().Count());
        }

        /// <summary>
        /// Call this method to update the states of all project checkboxes based on current configuration.
        /// </summary>
        /// <param name="checkName">Name of the check for which enablement states are to be updated.</param>
        private void UpdateStatesOfProjectCheckBoxes(string checkName)
        {
            var enabledProjects = Configuration.GetProjectsWithCheckEnabled(checkName).ToList(); //Enable checkboxes for these projects. Disable for anything else
            foreach (ListViewItem lvi in ProjectsList.Items)
            {
                var panel = (StackPanel)lvi.Content;
                var checkBox = panel.Children.ToList<CheckBox>().First();
                var project = lvi.Tag.ToString();
                checkBox.IsChecked = enabledProjects.Contains(project);
            }
            //Update the 'All' checkbox
            UpdateCheckBoxState(CheckBoxAllProjects, enabledProjects.Count, Configuration.GetAllProjects().Count());
        }

        /// <summary>
        /// Updates a three-state checkbox based on the relation of counts provided.
        /// If <paramref name="currentCount"/> is greater than 0 but less than <paramref name="maxCount"/>, the checkbox is assigned the grayed (third) state.
        /// </summary>
        /// <param name="checkBox">Checkbox to update state of.</param>
        /// <param name="currentCount">Count of elements in one list.</param>
        /// <param name="maxCount">Count of elements in super list (maximum size of the list).</param>
        private static void UpdateCheckBoxState(CheckBox checkBox, int currentCount, int maxCount)
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

        void DescriptionChanged(object sender, TextChangedEventArgs e)
        {
            var checkName = ((ListViewItem)CheckList.SelectedItem).Tag.ToString();
            Configuration.SetCheckDescription(checkName, DescriptionBlock.Text);
        }

        void OwnersChanged(object sender, TextChangedEventArgs e)
        {
            var checkName = ((ListViewItem)CheckList.SelectedItem).Tag.ToString();
            Configuration.SetCheckOwners(checkName, OwnersBlock.Text);
        }

        #endregion

        private void AddLanguageGroups(ListView languageGroupsList)
        {
            LanguageGroupsCollection = new LanguageGroupsContainer(Configuration);
            var items = from lgroup in Configuration.GetAllTags()
                        where Configuration.GetTagLanguages(lgroup).Count() > 0
                        select new LanguageGroupItem { Name = lgroup };
            LanguageGroupsCollection.Collection = items.ToArray();
            languageGroupsList.ItemsSource = LanguageGroupsCollection.Collection;
        }
        /// <summary>
        /// This is the collection that is bound to LanguageGroups list view.
        /// </summary>
        LanguageGroupsContainer LanguageGroupsCollection;

        /// <summary>
        /// Fills the specified list control with languages from configuration database.
        /// </summary>
        private void AddContent(ListView listView, IEnumerable<string> collection, ContextType elementType)
        {
            switch (elementType)
            {
                case ContextType.LanguageGroup: //This is now handled automatically through data binding.
                    break;
                default:
                    listView.Items.Clear();
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
                    break;
            }
        }

        /// <summary>
        /// Implement this method to respond to data changes in the <see cref="ViewHandler.Configuration"></see>.
        /// Whatever events update your data, in response to to such events, your code should call into this method
        /// to ensure that the view reflects what's in data tables.
        /// <para></para>For example, if the user adds a new tag on a view item, call this method. Implementation of this method
        /// should reread relevant data entries and update UI elements.
        /// </summary>
        public override void UpdateViewToMatchData()
        {
            //NOTHING AT THE MOMENT
        }

        /// <summary>
        /// Fills the view after it has been constructed with the appropriate contents of the configuration database.
        /// </summary>
        private void FillWithConfigurationData()
        {
            CheckList.ItemsSource = from check in Configuration.GetAllChecks()
                                    select new DisplayableCheck(check, Configuration);
            return;

            CheckList.Items.Clear();
            foreach (var check in Configuration.GetAllChecks())
            {
                var checkItem = new ListViewItem();
                var panel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(4, 2, 2, 2) };
                panel.Children.Add(ResourceHelper.GetImage(IconType.Check, 24, 24));
                panel.Children.Add(new Label { Content = check });
                checkItem.Content = panel;
                checkItem.Tag = check;
                CheckList.Items.Add(checkItem);
            }
        }

        /// <summary>
        /// Enables or disables the right side of the view depending on whether a check is selected in the left view.
        /// </summary>
        private bool IsRightSideEnabled { set { RightSideView.IsEnabled = value; } }

    }
    /// <summary>
    /// Used to display items in Language groups list.
    /// </summary>
    public class VeryGenericCheckBoxedItem : INotifyPropertyChanged
    {
        public string Name { get; set; }
        private bool? isChecked;
        public bool? IsChecked
        {
            get
            {
                return isChecked;
            }
            set
            {
                isChecked = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, IsCheckedProperty);
                }
            }
        }
        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;
        private static PropertyChangedEventArgs IsCheckedProperty = new PropertyChangedEventArgs("IsChecked");

        #endregion

        /// <summary>
        /// Updates a three-state checkbox based on the relation of counts provided.
        /// If <paramref name="currentCount"/> is greater than 0 but less than <paramref name="maxCount"/>, the checkbox is assigned the grayed (third) state.
        /// </summary>
        /// <param name="checkBox">Checkbox to update state of.</param>
        /// <param name="currentCount">Count of elements in one list.</param>
        /// <param name="maxCount">Count of elements in super list (maximum size of the list).</param>
        public void UpdateCheckBoxState(int currentCount, int maxCount)
        {
            if (currentCount == maxCount)
            {
                IsChecked = true;
                return;
            }
            if (currentCount == 0)
            {
                IsChecked = false;
                return;
            }
            //Some items are selected
            IsChecked = null;
        }
    }

    public class LanguageGroupItem : VeryGenericCheckBoxedItem
    {
        public static Image TagIcon { get { return ResourceHelper.GetImage(IconType.Tag, 24, 24); } }
    }

    public class LanguageGroupsContainer
    {
        public LanguageGroupsContainer(CheckConfiguration configuration)
        {
            Configuration = configuration;
        }
        public IEnumerable<LanguageGroupItem> Collection { get; set; }
        private string activeCheck;
        public string ActiveCheck
        {
            get
            {
                return activeCheck;
            }
            set
            {
                activeCheck = value;
                if (string.IsNullOrEmpty(activeCheck))
                {
                    Debug.Fail("This code should never execute.");
                }
                else
                {
                    foreach (var item in Collection)
                    {
                        var tagLanguages = Configuration.GetTagLanguages(item.Name);
                        var checkLanguages = Configuration.GetLanguagesWithCheckEnabled(activeCheck);

                        var enabledLanguages = tagLanguages.Intersect(checkLanguages);
                        //Update the 3-state checkbox
                        item.UpdateCheckBoxState(enabledLanguages.Count(), tagLanguages.Count());
                    }
                }
            }
        }
        public CheckConfiguration Configuration { get; set; }

    }
}
