using System;
using System.Text.RegularExpressions;
using System.Linq;
using Microsoft.Localization.OSLEBot.Core.KnowledgeBase;
using Microsoft.Localization.OSLEBot.ClassificationObjects;
using Microsoft.Localization.OSLEBot.Core.Misc;
using System.Globalization;

namespace Cerberus.Checks
{
    /// <summary>
    /// Description from the spec:
    /// Chinese translations should not use Chinese-specific punctuation characters. Standard (English) punctuation should be used instead.
    /// 
    /// Criteria:
    /// Search for certain commonly used Chinese-specific punctuation characters and warn if they are used.
    /// Output message: Chinese-specific punctuation characters should not be used. Use standard punctuation instead: " 
    /// </summary>
    public class ChinesePunctuationCheck : LocResourceRule
    {
        /// <summary>
        /// Default constructor of the check.
        /// </summary>
        /// <param name="owner">Rule manager passed by OSLEBot engine.</param>
        /// <param name="filteringExpression">Filtering expression passed from the configuration by OSLEBot engine. Used to determine if a check should look at the supplied classification objects depending on whether they meet the filter criteria.</param>
        public ChinesePunctuationCheck(RuleManager owner, string filteringExpression)
            : base(owner, filteringExpression)
        {
           
        }

        /// <summary>
        /// Method called by OSLEBot engine for every resource.
        /// This method contains one or more calls to Check() method that log messages to OSLEBot output when criteria specified in Check() are met.
        /// </summary>
        protected override void Run()
        {
            Check(lr => lr.TargetString.Contains(StringComparison.Ordinal, chinesePunctuation),
                        "Translation contains Chinese-specific punctuation characters. Use English punctuation instead."
                        );

        }

        readonly string[] chinesePunctuation = new string[]
        {
            Char.ConvertFromUtf32(0xFF1F),  //？	FF1F
            Char.ConvertFromUtf32(0xFF1a),  //：	FF1A
            Char.ConvertFromUtf32(0xFF08),  //（	FF08
            Char.ConvertFromUtf32(0xFF09)   //）	FF09
        };
    }
}