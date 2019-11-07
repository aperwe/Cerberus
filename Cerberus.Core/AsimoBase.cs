using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Localization.LocSolutions.Logger;
using Microsoft.Localization.OSLEBot.ClassificationObjects;
using Microsoft.Localization.OSLEBot.Core.DataSource.DataSourcePackages;
using Microsoft.Localization.OSLEBot.Core.Engine;
using Microsoft.Localization.OSLEBot.Core.Misc;
using Microsoft.Localization.OSLEBot.DataAdapters;
using Microsoft.Localization.OSLEBot.DataSource.LocResourcePropertyAdapters;
using Microsoft.Localization.OSLEBot.DataSources;
using Microsoft.Localization.OSLEBot.Output;

namespace Microsoft.Localization.LocSolutions.Cerberus.Core
{
    /// <summary>
    /// Base wrapper around OSLEBot
    /// </summary>
    public abstract class AsimoBase
    {
        /// <summary>
        /// Current object that manages our access to enlistment.
        /// </summary>
        protected Office14Enlistment Enlistment { get; private set; }

        /// <summary>
        /// Initializes an instance of Asimo with reference to enlistment to access some of its functionality.
        /// </summary>
        /// <param name="enlistment">Reference to enlistment.</param>
        protected AsimoBase(Office14Enlistment enlistment)
        {
            Enlistment = enlistment;
        }

        /// <summary>
        /// Gets or sets the path to a folder where Asimo (a.k.a. OSLEBot) checks are located.
        /// </summary>
        public string CheckFolderPath { get; set; }

        /// <summary>
        /// Runs Asimo (a.k.a. OSLEBot) on the specified input files.
        /// </summary>
        /// <param name="fileSet">Set of files to run Asimo on.</param>
        /// <param name="languageFilter">Collection of languages to filter enlistment by. If empty, no filtering is applied.</param>
        /// <param name="projectFilter">Collection of projects to filter enlistment by. If empty, no filtering is applied.</param>
        public abstract void Run(IEnumerable<ConfigItem> fileSet, IEnumerable<string> languageFilter, IEnumerable<string> projectFilter);

        /// <summary>
        /// Returns a list of full paths to all checks that are supported by the system.
        /// </summary>
        public IEnumerable<string> GetCheckFileLocations()
        {
            var rawProjects = Directory.GetFiles(CheckFolderPath, "*.cs", SearchOption.AllDirectories);
            return rawProjects;
        }

        /// <summary>
        /// Validates the filter entries to ensure that they do not contain any entries that are not in <paramref name="data"/> collection.
        /// For a filter to be valid, it ought to itemize only such entries that actually exist in the filtered data set.
        /// </summary>
        /// <param name="filter">Filter entries that should be validated against actual data.</param>
        /// <param name="data">Entries to validate the filter against.</param>
        /// <param name="filterId">Desciptive name of what the filter contains. used to log warning message about the invalid entries. For example 'language'.</param>
        /// <returns>True if filter is valid. False if the filter contains at least one invalid entry.</returns>
        protected bool ValidateFilterAgainstData(IEnumerable<string> filter, IEnumerable<string> data, string filterId)
        {
            var invalidEntries = filter.Where(item => !data.Any(d => d.Equals(item, StringComparison.OrdinalIgnoreCase)));
            if (invalidEntries.Count() > 0)
            {
                LoggerSAP.Warning("Invalid {0} filter item(s): {1}", filterId, string.Join(" ,", invalidEntries.ToArray()));
                return false;
            }
            return true;
        }

        /// <summary>
        /// Selects distinct Project names from the specified collection of items.
        /// </summary>
        public IEnumerable<string> GetDistinctProjects(IEnumerable<ConfigItem> items)
        {
            return from file in items
                   group file by file.Project
                   into projectGroup
                       select projectGroup.Key;
        }

        /// <summary>
        /// Selects distinct Language names from the specified collection of items.
        /// </summary>
        public IEnumerable<string> GetDistinctLanguages(IEnumerable<ConfigItem> items)
        {
            return from file in items
                   group file by file.Language
                   into languageGroup
                       select languageGroup.Key;
        }

        /// <summary>
        /// Applies language and project filters to the sequence. If either filtering sequence is empty, no filtering by this sequence is applied.
        /// </summary>
        /// <param name="inputSequence">Input sequence to apply filtering to.</param>
        /// <param name="languageFilter">Language filter to be applied.</param>
        /// <param name="projectFilter">Project filter to be applied.</param>
        /// <returns>A filtered sequence. If all filtering sequences are empty, the input sequence is returned.</returns>
        public static IEnumerable<ConfigItem> ApplyFilters(IEnumerable<ConfigItem> inputSequence, IEnumerable<string> languageFilter, IEnumerable<string> projectFilter)
        {
            var filteredFileSet = inputSequence;
            if (languageFilter.Count() > 0)
                filteredFileSet = from file in filteredFileSet
                                  where languageFilter.Contains(file.Language, StringComparer.OrdinalIgnoreCase)
                                  select file;
            if (projectFilter.Count() > 0)
                filteredFileSet = from file in filteredFileSet
                                  where projectFilter.Contains(file.Project, StringComparer.OrdinalIgnoreCase)
                                  select file;
            return filteredFileSet;
        }

        /// <summary>
        /// Builds OSLEBot configuration based on input data
        /// </summary>
        /// <param name="candidateFiles">Physical files to filter for checks. For global checks, all candidate files will be processed.</param>
        /// <param name="checkPaths">List of paths to C# files containing source code of checks to be compiled.</param>
        /// <param name="language">Language to filter the input file set on.</param>
        /// <param name="project">Project to filter the input file set on.</param>
        /// <param name="locgroup">LocGroup to filter the input file set on.</param>
        /// <returns>Configuration for OSLEBot that can be simply passed into OSLEBotEngine.Execute().</returns>
        internal EngineConfig CreateAsimoConfig(IEnumerable<ConfigItem> candidateFiles, IEnumerable<string> checkPaths, string language, string project, string locgroup)
        {
            //LoggerSAP.Trace("Creating OSLEBotConfig for [{0};{1}]", language, locgroup);

            //Currently only process global checks (should be 1 for proof of concept).
            var configuration = new EngineConfig { EngineLog = "Asimo log for Cerberus.txt" };

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
            var inputForGlobalChecks = from file in candidateFiles
                                       where file.Language.Equals(language)
                                       where file.LocGroup.Equals(locgroup)
                                       where file.Project.Equals(project)
                                       select file;
            foreach (var file in inputForGlobalChecks)
            {
                var package = new DataSourcePackage(); //This package will contain 2 data sources
                package.SetCOType<LocResource>();
                var dataSource = new DataSourceInfo();
                dataSource.SetSourceType<LocDocument>();
                dataSource.SetSourceLocation(file.PhysicalPath);
                package.AddDataSource(dataSource);

                dataSource = new DataSourceInfo();
                dataSource.SetSourceType<ConfigDictionary>();

                var staticProperties = new ConfigDictionary
                                           {
                                               {"BuildNumber", "1"},
                                               {"Project", file.Project},
                                               {"Locgroup", file.LocGroup}
                                           };

                dataSource.SetSourceLocation(staticProperties);

                package.AddDataSource(dataSource);

                configuration.AddDataSourcePackage(package);
            }
            #endregion

            #region Add a sample rule
            foreach (var check in checkPaths) //For test should be 1.
            {
                configuration.AddRule(check, string.Empty, RuleContainerType.Source);
            }
            #region And some configuration for dynamic compiler
            configuration.AddBinaryReference("System.Core.dll");
            configuration.AddBinaryReference("mscorlib.dll");
            configuration.AddBinaryReference("System.dll");
            configuration.AddBinaryReference("Microsoft.Localization.dll");
            configuration.AddBinaryReference("OSLEBotCore.dll");
            configuration.AddBinaryReference("LocResource.dll");
            configuration.AddBinaryReference("OSLEBot.LCXDataAdapter.dll");
            #endregion

            #endregion

            #region Configure output behavior
            var outputCfg = new OSLEBot.Core.Output.OutputWriterConfig();
            outputCfg.SetDataSourceProvider<LocResourcePerLCLOutputWriter>();
            outputCfg.Schema = "OSLEBotOutput.xsd";
            outputCfg.Path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            outputCfg.AddPropertyToIncludeInOutput("LSResID");
            outputCfg.AddPropertyToIncludeInOutput("SourceString");
            outputCfg.AddPropertyToIncludeInOutput("TargetString");
            outputCfg.AddPropertyToIncludeInOutput("Comments");
            outputCfg.AddPropertyToIncludeInOutput("TargetCulture");
            outputCfg.AddPropertyToIncludeInOutput("Locgroup");
            outputCfg.AddPropertyToIncludeInOutput("Project");
            configuration.OutputConfigs.Add(outputCfg);
            #endregion

            #region Because we don't store LSBuild enviornment, point compiler to that of Office14 source depot
            configuration.AddAssemblyResolverPaths(Enlistment.LSBuildToolsPath);
            #endregion

            return configuration;
        }

        /// <summary>
        /// Builds OSLEBot configuration based on input data
        /// </summary>
        /// <param name="candidateFiles">Physical files to filter for checks. For global checks, all candidate files will be processed.</param>
        /// <param name="checkPaths">List of paths to C# files containing source code of checks to be compiled.</param>
        /// <param name="language">Language to filter the input file set on.</param>
        /// <param name="project">Project to filter the input file set on.</param>
        /// <returns>Configuration for OSLEBot that can be simply passed into OSLEBotEngine.Execute().</returns>
        internal EngineConfig CreateAsimoConfig(IEnumerable<ConfigItem> candidateFiles, IEnumerable<string> checkPaths, string language, string project)
        {
            //Currently only process global checks (should be 1 for proof of concept).
            var configuration = new EngineConfig { EngineLog = "Asimo log for Cerberus.txt" };

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
            var inputForGlobalChecks = from file in candidateFiles
                                       where file.Language.Equals(language)
                                       where file.Project.Equals(project)
                                       select file;
            foreach (var file in inputForGlobalChecks)
            {
                var package = new DataSourcePackage(); //This package will contain 2 data sources
                package.SetCOType<LocResource>();
                var dataSource = new DataSourceInfo();
                dataSource.SetSourceType<LocDocument>();
                dataSource.SetSourceLocation(file.PhysicalPath);
                package.AddDataSource(dataSource);

                dataSource = new DataSourceInfo();
                dataSource.SetSourceType<ConfigDictionary>();

                var staticProperties = new ConfigDictionary
                                           {
                                               {"BuildNumber", "1"},
                                               {"Project", file.Project},
                                               {"Locgroup", file.LocGroup}
                                           };

                dataSource.SetSourceLocation(staticProperties);

                package.AddDataSource(dataSource);

                configuration.AddDataSourcePackage(package);
            }
            #endregion

            #region Add a sample rule
            foreach (var check in checkPaths) //For test should be 1.
            {
                configuration.AddRule(check, string.Empty, RuleContainerType.Source);
            }
            #region And some configuration for dynamic compiler
            configuration.AddBinaryReference("System.Core.dll");
            configuration.AddBinaryReference("mscorlib.dll");
            configuration.AddBinaryReference("System.dll");
            configuration.AddBinaryReference("Microsoft.Localization.dll");
            configuration.AddBinaryReference("OSLEBotCore.dll");
            configuration.AddBinaryReference("LocResource.dll");
            configuration.AddBinaryReference("OSLEBot.LCXDataAdapter.dll");
            #endregion

            #endregion

            #region Configure output behavior
            var outputCfg = new OSLEBot.Core.Output.OutputWriterConfig();
            outputCfg.SetDataSourceProvider<LocResourcePerLCLOutputWriter>();
            outputCfg.Schema = "OSLEBotOutput.xsd";
            outputCfg.Path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            outputCfg.AddPropertyToIncludeInOutput("LSResID");
            outputCfg.AddPropertyToIncludeInOutput("SourceString");
            outputCfg.AddPropertyToIncludeInOutput("TargetString");
            outputCfg.AddPropertyToIncludeInOutput("Comments");
            outputCfg.AddPropertyToIncludeInOutput("TargetCulture");
            outputCfg.AddPropertyToIncludeInOutput("Locgroup");
            outputCfg.AddPropertyToIncludeInOutput("Project");
            configuration.OutputConfigs.Add(outputCfg);
            #endregion

            #region Because we don't store LSBuild enviornment, point compiler to that of Office14 source depot
            configuration.AddAssemblyResolverPaths(Enlistment.LSBuildToolsPath);
            #endregion

            return configuration;
        }
    }
}