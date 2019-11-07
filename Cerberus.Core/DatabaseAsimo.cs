using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Localization.LocSolutions.Logger;
using Microsoft.Localization.OSLEBot.Core.Engine;
using Microsoft.Localization.OSLEBot.Core.Misc;
using Microsoft.Localization.OSLEBot.Exceptions;
using Microsoft.Practices.AssemblyManagement;

namespace Microsoft.Localization.LocSolutions.Cerberus.Core
{
    /// <summary>
    /// Asimo that talks to OSLEBot and gets its configuration data from the database rather than from command line input.
    /// </summary>
    public class DatabaseAsimo : AsimoBase
    {
        /// <summary>
        /// Initializes an instance of Asimo with reference to enlistment to access some of its functionality.
        /// </summary>
        /// <param name="enlistment">Reference to enlistment.</param>
        public DatabaseAsimo(Office14Enlistment enlistment)
            : base(enlistment)
        {
        }

        /// <summary>
        /// Runs Asimo (a.k.a. OSLEBot) on the specified input files and database information to control which checks
        /// are enabled for which {languages, projects}
        /// </summary>
        /// <param name="fileSet">Set of files to run Asimo on.</param>
        /// <param name="languageFilter">Collection of languages to filter enlistment by. If empty, no filtering is applied.</param>
        /// <param name="projectFilter">Collection of projects to filter enlistment by. If empty, no filtering is applied.</param>
        public override void Run(IEnumerable<ConfigItem> fileSet, IEnumerable<string> languageFilter, IEnumerable<string> projectFilter)
        {
            var readOnlyDB = new CheckConfiguration();
            LoadDataFromDatabase(readOnlyDB);

            if (fileSet == null) throw new ArgumentNullException("fileSet");
            if (languageFilter == null) throw new ArgumentNullException("languageFilter");
            if (projectFilter == null) throw new ArgumentNullException("projectFilter");

            var assemblyResolver = new AssemblyResolver(new[]
                                                            {
                                                                Enlistment.LSBuildToolsPath,
                                                                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
                                                            });
            assemblyResolver.Init();

            //Apply language and project filters
            fileSet = ApplyFilters(fileSet, languageFilter, projectFilter);

            //Split executions per language per locgroup to allow finite time of execution
            var distinctLanguages = GetDistinctLanguages(fileSet);
            var distinctProjects = GetDistinctProjects(fileSet);

            //Additional validation for incorrect filter values (e.g. misspelled language or project name)
            ValidateFilterAgainstData(languageFilter, distinctLanguages, "language");
            ValidateFilterAgainstData(projectFilter, distinctProjects, "project");

            var physicalChecks = GetCheckFileLocations();

            LoggerSAP.Log("One OSLEBot instance will be run against each project.");
            foreach (var language in distinctLanguages)
            {
                foreach (var project in distinctProjects)
                {
                    try
                    {
                        var checksEnabledForThisLanguageProject = readOnlyDB.GetEnabledChecks(language, project);
                        var availableEnabledChecks = physicalChecks.Select(c => Path.GetFileNameWithoutExtension(c))
                            .Intersect(checksEnabledForThisLanguageProject);
                        var phycicalCheckPathsToRun = physicalChecks
                            .Where(c => availableEnabledChecks
                                            .Any(cc => Path.GetFileNameWithoutExtension(c).Equals(cc)));
                        if (availableEnabledChecks.Count() == 0)
                        {
                            LoggerSAP.Trace(@"No checks are enabled for [{0}, {1}]. Skipping.", language, project);
                            continue;
                        }
                        var inputCfg = CreateAsimoConfig(fileSet, phycicalCheckPathsToRun, language, project);
                        if (inputCfg.DataSourcePkgs == null || inputCfg.DataSourcePkgs.Count == 0)
                        {
                            //Skip the execution when there is 0 file matches for the specified {language, project} pair.
                            continue;
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
                        }
                    }
                    catch (OperationCanceledException ex)
                    {
                        LoggerSAP.Error("OSLEBot engine failed ({0}) because {1}", ex.GetType().Name, ex.Message);
                    }
                    catch (OSLEBotEngineInitializationException ex)
                    //Thrown when there are no rules to run on the input set.
                    {
                        LoggerSAP.Error("OSLEBot engine failed ({0}) because {1}", ex.GetType().Name, ex.Message);
                    }
                }
            }
        }
        /// <summary>
        /// Loads data from the disk database (file or SQL) as in-memory data for manipulation.
        /// </summary>
        private void LoadDataFromDatabase(DataSet database)
        {
            if (Enlistment.IsDetected)
            {
                var myHomeDir = Enlistment.CerberusHomeDir;
                var dataFilePath = new FileInfo(Path.Combine(myHomeDir, "Configuration.xml"));
                if (dataFilePath.Exists)
                {
                    LoggerSAP.Trace("Loading data from Configuration.xml");
                    database.ReadXml(dataFilePath.FullName);
                }
                else
                {
                    LoggerSAP.Warning("No Cerberus database found. Check enablement information is not available.");
                }
            }
        }


        /// <summary>
        /// Processes the whole enlistment that is currently available and adds any missing {languages, projects, checks} to the database.
        /// No information is deleted from the database.
        /// </summary>
        /// <param name="database">Database that should be updates based on information available in the current enlistment.</param>
        /// <exception cref="ArgumentNullException">Thrown when the specified <paramref name="database"/> is null.</exception>
        public void UpdateDatabaseWithEnlistmentContent(CheckConfiguration database)
        {
            if (database == null) throw new ArgumentNullException("database");
            if (!Enlistment.IsDetected) return;
            var input = Enlistment.GetFiles(Path.Combine(Enlistment.Location, "intl"), LcxType.Lcl);

            //Distinct languages
            var enlistmentLanguages = from item in input
                                      group item by item.Language
                                          into languageGroups
                                          select languageGroups.Key;
            database.AddMissingLanguages(enlistmentLanguages);

            //Distinct projects
            var enlistmentProjects = from item in input
                                     group item by item.Project
                                         into projectGroups
                                         select projectGroups.Key;
            database.AddMissingProjects(enlistmentProjects);

            //Distinct checks - just file names (without extensions) instead of full paths.
            var checks = from file in GetCheckFileLocations()
                         select new CheckInfo
                         {
                             Name = Path.GetFileNameWithoutExtension(file),
                             SourceCode = File.ReadAllText(file),
                             Description = "New check",
                             FilePath = TryToUseEnvironmentVariablesInAbsoluteFilePath(file)
                         };
            database.AddMissingChecks(checks);
        }

        /// <summary>
        /// Assumes that the specified <paramref name="fullPath"/> is an absolute path.
        /// <para/>Attempts to substitute the beginning of the string with useful variable names.
        /// </summary>
        /// <param name="fullPath">Full path to be changed to use environment variables.</param>
        /// <returns>Original or 'improved' path that may reference environment variables.</returns>
        private string TryToUseEnvironmentVariablesInAbsoluteFilePath(string fullPath)
        {
            string retVal = fullPath;
            retVal = TryToUseSingleEnvironmentVariable("OTOOLS", retVal);
            retVal = TryToUseSingleEnvironmentVariable("BUILDROOT", retVal);
            retVal = TryToUseSingleEnvironmentVariable("INTL_TOOLS", retVal);
            retVal = TryToUseSingleEnvironmentVariable("SRCROOT", retVal);
            return retVal;
        }

        /// <summary>
        /// Attempts to substitute the beginning of the string with the name of the specified environment variable.
        /// </summary>
        /// <param name="fullPath">Full path to be changed to use environment variables.</param>
        /// <param name="envVar">Name of the environment variable, without percentage signs. For example: "OTOOLS".</param>
        /// <returns>Original or 'improved' path that may reference the specified environment variable.</returns>
        private string TryToUseSingleEnvironmentVariable(string envVar, string fullPath)
        {
            string retVal = fullPath;
            var envNameString = string.Format("%{0}%", envVar);
            var envValue = Environment.ExpandEnvironmentVariables(envNameString);
            if (retVal.StartsWith(envValue, StringComparison.InvariantCultureIgnoreCase))
            {
                retVal = retVal.Replace(envValue, envNameString);
            }
            return retVal;
        }

    }
}