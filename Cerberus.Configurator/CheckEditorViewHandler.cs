using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using Microsoft.Localization.LocSolutions.Cerberus.Configurator.Themes;
using Microsoft.Localization.LocSolutions.Cerberus.Core;
using Microsoft.Localization.LocSolutions.Cerberus.Resources;
using System;
using System.Windows;
using System.Windows.Media;
using System.IO;
using Microsoft.Localization.LocSolutions.Cerberus.Configurator.Properties;
using System.Collections;

namespace Microsoft.Localization.LocSolutions.Cerberus.Configurator
{
    internal class CheckEditorViewHandler : ViewHandler
    {
        private ListView ChecksList;

        private ListView EnabledVariablesList;
        private ListView AllVariablesList;
        public CheckEditorViewHandler(Grid viewContainer, CheckConfiguration configuration)
            : base(viewContainer, configuration)
        {

        }
        /// <summary>
        /// This is a view description provided by the view implementation to the container (calling code).
        /// The calling code places the view description in an appropriate location, so that the user can read it.
        /// </summary>
        public override string ViewDescription
        {
            get
            {
                return "Add and remove checks defined in configuration. Path to checks can be made independent on the machine by using any of environment variables. When choosing location of checks added to configuration, consider location described by environment variables which are common to all users of Cerberus, so that regardless of where Cerberus is used, checks can be located using common environment variables.";
            }
        }

        public override string ResourceName
        {
            get
            {
                return "checkEditorView";
            }
        }

        /// <summary>
        /// Called by the container (calling code) in order to show the view to the user.
        /// The base implementation of this method contains code to initialize common elements of each view
        /// and then it calls view-specific control construction.
        /// </summary>
        public override void Show()
        {
            base.Show();
        }

        /// <summary>
        /// Adds controls comprising the current view to the container of the view.
        /// <para></para>The view will be visible immediately.
        /// <para></para>This internal method is called from the publicly available Show() method.
        /// <para></para>Client code (container that uses this view), should not call this method directly, but it should call <see cref="Show"></see> instead.
        /// </summary>
        internal override void Reveal()
        {
            ChecksList = GetViewElement<ListView>("ChecksList");
            ChecksList.ItemsSource = (from checkName in Configuration.GetAllChecks()
                                      select new DisplayableCheck(checkName, Configuration)).ToArray();

            EnabledVariablesList = GetViewElement<ListView>("EnabledVariables");
            EnabledVariablesList.ItemsSource = Settings.Default.EnvironmentVariablesForNewChecks;

            AllVariablesList = GetViewElement<ListView>("AllVariables");

            #region Allow ordering by variable name by converting it to a list
            var all = Environment.GetEnvironmentVariables();
            var allList = new List<KeyValuePair<string, string>>();
            foreach (DictionaryEntry envVar in all)
            {
                allList.Add(new KeyValuePair<string, string>(envVar.Key.ToString(), envVar.Value.ToString()));
            }
            #endregion

            AllVariablesList.ItemsSource = allList.OrderBy(item => item.Key);

            GetViewElement<Button>("ButtonAdd").Click += ClickedAddButton;
            GetViewElement<Button>("ButtonRemove").Click += ClickedRemoveButton;
        }

        /// <summary>
        /// Reaction to user pressing 'Add' button.
        /// </summary>
        void ClickedAddButton(object sender, RoutedEventArgs e)
        {
            var fd = new System.Windows.Forms.OpenFileDialog { AutoUpgradeEnabled = true, DefaultExt = "cs", Filter = "C# class files|*.cs", Title = "Select a Cerberus check file to add", ValidateNames = true };
            fd.FileOk += NewCheckFileSelected;
            fd.ShowDialog();
        }

        /// <summary>
        /// Event handler called after the user presses OK in the file open dialog.
        /// This method adds the selected check file to the configuration.
        /// </summary>
        void NewCheckFileSelected(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var file = (sender as System.Windows.Forms.OpenFileDialog).FileName;
            var missingChecks = new List<CheckInfo>();
            missingChecks.Add(new CheckInfo
            {
                Name = Path.GetFileNameWithoutExtension(file),
                Description = "New check",
                FilePath = TryToUseEnvironmentVariablesInAbsoluteFilePath(file)
            });
            Configuration.AddMissingChecks(missingChecks);
            UpdateViewToMatchData();
        }

        /// <summary>
        /// Assumes that the specified <paramref name="fullPath"/> is an absolute path.
        /// <para/>Attempts to substitute the beginning of the string with useful variable names.
        /// </summary>
        /// <param name="fullPath">Full path to be changed to use environment variables.</param>
        /// <returns>Original or 'improved' path that may reference environment variables.</returns>
        private string TryToUseEnvironmentVariablesInAbsoluteFilePath(string fullPath)
        {
            string retVal = fullPath;
            foreach (var envVar in Settings.Default.EnvironmentVariablesForNewChecks)
            {
                retVal = TryToUseSingleEnvironmentVariable(envVar, retVal);
            }
            return retVal;
        }

        /// <summary>
        /// Attempts to substitute the beginning of the string with the name of the specified environment variable.
        /// </summary>
        /// <param name="fullPath">Full path to be changed to use environment variables.</param>
        /// <param name="envVar">Name of the environment variable, without percentage signs. For example: "OTOOLS".</param>
        /// <returns>Original or 'improved' path that may reference the specified environment variable.</returns>
        private string TryToUseSingleEnvironmentVariable(string envVar, string fullPath)
        {
            string retVal = fullPath;
            var envNameString = string.Format("%{0}%", envVar);
            var envValue = Environment.ExpandEnvironmentVariables(envNameString);
            if (retVal.StartsWith(envValue, StringComparison.InvariantCultureIgnoreCase))
            {
                retVal = retVal.Replace(envValue, envNameString);
            }
            return retVal;
        }

        /// <summary>
        /// Reaction to user pressing 'Remove' button.
        /// </summary>
        void ClickedRemoveButton(object sender, RoutedEventArgs e)
        {
            ShowNotImplementedUI();
        }
        /// <summary>
        /// Implement this method to respond to data changes in the <see cref="Configuration"></see>.
        /// Whatever events update your data, in response to to such events, your code should call into this method
        /// to ensure that the view reflects what's in data tables.
        /// <para></para>For example, if the user adds a new tag on a view item, call this method. Implementation of this method
        /// should reread relevant data entries and update UI elements.
        /// </summary>
        public override void UpdateViewToMatchData()
        {
            ChecksList.ItemsSource = (from checkName in Configuration.GetAllChecks()
                                      select new DisplayableCheck(checkName, Configuration)).ToArray();
        }
    }
}
