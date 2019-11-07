using System;
using System.Collections.Generic;
using Microsoft.OffGlobe.SourceDepot;
using System.Text.RegularExpressions;

namespace Microsoft.Localization.LocSolutions.Cerberus.SDUtility
{
    /// <summary>
    /// Source depot synchronizer that knows how to sync LCT files from store depot.
    /// </summary>
    public class StoreSynchronizer : SDSynchronizer
    {
        /// <summary>
        /// Checkpoint number to which the store should be synchronized.
        /// For example: '4229'.
        /// </summary>
        public string CheckpointNumber { get; private set; }

        /// <summary>
        /// Branch name on which files from the specified checkpoint are available.
        /// Store will need to be synced to this branch using this command:
        /// sd sync -b {2} {0}\intl_...{1}\...lct@{3}
        /// </summary>
        public string BranchName { get; private set; }

        /// <summary>
        /// Label name to which the files are synced to. This is calculated based on checkpoint number.
        /// </summary>
        public string LabelName { get; set; }

        /// <summary>
        /// Creates a default instance of <see cref="CoreSynchronizer"/>.
        /// </summary>
        /// <param name="languages">Languages to synchronize from store depot.</param>
        public StoreSynchronizer(IEnumerable<string> languages, string checkpointNumber) : base(languages)
        {
            CheckpointNumber = checkpointNumber;
        }

        /// <summary>
        /// This method tries to execute sd label -o bld14_xxxx_1000
        /// where xxxx is the <see cref="CheckPoint"/> number.
        /// <para/>It outputs the following:<para/>
        /// <example>
        /// # A Source Depot Label Specification.
        /// #
        /// #  Label:       The label name.
        /// #  Update:      The date this specification was last modified.
        /// #  Access:      The date of the last 'labelsync' on this label.
        /// #  Owner:       The user who created this label.
        /// #  Description: A short description of the label (optional).
        /// #  Options:     Label update options: locked or unlocked.
        /// #  View:        Lines to select depot files for the label.
        /// #
        /// # Use 'sd help label' to see more about label views.
        /// 
        /// Label:  bld14_4229_1000
        ///
        /// Update: 2009/07/02 23:15:05
        ///
        /// Access: 2009/07/08 08:52:54
        ///
        /// Owner:  REDMOND\y-arnold
        ///
        /// Description:
        ///         Office14 Build 4229.1000
        ///        ovintegrate=-l bld14_4229_1000 -d 3107333 -s 3107337 -e 3135268 -u ""
        ///        Status: complete
        ///
        /// Options:        locked
        ///
        /// View:
        ///         //depot/dev14lab3/...
        ///        -//depot/dev14lab3/otools/inc/otools/hotsync.txt
        /// </example>
        /// 
        /// <para/>From the output it retrieves the first line of view description to find dev14lab{x} branch name.
        /// <para/>If branch name is found, it attempts to sync the file pattern on branch using this file pattern:
        /// <para/>sd sync -b {2} {0}\intl_...{1}\...lct@{3}
        /// <para/> where {2} is branch name,
        /// <para/>       {3} is label name calculated from checkpoint number.
        /// <para/>       {0} is SD root directory
        /// <para/>       {1} is language name (ll-cc).
        /// </summary>
        /// <param name="iniLocation">Where source depot ini file is located.</param>
        /// <exception cref="IncorrectSDStateException">Thrown when branch information could not be found.</exception>
        private void FindBranchInformation(string iniLocation)
        {
            using (var depot = new SourceDepot(iniLocation))
            {
                LabelName = string.Format("bld14_{0}_1000", CheckpointNumber);
                var results = depot.LabelDescription(LabelName);
                var output = results.GetMessages();

                #region State machine that scans the label description
                var machine = LabelStateMachine.ExpectingViewLine;

                foreach (var line in output)
                {
                    switch (machine)
                    {
                        case LabelStateMachine.ExpectingViewLine:
                            if (Regex.IsMatch(line, "^View:"))
                            {
                                machine = LabelStateMachine.LineWithBranch;
                            }
                            break;

                        case LabelStateMachine.LineWithBranch:
                            var match = Regex.Match(line, "//depot/(?<branch>.+)/[.]{3}", RegexOptions.ExplicitCapture);
                            if (match.Success)
                            {
                                BranchName = match.Groups["branch"].Value;
                            }
                            else
                            {
                                throw new IncorrectSDStateException("No branch available") { ProblemCode = ExecutionResult.FailedSyncLabelNotPresent };
                            }
                            machine = LabelStateMachine.IgnoreRemainingLines;
                            break;

                        case LabelStateMachine.IgnoreRemainingLines: break; //Do nothing.
                    }
                }

                if (machine != LabelStateMachine.IgnoreRemainingLines)
                {
                    throw new IncorrectSDStateException("Branch information not found") { ProblemCode = ExecutionResult.FailedSyncLabelNotPresent };
                }
                if (string.IsNullOrEmpty(BranchName))
                {
                    throw new IncorrectSDStateException("Branch information not located") { ProblemCode = ExecutionResult.FailedSyncLabelNotPresent };
                }
                #endregion

            }
        }

        /// <summary>
        /// Possible states of state machine that finds branch information in ouptut of sd label command.
        /// </summary>
        public enum LabelStateMachine
        {
            /// <summary>
            /// Initial state of the machine. We are expecting 'View:' line.
            /// </summary>
            ExpectingViewLine,
            /// <summary>
            /// After encountering the 'View:' line, the machine enters this state.
            /// The line parsed in this state contains text line: "        //depot/dev14lab3/..."
            /// It will be parsed as "//depot/{0}/...", where {0} is branch name.
            /// </summary>
            LineWithBranch,
            /// <summary>
            /// All information from output was retrieved. Ignore the rest of the output.
            /// This is the final state of the machine.
            /// </summary>
            IgnoreRemainingLines
        }

        /// <summary>
        /// Synchronizes all files in the depot.
        /// </summary>
        public override ExecutionResult Synchronize()
        {
            SDIniLocation = Environment.ExpandEnvironmentVariables(@"%STORE%\sd.ini");
            SDRootDirectory = Environment.GetEnvironmentVariable("STORE");

            try
            {
                ValidateSourceDepotState(SDIniLocation, SDRootDirectory);

                FindBranchInformation(SDIniLocation);

                foreach (var language in Languages)
                {
                    var filePattern = string.Format(@"{0}\intl_...{1}\...lct", SDRootDirectory, language);
                    SyncBranchFilePattern(SDIniLocation, BranchName, LabelName, filePattern, language); //This will throw exception if necessary and pass control outside of the foreach loop
                }
            }
            catch (FilesOpenedException e) //Precondition failed.
            {
                if (!string.IsNullOrEmpty(e.Message))
                {
                    Console.WriteLine("Error: {0}", e.Message);
                }
                return ExecutionResult.BadEnlistmentStateFilesOpened;
            }
            catch (IncorrectSDStateException e) //Precondition failed.
            {
                if (!string.IsNullOrEmpty(e.Message))
                {
                    Console.WriteLine("Error: {0}", e.Message);
                }
                return e.ProblemCode;
            }

            return ExecutionResult.Success;
        }
    }
}
