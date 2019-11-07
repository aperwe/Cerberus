using System.Text.RegularExpressions;
using Microsoft.Localization.OSLEBot.Core.KnowledgeBase;

namespace Cerberus.Checks
{
    /// <summary>
    /// Description from the spec:
    /// Search for versioning strings and ensure that the translation matches the version in the source.
    /// Where the source contains versioning information and the translation does not match the versioning, flag as issue.
    /// Current versioning to detect:
    /// 14.0.xxxx.yyyy
    /// Office 2010
    /// Output message: "The versioning translation does not match the source."
    /// </summary>
    public class VersioningCheck : LocResourceRule
    {
        /// <summary>
        /// Default constructor of the check.
        /// </summary>
        /// <param name="owner">Rule manager passed by OSLEBot engine.</param>
        /// <param name="filteringExpression">Filtering expression passed from the configuration by OSLEBot engine. Used to determine if a check should look at the supplied classification objects depending on whether they meet the filter criteria.</param>
        public VersioningCheck(RuleManager owner, string filteringExpression) : base(owner, filteringExpression) { }

        /// <summary>
        /// Method called by OSLEBot engine for every resource.
        /// This method contains one or more calls to Check() method that log messages to OSLEBot output when criteria specified in Check() are met.
        /// </summary>
        protected override void Run()
        {
            Check(lr =>
                  lr.SourceStringNoHK.RegExp(versionStringDetector1)
                  && !lr.TargetStringNoHK.RegExp(versionStringDetector1)
                  , OutputMessage);
            Check(lr =>
                  lr.SourceStringNoHK.RegExp(versionStringDetector2)
                  && lr.TargetStringNoHK != lr.SourceStringNoHK
                  , OutputMessage);
            Check(lr =>
                  lr.SourceStringNoHK.RegExp(versionStringDetector3)
                  && !lr.TargetStringNoHK.RegExp(versionStringDetector3)
                  , OutputMessage);
        }

        private readonly Regex versionStringDetector1 = new Regex(@"14[.]0[.]\d{4}[.]\d{4}", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.ExplicitCapture);
        private readonly Regex versionStringDetector2 = new Regex(@"^14[.]0[.]\d{4}[.]\d{4}$", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.ExplicitCapture);
        private readonly Regex versionStringDetector3 = new Regex(@"Office\s+2010", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.ExplicitCapture);

        /// <summary>
        /// Output message written as member variable because it may be reused.
        /// </summary>
        private const string OutputMessage = "The versioning translation does not match the source.";
    }
}