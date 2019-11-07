using System;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;
using Microsoft.Localization.LocSolutions.Cerberus.Configurator.Properties;
using Microsoft.Localization.LocSolutions.Logger;
using System.Linq;
using Legacy = System.Windows.Forms;
using System.ComponentModel;

namespace Microsoft.Localization.LocSolutions.Cerberus.Configurator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Private members

        /// <summary>
        /// Dialog to browse for XML configuration file manually.
        /// </summary>
        private Legacy.OpenFileDialog configFileOpenDialog;

        private UserView activeView;
        /// <summary>
        /// Handler of the specific view that is currently active.
        /// The type specifies what is the type of container for the views.
        /// </summary>
        private ViewHandler viewHandler;
        #endregion

        #region Public members
        /// <summary>
        /// View that is currently active. Standard view contains less view options. Advanced view contains more options.
        /// </summary>
        public UserView ActiveView
        {
            get
            {
                return activeView;
            }
            set
            {
                if (activeView == value) return; //Don't do anything when the user has not really selected a different view.
                activeView = value;
                InitializeDataView(activeView);
            }
        }
        #endregion

        #region Public API
        /// <summary>
        /// Default constructor of the main window of Cerberus configurator application.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
        }
        #endregion

        /// <summary>
        /// Event handler invoked when the window has finished initializing all controls.
        /// Used by us to initialize our member variables on window startup.
        /// </summary>
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            UpdateWindowDimensions(this);
            InitializeStatusBar();
            InitializeTitleBar();
            ShowEnlistmentStatusOnStatusBar();
            LoadDataFromDatabase();

            //We don't sync enlistment anymore, because we have a complete database checked in.
            //PopluateFromEnlistment();

            InitializeDataView(ActiveView);
            //ActiveView = UserView.Advanced;
        }

        /// <summary>
        /// Event handler invoked when the window is about to close.
        /// </summary>
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            RememberWindowDimensions(this);
            Settings.Default.Save();
            base.OnClosing(e);
        }

        /// <summary>
        /// Initializes the list of available views and displays the default view on data.
        /// </summary>
        /// <param name="activeView">Indicates what kind of view to display. Currently not used based on user feedback. Always show all supported views.</param>
        private void InitializeDataView(UserView activeView)
        {
            ComboActiveView.Items.Clear();
            ComboActiveView.SelectionChanged -= ReactionToActiveViewChange;
            int defaultViewIndex = 0;

            #region Disabled code
            //switch (activeView)
            //{
            //    case UserView.Standard:
            //        defaultViewIndex = ComboActiveView.Items.Add(new ComboBoxItem { Tag = ViewType.NewCheckAssignment, Content = "Check configuration view" });
            //        ComboActiveView.Items.Add(new ComboBoxItem { Tag = ViewType.LanguageGroups, Content = "Language groups definition" });
            //        //ComboActiveView.Items.Add(new ComboBoxItem { Tag = ViewType.ViewOnEnlistment, Content = "View on enlistment" });
            //        //ComboActiveView.Items.Add(new ComboBoxItem { Tag = ViewType.ViewOnChecks, Content = "View on checks" });
            //        //ComboActiveView.Items.Add(new ComboBoxItem { Tag = ViewType.ViewOnProjects, Content = "View on projects" });
            //        //ComboActiveView.Items.Add(new ComboBoxItem { Tag = ViewType.CheckAssignment, Content = "Assignment of checks to enlistment items" });
            //        //ComboActiveView.Items.Add(new ComboBoxItem { Tag = ViewType.ConfigurationSummary, Content = "Configuration summary" });
            //        break;

            //    case UserView.Advanced:
            //        defaultViewIndex = ComboActiveView.Items.Add(new ComboBoxItem { Tag = ViewType.NewCheckAssignment, Content = "Check configuration view" });
            //        ComboActiveView.Items.Add(new ComboBoxItem { Tag = ViewType.LanguageGroups, Content = "Language groups definition" });
            //        ComboActiveView.Items.Add(new ComboBoxItem { Tag = ViewType.NewSummaryView, Content = "Summary view" });
            //        break;
            //}
            #endregion

            #region This code executes when we don't care for the value of 'activeView' parameter
            defaultViewIndex = ComboActiveView.Items.Add(new ComboBoxItem { Tag = ViewType.NewCheckAssignment, Content = "Check configuration view" });
            ComboActiveView.Items.Add(new ComboBoxItem { Tag = ViewType.LanguageGroups, Content = "Language groups definition" });
            ComboActiveView.Items.Add(new ComboBoxItem { Tag = ViewType.NewSummaryView, Content = "Summary view" });
            ComboActiveView.Items.Add(new ComboBoxItem { Tag = ViewType.CheckEditor, Content = "Check editor" });
            #endregion

            ComboActiveView.SelectionChanged += ReactionToActiveViewChange;
            ComboActiveView.SelectedIndex = defaultViewIndex;
        }

        /// <summary>
        /// Event handler that responds to selecting a new view type from the view type combo box.
        /// </summary>
        private void ReactionToActiveViewChange(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count != 1) return;
            var viewItem = e.AddedItems[0] as ComboBoxItem;
            SetStatusBarItem(1, viewItem.Content.ToString());
            SwitchView((ViewType) viewItem.Tag);
        }

        /// <summary>
        /// Clears the view container and activates the specified view in the main window.
        /// </summary>
        /// <param name="viewType">Required view to activate.</param>
        private void SwitchView(ViewType viewType)
        {
            #region Reinitialize the whole TreeView container to insulate ourselves form changes made by previous view.
            TreeViewContainer.Children.ToList<Grid>().ForEach(grid => grid.Children.Clear());
            TreeViewContainer.Children.Clear();
            var containerForChildView = TreeViewContainer.Children.AddAndReference(ConfiguratorApplication.Current.UITheme.CreateGrid());
            #endregion

            viewHandler = null;
            viewHandler = ViewFactory.CreateView(viewType, containerForChildView, ConfiguratorApplication.Current.Database);
            viewHandler.Show();
        }

        /// <summary>
        /// Loads data from the disk database (file or SQL) as in-memory data for manipulation.
        /// </summary>
        private void LoadDataFromDatabase()
        {
            //If there is no enlistment, try to see if the database file pointed to by application settings is present.
            LoadDataFromDatabase(Environment.ExpandEnvironmentVariables(Settings.Default.DatabaseLocation));
        }

        private void LoadDataFromDatabase(string dataFilePath)
        {
            var fileInfo = new FileInfo(dataFilePath);
            if (fileInfo.Exists)
            {
                //SetStatusBarItem(1, Properties.Resources.LoadingDataInDataInProgress);
                SetStatusBarItem(1, "Loading data from Configuration.xml");
                ConfiguratorApplication.Current.Database.ReadXml(dataFilePath);
                //SetStatusBarItem(1, Properties.Resources.LoadedData);
                SetStatusBarItem(1, "Loaded data from Configuration.xml");
            }
            else
            {
                //SetStatusBarItem(0, Properties.Resources.DataFileDoesNotExist);
                SetStatusBarItem(0, "Configuration data file cannot be located.");
                LoggerSAP.Error("Location of configuration data file is invalid: {0}", fileInfo.FullName);
            }
        }
        /// <summary>
        /// Updates the disk database (file or SQL) with the in-memory data for persistence.
        /// </summary>
        private void CommitDataToDatabase()
        {
            try
            {
                //If there is no enlistment, try to see if the database file pointed to by application settings is present.
                var dataFilePath = new FileInfo(Environment.ExpandEnvironmentVariables(Settings.Default.DatabaseLocation));

                //SetStatusBarItem(1, Properties.Resources.SavingDataInProgress);
                SetStatusBarItem(1, "Saving data to Configuration.xml");
                ConfiguratorApplication.Current.Database.WriteXml(dataFilePath.FullName);
                //SetStatusBarItem(1, Properties.Resources.SavedData);
                SetStatusBarItem(1, "Saved data to Configuration.xml");
            }
            #region File is write-protected or the user has no permissions to write.
            catch (UnauthorizedAccessException e)
            {
                MessageBox.Show(
                    //string.Format(Properties.Resources.SaveFileFailedFormat, e.Message),
                    string.Format("Cannot save your changes because: {0} If this file is under version control, make sure you check it out first, then try saving again.", e.Message),
                    //Properties.Resources.MessageBoxHeader,
                    "Cerberus",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            #endregion
        }

        /// <summary>
        /// Updates the first status bar item with the status of the enlistment.
        /// </summary>
        private void ShowEnlistmentStatusOnStatusBar()
        {
            if (ConfiguratorApplication.Current.Enlistment.IsDetected)
            {
                //SetStatusBarItem(0, string.Format(Properties.Resources.EnlistmentDetectedFormat, ConfiguratorApplication.Current.Enlistment.Location));
                SetStatusBarItem(0, string.Format("Enlistment detected at: {0}.", ConfiguratorApplication.Current.Enlistment.Location));
            }
            else
            {
                //SetStatusBarItem(0, Properties.Resources.EnlistmentNotDetected);
                SetStatusBarItem(0, "Office 14 enlistment not detected.");
            }
        }

        /// <summary>
        /// Updates status bar item with the specified content.
        /// </summary>
        /// <param name="item">0-based index of status bar item to update.</param>
        /// <param name="content">Content to set at the specified status bar item.</param>
        private void SetStatusBarItem(int item, string content)
        {
            ((StatusBarItem)MainStatusBar.Items[item]).Content = content;
        }

        /// <summary>
        /// Initializes status bar of the main window by creating necessary items.
        /// </summary>
        private void InitializeStatusBar()
        {
            MainStatusBar.Items.Add(new StatusBarItem()); //General status
            MainStatusBar.Items.Add(new StatusBarItem()); //Used to indicate data status.
        }

        /// <summary>
        /// Initializes title bar of the main window by adding current exe's version number.
        /// </summary>
        private void InitializeTitleBar()
        {
            //Title = string.Format(Properties.Resources.MainWindowTitle, ConfiguratorApplication.Current.ThisAssembly.GetName().Version);
            Title = string.Format("Cerberus Admin Config Tool v{0}", ConfiguratorApplication.Current.ThisAssembly.GetName().Version);
        }

        /// <summary>
        /// Indicates whether there is any dirty data that should be saved before exitting the application.
        /// If all data is clean, this method returns true.
        /// </summary>
        protected static bool ItIsSafeToExit
        {
            //Currently always returs false.
            get { return false; }
        }

        /// <summary>
        /// Invoked at application startup. This method is similar to the event handler of the removed 'Populate' button.
        /// This method adds data from enlistment to the database.
        /// </summary>
        private void PopluateFromEnlistment()
        {
            if (ConfiguratorApplication.Current.Enlistment.IsDetected)
            {
                //SetStatusBarItem(1, Properties.Resources.UpdatingDatabase);
                SetStatusBarItem(1, "Updating database...");
                ThreadPool.QueueUserWorkItem(delegate
                {
                    ConfiguratorApplication.Current.CurrentAsimo.UpdateDatabaseWithEnlistmentContent(ConfiguratorApplication.Current.Database);
                    this.InvokeOnUIThread(() =>
                    {
                        //SetStatusBarItem(1, Properties.Resources.DatabaseUpdateFinished);
                        SetStatusBarItem(1, "Database update finished.");
                        viewHandler.UpdateViewToMatchData();
                    });
                });
            }
            else
            {
                //SetStatusBarItem(1, Properties.Resources.DatabaseNotUpdated);
                SetStatusBarItem(1, "Database was not updated.");
            }
        }

        /// <summary>
        /// Shows a generic 'not implemented' message box.
        /// </summary>
        private static void ShowNotImplementedUI()
        {
            MessageBox.Show(
                //Properties.Resources.FeatureNotImplementedString,
                "This feature is not yet implemented.",
                //Properties.Resources.MessageBoxHeader,
                "Cerberus",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        #region Event handlers
        /// <summary>
        /// Opens a new database file after the user used File -> Open menu.
        /// </summary>
        private void MenuClickedOpen(object sender, RoutedEventArgs e)
        {
            if (configFileOpenDialog == null)
            {
                configFileOpenDialog = new Legacy.OpenFileDialog
                {
                    AutoUpgradeEnabled = true,
                    CheckFileExists = true,
                    CheckPathExists = true,
                    DefaultExt = "xml",
                    FileName = "Configuration.xml",
                    Filter = "Cerberus configuration files|*.xml|All files|*",
                    Multiselect = false,
                    SupportMultiDottedExtensions = true,
                    Title = "Browse for Cerberus configuration files",
                    ValidateNames = true
                };
                configFileOpenDialog.FileOk += ConfigFileSelected;
            }
            configFileOpenDialog.ShowDialog();
        }

        /// <summary>
        /// Called by the OpenFileDialog after the user has selected a file name.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ConfigFileSelected(object sender, CancelEventArgs e)
        {
            var fileDialog = sender as Legacy.OpenFileDialog;
            LoadDataFromDatabase(fileDialog.FileName);
        }

        /// <summary>
        /// Exits the application after the user used File -> Exit menu.
        /// </summary>
        private void MenuClickedExit(object sender, RoutedEventArgs e)
        {
            ButtonExit_Click(null, e);
        }

        /// <summary>
        /// Reacts to user clicking 'Persist changes' button. Results in storing current data set in persistent storage (such as disk file).
        /// </summary>
        private void ButtonPersist_Click(object sender, RoutedEventArgs e)
        {
            CommitDataToDatabase();
        }

        /// <summary>
        /// Reacts to user clicking 'Exit' button. Results in shutting down application.
        /// Database is not saved (the user has to explicitly do a save).
        /// </summary>
        private void ButtonExit_Click(object sender, RoutedEventArgs e)
        {
            if (!ItIsSafeToExit)
            {
                var userReaction = MessageBox.Show(
                    //Properties.Resources.ConfirmApplicationExit,
                    "Are you sure you want to exit. (Unsaved changes to the database will be lost.)",
                    //Properties.Resources.MessageBoxHeader,
                    "Cerberus",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);
                if (userReaction == MessageBoxResult.Yes)
                {
                    //CommitDataToDatabase();
                    Application.Current.Shutdown();
                }
                return;
            }
            //If it is OK to exit, then exit.
            Application.Current.Shutdown();
        }

        /// <summary>
        /// Reaction to the user pressing either 'Standard' or 'Advanced' view.
        /// </summary>
        /// <param name="sender">Menu item</param>
        /// <param name="e"></param>
        private void MenuClickedView(object sender, RoutedEventArgs e)
        {
            var clickedMenu = (MenuItem)sender; //Clicked menu item
            var tag = clickedMenu.Tag.ToString(); //Tag
            var parent = (MenuItem)clickedMenu.Parent; //Parent menu
            var notClicked = parent.Items.ToList<MenuItem>().Where(x => !x.Tag.ToString().Equals(tag)).ToList(); //Items not checked (to be unchecked)
            notClicked.ForEach(nn => nn.IsChecked = false);
            clickedMenu.IsChecked = true; //If the user unchecked, make sure the clicked menu item remains checked.

            switch (tag)
            {
                case "Standard": ActiveView = UserView.Standard; break;
                case "Advanced": ActiveView = UserView.Advanced; break;
            }
        }

        #endregion

        /// <summary>
        /// Updates the position and size of the main window based on application settings.
        /// <para/>Supports multiple-screen desktops.
        /// </summary>
        /// <param name="window">Window to update dimensions of.</param>
        private void UpdateWindowDimensions(MainWindow window)
        {
            var totalDesktop = Rect.Empty;
            foreach (var s in Legacy.Screen.AllScreens)
            {
                var currentScreen = new Rect(s.WorkingArea.X, s.WorkingArea.Y, s.WorkingArea.Width, s.WorkingArea.Height);
                totalDesktop.Union(currentScreen);
            }
            if (Settings.Default.MainWindowX != -1)
            {
                window.Left = Math.Max(Settings.Default.MainWindowX, totalDesktop.Left);
                window.Width = Math.Max(10, Math.Min(Settings.Default.MainWindowWidth, totalDesktop.Width - totalDesktop.Left));
            }
            if (Settings.Default.MainWindowY != -1)
            {
                window.Top = Math.Max(Settings.Default.MainWindowY, totalDesktop.Top);
                window.Height = Math.Max(10, Math.Min(Settings.Default.MainWindowsHeight, totalDesktop.Height - totalDesktop.Top));
            }
        }

        /// <summary>
        /// Remembers the position and size of the main window by storing them in application settings.
        /// </summary>
        /// <param name="window">Window to remember dimensions of.</param>
        private void RememberWindowDimensions(MainWindow window)
        {
            Settings.Default.MainWindowsHeight = window.Height;
            Settings.Default.MainWindowWidth = window.Width;
            Settings.Default.MainWindowX = window.Left;
            Settings.Default.MainWindowY = window.Top;
        }

    }
}