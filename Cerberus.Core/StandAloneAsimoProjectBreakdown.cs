using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.Localization.LocSolutions.Logger;
using Microsoft.Localization.OSLEBot.Core.Engine;
using Microsoft.Localization.OSLEBot.Core.Misc;
using Microsoft.Localization.OSLEBot.Exceptions;
using Microsoft.Practices.AssemblyManagement;

namespace Microsoft.Localization.LocSolutions.Cerberus.Core
{
    /// <summary>
    /// Wrapper object around OSLEBot that works without database and creates one instance of OSLEBot engine per project.
    /// </summary>
    public class StandAloneAsimoProjectBreakdown : StandaloneAsimo
    {
        /// <summary>
        /// Initializes an instance of Asimo with reference to enlistment to access some of its functionality.
        /// </summary>
        /// <param name="enlistment">Reference to enlistment.</param>
        public StandAloneAsimoProjectBreakdown(Office14Enlistment enlistment)
            : base(enlistment)
        {
        }

        /// <summary>
        /// Runs Asimo (a.k.a. OSLEBot) on the specified input files.
        /// </summary>
        /// <param name="fileSet">Set of files to run Asimo on.</param>
        /// <param name="languageFilter">Collection of languages to filter enlistment by. If empty, no filtering is applied.</param>
        /// <param name="projectFilter">Collection of projects to filter enlistment by. If empty, no filtering is applied.</param>
        public override void Run(IEnumerable<ConfigItem> fileSet, IEnumerable<string> languageFilter, IEnumerable<string> projectFilter)
        {
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

            var checks = GetCheckFileLocations();
            LoggerSAP.Log("One OSLEBot instance will be run against each project.");
            foreach (var language in distinctLanguages)
            {
                foreach (var project in distinctProjects)
                {
                    try
                    {
                        var inputCfg = CreateAsimoConfig(fileSet, checks, language, project);
                        if (inputCfg.DataSourcePkgs == null || inputCfg.DataSourcePkgs.Count == 0)
                        {
                            //LoggerSAP.Log("No data sources for {0}, {1}, {2}. Skipping.", language, project, locgroup);
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
    }
}