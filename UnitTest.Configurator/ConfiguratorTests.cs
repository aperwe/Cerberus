using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Localization.LocSolutions.Cerberus.Core;
using Microsoft.Localization.LocSolutions.Cerberus.UnitTest.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Localization.LocSolutions.Cerberus.UnitTest.Configurator
{
    /// <summary>
    /// Tests functionality of the UI component (Configurator) of Cerberus.
    /// </summary>
    [TestClass]
    public class ConfiguratorTests : CerberusTestClassBase
    {
        /// <summary>
        /// Tests whether the method to enable a check on all languages works correctly.
        /// </summary>
        [Owner("arturp"), CssIteration("vstfs:///Classification/Node/357fe10e-9d21-4152-ab04-30b91c940cf4"), TestMethod]
        public void TestEnablingCheckOnLanguage()
        {
            var confiuration = new CheckConfiguration();
            var enlistment = new Office14Enlistment();
            var asimo = new DatabaseAsimo(enlistment);
            asimo.CheckFolderPath = Path.Combine(enlistment.CerberusHomeDir, "Checks");
            TestContext.WriteLine("Populating database with information from the current enlistment.");
            asimo.UpdateDatabaseWithEnlistmentContent(confiuration);
            Assert.AreNotEqual(0, confiuration.GetAllChecks());
            foreach (var check in confiuration.GetAllChecks())
            {
                TestContext.WriteLine(check);
            }
            const string checkToRun = "BrandingCheck";
            confiuration.EnableCheckForLanguage(checkToRun, "pl-pl");
            Assert.AreEqual(1, confiuration.LanguageChecks.Count);

            var unfilteredFiles = enlistment.GetFiles("intl", LcxType.Lcl);
            TestContext.WriteLine("Total items: {0}", unfilteredFiles.Count());

            //Split executions per language per locgroup to allow finite time of execution
            var distinctLanguages = asimo.GetDistinctLanguages(unfilteredFiles);
            var distinctProjects = asimo.GetDistinctProjects(unfilteredFiles);

            TestContext.WriteLine("Distinct languages: {0}; {1}", distinctLanguages.Count(), string.Join(", ", distinctLanguages.ToArray()));
            TestContext.WriteLine("Distinct projects: {0}; {1}", distinctProjects.Count(), string.Join(", ", distinctProjects.ToArray()));

            var enabledChecks = confiuration.GetEnabledChecks("pl-pl", "mso");
            TestContext.WriteLine("Enabled checks: {0}", string.Join(", ", enabledChecks.ToArray()));
            Assert.AreEqual(1, enabledChecks.Count());
            var availableChecks = new List<string> {checkToRun, "Not enabled check"};
            TestContext.WriteLine("Physically available checks: {0}", string.Join(", ", availableChecks.ToArray()));

            var availableEnabledCheck = availableChecks.Intersect(enabledChecks);
            Assert.AreEqual(1, availableEnabledCheck.Count());
            TestContext.WriteLine("Available enabled check: {0}", string.Join(", ", availableEnabledCheck.ToArray()));
        }

        /// <summary>
        /// Ensures the API for enabling a check on a project works as expected.
        /// </summary>
        [Owner("arturp"), CssIteration("vstfs:///Classification/Node/357fe10e-9d21-4152-ab04-30b91c940cf4"), TestMethod]
        public void TestEnablingCheckOnProject()
        {
            var confiuration = new CheckConfiguration();
            var enlistment = new Office14Enlistment();
            var asimo = new DatabaseAsimo(enlistment);
            asimo.CheckFolderPath = Path.Combine(enlistment.CerberusHomeDir, "Checks");
            TestContext.WriteLine("Populating database with information from the current enlistment.");
            asimo.UpdateDatabaseWithEnlistmentContent(confiuration);
            Assert.AreNotEqual(0, confiuration.GetAllChecks());
            foreach (var check in confiuration.GetAllChecks())
            {
                TestContext.WriteLine(check);
            }
            const string checkToRun = "BrandingCheck";
            confiuration.EnableCheckForProject(checkToRun, "access");
            Assert.AreEqual(1, confiuration.ProjectChecks.Count);

            var unfilteredFiles = enlistment.GetFiles("intl", LcxType.Lcl);
            TestContext.WriteLine("Total items: {0}", unfilteredFiles.Count());

            //Split executions per language per locgroup to allow finite time of execution
            var distinctLanguages = asimo.GetDistinctLanguages(unfilteredFiles);
            var distinctProjects = asimo.GetDistinctProjects(unfilteredFiles);

            TestContext.WriteLine("Distinct languages: {0}; {1}", distinctLanguages.Count(), string.Join(", ", distinctLanguages.ToArray()));
            TestContext.WriteLine("Distinct projects: {0}; {1}", distinctProjects.Count(), string.Join(", ", distinctProjects.ToArray()));

            var enabledChecks = confiuration.GetEnabledChecks("pl-pl", "word");
            TestContext.WriteLine("Enabled checks: {0}", string.Join(", ", enabledChecks.ToArray()));
            Assert.AreEqual(0, enabledChecks.Count());

            enabledChecks = confiuration.GetEnabledChecks("pl-pl", "access");
            TestContext.WriteLine("Enabled checks: {0}", string.Join(", ", enabledChecks.ToArray()));
            Assert.AreEqual(1, enabledChecks.Count());

            var availableChecks = new List<string> { checkToRun, "Not enabled check" };
            TestContext.WriteLine("Physically available checks: {0}", string.Join(", ", availableChecks.ToArray()));

            var availableEnabledCheck = availableChecks.Intersect(enabledChecks);
            Assert.AreEqual(1, availableEnabledCheck.Count());
            TestContext.WriteLine("Available enabled check: {0}", string.Join(", ", availableEnabledCheck.ToArray()));
        }
    }
}