using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Microsoft.Localization.LocSolutions.Cerberus.Core;
using Microsoft.Localization.LocSolutions.Cerberus.Executor.Properties;
using Microsoft.Localization.LocSolutions.Logger;
using System.Collections;

namespace Microsoft.Localization.LocSolutions.Cerberus.Executor
{
    /// <summary>
    /// Contains entry point into Executor program.
    /// </summary>
    public class Program
    {
        private static Program _instance;
        private CerberusConsoleLogger _consoleLogger;

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
            //create and initialize an assembly resolver that will allow Cerberus to find OSLEBot libraries
            var assemblyResolver = new Microsoft.Practices.AssemblyManagement.AssemblyResolver(Settings.Default.AssemblyResolvePaths.Cast<string>().ToArray());
            //attaches the resolver to the current AppDomain.
            assemblyResolver.Init();
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

            var arguments = new Arguments(args);

            foreach (var arg in new[]{"config", "response", "output", "enginelog"})
            {
                if (!arguments.Contains(arg))
                {
                    LoggerSAP.Error("Missing argument: {0}", arg);
                    LoggerSAP.Log();
                    ShowUsage();
                    return ProgramReturnValue.ErrorInvalidCommandLine;
                }
            }
            // verify arguments are valid
            // check if input files exist
            foreach (var arg in new[]{"config", "response"})
            {
                
                if (!File.Exists(arguments[arg]))
                {
                    LoggerSAP.Error("Argument \"{0}\" points to a non-existing file:\n{1}", arg, arguments[arg]);
                    return ProgramReturnValue.ErrorInvalidCommandLine;
                }
            }
            // check if output files are valid file names
            foreach (var arg in new[] { "output", "enginelog" })
            {
                FileInfo fileInfo = null;
                string fileNameError = null;
                try{
                    fileInfo = new FileInfo(arguments[arg]);
                }
                catch(ArgumentException)
                {
                    fileNameError = "Filename contains incorrect characters";
                }
                catch(PathTooLongException)
                {
                    fileNameError = "File path is too long";
                }
                catch(NotSupportedException)
                {
                    fileNameError = "Filename contains a colon (:)";
                }
                if (!String.IsNullOrEmpty(fileNameError))
                {
                    LoggerSAP.Error("Argument \"{0}\" contains invalid file name:\n{1}", arg, fileNameError);
                    return ProgramReturnValue.ErrorInvalidCommandLine;
                }
            }
            LoggerSAP.Log("{0} is starting.", ExeID);

            //OSLEBot engine executes using ThreadPool - each rule is queued separately.
            //since OSLEBot code does block threads, many extra threads will be created by ThreadPool which may impair performance.
            //setting max threadpool threads will limit the number of threadpool threads
            ThreadPool.SetMaxThreads(Environment.ProcessorCount, Environment.ProcessorCount);

            var executor = new OSLEBotExecutor();
            try
            {
                var result = executor.Run(arguments["response"], arguments["config"], arguments["output"], arguments["enginelog"]);
                if (result != OSLEBotExecutor.OSLEBotResult.Success)
                    throw new InvalidOperationException("OSLEBot failed.");
            }
            catch (Exception e)
            {
                LoggerSAP.Critical("OSLEBot execution unsuccessful, because: {0}: {1}", e.ToString(), e.Message);
                return ProgramReturnValue.InternalFailure;
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
            LoggerSAP.Log("{0} -config=<path to Cerberus config> -response=<path to LCL response file> -output=<path to output file> -enginelog=<path to OSLEBot engine log>", ExeName);
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
        /// Indicates that at least one command line argument was invalid.
        /// </summary>
        ErrorInvalidCommandLine,
        /// <summary>
        /// Indicates that the program failed to finish successfully because of a missing directory (e.g. Checks directory).
        /// </summary>
        ErrorMissingDirectory,

        InternalFailure
    }
}