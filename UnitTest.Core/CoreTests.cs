using System.Collections.Generic;
using System.Linq;
using Microsoft.Localization.LocSolutions.Cerberus.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Localization.LocSolutions.Cerberus.UnitTest.Core
{
    /// <summary>
    /// Tests for the functionality of Cerberus core library.
    /// </summary>
    [TestClass]
    public class CoreTests : CerberusTestClassBase
    {
        /// <summary>
        /// Tests that filling the data set works well.
        /// </summary>
        [CssIteration("vstfs:///Classification/Node/357fe10e-9d21-4152-ab04-30b91c940cf4"), Owner("arturp"), TestMethod]
        [DeploymentItem("SampleCheck.cs")]
        public void TestFillingDataTablesInCheckConfigurationDataSet()
        {
            var configuration = new CheckConfiguration();
            configuration.FillWithTestData();
            Assert.AreNotEqual(0, configuration.ConfigItems.Count);

            configuration = new CheckConfiguration();
            Assert.AreEqual(0, configuration.GetAllChecks().Count());
        }

        /// <summary>
        /// Tests the mechanism to get locations of LCL files from enlistment.
        /// </summary>
        [CssIteration("vstfs:///Classification/Node/357fe10e-9d21-4152-ab04-30b91c940cf4"), TestMethod]
        [Owner("arturp")]
        public void TestGettingLclFilesFromEnlistment()
        {
            var enlistment = new Office14Enlistment();
            Assert.AreEqual(true, enlistment.IsDetected);
            TestContext.WriteLine("Enlistment detected, getting LCL files");
            var lclFiles = enlistment.GetFiles("intl", LcxType.Lcl);
            Assert.AreNotEqual(0, lclFiles.Count());
            TestContext.WriteLine("Found {0} lcl files under enlistment.", lclFiles.Count());
        }

        /// <summary>
        /// Tests the mechanism to get loctions of check source files.
        /// </summary>
        [CssIteration("vstfs:///Classification/Node/357fe10e-9d21-4152-ab04-30b91c940cf4"), TestMethod]
        [Owner("arturp")]
        [DeploymentItem(@"UnitTest.Core\SampleCheck.cs")]
        public void TestGettingListOfPathsToChecks()
        {
            var enlistment = new Office14Enlistment();
            Assert.AreEqual(true, enlistment.IsDetected);
            TestContext.WriteLine("Enlistment detected, checking access to the list of checks");
            var asimo = new StandAloneAsimoLocGroupBreakdown(enlistment) {CheckFolderPath = TestContext.TestDeploymentDir};
            var checkPaths = asimo.GetCheckFileLocations();
            Assert.AreNotEqual(0, checkPaths.Count());
            TestContext.WriteLine("Returned {0} paths to checks.", checkPaths.Count());
        }

        /// <summary>
        /// Tests whether it is possible to filter an arbitrary sequence of file entries.
        /// </summary>
        [CssIteration("vstfs:///Classification/Node/357fe10e-9d21-4152-ab04-30b91c940cf4"), TestMethod]
        [Owner("arturp")]
        public void TestFilteringOfFileSequence()
        {
            var list = new List<ConfigItem>
                           {
                               new ConfigItem {File = "File1", Language = "Polish", Project = "Word"},
                               new ConfigItem {File = "File2", Language = "Polish", Project = "Word"},
                               new ConfigItem {File = "File3", Language = "Polish", Project = "Excel"},
                               new ConfigItem {File = "File1", Language = "English", Project = "Word"},
                               new ConfigItem {File = "File2", Language = "English", Project = "Word"},
                               new ConfigItem {File = "File3", Language = "English", Project = "Excel"},
                               new ConfigItem {File = "File1", Language = "German", Project = "Word"},
                               new ConfigItem {File = "File2", Language = "German", Project = "Word"},
                               new ConfigItem {File = "File3", Language = "German", Project = "Excel"}
                           };
            TestContext.WriteLine("Testing filtering using input sequence of {0} elements.", list.Count);
            Assert.AreEqual(list.Count, AsimoBase.ApplyFilters(list, new string[0], new string[0]).Count(),
                            "With empty filters the filtered sequence should be of the same length as input sequence.");
            Assert.AreEqual(3, AsimoBase.ApplyFilters(list, new[] {"Polish"}, new string[0]).Count(),
                            "Expected 3 items with language Polish.");
            Assert.AreEqual(6, AsimoBase.ApplyFilters(list, new[] {"Polish", "German"}, new string[0]).Count(),
                            "Expected 6 items with language {Polish, German}.");
            Assert.AreEqual(6, AsimoBase.ApplyFilters(list, new string[0], new[] {"Word"}).Count(),
                            "Expected 6 items with project Word.");
            Assert.AreEqual(3, AsimoBase.ApplyFilters(list, new string[0], new[] {"excel"}).Count(),
                            "Expected 6 items with project excel.");
            Assert.AreEqual(1, AsimoBase.ApplyFilters(list, new string[] {"German"}, new[] {"excel"}).Count(),
                            "Expected 1 items with language German and project excel.");
        }
    }
}