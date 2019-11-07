using System.Linq;
using Microsoft.Localization.LocSolutions.Cerberus.Executor;
using Microsoft.Localization.LocSolutions.Cerberus.UnitTest.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Localization.LocSolutions.Cerberus.UnitTest.Executor
{
    /// <summary>
    /// Tests of the Executor piece of Cerberus.
    /// </summary>
    [TestClass]
    public class ExecutorTests : CerberusTestClassBase
    {
        [CssIteration("vstfs:///Classification/Node/357fe10e-9d21-4152-ab04-30b91c940cf4"), Description("Makes sure that Executor, when executed with no parameters, returns success.")]
        [Owner("arturp")]
        [TestMethod]
        public void TestThatProgramReturnsSuccessWithDefaultParameters()
        {
            var instance = new Program {TestMode = true};
            TestContext.WriteLine("Creating program instance.");
            var returnValue = instance.RunProgram(new string[0]);
            Assert.AreEqual(ProgramReturnValue.Success, returnValue);
        }
        [CssIteration("vstfs:///Classification/Node/357fe10e-9d21-4152-ab04-30b91c940cf4"), TestMethod]
        [Owner("arturp")]
        [Description("Tests the behavior of command line parser when presented with 0 arguments.")]
        public void TestArgumentParserWithNoArguments()
        {
            var parser = ArgumentParser.Parse(new string[] {});
            TestContext.WriteLine("Testing parser with 0 arguments.");
            Assert.IsTrue(parser.Correct);
            Assert.AreEqual(string.Empty, parser.InvalidArgument);
            Assert.AreEqual(0, parser.Languages.Count());
            Assert.IsFalse(parser.ShouldFilterByLanguage);
            Assert.AreEqual(0, parser.Projects.Count());
            Assert.IsFalse(parser.ShouldFilterByProject);
        }
        [CssIteration("vstfs:///Classification/Node/357fe10e-9d21-4152-ab04-30b91c940cf4"), TestMethod]
        [Owner("arturp")]
        [Description("Tests the behavior of command line parser when presented with 2 languages.")]
        public void TestArgumentParserWith2Languages()
        {
            var args = new[]
                           {
                               "-l",
                               "ja-jp.pseudo",
                               "-l",
                               "de-de"
                           };
            var parser = ArgumentParser.Parse(args);
            TestContext.WriteLine("Testing parser with {0} arguments.", args.Length);
            TestContext.WriteLine("Arguments: {0}", string.Join(" ", args));
            Assert.IsTrue(parser.Correct);
            Assert.AreEqual(string.Empty, parser.InvalidArgument);
            Assert.AreEqual(2, parser.Languages.Count());
            TestContext.WriteLine("Languages identfied: {0}", string.Join(", ", parser.Languages.ToArray()));
            TestContext.WriteLine("Projects identfied: {0}", string.Join(", ", parser.Projects.ToArray()));
            Assert.IsTrue(parser.ShouldFilterByLanguage);
            Assert.AreEqual(0, parser.Projects.Count());
            Assert.IsFalse(parser.ShouldFilterByProject);
        }
        [CssIteration("vstfs:///Classification/Node/357fe10e-9d21-4152-ab04-30b91c940cf4"), TestMethod]
        [Owner("arturp")]
        [Description("Tests the behavior of command line parser when presented with 3 projects.")]
        public void TestArgumentParserWith3Projects()
        {
            var args = new[]
                           {
                               "-p",
                               "wordtemplates",
                               "-p",
                               "xdocs",
                               "-p",
                               "xdserver"
                           };
            var parser = ArgumentParser.Parse(args);
            TestContext.WriteLine("Testing parser with {0} arguments.", args.Length);
            TestContext.WriteLine("Arguments: {0}", string.Join(" ", args));
            Assert.IsTrue(parser.Correct, "Should be correct.");
            Assert.AreEqual(string.Empty, parser.InvalidArgument, "There should be no invalid argument.");
            Assert.AreEqual(0, parser.Languages.Count(), "Language count should be 0.");
            TestContext.WriteLine("Languages identfied: {0}", string.Join(", ", parser.Languages.ToArray()));
            TestContext.WriteLine("Projects identfied: {0}", string.Join(", ", parser.Projects.ToArray()));
            Assert.IsFalse(parser.ShouldFilterByLanguage, "Expected no filter by language.");
            Assert.AreEqual(3, parser.Projects.Count(), "Expected 3 projects.");
            Assert.IsTrue(parser.ShouldFilterByProject, "Excpected filter by project to be enabled.");
        }
        [CssIteration("vstfs:///Classification/Node/357fe10e-9d21-4152-ab04-30b91c940cf4"), TestMethod]
        [Owner("arturp")]
        [Description("Tests the behavior of command line parser when presented with 3 projects.")]
        public void TestArgumentParserWithProjectsAndLanguages()
        {
            var args = new[]
                           {
                               "-p",
                               "wordtemplates",
                               "-p",
                               "xdocs",
                               "-l",
                               "ja-jp.pseudo",
                               "-p",
                               "xdserver"
                           };
            var parser = ArgumentParser.Parse(args);
            TestContext.WriteLine("Testing parser with {0} arguments.", args.Length);
            TestContext.WriteLine("Arguments: {0}", string.Join(" ", args));
            Assert.IsTrue(parser.Correct);
            Assert.AreEqual(string.Empty, parser.InvalidArgument);
            Assert.AreEqual(1, parser.Languages.Count(), "Expected 1 language.");
            TestContext.WriteLine("Languages identfied: {0}", string.Join(", ", parser.Languages.ToArray()));
            TestContext.WriteLine("Projects identfied: {0}", string.Join(", ", parser.Projects.ToArray()));
            Assert.IsTrue(parser.ShouldFilterByLanguage);
            Assert.AreEqual(3, parser.Projects.Count());
            Assert.IsTrue(parser.ShouldFilterByProject);
        }
        [CssIteration("vstfs:///Classification/Node/357fe10e-9d21-4152-ab04-30b91c940cf4"), TestMethod]
        [Owner("arturp")]
        [Description("Tests the behavior of command line parser when presented with invalid arguments.")]
        public void TestArgumentParserWithInvalidArguments()
        {
            var args = new[]
                           {
                               "-s",
                               "wordtemplates",
                               "-p",
                               "xdocs",
                               "-l",
                               "ja-jp.pseudo",
                               "-p",
                               "xdserver"
                           };
            var parser = ArgumentParser.Parse(args);
            TestContext.WriteLine("Testing parser with {0} arguments.", args.Length);
            TestContext.WriteLine("Arguments: {0}", string.Join(" ", args));
            Assert.IsFalse(parser.Correct);
            TestContext.WriteLine("Invalid argument: {0}", parser.InvalidArgument);
            Assert.AreEqual("-s", parser.InvalidArgument);
            Assert.AreEqual(0, parser.Languages.Count(), "Expected 0 languages.");
            TestContext.WriteLine("Languages identfied: {0}", string.Join(", ", parser.Languages.ToArray()));
            TestContext.WriteLine("Projects identfied: {0}", string.Join(", ", parser.Projects.ToArray()));
            Assert.IsFalse(parser.ShouldFilterByLanguage);
            Assert.AreEqual(0, parser.Projects.Count(), "Expected 0 projects.");
            Assert.IsFalse(parser.ShouldFilterByProject);
        }
    }
}