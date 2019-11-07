using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Localization.LocSolutions.Cerberus.UnitTest.Core;
using Microsoft.Localization.LocSolutions.Cerberus.SDUtility;
using Microsoft.OffGlobe.SourceDepot;
using SourceDepotClient;

namespace UnitTest.SDUtility
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class TestsOnCoreDepot : CerberusTestClassBase
    {
        /// <summary>
        /// Dumps results to TestContext.
        /// </summary>
        private Action<SourceDepotCommandResult> DumpResults;
        /// <summary>
        /// Location of source depot ini file (sd.ini).
        /// </summary>
        private string SDIniLocation;
        /// <summary>
        /// Reference file pattern for source depot.
        /// </summary>
        private string FilePattern;

        /// <summary>
        /// Makes sure the reference file that is used to trigger certain test scenario is initially in a fully synced (forced) state.
        /// </summary>
        [TestInitialize]
        public void SyncTestFile()
        {
            SDIniLocation = Environment.ExpandEnvironmentVariables(@"%SRCROOT%\..\sd.ini");
            var SDRootDirectory = Environment.GetEnvironmentVariable("SRCROOT");
            var language = "pl-pl";
            FilePattern = string.Format(@"{0}\intl\word\{1}\main_dll\bibform.xml.lcl", SDRootDirectory, language);
            using (SourceDepot store = new SourceDepot(SDIniLocation))
            {
                DumpResults = r =>
                    {
                        TestContext.WriteLine(r.ResultType.ToString());
                        foreach (SDCommandOutput x in r.Outputs)
                        {
                            TestContext.WriteLine(x.Message);
                        }
                    };
                TestContext.WriteLine("Reverting...");
                var revertResults = store.Revert(FilePattern);
                DumpResults(revertResults);
                TestContext.WriteLine("Forced syncing...");
                var results = store.ForceSync(FilePattern);
                DumpResults(results);
                TestContext.WriteLine("Reference file {0} is now hot-synced.", FilePattern);
            }
        }

        [TestMethod]
        [Description("Makes sure the sync operation on a synced file is successful in core.")]
        public void TestSyncingPlPlLanguage()
        {
            var context = new SDInterface(new[] { "-core", "pl-pl" });
            var retVal = context.Execute();
            Assert.AreEqual(ExecutionResult.Success, retVal);
        }

        [TestMethod]
        [Description("Makes sure that when a file is sd edited (but is not modified) causes the utility to return error code BadEnlistmentStateFilesOpened.")]
        public void TestSyncingEditedFile()
        {
            #region Setting the reference file into 'edited' state
            using (SourceDepot store = new SourceDepot(SDIniLocation))
            {
                var cdResults = store.GetChangeList("Test change");
                TestContext.WriteLine("Created changelist: {0}", cdResults);

                TestContext.WriteLine("Editing file...");
                var editResults = store.Edit(FilePattern);
                DumpResults(editResults);

                var context = new SDInterface(new[] { "-core", "pl-pl" });
                var retVal = context.Execute();
                Assert.AreEqual(ExecutionResult.BadEnlistmentStateFilesOpened, retVal);

                
                TestContext.WriteLine("Deleting test change list...");
                var revertResults = store.RevertA();
                DumpResults(revertResults);
                var delCdResults = store.DeleteChangeList();
                DumpResults(delCdResults);
            }
            #endregion

        }

        [TestMethod]
        [Description("Makes sure the command line for core is validated correctly with various sequences of arguments.")]
        public void TestInputForCore()
        {
            var context = new SDInterface(new[] { "-core", "4229", "pl-pl" });
            Assert.AreEqual(false, context.ParsedArguments.Correct, "Checkpoint number should be unexpected.");
            Assert.AreEqual("4229", context.ParsedArguments.InvalidArgument);

            context = new SDInterface(new[] { "-core", "pl-pl" });
            Assert.AreEqual(true, context.ParsedArguments.Correct, "This command line for core is correct.");

            context = new SDInterface(new[] { "-store", "pl-pl" });
            Assert.AreEqual(false, context.ParsedArguments.Correct, "Missing checkpoint argument should be detected.");

            context = new SDInterface(new[] { "-store", "4229", "pl-pl" });
            Assert.AreEqual(true, context.ParsedArguments.Correct, "This command line for store is correct.");
        }

        [TestMethod]
        [Description("Makes sure the sync operation works correctly with with checkpoint number specified in store.")]
        public void TestSyncingEsEsStoreLanguage()
        {
            var context = new SDInterface(new[] { "-store", "4229", "es-es" });
            var retVal = context.Execute();
            Assert.AreEqual(ExecutionResult.Success, retVal);
        }

        [TestMethod]
        [Description("Makes sure the sync operation works correctly and doesn't sync when non-existing checkpoint number is specified in store.")]
        public void TestSyncingNonExistingCheckPointInFrFrStoreLanguage()
        {
            var context = new SDInterface(new[] { "-store", "7229", "fr-fr" });
            var retVal = context.Execute();
            Assert.AreEqual(ExecutionResult.FailedSyncLabelNotPresent, retVal);
        }
    }
}
