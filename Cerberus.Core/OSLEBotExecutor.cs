using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Microsoft.Localization.OSLEBot.Core.Engine;
using Microsoft.Localization.LocSolutions.Logger;
using Microsoft.Localization.OSLEBot.DataSources;
using Microsoft.Localization.OSLEBot.DataSource.LocResourcePropertyAdapters;
using Microsoft.Localization.OSLEBot.DataAdapters;
using Microsoft.Localization.OSLEBot.ClassificationObjects;
using Microsoft.Localization.OSLEBot.Core.DataSource.DataSourcePackages;
using Microsoft.Localization.OSLEBot.Core.Misc;
using Microsoft.Localization.OSLEBot.Output;
using System.Xml.Linq;
using System.Globalization;
using Microsoft.Localization.OSLEBot.Exceptions;
using System.Reflection;
using System.IO;

namespace Microsoft.Localization.LocSolutions.Cerberus.Core
{
    /// <summary>
    /// This class takes 2 inputs: a cerberus config database and a response file with a collection of LCX files to be processed.
    /// It then builds an OSLEBot Configuration and executes OSLEBot with that configuration.
    /// </summary>
    public class OSLEBotExecutor
    {
        /// <summary>
        /// Runs OSLEBot on a set of files. The set of files specified in response file is cross-joined with data in configuration file
        /// to produce an input configuration for OSLEBot engine.
        /// </summary>
        /// <param name="responseFilePath">A list of references to LCX files for OSLEBot processing. Includes additional metadata about project and locgroup.</param>
        /// <param name="cerberusConfigPath">A Cerberus configuration database which defines the checks that are to be applied by OSLEBot.</param>
        /// <param name="cerberusOutputPath">A path to be used by Cerberus to create merged output.</param>
        public OSLEBotResult Run(string responseFilePath, string cerberusConfigPath, string cerberusOutputPath, string oslebotEngineLogPath)
        {
            OSLEBotResult result = OSLEBotResult.Success;

            if (responseFilePath == null) throw new ArgumentNullException("responseFilePath");
            if (cerberusConfigPath == null) throw new ArgumentNullException("cerberusConfigPath");
            if (cerberusOutputPath == null) throw new ArgumentNullException("ceberusOutputPath");
            if (oslebotEngineLogPath == null) throw new ArgumentNullException("oslebotEngineLogPath");

            IList<InputFileItem> fileList = null;
            try
            {
                fileList = LoadResponseFile(responseFilePath);
            }
            catch (Exception e)
            {
                throw;
            }
            var readOnlyDB = new CheckConfiguration();
            LoadDataFromDatabase(readOnlyDB, cerberusConfigPath);

            //initialize configuration helper. WARNING: thread unsafe if multiple configurations are ever to be used.
            ConfigurationHelper.Initialize(readOnlyDB);

            var checkConfigs = ReadCheckConfigsFromDatabase(readOnlyDB);

            var inputCfg = CreateConfig(fileList, checkConfigs, cerberusOutputPath, oslebotEngineLogPath);

            try
            {
                if (inputCfg.DataSourcePkgs == null || inputCfg.DataSourcePkgs.Count == 0)
                {
                    //Skip the execution when there is 0 file matches for the specified {language, project} pair.
                }

                var engine = new OSLEBotEngine(inputCfg);
                var engineRan = engine.StartRun();
                if (engineRan)
                {
                    LoggerSAP.Trace("Cerberus is waiting for engine activity to complete...");
                    engine.WaitForJobFinish(); //Wait for complete stop of activity.
                    engine.Cleanup();
                }
                if (engine.EngineLoggedErrors)
                {
                    LoggerSAP.Error("Engine reported some execution errors.");
                    result = OSLEBotResult.HandledExceptions;
                }
            }
            // catch and log any exception that was not handled by OSLEBot engine
            catch (Exception e)
            {
                LoggerSAP.Error("OSLEBot engine failed with the following exception:\n{0}", e.GetExceptionDetails(true));
                result = OSLEBotResult.HandledExceptions;
            }

            return result;
        }

        /// <summary>
        /// Reads information about all checks stored in the configuration database, including information about what items
        /// each check is enabled on, and transforms it into a strongly typed list of check config items that can be easily consumed
        /// by the caller to specify which input should be checked by which check.
        /// </summary>
        /// <param name="configuration">Configuration database</param>
        private static IList<CheckConfig> ReadCheckConfigsFromDatabase(CheckConfiguration configuration)
        {
            var checkConfigs = new List<CheckConfig>();
            var allChecks = configuration.GetAllChecks();

            foreach (var check in allChecks)
            {
                bool isAllLangs = configuration.IsCheckGlobalForAllLanguages(check);
                IList<CultureInfo> enabledLangs = null;
                if (!isAllLangs)
                {
                    enabledLangs = configuration.GetLanguagesWithCheckEnabled(check).Select(lang => new CultureInfo(lang)).ToList();
                }

                bool isAllProjects = configuration.IsCheckGlobalForAllProjects(check);
                IList<string> enabledProjects = null;
                if (!isAllProjects)
                {
                    enabledProjects = configuration.GetProjectsWithCheckEnabled(check).ToList();
                }
                //Extra check for presence of physical file for this check.
                //If the file is not present, log a warning, and skip the check, but allow the executor to continue.
                var phycalFile = Environment.ExpandEnvironmentVariables(configuration.GetCheckFilePath(check));
                if (File.Exists(phycalFile))
                {
                    checkConfigs.Add(new CheckConfig(
                        check,
                        configuration.GetCheckFilePath(check),
                        isAllLangs,
                        enabledLangs,
                        isAllProjects,
                        enabledProjects,
                        RuleContainerType.Source // we are hard-coding the type of rule to source files
                        ));
                }
                else //The physical file does not exist. Log a warning
                {
                    LoggerSAP.Warning("Could not locate '{0}' file containing source code of {1} check. This check will be ignored.", configuration.GetCheckFilePath(check), check);
                }
            }
            return checkConfigs;
        }

        /// <summary>
        /// Processes the response file and transforms it into a strongly-typed list of input files with metadata properties.
        /// </summary>
        /// <param name="responseFilePath">Path to response file that should be transformed.</param>
        private static IList<InputFileItem> LoadResponseFile(string responseFilePath)
        {
            var doc = XDocument.Parse(String.Format("<root>{0}</root>", System.IO.File.ReadAllText(responseFilePath)));
            return doc.Descendants(XName.Get("file")).Select(x => new InputFileItem
                {
                    File = x.Value,
                    Project = x.Attribute(XName.Get("Project")).Value,
                    BuildNumber = x.Attribute(XName.Get("BuildNumber")).Value,
                    LocGroup = x.Attribute(XName.Get("Locgroup")).Value,
                }).ToList();
        }

        /// <summary>
        /// Loads data from the disk database (file or SQL) as in-memory data for manipulation.
        /// </summary>
        private void LoadDataFromDatabase(DataSet database, string cerberusConfigPath)
        {
            try
            {
                database.ReadXml(cerberusConfigPath);
            }
            catch (Exception e)
            {
                LoggerSAP.Critical("Failed to load Cerberus Configuration from: {0}. Exception: {1}, message: {2}", cerberusConfigPath, e.ToString(), e.Message);
                throw;
            }
        }
        /// <summary>
        /// Builds OSLEBot configuration based on file list and Cerberus check configuration.
        /// </summary>
        /// <param name="fileList">List of files to be converted into OSLEBot data sources.</param>
        /// <param name="checkConfigs">List of checks that will be executed against the data sources.</param>
        /// <param name="cerberusOutputPath">Full path to Cerberus output XML file.</param>
        /// <returns>Configuration data that can be passed into OSLEBot for execution.</returns>
        internal static EngineConfig CreateConfig(IEnumerable<InputFileItem> fileList, IEnumerable<CheckConfig> checkConfigs, string cerberusOutputPath, string oslebotEngineLogPath)
        {
            var configuration = new EngineConfig { EngineLog = oslebotEngineLogPath };


            #region DataSourceProviderTypes
            configuration.AddDataSourceProvider<LcxDataSource>();
            configuration.AddDataSourceProvider<ConfigDictionaryDataSource>();
            #endregion

            #region PropertyAdapterTypes
            configuration.AddPropertyAdapter<LocItemPropertyAdapter>();
            configuration.AddPropertyAdapter<ConfigDictPropertyAdapter>();
            configuration.AddPropertyAdapter<LocResourceSelfPropertyAdapter>(); //Although we don't use it explicitly, LCXLocResourceDataAdapter.LoadObjects() will crash when starting OSLEBot because it expects this property adapter to be available.???
            #endregion

            #region COAdatpers
            configuration.AddCOAdapter<LCXLocResourceDataAdapter>();
            #endregion

            #region COTypes
            configuration.AddClassificationObject<LocResource>();
            #endregion

            #region DataSourcePackages
            foreach (var file in fileList)
            {
                var package = new DataSourcePackage(); //This package will contain 2 data sources
                package.SetCOType<LocResource>();
                var dataSource = new DataSourceInfo();
                dataSource.SetSourceType<LocDocument>();
                dataSource.SetSourceLocation(file.File);
                package.AddDataSource(dataSource);

                dataSource = new DataSourceInfo();
                dataSource.SetSourceType<ConfigDictionary>();

                var staticProperties = new ConfigDictionary
                                           {
                                               {"BuildNumber", file.BuildNumber},
                                               {"Project", file.Project},
                                               {"Locgroup", file.LocGroup}
                                           };
                dataSource.SetSourceLocation(staticProperties);
                package.AddDataSource(dataSource);
                configuration.AddDataSourcePackage(package);
            }
            #endregion

            #region Add checks
            // only add checks that are not globally disabled (could never execute)
            foreach (var check in checkConfigs.Where(ch => !ch.IsGloballyDisabled))
            {
                configuration.AddRule(check.PhysicalFile, check.GetOSLEBotFilteringExpression(), check.ContainerType);
            }
            #region And some configuration for dynamic compiler
            var executingAssemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            // figure out the directory where LocStudio libs are located
            var locstudioLibsPath = Path.GetDirectoryName(typeof(LocDocument).Assembly.Location);
            // figure out the directory where OSLEBot libs are located
            var oslebotLibsPath = Path.GetDirectoryName(typeof(OSLEBotEngine).Assembly.Location);
            configuration.AddBinaryReference("System.Core.dll");
            configuration.AddBinaryReference("mscorlib.dll");
            configuration.AddBinaryReference("System.dll");
            configuration.AddBinaryReference("System.Xml.dll");
            configuration.AddBinaryReference("System.Xml.Linq.dll");
            configuration.AddBinaryReference(Path.Combine(locstudioLibsPath, "Microsoft.Localization.dll"));
            configuration.AddBinaryReference(Path.Combine(oslebotLibsPath, "OSLEBotCore.dll"));
            configuration.AddBinaryReference(Path.Combine(oslebotLibsPath, "LocResource.dll"));
            configuration.AddBinaryReference(Path.Combine(oslebotLibsPath, "OSLEBot.LCXDataAdapter.dll"));
            configuration.AddBinaryReference(Path.Combine(executingAssemblyPath, "Cerberus.Core.dll"));
            
            #endregion

            #endregion

            #region Configure output behavior
            // define all properties here to be consistent for all output writers
            string[] propertiesToInclude = {
                                    "LSResID",
                                    "SourceString",
                                    "TargetString",
                                    "Comments",
                                    "TargetCulture",
                                    "Locgroup",
                                    "Project",
                                    "LcxFileName"
                                           };
            Microsoft.Localization.OSLEBot.Core.Output.OutputWriterConfig outputCfg;
            // set up output writer that creates one merged output for all files
            outputCfg = new OSLEBot.Core.Output.OutputWriterConfig();
            outputCfg.SetDataSourceProvider<Microsoft.Localization.OSLEBot.Core.Output.Specialized.XMLDOMOutputWriter>();
            outputCfg.Schema = "OSLEBotOutput.xsd";
            outputCfg.Path = cerberusOutputPath;
            Array.ForEach(propertiesToInclude, outputCfg.AddPropertyToIncludeInOutput);
            configuration.OutputConfigs.Add(outputCfg);
            // set up output writer that creates one output file per LCX processed.
            outputCfg = new OSLEBot.Core.Output.OutputWriterConfig();
            outputCfg.SetDataSourceProvider<LocResourcePerLCLOutputWriter>();
            outputCfg.Schema = "OSLEBotOutput.xsd";
            outputCfg.Path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            Array.ForEach(propertiesToInclude, outputCfg.AddPropertyToIncludeInOutput);
            configuration.OutputConfigs.Add(outputCfg);
            #endregion

            #region Because we don't store LSBuild enviornment, point compiler to that of Office14 source depot
            configuration.AddAssemblyResolverPaths(locstudioLibsPath);
            #endregion

            return configuration;
        }

        /// <summary>
        /// Possible interpretations of OSLEBot executions.
        /// </summary>
        public enum OSLEBotResult
        {
            /// <summary>
            /// Indication that OSLEBot execution finished successfully.
            /// </summary>
            Success,
            /// <summary>
            /// Indication that OSLEBot execution failed to finish successfully, but internal error handling has decoded the reason for failure and informed the user about that failure.
            /// </summary>
            HandledExceptions,
            /// <summary>
            /// Indication that OSLEBot execution failed to finish successfully, and that internal error handling routines were not able to identify a reason or a fix for that failure.
            /// </summary>
            UnhandledException
        }
    }
}
