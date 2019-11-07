using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using Microsoft.Localization.LocSolutions.Cerberus.Core;
using System;
using System.Windows;

namespace Microsoft.Localization.LocSolutions.Cerberus.Configurator
{
    /// <summary>
    /// New summary view that shows which checks are enabled for a language or language group.
    /// </summary>
    internal class NewSummaryViewHandler : ViewHandler
    {
        /// <summary>
        /// A list that presents languages and language groups. Placed on the left side of the view.
        /// </summary>
        public ListView LanguageOrProjectList { get; set; }

        /// <summary>
        /// A list that presents checks that are enabled for the selected language or language group.
        /// Placed on the right side of the view.
        /// </summary>
        public ListView CheckList { get; set; }

        /// <summary>
        /// Group box that contains languages or projects.
        /// </summary>
        public GroupBox PivotGroup { get; set; }

        private ViewPivotMode pivotMode;

        /// <summary>
        /// What data we are displaying.
        /// </summary>
        protected ViewPivotMode PivotMode
        {
            get
            {
                return pivotMode;
            }
            set
            {
                pivotMode = value;
                SwitchPivot(value);
            }
        }

        /// <summary>
        /// Default constructro.
        /// </summary>
        /// <param name="viewContainer">Container grid that this view can lay out its controls.</param>
        /// <param name="configuration">Configuration that is used to display data in this view.</param>
        internal NewSummaryViewHandler(Grid viewContainer, CheckConfiguration configuration) : base(viewContainer, configuration)
        {
            pivotMode = ViewPivotMode.Languages; //Initial setting.
        }

        /// <summary>
        /// This is a view description provided by the view implementation to the container (calling code).
        /// The calling code places the view description in an appropriate location, so that the user can read it.
        /// </summary>
        public override string ViewDescription
        {
            get
            {
                //return "Shows the list of checks enabled on the selected language or language group. Hover over a language group to see languages being part of that group. Hover over a partially enabled check to see languages of the language group, for which that check is enabled.";
                return "Shows the list of checks enabled on the selected language, language group or project. Pivoting data between languages and projects allows you to see how checks are configured. Hover over a language group to see languages being part of that group. Hover over a check to see all projects, for which that check is enabled.";
            }
        }

        /// <summary>
        /// Adds controls comprising the current view to the container of the view.
        /// <para/>The view will be visible immediately.
        /// <para/>This internal method is called from the publicly available Show() method.
        /// </summary>
        internal override void Reveal()
        {
            SetUpRowsAndColumns();

            SetUpLanguageAndProjectSelectors(ViewContainer);
            SetUpCheckList(ViewContainer);
            FillWithConfigurationData();
        }

        /// <summary>
        /// Sets up the left portion of the view where languages or projects will be displayed.
        /// </summary>
        /// <param name="container">Container where the controls are to be placed.</param>
        private void SetUpLanguageAndProjectSelectors(Grid container)
        {
            var subContainer = container.AddGridItem(0, 0, new Grid());

            subContainer.RowDefinitions.Add(new RowDefinition { Height = new GridLength(0, GridUnitType.Auto) });
            subContainer.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            var selectorGroup = subContainer.AddGridItem(0, 0, UITheme.CreateGroupBox("Pivoting mode"));
            var selectorPanel = new StackPanel { Orientation = Orientation.Horizontal };
            selectorGroup.Content = selectorPanel;
            var radioButtonLanguages = selectorPanel.Children.AddAndReference(UITheme.CreateRadioButton("Languages"));
            radioButtonLanguages.IsChecked = true;
            radioButtonLanguages.Checked += (x, y) => PivotMode = ViewPivotMode.Languages;
            var radioButtonProjects = selectorPanel.Children.AddAndReference(UITheme.CreateRadioButton("Projects"));
            radioButtonProjects.Checked += (x, y) => PivotMode = ViewPivotMode.Projects;


            PivotGroup = subContainer.AddGridItem(1, 0, UITheme.CreateGroupBox("Language groups and languages"));
            LanguageOrProjectList = new ListView();
            LanguageOrProjectList.ItemTemplate = (DataTemplate)LanguageOrProjectList.FindResource("LanguageListItemTemplate");
            PivotGroup.Content = LanguageOrProjectList;
        }

        /// <summary>
        /// Sets up the right portion of the view where checks will be displayed.
        /// </summary>
        /// <param name="container">Container where the controls are to be placed.</param>
        private void SetUpCheckList(Grid container)
        {
            var group = container.AddGridItem(0, 1, UITheme.CreateGroupBox("Checks"));
            CheckList = new ListView();
            CheckList.ItemTemplate = (DataTemplate)LanguageOrProjectList.FindResource("CheckItemTemplate");
            group.Content = CheckList;
        }

        /// <summary>
        /// Configures rows and columns of the client area of this view.
        /// </summary>
        private void SetUpRowsAndColumns()
        {
            //ViewContainer.ShowGridLines = true;

            #region Rows
            ViewContainer.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }); //Languages, projects, checks
            #endregion

            #region Columns
            ViewContainer.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); //Languages or projects
            ViewContainer.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); //Checks
            #endregion

        }

        /// <summary>
        /// Implement this method to respond to data changes in the <see cref="ViewHandler.Configuration"/>.
        /// Whatever events update your data, in response to to such events, your code should call into this method
        /// to ensure that the view reflects what's in data tables.
        /// <para></para>For example, if the user adds a new tag on a view item, call this method. Implementation of this method
        /// should reread relevant data entries and update UI elements.
        /// </summary>
        public override void UpdateViewToMatchData()
        {
            #region Currently we do nothing
            #endregion
        }

        /// <summary>
        /// Fills the left side of the view list of all langauge gropus and languages from the configuration database.
        /// </summary>
        private void FillWithConfigurationData()
        {
            CheckList.ItemsSource = null; //Clean the checks first.

            switch (PivotMode)
            {
                case ViewPivotMode.Languages: FillWithLanguages(LanguageOrProjectList); break;
                case ViewPivotMode.Projects: FillWithProjects(LanguageOrProjectList); break;
            }
        }


        /// <summary>
        /// Called from <see cref="FillWithConfigurationData"/> when languages should be displayed.
        /// </summary>
        /// <param name="listView">Container that will hold languages.</param>
        private void FillWithLanguages(ListView listView)
        {
            LanguageOrProjectList.SelectionChanged -= LanguageOrTagOrProjectSelected;
            LanguageOrProjectList.ItemsSource = null;
            var listInput = new List<DisplayableItem>();
            foreach (var tag in Configuration.GetAllTags())
            {
                if (Configuration.GetTagLanguages(tag).Count() == 0) continue; //Skip tags with no languages assigned.
                DisplayableItem t = new LanguageTagItem(tag, Configuration);
                listInput.Add(t);
            }

            foreach (var language in Configuration.GetAllLanguages())
            {
                DisplayableItem l = new Language(language, Configuration);
                listInput.Add(l);
            }

            LanguageOrProjectList.ItemsSource = listInput;
            LanguageOrProjectList.SelectionChanged += LanguageOrTagOrProjectSelected;
        }

        /// <summary>
        /// Called from <see cref="FillWithConfigurationData"/> when projects should be displayed.
        /// </summary>
        /// <param name="listView">Container that will hold projects.</param>
        private void FillWithProjects(ListView listView)
        {
            LanguageOrProjectList.SelectionChanged -= LanguageOrTagOrProjectSelected;
            LanguageOrProjectList.ItemsSource = null;
            var listInput = new List<DisplayableItem>();

            foreach (var project in Configuration.GetAllProjects())
            {
                DisplayableItem p = new Project(project, Configuration);
                listInput.Add(p);
            }

            LanguageOrProjectList.ItemsSource = listInput;
            LanguageOrProjectList.SelectionChanged += LanguageOrTagOrProjectSelected;
        }

        /// <summary>
        /// Event raised when selection of language or language group or project changes.
        /// This event handler updates the list of checks on the right side of the view.
        /// For the selected language or language group or project, this method displays checks enabled for that language or language group or project.
        /// </summary>
        private void LanguageOrTagOrProjectSelected(object sender, SelectionChangedEventArgs e)
        {
            CheckList.ItemsSource = null;
            var item = LanguageOrProjectList.SelectedItem as DisplayableItem;
            if (item == null) return;
            
            switch (item.ItemType)
            {
            	case ItemType.Language:
                    var languageChecks = Configuration.GetChecksEnabledForLanguage(item.Name);
                    CheckList.ItemsSource = languageChecks.Select(check => new LanguagePivotedCheck(check, Configuration)).ToArray();
            		break;
                case ItemType.Tag:
                    var tagLanguages = Configuration.GetTagLanguages(item.Name).ToList();
                    var anyChecks = tagLanguages.SelectMany(lang => Configuration.GetChecksEnabledForLanguage(lang)).Distinct().OrderBy(ch => ch).ToList();
                    var notGlobalChecks = anyChecks.Where(check => Configuration.GetLanguagesWithCheckEnabled(check).Intersect(tagLanguages).Count() < tagLanguages.Count());
                    var globalChecks = anyChecks.Except(notGlobalChecks).ToList();
                    var list = new List<DisplayableItem>();
                    foreach (var gCheck in globalChecks)
                    {
                        list.Add(new LanguagePivotedCheck(gCheck, Configuration));
                    }
                    #region Per Peter's request, we do not show non-global checks
                    foreach (var ngCheck in notGlobalChecks)
                    {
                        continue; //Remove this line to add non-global checks into the view.
                        list.Add(new PartialCheck(ngCheck, Configuration, tagLanguages));
                    }
                    #endregion

                    CheckList.ItemsSource = list;
                    break;
                case ItemType.Project:
                    var projectChecks = Configuration.GetChecksEnabledForProject(item.Name);
                    CheckList.ItemsSource = projectChecks.Select(check => new ProjectPivotedCheck(check, Configuration)).ToArray();
                    break;
            }
        }

        /// <summary>
        /// Called by <see cref="PivotMode"/> setter whenever data pivot changes from languages to projects or vice versa.
        /// Updates group box header to indicate what is displayed in the view.
        /// </summary>
        /// <param name="newPivot">New pivot for data.</param>
        private void SwitchPivot(ViewPivotMode newPivot)
        {
            switch (newPivot)
            {
                case ViewPivotMode.Languages: PivotGroup.Header = "Language groups and languages"; break;
                case ViewPivotMode.Projects: PivotGroup.Header = "Projects"; break;
            }
            FillWithConfigurationData();
        }

        /// <summary>
        /// Indicates what data we are showing in the left view.
        /// </summary>
        protected enum ViewPivotMode
        {
            /// <summary>
            /// The left part of the view should be showing languages and language groups.
            /// </summary>
            Languages,
            /// <summary>
            /// The left part of the view should be showing projects.
            /// </summary>
            Projects
        }
    }
}
