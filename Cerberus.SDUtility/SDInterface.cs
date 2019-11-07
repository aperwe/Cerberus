using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Globalization;
using System.IO;

namespace Microsoft.Localization.LocSolutions.Cerberus.SDUtility
{
    /// <summary>
    /// Indicates possible program execution results that are returned to operating system as <see cref="System.Int32"/>.
    /// </summary>
    public enum ExecutionResult
    {
        /// <summary>
        /// Execution was OK.
        /// </summary>
        Success,
        /// <summary>
        /// Invalid arguments were specified on command line.
        /// </summary>
        InvalidArguments,
        /// <summary>
        /// Invalid program state. Program tried to execute synchronization according to user-supplied parameters
        /// but the selected run mode was unrecognized (unexpected). This is a bug to the dev team.
        /// </summary>
        UnsupportedRunMode,
        /// <summary>
        /// Tried to synchronize, but enlistment was not detected.
        /// </summary>
        NoEnlistment,
        /// <summary>
        /// Tried to synchronize, but could not locate sd.ini file in the expected location.
        /// </summary>
        BadEnlistment,
        /// <summary>
        /// While trying to synchronize it turned out that wrong language name was specified. Cannot continue with syncing.
        /// </summary>
        WrongLanguage,
        /// <summary>
        /// While trying to synchronize it was detected that files were opened (edited) in the file pattern.
        /// When synchronizing, no files must be opened for edits.
        /// </summary>
        BadEnlistmentStateFilesOpened,
        /// <summary>
        /// Attempted to sync the maximum number of times, but the sync failed.
        /// </summary>
        FailedSync,
        /// <summary>
        /// Attempted to perform a source depot operation, but connection to source depot failed (network down or server inaccessible).
        /// </summary>
        BadEnlistmentStateConnectionFailed,
        /// <summary>
        /// Store attempted to sync to a checkpoint label, but the label was not available.
        /// </summary>
        FailedSyncLabelNotPresent,
    }

    /// <summary>
    /// This object is an entry point to all the logic in SDUtility.
    /// </summary>
    public class SDInterface
    {
        /// <summary>
        /// Arguments passed from command line.
        /// </summary>
        public string[] CommandLine { get; private set; }

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
        /// Command line arguments parsed into strongly-typed data structure.
        /// </summary>
        public ArgumentParser ParsedArguments { get; private set; }

        /// <summary>
        /// Constructs an instance of <see cref="SDInterface"/>.
        /// </summary>
        /// <param name="args">Arguments from command line.</param>
        public SDInterface(string[] args)
        {
            CommandLine = args;
            ParsedArguments = ArgumentParser.Parse(CommandLine);
        }

        /// <summary>
        /// Exectues all the logic of this program.
        /// </summary>
        /// <returns>Returns a return value that should be passed to the operating system.</returns>
        public ExecutionResult Execute()
        {
            Initialize();

            if (!ParsedArguments.Correct)
            {
                if (string.IsNullOrEmpty(ParsedArguments.InvalidArgument))
                {
                    Console.WriteLine("Invalid arguments.");
                    if (ParsedArguments.Languages.Count() == 0)
                    {
                        Console.WriteLine("At least one language must be specified.");
                    }
                }
                else
                {
                    Console.WriteLine("Invalid argument: {0}", ParsedArguments.InvalidArgument);
                }
                ShowUsage();
                return ExecutionResult.InvalidArguments;
            }

            if (ParsedArguments.RunMode == ProgramRunMode.ShowHelpAndQuit)
            {
                ShowUsage();
                return ExecutionResult.Success;
            }

            SDSynchronizer synchronizer = null;
            switch (ParsedArguments.RunMode)
            {
                case  ProgramRunMode.SyncCore:
                    synchronizer = new CoreSynchronizer(ParsedArguments.Languages);
                    break;
                case ProgramRunMode.SyncStore:
                    synchronizer = new StoreSynchronizer(ParsedArguments.Languages, ParsedArguments.CheckpointNumber);
                    break;
                default:
                    Console.WriteLine("Unsupported run mode: {0}. Please contact arturp@microsoft.com", ParsedArguments.RunMode);
                    return ExecutionResult.UnsupportedRunMode;
            }
            var synchronizationResults = synchronizer.Synchronize();
            return synchronizationResults;
        }

        /// <summary>
        /// Prints out usage of the tool to console window.
        /// </summary>
        public void ShowUsage()
        {
            Console.WriteLine("Cerberus SD synchronization utility ({0})", ExeID);
            Console.WriteLine();
            Console.WriteLine("Usage:");
            Console.WriteLine("{0} {{(-store <checkpoint>) | -core}} <ll-cc> [<ll-cc> ...]", ExeName);
            Console.WriteLine("   -store - instructs the tool to sync LCT files in the store.");
            Console.WriteLine("            With this mode, checkpoint number must be specified.");
            Console.WriteLine("            (NOTE: You need to be enlisted to store.");
            Console.WriteLine("   -core  - instructs the tool to sync LCL files in core enlistment.");
            Console.WriteLine("            With this mode, you cannot specify the checkpoint.");
            Console.WriteLine("   checkpoint - Specifies the number of checkpoint build to sync to.");
            Console.WriteLine("   ll-cc  - language(s) which you want to sync LCL or LCT files for.");
            Console.WriteLine();
            Console.WriteLine("Example:");
            Console.WriteLine("{0} -core pl-pl", ExeName);
            Console.WriteLine("{0} -store 4229 ru-ru", ExeName);
        }

        /// <summary>
        /// Initializes variables of Executor Program instance on program startup.
        /// </summary>
        private void Initialize()
        {
            ThisAssembly = Assembly.GetExecutingAssembly();
            ExeID = string.Format(CultureInfo.InvariantCulture, "{0}, Version={1}", ThisAssembly.GetName().Name, ThisAssembly.GetName().Version);
            ExeName = ThisAssembly.ManifestModule.Name;
            ExeDir = Path.GetDirectoryName(ThisAssembly.Location);
        }

    }
}
