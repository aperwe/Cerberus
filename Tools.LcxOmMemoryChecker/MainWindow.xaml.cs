using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;
using Microsoft.Localization.LocSolutions.Cerberus.Configurator;
using System.Diagnostics;
using System.Windows.Forms;

namespace Microsoft.Localization.LocSolutions.Tools.LcxOmMemoryChecker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Responds to user selecting menu File -> Open -> OSLEBot response file.
        /// </summary>
        private void LoadOsleBotResponseFile(object sender, RoutedEventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog { AutoUpgradeEnabled = true, CheckFileExists = true })
            {
                ofd.FileOk += FileSelected;
                ofd.Filter = "Response file|*.txt";
                ofd.Title = "Select an OSLEBot response file to load";
                ofd.Multiselect = false;
                ofd.ShowDialog();
            }
        }

        /// <summary>
        /// User selected a file.
        /// </summary>
        void FileSelected(object sender, System.ComponentModel.CancelEventArgs e)
        {
            OpenFileDialog ofd = (OpenFileDialog)sender;
            ThreadPool.QueueUserWorkItem((x) =>
            {
                var path = ofd.FileName;
                Facade facade = new Facade(path);
                facade.LoadingStarted += EventHandlerForLoadingStarted;
                facade.ItemLoaded += EventHandlerForItemLoaded;
                facade.LoadingFinished += EventHandlerForoadingFinished;
                facade.LoadAll();
            });
        }

        void EventHandlerForLoadingStarted(object sender, LcxLoaderEventData e)
        {
            this.InvokeOnUIThread(() =>
            {
                OutputList.Items.Clear();
                OutputList.Items.Add(new LcxFile { Type = MessageType.Started, WorkingSet = Process.GetCurrentProcess().WorkingSet64 });
            });
        }

        void EventHandlerForItemLoaded(object sender, LcxLoaderEventData e)
        {
            this.InvokeAsynchronouslyOnUIThread(() =>
            {
                OutputList.Items.Add(new LcxFile { Data = e.Data, Type = MessageType.File, WorkingSet = Process.GetCurrentProcess().WorkingSet64, Content = e.FileName });
            });
        }

        void EventHandlerForoadingFinished(object sender, LcxLoaderEventData e)
        {
            this.InvokeAsynchronouslyOnUIThread(() =>
            {
                OutputList.Items.Add(new LcxFile { Type = MessageType.Finished, WorkingSet = Process.GetCurrentProcess().WorkingSet64 });
            });
        }

        private void CloseButtonClicked(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();

        }

    }
}