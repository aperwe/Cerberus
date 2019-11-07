using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Microsoft.Localization.LocSolutions.Cerberus.Core;
using Microsoft.Localization.LocSolutions.Cerberus.Executor.Properties;
using Microsoft.Localization.LocSolutions.Logger;

namespace Microsoft.Localization.LocSolutions.Cerberus.Executor
{
    /// <summary>
    /// Contains entry point into Executor program.
    /// </summary>
    public class Program
    {
        private static Program _instance;
        private CerberusConsoleLogger _consoleLogger;
        private Office14Enlistment _enlistment;
        private AsimoBase _asimo;

        /// <summary>
        /// Current assembly's full name identification.
        /// </summary>
        private string ExeID { get; set; }
        /// <summary>
        /// Name of the exectuable. To be used in user-friendly test (help).
        /// </summary>
        private string ExeName { get; set; }
        /// <summary>
        /// Path to directory where this exe is located.
        /// </summary>
        private string ExeDir { get; set; }
        /// <summary>
        /// Reference to the assembly object containing this code (client exe).
        /// </summary>
        private Assembly ThisAssembly { get; set; }

        /// <summary>
        /// Instance of Program created in Main(). Not used by external (test) callers.
        /// </summary>
        public static Program Instance
        {
            get { return _instance; }
        }

        /// <summary>
        /// Application entry point.
        /// </summary>
        /// <param name="args">Command line arguments.</param>
        static int Main(string[] args)
        {
            _instance = new Program();
            SetThreadName();
            Instance.EnableLogging();
            var programResult = Instance.RunProgram(args);
            Instance.DisableLogging();
            return (int)programResult;
        }

        /// <summary>
        /// Sets default thread name of the main thread.
        /// </summary>
        private static void SetThreadName()
        {
            try
            {
                Thread.CurrentThread.Name = "Executor";
            }
            catch (InvalidOperationException)
            {
                //Thrown when thread name has already been set and could not be set again.
            }
        }

        private void DisableLogging()
        {
            DisableConsoleLogger();
        }

        private void DisableConsoleLogger()
        {
            if (_consoleLogger == null) return;
            LoggerSAP.UnregisterLogger(_consoleLogger);
            _consoleLogger = null;
        }

        private void EnableLogging()
        {
            EnableConsoleLogger();
        }

        private void EnableConsoleLogger()
        {
            _consoleLogger = new CerberusConsoleLogger();
            LoggerSAP.RegisterLogger(_consoleLogger, LogLevel.Verbose);
        }

        /// <summary>
        /// Program's entry point on a program instance. Useful for testing program behavior.
        /// </summary>
        /// <param name="args">Command line arguments</param>
        /// <returns>Success indicator</returns>
        public ProgramReturnValue RunProgram(string[] args)
        {
            Initialize();
            IntroduceYourself();

            var input = ArgumentParser.Parse(args);
            if (!input.Correct)
            {
                LoggerSAP.Error("Unexpected argument: {0}", input.InvalidArgument);
                LoggerSAP.Log();
                ShowUsage();
                return ProgramReturnValue.ErrorInvalidCommandLine;
            }

            LoggerSAP.Log("{0} is starting.", ExeID);
            if (!_enlistment.IsDetected)
            {
                LoggerSAP.Error(
                    "Office 14 enlistment not detected. This program must be run from within Office14 development environment.");
                return ProgramReturnValue.ErrorNoEnlistment;
            }
            LoggerSAP.Trace("Enlistment detected at {0}", _enlistment.Location);
            var unfilteredFiles = _enlistment.GetFiles("intl", LcxType.Lcl);
            switch (input.RunMode)
            {
                case ProgramRunMode.StandaloneLocGroups:
                    _asimo = new StandAloneAsimoLocGroupBreakdown(_enlistment);
                    break;
                case ProgramRunMode.StandaloneProjects:
                    _asimo = new StandAloneAsimoProjectBreakdown(_enlistment);
                    break;
                case ProgramRunMode.DatabaseCentralized:
                    _asimo = new DatabaseAsimo(_enlistment);
                    break;
                case ProgramRunMode.ShowHelpAndQuit:
                    ShowUsage();
                    return ProgramReturnValue.Success;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            _asimo.CheckFolderPath = Path.Combine(_enlistment.Location, Settings.Default.ChecksRelativePath);
            try
            {
                if (TestMode)
                {
                    _asimo.Run(unfilteredFiles.Take(10), input.Languages, input.Projects);
                }
                else
                {
                    _asimo.Run(unfilteredFiles, input.Languages, input.Projects);
                }
            }
            catch (DirectoryNotFoundException e)
            {
                LoggerSAP.Error("OSLEBot execution unsuccessful, because: {0}", e.Message);
                return ProgramReturnValue.ErrorMissingDirectory;
            }
            return ProgramReturnValue.Success;
        }

        /// <summary>
        /// Test should set it to true. Normal execution leaves it at false.
        /// </summary>
        public bool TestMode { get; set; }

        /// <summary>
        /// Initializes variables of Executor Program instance on program startup.
        /// </summary>
        private void Initialize()
        {
            ThisAssembly = Assembly.GetExecutingAssembly();
            ExeID = string.Format(CultureInfo.InvariantCulture, "{0}, Version={1}", ThisAssembly.GetName().Name, ThisAssembly.GetName().Version);
            ExeName = ThisAssembly.ManifestModule.Name;
            ExeDir = Path.GetDirectoryName(ThisAssembly.Location);
            _enlistment = new Office14Enlistment();
        }

        /// <summary>
        /// Displays general introduction of the tool.
        /// </summary>
        private void IntroduceYourself()
        {
            LoggerSAP.Log("{0} ({1})", ExeName, ExeID); //Introduce yourself
            LoggerSAP.Log("Product created on May 2009. Support alias: {0}.", Settings.Default.SupportDetails);
            LoggerSAP.Log("Product specification: {0}", Settings.Default.DocumentationLink);
            LoggerSAP.Log();
        }

        /// <summary>
        /// Shows usage to the command line.
        /// <para/>This is invoked when invalid command line is specified.
        /// </summary>
        private void ShowUsage()
        {
            LoggerSAP.Log("Usage:");
            LoggerSAP.Log("{0} [-l <ll-cc> [-l <ll-cc>[...]]] [-p <project> [-p <project> [...]]] [-m <mode>]", ExeName);
            LoggerSAP.Log("Where ll-cc is a language identfier, for example 'de-DE'");
            LoggerSAP.Log("      project is a project identifier, for example 'word'.");
            LoggerSAP.Log("      mode is one of {'locgroups', 'projects', 'centralized'}.");
            LoggerSAP.Log("         locgroups   - standalone mode with one instance of OSLEBot created per every locgroup.");
            LoggerSAP.Log("         projects    - standalone mode with one instance of OSLEBot created per every project.");
            LoggerSAP.Log("         centralized - same as 'projects' mode, but information about which checks are enabled for each {language, project} is obtained from a database.");
            LoggerSAP.Log("Sample command line: {0} -l fr-FR -l de-DE -p word -p mso", ExeName);
            LoggerSAP.Log("                     {0} -l fr-FR -m centralized", ExeName);
            LoggerSAP.Log("Without parameters, all files in enlistment are scanned and {0} runs in standalone mode.", ExeName);
        }
    }

    /// <summary>
    /// Possible return values of Cerberus Executor program when run from console.
    /// </summary>
    public enum ProgramReturnValue
    {
        /// <summary>
        /// Indicates that program execution was successful.
        /// </summary>
        Success,
        /// <summary>
        /// Indicates an error because enlistment was not detected.
        /// </summary>
        ErrorNoEnlistment,
        /// <summary>
        /// Indicates that at least one command line argument was invalid.
        /// </summary>
        ErrorInvalidCommandLine,
        /// <summary>
        /// Indicates that the program failed to finish successfully because of a missing directory (e.g. Checks directory).
        /// </summary>
        ErrorMissingDirectory
    }
}