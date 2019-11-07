using System;
using Microsoft.Localization.OSLEBot.Core.KnowledgeBase;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Cerberus.Checks
{
    /// <summary>
    /// PHONETICSYM check
    /// Description from the spec:
    /// Resources that contain the word PHONETICSYM in Resource ID. These resources require translation only in Hiragana, 
    /// English alphabet, or numeral. No Katakana or Kanji characters should be allowed. There are cases where source and 
    /// translation are both blank, which is expected.
    /// 
    /// Criteria:
    /// Check if PHONETICSYM strings conain any Katakana or Kanji.
    /// 
    /// Output message: "This resource is marked as PHONETICSYM and should be contain Hiragana, Latin or numeric characters."
    /// </summary>
    public class JapanesePhoneticsymCheck : LocResourceRule
    {
        /// <summary>
        /// Default constructor of the check.
        /// </summary>
        /// <param name="owner">Rule manager passed by OSLEBot engine.</param>
        /// <param name="filteringExpression">Filtering expression passed from the configuration by OSLEBot engine. Used to determine if a check should look at the supplied classification objects depending on whether they meet the filter criteria.</param>
        public JapanesePhoneticsymCheck(RuleManager owner, string filteringExpression) : base(owner, filteringExpression) { }

        /// <summary>
        /// Method called by OSLEBot engine for every resource.
        /// This method contains one or more calls to Check() method that log messages to OSLEBot output when criteria specified in Check() are met.
        /// </summary>
        protected override void Run()
        {

            Check(lr => 
                lr.LSResID.Value.Contains("PHONETICSYM")
                &&
                lr.TargetString.RegExp(containsKatakanaOrKanji)
                  , "This resource is marked as PHONETICSYM and should not contain Katakana or Kanji.");
            
        }
        /// <summary>
        /// Checks if string contains at least one Katakana or Kanji character.
        /// </summary>
        private readonly Regex containsKatakanaOrKanji = new Regex(@"\p{IsKatakana}|\p{IsCJKUnifiedIdeographs}", RegexOptions.Compiled);
    }
}