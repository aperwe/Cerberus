using System;
using System.Collections.Generic;

namespace Microsoft.Localization.LocSolutions.Cerberus.SDUtility
{
    /// <summary>
    /// Source depot synchronizer that knows how to sync LCL files from core depot.
    /// </summary>
    public class CoreSynchronizer : SDSynchronizer
    {
        /// <summary>
        /// Creates a default instance of <see cref="CoreSynchronizer"/>.
        /// </summary>
        /// <param name="languages">Languages to synchronize from core depot.</param>
        public CoreSynchronizer(IEnumerable<string> languages) : base(languages)
        {
        }
        /// <summary>
        /// Synchronizes all files in the depot.
        /// </summary>
        public override ExecutionResult Synchronize()
        {
            SDIniLocation = Environment.ExpandEnvironmentVariables(@"%SRCROOT%\..\sd.ini");
            SDRootDirectory = Environment.GetEnvironmentVariable("SRCROOT");

            try
            {
                ValidateSourceDepotState(SDIniLocation, SDRootDirectory);

                foreach (var language in Languages)
                {
                    var filePattern = string.Format("{0}\\intl\\...{1}\\...lcl", SDRootDirectory, language);
                    SyncFilePattern(SDIniLocation, filePattern, language); //This will throw exception if necessary and pass control outside of the foreach loop
                }
            }
            catch (FilesOpenedException e) //Precondition failed.
            {
                return ExecutionResult.BadEnlistmentStateFilesOpened;
            }
            catch (IncorrectSDStateException e) //Precondition failed.
            {
                return e.ProblemCode;
            }

            return ExecutionResult.Success;
        }
    }
}
