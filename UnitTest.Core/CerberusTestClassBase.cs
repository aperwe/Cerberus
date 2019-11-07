using Microsoft.Localization.LocSolutions.Logger;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Localization.LocSolutions.Cerberus.UnitTest.Core
{
    /// <summary>
    /// Base class for any UnitTest class. Contains stuff that is repeated everywhere.
    /// </summary>
    public class CerberusTestClassBase
    {
        private TCLogger _osleBotRedirectedLogger;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        /// <summary>
        /// Initializes the test context for for each test.
        /// </summary>
        [TestInitialize]
        public void Initalize()
        {
            _osleBotRedirectedLogger = new TCLogger(TestContext);
            LoggerSAP.RegisterLogger(_osleBotRedirectedLogger);
        }

        /// <summary>
        /// Cleans up the test context after each test.
        /// </summary>
        [TestCleanup]
        public void Cleanup()
        {
            if (_osleBotRedirectedLogger == null) return;
            LoggerSAP.UnregisterLogger(_osleBotRedirectedLogger);
            _osleBotRedirectedLogger = null;
        }
    }
}