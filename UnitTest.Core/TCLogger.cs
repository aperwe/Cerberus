using System.Text.RegularExpressions;
using Microsoft.Localization.LocSolutions.Logger;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Localization.LocSolutions.Cerberus.UnitTest.Core
{
    /// <summary>
    /// Logger that can log to TestContext. Use in UnitTests methods to intercept OSLEBot debug logs.
    /// </summary>
    public class TCLogger : ILogger
    {
        #region ILogger Members
        /// <summary>
        /// Logs to TestContext and removes the very long thread name.
        /// </summary>
        public void AcceptLogEntry(LogLevel logLevel, string entry)
        {
            Output.WriteLine("{0}: {1}", logLevel, _overheadRemover.Replace(entry, "[]"));
        }

        readonly Regex _overheadRemover = new Regex(@"\[Agent:.+'\]", RegexOptions.Compiled);

        #endregion

        TestContext Output { get; set; }

        /// <summary>
        /// Default constructor of logger that writes output entries to TestContext.
        /// </summary>
        /// <param name="output">Receiver of output.</param>
        public TCLogger(TestContext output)
        {
            Output = output;
        }
    }
}