using System.Text.RegularExpressions;
using Microsoft.Localization.OSLEBot.ClassificationObjects;
using Microsoft.Localization.OSLEBot.Core.Engine.Properties;
using Microsoft.Localization.OSLEBot.Core.KnowledgeBase;

namespace Cerberus.Checks
{
    /// <summary>
    /// Description from the spec:
    /// If a source string contains URL’s which have lcid information
    /// or language tag information in the link, and the translation
    /// does not have the correct language lcid or language tag
    /// for the language being checked OSLEBOT should flag this.
    /// See below for example strings. An exception table should be allowed for languages
    /// that do not have localized pages.
    /// Example strings:
    /// http://r.office.microsoft.com/r/rlidGrooveC103?clid=en-us
    /// http://r.office.microsoft.com/r/rlidSCGuidePictureOCR?clid=1033&ver=12&app=onenote.exe
    /// Criteria:
    /// [Where source contains LCID or language tag, flag if an issue if incorrect language LCID
    /// or language tag is not present in the translation]
    /// Output message: "The URL is pointing to a US site. It should point to localized page."
    /// </summary>
    public class UrlCheck : LocResourceRule
    {
        /// <summary>
        /// Default constructor of the check.
        /// </summary>
        /// <param name="owner">Rule manager passed by OSLEBot engine.</param>
        /// <param name="filteringExpression">Filtering expression passed from the configuration by OSLEBot engine. Used to determine if a check should look at the supplied classification objects depending on whether they meet the filter criteria.</param>
        public UrlCheck(RuleManager owner, string filteringExpression) : base(owner, filteringExpression) { }

        /// <summary>
        /// Method called by OSLEBot engine for every resource.
        /// This method contains one or more calls to Check() method that log messages to OSLEBot output when criteria specified in Check() are met.
        /// </summary>
        protected override void Run()
        {
            Check(lr =>
                lr.SourceStringNoHK.RegExp(_urlllccDetector)
                  && !lr.TargetStringNoHK.RegExp(string.Format("http://.+{0}", lr.TargetCulture.Value.Name))
                  , _outputMessage);
            Check(lr =>
                lr.SourceStringNoHK.RegExp(_urllcidDetector)
                  && !lr.TargetStringNoHK.RegExp(string.Format("http://.+{0}", lr.TargetCulture.Value.LCID))
                  , _outputMessage);
        }

        /// <summary>
        /// Check initialization before the first call to Run(). Called by OSLEBot engine.
        /// </summary>
        protected override void Init()
        {
            base.Init();
            _outputMessage = "The URL is pointing to a US site. It should point to localized page.";
        }

        private readonly Regex _urlllccDetector = new Regex(@"http://.+en-us", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.ExplicitCapture);
        private readonly Regex _urllcidDetector = new Regex(@"http://.+1033", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.ExplicitCapture);
        private string _outputMessage;
    }

    /// <summary>
    /// Convenience methods.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Determines whether this string property matches the specified regex string.
        /// This is a convenience method.
        /// Casing of the input and output strings is ignored.
        /// </summary>
        /// <param name="property"></param>
        /// <param name="regexString">String for a regular expression</param>
        /// <returns>True if the string in the string property matches the specifed regex. False otherwise.</returns>
        public static bool RegExp(this StringProperty property, string regexString)
        {
            return Regex.IsMatch(property.Value, regexString, RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase);
        }
    }
}