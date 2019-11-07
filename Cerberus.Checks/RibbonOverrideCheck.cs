using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Localization.OSLEBot.ClassificationObjects;
using Microsoft.Localization.OSLEBot.Core.KnowledgeBase;
using System.Text.RegularExpressions;
using Microsoft.Localization.LocSolutions.Cerberus.Core;

namespace Cerberus.Checks
{
    /// <summary>
    /// <list type="ol">
    /// <listheader>This check implements logic related to Ribbon Override strings.</listheader>
    /// Description:
    /// Ribbon Override strings (string that contain "KeytipOverride" in their LocStudio resource ID) need to be same as source for 
    /// EA languages and empty for all others.
    /// Output message: 
    /// "Incorrect Ribbon Override value. Must be same as source for EA languages and empty for other languages."
    /// </list>
    /// </summary>
    public class RibbonOverrideCheck : LocResourceRule
    {
        /// <summary>
        /// Default constructor of the check.
        /// </summary>
        /// <param name="owner">Rule manager passed by OSLEBot engine.</param>
        /// <param name="filteringExpression">Filtering expression passed from the configuration by OSLEBot engine. Used to determine if a check should look at the supplied classification objects depending on whether they meet the filter criteria.</param>
        public RibbonOverrideCheck(RuleManager owner, string filteringExpression)
            : base(owner, filteringExpression)
        {
        }

        /// <summary>
        /// Method called by OSLEBot engine for every resource.
        /// This method contains one or more calls to Check() method that log messages to OSLEBot output when criteria specified in Check() are met.
        /// </summary>
        protected override void Run()
        {
            Check(lr => 
                ResourceIsAKeytipOverride(lr)
                &&
                LanguageIsEA(lr.TargetCulture) ? !lr.SourceString.Value.Equals(lr.TargetString.Value, StringComparison.Ordinal) : lr.TargetString.Length != 0,
                "Incorrect Ribbon Override value. Must be same as source for EA languages and empty for other languages.");

        }

        static private bool LanguageIsEA(Microsoft.Localization.LocCulture lc)
        {
            return ConfigurationHelper.EALanguages.Contains(lc.Name);
        }

        static private bool ResourceIsAKeytipOverride(LocResource lr)
        {
            return lr.LSResID.Value.IndexOf("KeytipOverride", StringComparison.OrdinalIgnoreCase) > -1;
        }
    }

}