using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows;
using Microsoft.Localization.LocSolutions.Cerberus.Configurator.Themes;
using Microsoft.Localization.LocSolutions.Cerberus.Core;
using Microsoft.Localization.LocSolutions.Cerberus.Configurator.Properties;
using System.Xml.Linq;
using System.Linq;

namespace Microsoft.Localization.LocSolutions.Cerberus.Configurator
{
    /// <summary>
    /// Interaction logic for ConfiguratorApplication.xaml
    /// </summary>
    public partial class ConfiguratorApplication : Application
    {
        private Office14Enlistment _enlistment;
        private DatabaseAsimo _asimo;
        private WPFThemeBase _uiTheme;

        /// <summary>
        /// Reference to Asimo that talks to OSLEBot.
        /// </summary>
        public DatabaseAsimo CurrentAsimo
        {
            get { return _asimo; }
        }

        /// <summary>
        /// Gets a reference to the only instance of <see cref="Office14Enlistment"/>.
        /// </summary>
        public Office14Enlistment Enlistment
        {
            get { return _enlistment; }
        }

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
        /// Data read from the underlying database that allows for queries to be run against.
        /// Calling code of Configurator can also make modifications to this data and commit it back to the underlying database for persistence.
        /// </summary>
        public CheckConfiguration Database { get; set; }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public ConfiguratorApplication()
        {
            var assemblyResolver = new Microsoft.Practices.AssemblyManagement.AssemblyResolver(Settings.Default.AssemblyResolvePaths.Cast<string>().ToArray());
            //attaches the resolver to the current AppDomain.
            assemblyResolver.Init();
        }

        /// <summary>
        /// Reacts to application startup event.
        /// </summary>
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            Initialize();
        }

        /// <summary>
        /// References the only instance of <see cref="ConfiguratorApplication"/>.
        /// </summary>
        public new static ConfiguratorApplication Current
        {
            get { return Application.Current as ConfiguratorApplication; }
        }

        /// <summary>
        /// Initializes variables of Configurator program instance on program startup.
        /// </summary>
        private void Initialize()
        {
            ThisAssembly = Assembly.GetExecutingAssembly();
            ExeID = string.Format(CultureInfo.InvariantCulture, "{0}, Version={1}", ThisAssembly.GetName().Name, ThisAssembly.GetName().Version);
            ExeName = ThisAssembly.ManifestModule.Name;
            ExeDir = Path.GetDirectoryName(ThisAssembly.Location);
            _enlistment = new Office14Enlistment();
            _asimo = new DatabaseAsimo(_enlistment);
            if (_enlistment.IsDetected) _asimo.CheckFolderPath = Path.Combine(_enlistment.CerberusHomeDir, "Checks");
            Database = new CheckConfiguration();
            _uiTheme = new StandardTheme();
        }

        /// <summary>
        /// Reference to UI objects factory that creates objects matching the theme's look and feel.
        /// </summary>
        public WPFThemeBase UITheme { get { return _uiTheme; } }
    }
}