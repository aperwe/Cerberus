using Cerberus.LogAnalyser;
using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace Cerberus.Checks
{
    
    
    /// <summary>
    ///This is a test class for ArgumentParserTest and is intended
    ///to contain all ArgumentParserTest Unit Tests
    ///</summary>
    [TestClass()]
    public class ArgumentParserTest
    {


        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion


        [TestMethod()]
        [ExpectedException(typeof(System.ArgumentNullException))]
        public void ArgumentParser_NoParamaters()
        {
            ArgumentParser target = new ArgumentParser(null);
        }



        [TestMethod()]
        public void ArgumentParser_CorrectParamaters_FastTrack()
        {
            string[] args = { "c:\\office", "FastTrack" };
            ArgumentParser target = new ArgumentParser(args);
            Assert.AreEqual(UploadLocation.FastTrack, target.Upload);
            Assert.AreEqual("c:\\office", target.OfficeDir);
        }


        [TestMethod()]
        public void ArgumentParser_CorrectParamaters_Backup()
        {
            string[] args = { "c:\\office", "Backup" };
            ArgumentParser target = new ArgumentParser(args);
            Assert.AreEqual(UploadLocation.BackUp , target.Upload);
            Assert.AreEqual("c:\\office", target.OfficeDir);
        }

        [TestMethod()]
        public void ArgumentParser_CorrectParamaters_CleanUp()
        {
            string[] args = { "c:\\office", "CleanUp" };
            ArgumentParser target = new ArgumentParser(args);
            Assert.AreEqual(UploadLocation.CleanUp, target.Upload);
            Assert.AreEqual("c:\\office", target.OfficeDir);
        }


        [TestMethod()]
        public void ArgumentParser_CorrectParamaters_Local()
        {
            string[] args = { "c:\\office", "Local" };
            ArgumentParser target = new ArgumentParser(args);
            Assert.AreEqual(UploadLocation.Local , target.Upload);
            Assert.AreEqual("c:\\office", target.OfficeDir);
        }


        [TestMethod()]
        [ExpectedException(typeof(System.IO.DirectoryNotFoundException))]
        public void ArgumentParser_BothArgsInvalidDir()
        {
            string[] args = { "c:\\brokendirectory", "FastTrack" };
            ArgumentParser target = new ArgumentParser(args);
        }

        [TestMethod()]
        [ExpectedException(typeof(System.ArgumentException))]
        public void ArgumentParser_BothArgsInvalidUpload()
        {
            string[] args = { "c:\\office", "Wibble" };
            ArgumentParser target = new ArgumentParser(args);
        }

        [TestMethod()]
        public void ArgumentParser_OneArgumentHelp()
        {
            string[] args = { "help" };
            ArgumentParser target = new ArgumentParser(args);
            Assert.AreEqual(UploadLocation.Help, target.Upload);
        }

        [TestMethod()]
        [ExpectedException(typeof(System.ArgumentException))]
        public void ArgumentParser_OneArgumentNotHelp()
        {
            string[] args = { "notHelp" };
            ArgumentParser target = new ArgumentParser(args);
        }
        
    }
}
