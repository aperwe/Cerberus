using Microsoft.Localization;
using Microsoft.Localization.OSLEBot.Core.KnowledgeBase;

namespace Cerberus.Checks
{
    /// <summary>
    /// Description from the spec:
    /// Search each resource for [Where TranslationOrigin=PseudoLoc] then flag an issue.
    /// Output message: "Translation is a pseudo string (marked as TranslationOrigin == PseudoLoc)."
    /// </summary>
    public class PseudoCheck : LocResourceRule
    {
        /// <summary>
        /// Default constructor of the check.
        /// </summary>
        /// <param name="owner">Rule manager passed by OSLEBot engine.</param>
        /// <param name="filteringExpression">Filtering expression passed from the configuration by OSLEBot engine. Used to determine if a check should look at the supplied classification objects depending on whether they meet the filter criteria.</param>
        public PseudoCheck(RuleManager owner, string filteringExpression) : base(owner, filteringExpression) { }

        /// <summary>
        /// Method called by OSLEBot engine for every resource.
        /// This method contains one or more calls to Check() method that log messages to OSLEBot output when criteria specified in Check() are met.
        /// </summary>
        protected override void Run()
        {
            Check(lr =>
                  lr.TranslationOrigin == TransOrigin.PseudoLoc
                  , "Translation is a pseudo string (marked as TranslationOrigin == PseudoLoc).");
        }
    }
}