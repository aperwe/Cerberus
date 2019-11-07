using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Localization.LocSolutions.Logger;

namespace Microsoft.Localization.LocSolutions.Cerberus.Core
{
    /// <summary>
    /// Wrapper object around Office14 enlistment that allows to inspect the contents of enlistment.
    /// </summary>
    public class Office14Enlistment
    {
        /// <summary>
        /// Indicates whether Office14 enlistment is known to the current program.
        /// Returns true if environment is configured.
        /// False indicates that the program has been started from outside of Office 14 enlistment.
        /// </summary>
        public bool IsDetected
        {
            get { return !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("SRCROOT")); }
        }
        /// <summary>
        /// Returns a location corresponding to SRCROOT variable of your enlistment.
        /// This is valid only if <see cref="IsDetected"/> returns true.
        /// </summary>
        public string Location
        {
            get
            {
                return Environment.ExpandEnvironmentVariables("%SRCROOT%");
            }
        }
        /// <summary>
        /// Returns path (if known) to "%OTOOLS%\bin\lsbuild6.0" folder that is part of Office 14 enlistment.
        /// This is valid only if <see cref="IsDetected"/> returns true.
        /// </summary>
        public string LSBuildToolsPath
        {
            get
            {
                return Environment.ExpandEnvironmentVariables(@"%OTOOLS%\bin\lsbuild6.0");
            }
        }

        /// <summary>
        /// Returns path to %OTOOLS%\bin\intl\Cerberus within Office 14 enlistment.
        /// This is valid only if <see cref="IsDetected"/> returns true.
        /// </summary>
        public string CerberusHomeDir
        {
            get { return Environment.ExpandEnvironmentVariables(@"%OTOOLS%\bin\intl\Cerberus"); }
        }

        /// <summary>
        /// Builds a flat list of files under the specified path (under enlistment location) that match the specified filter.
        /// These can be the files that can be processed by OSLEBot engine.
        /// </summary>
        /// <param name="relativePath">Path under enlistment location to look for files.</param>
        /// <param name="fileType">One of the file types supported by enlistment.</param>
        public IEnumerable<ConfigItem> GetFiles(string relativePath, LcxType fileType)
        {
            var retVal = new List<ConfigItem>();
            if (!IsDetected) throw new EnlistmentException("Cannot get files because enlistment location is unknown.");

            var searchPath = Path.Combine(Location, relativePath);
            var searchPattern = GetFileExtensionFromType(fileType);
            var rawProjects = Directory.GetFiles(searchPath, "*.snt", SearchOption.AllDirectories);
            LoggerSAP.Trace("{0} projects identified under {1}.", rawProjects.Length, searchPath);
            foreach (var project in rawProjects)
            {
                var projectSentinelFile = new FileInfo(project);
                var projectName = projectSentinelFile.Directory.Name;
                var projectDirectory = projectSentinelFile.DirectoryName;

                var rawLanguages = Directory.GetFiles(projectDirectory, ".language", SearchOption.AllDirectories);
                foreach (var language in rawLanguages)
                {
                    var languageSentinelFile = new FileInfo(language);
                    var languageName = languageSentinelFile.Directory.Name;
                    var languageDirectory = languageSentinelFile.DirectoryName;

                    var rawLocGroups = Directory.GetFiles(languageDirectory, ".locgroup", SearchOption.AllDirectories);
                    foreach (var locGroup in rawLocGroups)
                    {
                        var locGroupSentinelFile = new FileInfo(locGroup);
                        var locGroupName = locGroupSentinelFile.Directory.Name;
                        var locGroupDirectory = locGroupSentinelFile.DirectoryName;

                        var rawInput = Directory.GetFiles(locGroupDirectory, searchPattern, SearchOption.AllDirectories);

                        foreach (var inputFile in rawInput) //These are possible input files for OSLEBot (*.lcl files).
                        {
                            var fileObject = new FileInfo(inputFile);

                            var inputConfig = new ConfigItem
                                                  {
                                                      Project = projectName,
                                                      Language = languageName,
                                                      LocGroup = locGroupName,
                                                      File = fileObject.Name,
                                                      PhysicalPath = fileObject.FullName,
                                                  };
                            retVal.Add(inputConfig);
                        }
                    }
                }
            }
            return retVal;
        }
        /// <summary>
        /// Maps a file type enumeration into actual search pattern that can be used in Directory.GetFiles() call.
        /// </summary>
        /// <param name="fileType">File type to get a corresponding file pattern.</param>
        private static string GetFileExtensionFromType(LcxType fileType)
        {
            switch (fileType)
            {
                case LcxType.Lcl:
                    return "*.lcl";
            }
            throw new EnlistmentException(string.Format("Unsupported {0} file type", fileType));
        }

    }

    /// <summary>
    /// Types of files recognized by Office14Entlistment.
    /// </summary>
    public enum LcxType
    {
        /// <summary>
        /// LCX files with extension *.lcl
        /// </summary>
        Lcl
    }
}