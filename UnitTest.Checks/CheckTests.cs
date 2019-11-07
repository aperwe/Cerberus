using System;
using System.IO;
using Microsoft.Localization.LocSolutions.Cerberus.UnitTest.Core;
using Microsoft.Localization.OSLEBot.ClassificationObjects;
using Microsoft.Localization.OSLEBot.Core.DataSource.DataSourcePackages;
using Microsoft.Localization.OSLEBot.Core.Engine;
using Microsoft.Localization.OSLEBot.Core.Misc;
using Microsoft.Localization.OSLEBot.DataAdapters;
using Microsoft.Localization.OSLEBot.DataSource.LocResourcePropertyAdapters;
using Microsoft.Localization.OSLEBot.DataSources;
using Microsoft.Localization.OSLEBot.Exceptions;
using Microsoft.Localization.OSLEBot.Output;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using Microsoft.Practices.AssemblyManagement;

namespace Microsoft.Localization.LocSolutions.Cerberus.UnitTest.Configurator.Checks
{
    /// <summary>
    /// Runs each of the implemented checks individually on a specific test LCL file named Sample.lcl.
    /// </summary>
    [TestClass]
    [DeploymentItem(@"Sample.lcl")]
    [DeploymentItem(@"default.locconfig")]
    [DeploymentItem(@"default.macros")]
    [DeploymentItem(@"LocCultures.xml")]
    [DeploymentItem(@"OSLEBotOutput.xsd")]
    public class CheckTests : CerberusTestClassBase
    {
        /// <summary>
        /// Uses the dll compiled in Cerberus.Checks. this test can be used to run in Debug mode and debug Check code.
        /// </summary>
        [TestMethod]
        [DeploymentItem(@"Cerberus.Checks.dll")]
        public void TestAllRulesInADLL()
        {
            RunAsimoWithChecks("Sample.lcl", "Cerberus.Checks.dll");
        }
        /// <summary>
        /// Uses rule source files to be compiled at runtime. This test is useful to check if rules will compile successfully at runtime but cannot
        /// be used to debug the code of rules.
        /// </summary>
        [TestMethod]
        [DeploymentItem(@".\\Cerberus.Checks\\", "TestChecks")]
        public void TestAllRulesUsingSourceFiles()
        {
            RunAsimoWithChecks("Sample.lcl", Directory.GetFiles(@".\\TestChecks\\", "*.cs"));
        }
        [TestMethod]
        [DeploymentItem(@"OfficeOpenXMLCheck.cs")]
        public void TestOfficeOpenXML()
        {
            RunAsimoWithChecks("Sample.lcl", "OfficeOpenXMLCheck.cs");
        }

        [TestMethod]
        [DeploymentItem(@"VersioningCheck.cs")]
        public void TestVersioningCheck()
        {
            RunAsimoWithChecks("Sample.lcl", "VersioningCheck.cs");
        }

        [TestMethod]
        [DeploymentItem(@"PseudoCheck.cs")]
        public void TestPseudoCheck()
        {
            RunAsimoWithChecks("Sample.lcl", "PseudoCheck.cs");
        }

        [TestMethod]
        [DeploymentItem(@"HybridPseudoCheck.cs")]
        public void TestHybridPseudoCheck()
        {
            RunAsimoWithChecks("Sample.lcl", "HybridPseudoCheck.cs");
        }

        [TestMethod]
        [DeploymentItem(@"BrandingCheck.cs")]
        public void TestBrandingCheck()
        {
            RunAsimoWithChecks("Sample.lcl", "BrandingCheck.cs");
        }

        [TestMethod]
        [DeploymentItem(@"UrlCheck.cs")]
        public void TestUrlCheck()
        {
            RunAsimoWithChecks("Sample.lcl", "UrlCheck.cs");
        }

        [TestMethod]
        [DeploymentItem(@"CopyrightsCheck.cs")]
        public void TestCopyrightsCheck()
        {
            RunAsimoWithChecks("Sample.lcl", "CopyrightsCheck.cs");
        }

        [TestMethod]
        [DeploymentItem(@"JapanKatakanaOnbikiCheck.cs")]
        public void TestJapanKatakanaOnbikiCheck()
        {
            RunAsimoWithChecks("Sample.lcl", "JapanKatakanaOnbikiCheck.cs");
        }

        [TestMethod]
        [DeploymentItem(@"PhoneticsymCheck.cs")]
        public void TestPhoneticsymCheck()
        {
            RunAsimoWithChecks("Sample.lcl", "PhoneticsymCheck.cs");
        }

        [TestMethod]
        [DeploymentItem(@"JapanWorkProjectCheck.cs")]
        public void TestJapanWorkProjectCheck()
        {
            RunAsimoWithChecks("Sample.lcl", "JapanWorkProjectCheck.cs");
        }

        [TestMethod]
        [DeploymentItem(@"RibbonOverrideCheck.cs")]
        [Description("Incorrect KeyTip values should be reported.")]
        public void TestRibbonOverrideCheck()
        {
            RunAsimoWithChecks("Sample.lcl", "RibbonOverrideCheck.cs");
        }

        [TestMethod]
        [DeploymentItem(@"BrandingTokenCheck.cs")]
        [Description("Corrupt branding tokens should be reported.")]
        public void TestBrandingTokenCheck()
        {
            RunAsimoWithChecks("Sample.lcl", "BrandingTokenCheck.cs");
        }

        [TestMethod]
        [DeploymentItem(@"ChinesePunctuationCheck.cs")]
        [Description("")]
        public void TestChinesePunctuationCheck()
        {
            RunAsimoWithChecks("Sample.lcl", "ChinesePunctuationCheck.cs");
        }

        [TestMethod]
        [DeploymentItem(@"EscapeCharsForXMLLocalBindingCheck.cs")]
        [Description("")]
        public void TestEscapeCharsForXMLLocalBindingCheck()
        {
            RunAsimoWithChecks("Sample.lcl", "EscapeCharsForXMLLocalBindingCheck.cs");
        }

        [TestMethod]
        [DeploymentItem(@"KeytipCharactersCheck.cs")]
        [Description("")]
        public void TestKeytipCharactersCheck()
        {
            RunAsimoWithChecks("Sample.lcl", "KeytipCharactersCheck.cs");
        }

        [TestMethod]
        [DeploymentItem(@"AllRightsReservedCheck.cs")]
        [Description("")]
        public void TestAllRightsReservedCheck()
        {
            RunAsimoWithChecks("Sample.lcl", "AllRightsReservedCheck.cs");
        }

        [TestMethod]
        [DeploymentItem(@"KoreanPostPositionsCheck.cs")]
        [Description("")]
        public void TestKoreanPostPositionsCheck()
        {
            RunAsimoWithChecks("Sample.lcl", "KoreanPostPositionsCheck.cs");
        }

        [TestMethod]
        [DeploymentItem(@"BrandingTranslationCheck.cs")]
        [Description("")]
        public void TestBrandingTranslationCheck()
        {
            RunAsimoWithChecks("Sample.lcl", "BrandingTranslationCheck.cs");
        }

        [TestMethod]
        [DeploymentItem(@"ExcelShortcutsCheck.cs")]
        [DeploymentItem(@"xlintl32.rest.lcl")]
        [Description("Checks ExcelShortcutsCheck.cs behavior")]
        public void TestExcelShortcutsCheck()
        {
            RunAsimoWithChecks("xlintl32.rest.lcl", "ExcelShortcutsCheck.cs");
        }

        [TestMethod]
        [DeploymentItem(@"ExcelLocaliseFuncCheck.cs")]
        [DeploymentItem(@"xllex.rest.lcl")]
        [Description("Checks ExcelLocaliseFuncCheck.cs behavior")]
        public void TestExcelLocaliseFuncCheck()
        {
            RunAsimoWithChecks("xllex.rest.lcl", "ExcelLocaliseFuncCheck.cs");
        }

        /********************* Tests go above ****************************************************
         * */
        public void RunAsimoWithChecks(string testDataFile, params string[] testRuleFileNames)
        {
            try
            {
                var assemblyResolver = new AssemblyResolver(TestContext.TestDeploymentDir, @"%OTOOLS%\bin\lsbuild6.0");
                assemblyResolver.Init();
                var inputCfg = CreateTestConfig(TestContext, testDataFile, testRuleFileNames);
                var engine = new OSLEBotEngine(inputCfg);
                var engineRan = engine.StartRun();
                if (engineRan)
                {
                    Console.WriteLine("OSLEBotExec is waiting for engine activity to complete...");
                    engine.CoProcessingDone.WaitOne();  // block this thread until the engine is done, since we have nothing else to do and this is the main thread.
                    engine.AllOutputFinished.WaitOne(); //wait for output to finish
                    engine.Cleanup();
                }
                Assert.IsFalse(engine.EngineLoggedErrors, "Engine reported some execution errors.");
                //Attach results from OSLEBot to the test results.
                var fileInfo = new FileInfo(string.Format("{0}_OSLEBotOutput.xml", testDataFile));
                if (fileInfo.Exists)
                {
                    TestContext.WriteLine("Adding Asimo results file ({0}) to test results.", fileInfo.Name);
                    TestContext.AddResultFile(fileInfo.Name);
                    TestContext.AddResultFile(fileInfo.FullName);
                }
            }
            catch (OperationCanceledException ex) //Thrown when Data source providers were not specified.
            //Thrown when Property providers were not specified
            //Thrown when Data adapters were not specified
            //Thrown when Data Source packages list is null.
            {
                if (ex.InnerException != null)
                {
                    TestContext.WriteLine("InnerException: {0}", ex.InnerException.Message);
                    if (ex.InnerException.InnerException != null)
                    {
                        TestContext.WriteLine("InnerException.Inner: {0}", ex.InnerException.InnerException.Message);
                    }
                }
                Assert.Fail("OSLEBot engine failed because {0}", ex.Message);
            }
            catch (OSLEBotEngineInitializationException ex)//Thrown when there are no rules to run on the input set.
            {
                if (ex.InnerException != null)
                {
                    TestContext.WriteLine("InnerException: {0}", ex.InnerException.Message);
                    if (ex.InnerException.InnerException != null)
                    {
                        TestContext.WriteLine("InnerException.Inner: {0}", ex.InnerException.InnerException.Message);
                    }
                }
                Assert.Fail("OSLEBot engine failed because {0}", ex.Message);
            }
        }
        internal static EngineConfig CreateTestConfig(TestContext testContext, string testDataFile, params string[] testRuleFileNames)
        {
            var configuration = new EngineConfig { EngineLog = string.Format("Engine log for {0}.txt", testContext.TestName) };

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
            var package = new DataSourcePackage(); //This package will contain 2 data sources
            package.SetCOType<LocResource>();

            var dataSource = new DataSourceInfo();
            dataSource.SetSourceType<LocDocument>();
            dataSource.SetSourceLocation(testDataFile);
            package.AddDataSource(dataSource);

            dataSource = new DataSourceInfo();
            dataSource.SetSourceType<ConfigDictionary>();
            var configDictionary = new ConfigDictionary
                                       {
                                           {"BuildNumber", "1"},
                                           {"Project", "test Romka"},
                                           {"Locgroup", "MacBU"}
                                       };
            dataSource.SetSourceLocation(configDictionary);
            package.AddDataSource(dataSource);

            configuration.AddDataSourcePackage(package);
            #endregion

            #region Add rules to be tested
            foreach (var ruleFileName in testRuleFileNames)
            {
                configuration.AddRule(
                    ruleFileName,
                    string.Empty,
                    Path.GetExtension(ruleFileName).Equals(".cs", StringComparison.OrdinalIgnoreCase) ? RuleContainerType.Source : RuleContainerType.Module);
            }
            
            #endregion
            #region And some configuration for dynamic compiler
            configuration.AddBinaryReference("System.Core.dll");
            configuration.AddBinaryReference("mscorlib.dll");
            configuration.AddBinaryReference("System.dll");
            configuration.AddBinaryReference("Microsoft.Localization.dll");
            configuration.AddBinaryReference("OSLEBotCore.dll");
            configuration.AddBinaryReference("LocResource.dll");
            configuration.AddBinaryReference("OSLEBot.LCXDataAdapter.dll");
            configuration.AddBinaryReference("Logger.dll");
            configuration.AddBinaryReference("Cerberus.Core.dll");
            configuration.AddBinaryReference("System.Xml.dll");
            configuration.AddBinaryReference("System.Xml.Linq.dll");
            #endregion
            

            #region Configure output behavior
            var outputCfg = new OSLEBot.Core.Output.OutputWriterConfig();
            outputCfg.SetDataSourceProvider<LocResourcePerLCLOutputWriter>();
            outputCfg.Schema = "OSLEBotOutput.xsd";
            outputCfg.Path = testContext.TestDeploymentDir;
            outputCfg.AddPropertyToIncludeInOutput("LSResID");
            outputCfg.AddPropertyToIncludeInOutput("SourceString");
            outputCfg.AddPropertyToIncludeInOutput("Comments");
            outputCfg.AddPropertyToIncludeInOutput("TargetString");
            outputCfg.AddPropertyToIncludeInOutput("TargetCulture");
            outputCfg.AddPropertyToIncludeInOutput("Locgroup");
            outputCfg.AddPropertyToIncludeInOutput("Project");
            configuration.OutputConfigs.Add(outputCfg);
            #endregion

            return configuration;
        }
    }
}
