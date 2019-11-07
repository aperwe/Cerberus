using System;
using System.Text.RegularExpressions;
using Microsoft.Localization.OSLEBot.Core.KnowledgeBase;

namespace Cerberus.Checks
{
    /// <summary>
    /// Japan "Work" translation check
    /// Description from the spec:
    /// OSLEBot checks through Japanese translations for entries that contain incorrect translations for term "work".
    /// Criteria:
    /// [Ensure translation does not contain old Japanese translation for Work.
    /// Essentially, look for this sequence 稼動 in a ny Japanese string.
    /// 
    /// Output message: "Japanese translation for \"work\" should be 稼働 and not 稼動."
    /// </summary>
    public class JapaneseWorkTranslationCheck : LocResourceRule
    {
        /// <summary>
        /// Default constructor of the check.
        /// </summary>
        /// <param name="owner">Rule manager passed by OSLEBot engine.</param>
        /// <param name="filteringExpression">Filtering expression passed from the configuration by OSLEBot engine. Used to determine if a check should look at the supplied classification objects depending on whether they meet the filter criteria.</param>
        public JapaneseWorkTranslationCheck(RuleManager owner, string filteringExpression) : base(owner, filteringExpression) { }

        /// <summary>
        /// Method called by OSLEBot engine for every resource.
        /// This method contains one or more calls to Check() method that log messages to OSLEBot output when criteria specified in Check() are met.
        /// </summary>
        protected override void Run()
        {
            Check(lr =>
                  lr.TargetStringNoHK.Contains(StringComparison.OrdinalIgnoreCase, incorrectJapaneseTranslation)
                  , "Japanese translation for \"work\" should be 稼働 and not 稼動.");
        }

        /// <summary>
        /// expected translation for the term "work"
        /// </summary>
        private readonly string correctJapaneseTranslation = "稼働";
        /// <summary>
        /// Incorrect translation for the term "work"
        /// </summary>
        private readonly string incorrectJapaneseTranslation = "稼動";
        
    }
}
