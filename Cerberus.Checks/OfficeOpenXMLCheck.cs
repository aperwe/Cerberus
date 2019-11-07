using System.Text.RegularExpressions;
using Microsoft.Localization.OSLEBot.Core.KnowledgeBase;

namespace Cerberus.Checks
{
    /// <summary>
    /// Description from the spec:
    /// Search all source terms for instances of "Office Open XML" and ensure that the exact term is present in the translation.
    /// When the source contains the search term and the translation does not, flag as issue.
    /// Output message: "Office Open XML has been translated or is missing from the translation in this resource."
    /// </summary>
    public class OfficeOpenXMLCheck : LocResourceRule
    {
        /// <summary>
        /// Default constructor of the check.
        /// </summary>
        /// <param name="owner">Rule manager passed by OSLEBot engine.</param>
        /// <param name="filteringExpression">Filtering expression passed from the configuration by OSLEBot engine. Used to determine if a check should look at the supplied classification objects depending on whether they meet the filter criteria.</param>
        public OfficeOpenXMLCheck(RuleManager owner, string filteringExpression) : base(owner, filteringExpression) { }

        /// <summary>
        /// Method called by OSLEBot engine for every resource.
        /// This method contains one or more calls to Check() method that log messages to OSLEBot output when criteria specified in Check() are met.
        /// </summary>
        protected override void Run()
        {
            Check(lr =>
                  lr.SourceStringNoHK.RegExp(_officeOpenXMLDetector)
                  && !lr.TargetStringNoHK.RegExp(_officeOpenXMLDetector)
                  , "Office Open XML has been translated or is missing from the translation in this resource.");
        }

        private readonly Regex _officeOpenXMLDetector = new Regex(@"Office\W+Open\W+XML",
                                                                  RegexOptions.IgnoreCase | RegexOptions.Compiled |
                                                                  RegexOptions.ExplicitCapture);
    }
}