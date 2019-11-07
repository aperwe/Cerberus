using System;
using System.Text.RegularExpressions;
using Microsoft.OffGlobe.SourceDepot;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace Microsoft.Localization.LocSolutions.Cerberus.SDUtility
{
    /// <summary>
    /// Base class for source depot synchronizers.
    /// Implementations can be: Store synchronizer; Core synchronizer.
    /// </summary>
    public abstract class SDSynchronizer
    {
        /// <summary>
        /// Synchronizes all files in the depot.
        /// </summary>
        public abstract ExecutionResult Synchronize();

        /// <summary>
        /// Location of the specific source depot sd.ini file location.
        /// </summary>
        protected string SDIniLocation { get; set; }

        /// <summary>
        /// Root directory of where sd.ini file is placed.
        /// </summary>
        protected string SDRootDirectory { get; set; }

        /// <summary>
        /// Set of languages to synchronize.
        /// </summary>
        public IEnumerable<string> Languages { get; set; }

        /// <summary>
        /// Maximum retry count in case the first attempt to synchronize doesn't work.
        /// </summary>
        protected int RetryCount { get; set; }

        /// <summary>
        /// Indicates the current retry attempt when syncing.
        /// </summary>
        protected int CurrentRetryAttempt { get; set; }

        /// <summary>
        /// Indicates whether the last sync was successful or not.
        /// </summary>
        protected bool SuccessfulSync { get; set; }

        /// <summary>
        /// Initializes default values of common properties.
        /// </summary>
        /// <param name="languages">Languages to initialize this synchronizer with.</param>
        public SDSynchronizer(IEnumerable<string> languages)
        {
            Languages = languages;
            RetryCount = 3; // How many sync attempts should be made at maximum
        }

        /// <summary>
        /// Parses the output of the command line to determine if this is actually a successful sync or a reason to try to sync again.
        /// </summary>
        /// <param name="results">Results from the last sync operation.</param>
        /// <remarks>Doesn't seem to catch editable files. Something wrong with sdapi.dll.</remarks>
        /// <returns>True if there is nothing wrong with the operation, False if the sync should be retried with -f flag (forced sync).</returns>
        protected SourceDepotResult ResultsAreActuallySuccess(SourceDepotCommandResult results)
        {
            switch (results.Outputs.Count)
            {
                case 1:
                    var x = results.Outputs[0];
                    if (Regex.IsMatch(x.Message, @"up-to-date.$"))
                        return SourceDepotResult.Success;
                    if (Regex.IsMatch(x.Message, @"no such file\(s\).$"))
                        return SourceDepotResult.NoSuchFiles;
                    break;
            }
            return SourceDepotResult.UnspecifiedError;
        }

        /// <summary>
        /// Checks a precondition that there are no open files in the specified file pattern.
        /// </summary>
        /// <param name="depot">Source depot where pattern is to be searched for open files.</param>
        /// <param name="filePattern">Pattern of files we want to ensure that are not opened.</param>
        /// <exception cref="FilesOpenedException">Thrown when files are actually opened in the specified pattern.</exception>
        /// <exception cref="IncorrectSDStateException">Thrown when network connectivity is lost.</exception>
        protected void CheckPreconditionThatNoFilesAreOpened(SourceDepot depot, string filePattern)
        {
            bool okToExit = false; //When set to true, the inner loop can break and we can return to the caller.

            Enumerable.Range(1, 3) //3 retries maximum
                .ToList().ForEach((retry) =>
            {
                try
                {
                    if (!okToExit)
                    {
                        if (retry != 1)
                        {
                            Console.WriteLine();
                            Console.WriteLine("Retrying({0})...", retry);
                            Thread.Sleep(2000);
                        }

                        var opened = depot.Opened(filePattern);
                        if (opened.ResultType == SourceDepotCommandResultType.Infos)
                        {
                            if (opened.GetMessages().Count != 0)
                            {
                                Console.WriteLine("Error: Some files are opened in this file pattern: {0}", filePattern);
                                Console.WriteLine("This tool requires that all files in enlistment be unmodified.");
                                throw new FilesOpenedException();
                            }
                        }
                        okToExit = true; //Successful operation
                    }
                }
                catch (SourceDepotException e) //Thrown when network connectivity is lost
                {
                    Console.WriteLine("Error connecting to source depot:");
                    Console.WriteLine(e.Message);
                }
            });

            if (!okToExit) //Unable to successfully connect to source depot
            {
                throw new IncorrectSDStateException { ProblemCode = ExecutionResult.BadEnlistmentStateConnectionFailed };
            }
        }

        /// <summary>
        /// Uses normal sync or forced sync (depending on retry attempt) to the appropriate depot.
        /// </summary>
        /// <param name="depot">Depot to synchronize</param>
        /// <param name="retryNumber">Number of the current retry. If it is 1, normal sync is used. If it is greater than 1, uses forced sync.</param>
        /// <param name="filePattern">Pattern of files to sync.</param>
        /// <returns>Command results.</returns>
        protected static SourceDepotCommandResult SyncDepot(SourceDepot depot, int retryNumber, string filePattern)
        {
            Console.WriteLine("Attempt {0}...", retryNumber);

            SourceDepotCommandResult results = null;
            if (retryNumber == 1)
                results = depot.Sync(filePattern);
            else
                results = depot.ForceSync(filePattern);
            return results;
        }

        /// <summary>
        /// Uses normal sync or forced sync (depending on retry attempt) to the appropriate depot.
        /// </summary>
        /// <param name="depot">Depot to synchronize</param>
        /// <param name="retryNumber">Number of the current retry. If it is 1, normal sync is used. If it is greater than 1, uses forced sync.</param>
        /// <param name="filePattern">Pattern of files to sync.</param>
        /// <param name="branchName">Branch on which to synchronize the files.</param>
        /// <returns>Command results.</returns>
        protected static SourceDepotCommandResult SyncDepotToBranch(SourceDepot depot, string branchName, int retryNumber, string filePattern)
        {
            Console.WriteLine("Attempt {0}...", retryNumber);

            SourceDepotCommandResult results = null;
            if (retryNumber == 1)
                results = depot.Sync(filePattern, branchName);
            else
                results = depot.ForceSync(filePattern, branchName);
            return results;
        }

        /// <summary>
        /// Looks at the results of a sync operation to see if the sync was entirely successful or not.
        /// <para></para>If it was entirely successful, this method does nothing.
        /// <para></para>If there was indication that at least one file might be out of date or not synced, it throws the exception that should be handled by returning problem code to the parent caller.
        /// </summary>
        /// <param name="syncResults">Results of a sync operation to be analysed to see whether the sync operation was entirely successful or not.</param>
        /// <param name="language">Language of the sync.</param>
        /// <exception cref="IncorrectSDStateException">Thrown when <paramref name="syncResults"></paramref> indicate the sync operation was not entirely successful.</exception>
        protected void AnalyseSdSyncResults(SourceDepotCommandResult syncResults, string language)
        {
            if (syncResults.ResultType == SourceDepotCommandResultType.Warnings)
            {
                switch (ResultsAreActuallySuccess(syncResults))
                {
                    case SourceDepotResult.Success:
                        SuccessfulSync = true;
                        Console.WriteLine("Succeeded.");
                        break;
                    case SourceDepotResult.NoSuchFiles:
                        SuccessfulSync = false;
                        Console.WriteLine("No such files. Have you specified the correct language? {0}", language);
                        throw new IncorrectSDStateException { ProblemCode = ExecutionResult.WrongLanguage };
                    case SourceDepotResult.Error:
                        Console.WriteLine("Error while syncing. Retrying.");
                        break;
                    default:
                        Console.WriteLine("Unspecified problem while syncing.");
                        break;
                }
            }
            else
            {
                //Break the while loop, because everthing else seems successful.
                SuccessfulSync = true;
                Console.WriteLine("File(s) updated.");
                Console.WriteLine("Succeeded.");
            }
        }

        /// <summary>
        /// Validates whether the enlistment is present and properly configured.
        /// </summary>
        /// <exception cref="IncorrectSDStateException">Thrown with the appropriate problem code that can be passed by the caller to the parent method.</exception>
        protected void ValidateSourceDepotState(string iniLocation, string sdRoot)
        {
            if (string.IsNullOrEmpty(SDRootDirectory))
            {
                Console.WriteLine("You are not enlisted to store.");
                throw new IncorrectSDStateException { ProblemCode = ExecutionResult.NoEnlistment };
            }
            if (!File.Exists(SDIniLocation))
            {
                Console.WriteLine("Could not find sd.ini");
                throw new IncorrectSDStateException { ProblemCode = ExecutionResult.BadEnlistment };
            }
        }

        /// <summary>
        /// Syncs file pattern and throws an exception with appropriate code if the sync was not entirely successful.
        /// </summary>
        /// <param name="sdIni">Location of sd.ini file to use when syncing.</param>
        /// <param name="filePattern">Specifies the sync file pattern. For example: "pl-pl...lcl".</param>
        /// <param name="language">Language that is being synced.</param>
        /// <exception cref="IncorrectSDStateException">Thrown when sync for this language could not be completed.</exception>
        protected void SyncFilePattern(string sdIni, string filePattern, string language)
        {
            using (SourceDepot depot = new SourceDepot(sdIni))
            {
                CurrentRetryAttempt = 1;
                SuccessfulSync = false;
                Console.WriteLine("Syncing language: {0}", language);
                CheckPreconditionThatNoFilesAreOpened(depot, filePattern); //Will throw exception if this precondition fails.
                while (!SuccessfulSync && (CurrentRetryAttempt <= RetryCount))
                {
                    try
                    {
                        var results = SyncDepot(depot, CurrentRetryAttempt, filePattern);
                        AnalyseSdSyncResults(results, language); //This will throw exception if necessary and pass control outside of the 'while' and 'using' blocks.
                    }
                    catch (SourceDepotException e)
                    {
                        Console.WriteLine("Error connecting to source depot:");
                        Console.WriteLine(e.Message);
                        Console.WriteLine();
                    }
                    CurrentRetryAttempt++;
                }
                if (!SuccessfulSync)
                {
                    Console.WriteLine("Failed to sync {0}. Giving up.", language);
                    throw new IncorrectSDStateException("SyncFilePattern") { ProblemCode = ExecutionResult.FailedSync };
                }
            }
        }

        /// <summary>
        /// Syncs file pattern and throws an exception with appropriate code if the sync was not entirely successful.
        /// </summary>
        /// <param name="sdIni">Location of sd.ini file to use when syncing.</param>
        /// <param name="filePattern">Specifies the sync file pattern. For example: "pl-pl...lcl". NOTE: Label specifier will be added by this method for syncing, so this pattern must not contain label specifier.</param>
        /// <param name="language">Language that is being synced.</param>
        /// <param name="branchName">Branch on which to sync the files.</param>
        /// <param name="labelName">Label to which to sync the files on that branch.</param>
        /// <exception cref="IncorrectSDStateException">Thrown when sync for this language could not be completed.</exception>
        protected void SyncBranchFilePattern(string sdIni, string branchName, string labelName, string filePattern, string language)
        {
            using (SourceDepot depot = new SourceDepot(sdIni))
            {
                CurrentRetryAttempt = 1;
                SuccessfulSync = false;
                Console.WriteLine("Syncing language: {0} on branch {1} to label {2}.", language, branchName, labelName);
                CheckPreconditionThatNoFilesAreOpened(depot, filePattern); //Will throw exception if this precondition fails.


                var labelledFilePattern = string.Format("{0}@{1}", filePattern, labelName);

                while (!SuccessfulSync && (CurrentRetryAttempt <= RetryCount))
                {
                    try
                    {
                        var results = SyncDepotToBranch(depot, branchName, CurrentRetryAttempt, labelledFilePattern);
                        AnalyseSdSyncResults(results, language); //This will throw exception if necessary and pass control outside of the 'while' and 'using' blocks.
                    }
                    catch (SourceDepotException e)
                    {
                        Console.WriteLine("Error connecting to source depot:");
                        Console.WriteLine(e.Message);
                        Console.WriteLine();
                    }
                    CurrentRetryAttempt++;
                }
                if (!SuccessfulSync)
                {
                    Console.WriteLine("Failed to sync {0}. Giving up.", language);
                    throw new IncorrectSDStateException("SyncFilePattern") { ProblemCode = ExecutionResult.FailedSync };
                }
            }
        }

    }
}
