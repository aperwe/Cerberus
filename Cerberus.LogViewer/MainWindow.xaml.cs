using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Threading;
using System.Collections;
using System.Windows.Controls;
using Forms20 = System.Windows.Forms;
using Xceed.Wpf.DataGrid.Views;
using System.Windows.Media;
using Xceed.Wpf.DataGrid.ThemePack;
using System.Windows.Media.Animation;
using System.Data;
using System.Collections.Generic;
using System.ComponentModel;
using System.Printing;
using Xceed.Wpf.DataGrid.Print;
using System.Windows.Documents;

namespace Microsoft.Localization.LocSolutions.Cerberus.LogViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Private members
        /// <summary>
        /// Dialog used for loading OSLEBot log files.
        /// </summary>
        private Forms20.OpenFileDialog OpenLogFileDialog;
        /// <summary>
        /// Dialog used for exporting the contents of the data grid into CSV format.
        /// </summary>
        private Forms20.SaveFileDialog ExportDataAsCsvFileDialog;
        /// <summary>
        /// Dialog used for exporting the contents of the data grid into Excel xmlss format.
        /// </summary>
        private Forms20.SaveFileDialog ExportDataAsExcelFileDialog;
        /// <summary>
        /// Dialog used for exporting the contents of the data grid into XPS format.
        /// </summary>
        private Forms20.SaveFileDialog ExportDataAsXpsFileDialog;
        #endregion

        #region Public members

        private OSLEBotOutputDataSet activeDataSet;
        /// <summary>
        /// Data set that has been loaded from the file indicated by <see cref="LogFileName"></see>.
        /// </summary>
        public OSLEBotOutputDataSet ActiveDataSet
        {
            get
            {
                return activeDataSet;
            }
            set
            {
                activeDataSet = value;
                Dispatcher.Invoke(() => MenuPrint.IsEnabled = MenuExport.IsEnabled = (activeDataSet != null));
            }
        }
        /// <summary>
        /// Name of the active log that is being viewed.
        /// </summary>
        public string LogFileName { get; private set; }
        #endregion

        #region Public API
        /// <summary>
        /// Default constructor.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            #region Initialize the dialog that loads OSLEBot log files.
            OpenLogFileDialog = new Forms20.OpenFileDialog
            {
                CheckFileExists = true,
                CheckPathExists = true,
                DefaultExt = "xml",
                Filter = "OSLEBot output files|*.xml|All files|*",
                Title = "Select log file created by Cerberus",
                AutoUpgradeEnabled = true,
                SupportMultiDottedExtensions = true,
                ValidateNames = true
            };
            OpenLogFileDialog.FileOk += FileSelectedInOpenFileDialog;
            #endregion

            #region Initialize the dialog that exports the contents of the data grid as CSV.
            ExportDataAsCsvFileDialog = new Forms20.SaveFileDialog
            {
                CheckFileExists = false,
                CheckPathExists = true,
                DefaultExt = "csv",
                Filter = "CSV files|*.csv|All files|*",
                Title = "Select path to export data as CSV file",
                AddExtension = true,
                AutoUpgradeEnabled = true,
                DereferenceLinks = true,
                CreatePrompt = false,
                OverwritePrompt = true,
                SupportMultiDottedExtensions = true,
                ValidateNames = true
            };
            ExportDataAsCsvFileDialog.FileOk += ExportAsCsv;
            #endregion

            #region Initialize the dialog that exports the contents of the data grid as Excel.
            ExportDataAsExcelFileDialog = new Forms20.SaveFileDialog
            {
                CheckFileExists = false,
                CheckPathExists = true,
                DefaultExt = "xmlss",
                Filter = "Excel XML Spreadsheet format files|*.xml|All files|*",
                Title = "Select path to export data as Excel XMLSS file",
                AddExtension = true,
                AutoUpgradeEnabled = true,
                DereferenceLinks = true,
                CreatePrompt = false,
                OverwritePrompt = true,
                SupportMultiDottedExtensions = true,
                ValidateNames = true
            };
            ExportDataAsExcelFileDialog.FileOk += ExportAsExcel;
            #endregion

            #region Initialize the dialog that exports the contents of the data grid as XPS.
            ExportDataAsXpsFileDialog = new Forms20.SaveFileDialog
            {
                CheckFileExists = false,
                CheckPathExists = true,
                DefaultExt = "xps",
                Filter = "XPS files|*.xps|All files|*",
                Title = "Select path to export data as XPS file",
                AddExtension = true,
                AutoUpgradeEnabled = true,
                DereferenceLinks = true,
                CreatePrompt = false,
                OverwritePrompt = true,
                SupportMultiDottedExtensions = true,
                ValidateNames = true
            };
            ExportDataAsXpsFileDialog.FileOk += ExportAsXps;
            #endregion
        }
        #endregion

        #region Event handlers

        /// <summary>
        /// User clicked on 'Close' button.
        /// </summary>
        private void ButtonClickedClose(object sender, RoutedEventArgs e)
        {
            CerberusViewerApplication.TerminateQuietly();
        }
        /// <summary>
        /// User clicked File->Close menu.
        /// </summary>
        private void MenuClickedClose(object sender, RoutedEventArgs e)
        {
            CerberusViewerApplication.TerminateQuietly();
        }
        /// <summary>
        /// User clicked File->Open menu.
        /// </summary>
        private void MenuClickedOpen(object sender, RoutedEventArgs e)
        {
            OpenLogFileDialog.ShowDialog();
        }

        /// <summary>
        /// Reacts to the user changing the view type of the grid using radio buttons.
        /// </summary>
        private void ViewTypeChanged(object sender, RoutedEventArgs e)
        {
            var type = ((RadioButton)sender).Content.ToString();
            if (OSLEBotDataGrid == null) return; //Ignore view switches happening before the view is initialized.
            UIViewBase newView = null;

            switch (type)
            {
                case "Cards": newView = TryFindResource("compactCardView") as UIViewBase; break;
                case "List": newView = TryFindResource("tableView") as UIViewBase; break;
                case "TableFlow": newView = TryFindResource("tableFlowView") as UIViewBase; break;
            }
            if (newView != null) OSLEBotDataGrid.View = newView;
        }
        #endregion

        /// <summary>
        /// Responds to the event when user has selected an OSLEBot output xml file for viewing and pressed 'OK' button.
        /// </summary>
        private void FileSelectedInOpenFileDialog(object sender, CancelEventArgs e)
        {
            var fd = (Forms20.OpenFileDialog)sender;
            var fileName = fd.FileName;
            LoadFile(fileName);
        }

        /// <summary>
        /// The window has finished initializing. See if there are command args and if so, try to load a file.
        /// </summary>
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            InitializeTitleBar();

            if (CerberusViewerApplication.TheApp.StartupArgs.Count() > 0)
            {
                LoadFile(CerberusViewerApplication.TheApp.StartupArgs[0]);
            }
        }

        /// <summary>
        /// Initializes title bar of the main window by adding current exe's version number.
        /// </summary>
        private void InitializeTitleBar()
        {
            Title = string.Format("Cerberus log viewer v{0}", CerberusViewerApplication.TheApp.ThisAssembly.GetName().Version);
        }

        /// <summary>
        /// Loads the OSLEBot output file name.
        /// </summary>
        /// <param name="fileName">OSLEBot output xml file name to load</param>
        private void LoadFile(string fileName)
        {
            if (!File.Exists(fileName))
            {
                System.Windows.MessageBox.Show(string.Format("The specified file: '{0}' does not exist or cannot be located.", fileName), "Cerberus log viewer", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                OSLEBotDataGrid.Visibility = Visibility.Hidden;
                return;
            }
            BindData(fileName);
        }

        /// <summary>
        /// Binds data from an XML file to the data table.
        /// </summary>
        /// <param name="fileName">XML file name containing data.</param>
        private void BindData(string fileName)
        {
            BindDataAsync(fileName); //Asynchronous load, because it is very time consuming.
        }

        /// <summary>
        /// Synchronous processing (old) of the input datasource. Deprecated because it is very time consuming.
        /// </summary>
        /// <param name="fileName">Name of the file to process</param>
        private void BindDataSync(string fileName)
        {
            var fileNameWithoutPath = Path.GetFileName(fileName);
            StatusText.Content = string.Format("Loading {0} file please wait...", fileNameWithoutPath);

            var data = LoadDataSource(fileName);
            OSLEBotDataGrid.ItemsSource = data;
            OSLEBotDataGrid.Visibility = Visibility.Visible;
            StatusText.Content = string.Format("Loaded \"{0}\" -> {1} items", fileNameWithoutPath, data.Count);
        }

        /// <summary>
        /// Asynchronous processing of the input datasource because it is very time consuming.
        /// </summary>
        /// <param name="fileName">Name of the file to process</param>
        private void BindDataAsync(string fileName)
        {
            var fileNameWithoutPath = Path.GetFileName(fileName);

            var sp = TryFindResource("BindingInProgress") as StackPanel;
            var pb = TryFindResource("loaderProgressBar") as ProgressBar;
            sp.Children.Clear(); //Have to clear, because resources loaded via TryFindResource() are cached and and exist all the time.
            sp.Children.Add(new TextBlock { Text = string.Format("Loading \"{0}\" file please wait...", fileNameWithoutPath) });
            sp.Children.Add(pb);
            StatusText.Content = sp;

            pb.BeginAnimation(ProgressBar.ValueProperty, TryFindResource("progress") as DoubleAnimation); //Some animation for the progress bar

            #region This piece runs asynchronously
            ThreadPool.QueueUserWorkItem(state =>
            {
                var data = LoadDataSource(fileName);
                 
                //This piece has to be run on UI thread.
                Dispatcher.Invoke(delegate
                    {
                        OSLEBotDataGrid.ItemsSource = data;
                        OSLEBotDataGrid.Visibility = Visibility.Visible;
                        StatusText.Content = string.Format("Loaded \"{0}\" -> {1} items", fileNameWithoutPath, data.Count);
                    });
            });
            #endregion

        }

        /// <summary>
        /// Loads data from the data source.
        /// </summary>
        private IList<ReportItem> LoadDataSource(string fileName)
        {
            var ds = new OSLEBotOutputDataSet();
            var readResult = ds.ReadXml(fileName, XmlReadMode.ReadSchema);

            //No exception thrown, so safe to initialize member properties.
            LogFileName = fileName;
            ActiveDataSet = ds;
            var displayableReportData = ActiveDataSet.GetReport();

            return displayableReportData;
        }

        /// <summary>
        /// Export as CSV the current content.
        /// </summary>
        private void MenuClickedExportAsCsv(object sender, RoutedEventArgs e)
        {
            ExportDataAsCsvFileDialog.ShowDialog();
        }

        /// <summary>
        /// Responds to the event when user has selected a file name to save and pressed 'OK' button.
        /// </summary>
        private void ExportAsCsv(object sender, CancelEventArgs e)
        {
            var fd = (Forms20.SaveFileDialog)sender;
            var fileName = fd.FileName;
            var fileNameWithoutPath = Path.GetFileName(fileName);

            StatusText.Content = string.Format("Exporting data to {0} file is in progress...", fileNameWithoutPath);
            ThreadPool.QueueUserWorkItem((state) =>
            {
                Dispatcher.Invoke(delegate
                {
                    OSLEBotDataGrid.ExportToCsv(fileName);
                    StatusText.Content = string.Format("Data export to {0} completed", fileNameWithoutPath);
                });
            });
        }

        /// <summary>
        /// Export as Excel format.
        /// </summary>
        private void MenuClickedExportAsExcel(object sender, RoutedEventArgs e)
        {
            ExportDataAsExcelFileDialog.ShowDialog();
        }

        /// <summary>
        /// Responds to the event when user has selected a file name to save and pressed 'OK' button.
        /// </summary>
        private void ExportAsExcel(object sender, CancelEventArgs e)
        {
            var fd = (Forms20.SaveFileDialog)sender;
            var fileName = fd.FileName;
            var fileNameWithoutPath = Path.GetFileName(fileName);

            StatusText.Content = string.Format("Exporting data to {0} file is in progress...", fileNameWithoutPath);
            ThreadPool.QueueUserWorkItem((state) =>
            {
                Dispatcher.Invoke(delegate
                {
                    OSLEBotDataGrid.ExportToExcel(fileName);
                    StatusText.Content = string.Format("Data export to {0} completed", fileNameWithoutPath);
                });
            });
        }

        /// <summary>
        /// Export as XPS format.
        /// </summary>
        private void MenuClickedExportAsXps(object sender, RoutedEventArgs e)
        {
            ExportDataAsXpsFileDialog.ShowDialog();
        }

        /// <summary>
        /// Responds to the event when user has selected a file name to save and pressed 'OK' button.
        /// </summary>
        private void ExportAsXps(object sender, CancelEventArgs e)
        {
            var fd = (Forms20.SaveFileDialog)sender;
            var fileName = fd.FileName;
            var fileNameWithoutPath = Path.GetFileName(fileName);

            StatusText.Content = string.Format("Exporting data to {0} file is in progress...", fileNameWithoutPath);
            ThreadPool.QueueUserWorkItem((state) =>
            {
                const double a4Width = 8.3 * 96; //inches * dpi
                const double a4Height = 11.7 * 96; //inches * dpi

                Dispatcher.Invoke(delegate
                {
                    OSLEBotDataGrid.ExportToXps(fileName, new Size(a4Width, a4Height), true);
                    StatusText.Content = string.Format("Data export to {0} completed", fileNameWithoutPath);
                });
            });
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            var printDialog = new PrintDialog();
            if (printDialog.ShowDialog().GetValueOrDefault())
            {
                DataGridPaginator dataGridPaginator = ((IDocumentPaginatorSource)OSLEBotDataGrid).DocumentPaginator as DataGridPaginator;
                dataGridPaginator.InitializeSettings(printDialog);

                dataGridPaginator.PageRange = printDialog.PageRange;
                printDialog.PrintDocument(dataGridPaginator, LogFileName);
            }
        }
    }

}
