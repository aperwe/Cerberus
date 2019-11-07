using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Localization.OSLEBot.ClassificationObjects;
using Microsoft.Localization.OSLEBot.Core.KnowledgeBase;

namespace Cerberus.Checks
{
    /// <summary>
    /// Description:
    /// Check the target strings for existence of obsolete or otherwise incorrect branding names.
    /// 
    /// Criteria:
    /// TO BE DECIDED: we probably do not want to flag cases where Source string contains an incorrect branding name, as
    /// we would be flagging issues in source strings for each language we are processing. This has to be probably done centrally,
    /// using a different tool. We may want to limit cerberus to detecting cases where the English string does not contain the incorrect entry,
    /// but target string still contains such entry.
    /// </summary>
    public class IncorrectBrandingCheck : LocResourceRule
    {
        /// <summary>
        /// Default constructor of the check.
        /// </summary>
        /// <param name="owner">Rule manager passed by OSLEBot engine.</param>
        /// <param name="filteringExpression">Filtering expression passed from the configuration by OSLEBot engine. Used to determine if a check should look at the supplied classification objects depending on whether they meet the filter criteria.</param>
        public IncorrectBrandingCheck(RuleManager owner, string filteringExpression) : base(owner, filteringExpression) { }

        /// <summary>
        /// Method called by OSLEBot engine for every resource.
        /// This method contains one or more calls to Check() method that log messages to OSLEBot output when criteria specified in Check() are met.
        /// </summary>
        protected override void Run()
        {
           
        }
    }
   
}