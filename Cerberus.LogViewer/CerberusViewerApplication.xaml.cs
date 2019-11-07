using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using System.Diagnostics;
using System.Reflection;
using System.Globalization;
using System.IO;

namespace Microsoft.Localization.LocSolutions.Cerberus.LogViewer
{
    /// <summary>
    /// Interaction logic for CerberusViewerApplication.xaml
    /// </summary>
    public partial class CerberusViewerApplication : Application
    {
        /// <summary>
        /// Startup arguments passed into the application from the operating system.
        /// </summary>
        public string[] StartupArgs { get; private set; }

        /// <summary>
        /// Name of the source that for Windows EventLog mechanism.
        /// </summary>
        private string EventSource { get; set; }

        /// <summary>
        /// Current assembly's full name identification.
        /// </summary>
        public string ExeID { get; set; }
        /// <summary>
        /// Name of the exectuable. To be used in user-friendly test (help).
        /// </summary>
        public string ExeName { get; set; }
        /// <summary>
        /// Path to directory where this exe is located.
        /// </summary>
        public string ExeDir { get; set; }
        /// <summary>
        /// Reference to the assembly object containing this code (client exe).
        /// </summary>
        public Assembly ThisAssembly { get; set; }

        /// <summary>
        /// Initializes the application.
        /// </summary>
        public CerberusViewerApplication()
        {
            //3.2 key
            //Xceed.Wpf.DataGrid.Licenser.LicenseKey = "DGP32-PF1JG-78DHW-5HXA";
            //3.5 key
            Xceed.Wpf.DataGrid.Licenser.LicenseKey = "DGP35LEEPGTHM883YXA";
        }

        /// <summary>
        /// Quits the application immediately.
        /// </summary>
        public static void TerminateQuietly()
        {
            CerberusViewerApplication.Current.Shutdown();
        }

        /// <summary>
        /// Event handler called when the application starts up.
        /// </summary>
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            StartupArgs = e.Args;

            Initialize();
        }

        /// <summary>
        /// Initializes variables of Log viewer program instance on program startup.
        /// </summary>
        private void Initialize()
        {
            ThisAssembly = Assembly.GetExecutingAssembly();
            ExeID = string.Format(CultureInfo.InvariantCulture, "{0}, Version={1}", ThisAssembly.GetName().Name, ThisAssembly.GetName().Version);
            ExeName = ThisAssembly.ManifestModule.Name;
            ExeDir = Path.GetDirectoryName(ThisAssembly.Location);
            DispatcherUnhandledException += CerberusViewerApplication_DispatcherUnhandledException;

            EventSource = "Cerberus.LogViewer";

        }
        /// <summary>
        /// A chance for our application to trace any unhandled exception and store it in Windows event log.
        /// </summary>
        void CerberusViewerApplication_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            EventLog.WriteEntry(EventSource, string.Format("Unhandled exception intercepted by global handler: {0} - {1}. Stack trace: {2}.", e.Exception.GetType().Name, e.Exception.Message, e.Exception.StackTrace), EventLogEntryType.Error);
        }

        /// <summary>
        /// Reference to the current instance of the appliation.
        /// </summary>
        public static CerberusViewerApplication TheApp
        {
            get
            {
                return (CerberusViewerApplication)CerberusViewerApplication.Current;
            }
        }
    }
}
