using System;
using System.Text.RegularExpressions;
using System.Linq;
using Microsoft.Localization.OSLEBot.Core.KnowledgeBase;
using Microsoft.Localization.OSLEBot.ClassificationObjects;
using Microsoft.Localization.OSLEBot.Core.Misc;
using System.Globalization;
using System.Collections.Generic;
using Microsoft.Localization;
using Microsoft.Localization.OSLEBot.Core.Engine.Properties;

namespace Cerberus.Checks
{
    /// <summary>
    /// Description from the spec:
    /// Ensure that if the following two resources:
    /// xllex.resT: "idftCOUNT" &  "idftAVERAGE" are localised that the value for
    /// Resource id: idIntlSwapFuncs should be equal to 1.
    /// If the two resources are unlocalised then the value should be 0.
    /// Check will be:
    /// [if source != translation for xllex.resT: "idftCOUNT" &  "idftAVERAGE" then idIntlSwapFuncs = 1.
    /// If source == translation for xllex.resT: "idftCOUNT" &  "idftAVERAGE" then idIntlSwapFuncs = 0]
    /// 
    /// Requested by: sarahs
    /// Project: Excel (xl)
    /// 
    /// Criteria:
    /// TBD
    /// 
    /// Check message: "The translation for this shortcut is incorrect."
    /// </summary>
    public class ExcelLocaliseFuncCheck : LocResourceRule
    {
        /// <summary>
        /// Name of the file that this check looks at.
        /// </summary>
        private const string XlFileName = "xllex.rest";

        private readonly StringProperty PropertyFriendlyIdftCount = new StringProperty("FriendlyID", "idftCOUNT");
        private readonly StringProperty PropertyFriendlyIdftAverage = new StringProperty("FriendlyID", "idftAVERAGE");

        /// <summary>
        /// Default constructor of the check.
        /// </summary>
        /// <param name="owner">Rule manager passed by OSLEBot engine.</param>
        /// <param name="filteringExpression">Filtering expression passed from the configuration by OSLEBot engine. Used to determine if a check should look at the supplied classification objects depending on whether they meet the filter criteria.</param>
        public ExcelLocaliseFuncCheck(RuleManager owner, string filteringExpression)
            : base(owner, filteringExpression)
        {
        }

        /// <summary>
        /// Method called by OSLEBot engine for every resource.
        /// This method contains one or more calls to Check() method that log messages to OSLEBot output when criteria specified in Check() are met.
        /// </summary>
        protected override void Run()
        {
            var currentResource = (LocResource)CurrentCO;
            if (!currentResource.FileName.Equals(StringComparison.InvariantCultureIgnoreCase, XlFileName)) //Skip any file that is not this Excel file
            {
                return;
            }

            if (currentResource.FriendlyID != "idIntlSwapFuncs") //We put this check in front because it is repeated 4 times below and we want to improve performance, by skipping upfront any resource that doesn't match all 4.
            {
                return;
            }

            //Any lr (resource) executing the code below will have lr.FriendlyID == "idIntlSwapFuncs", so no need to check it explicitly below.

            Check(lr =>
                lr.TargetStringNoHK == "0" && //Indicates the functions should be unlocalised
                !lr.GetDimensionIntersection(lr.FileName, PropertyFriendlyIdftCount).First().SourceEqualsTarget(),
                "idIntlSwapFuncs resource is set to 0, but idftCOUNT resource is localized (it shouldn't be).");

            Check(lr =>
                lr.TargetStringNoHK == "0" && //Indicates the functions should be unlocalised
                !lr.GetDimensionIntersection(lr.FileName, PropertyFriendlyIdftAverage).First().SourceEqualsTarget(),
                "idIntlSwapFuncs resource is set to 0, but idftAVERAGE resource is localized (it shouldn't be).");

            Check(lr =>
                lr.TargetStringNoHK == "1" && //Indicates the functions should be localised
                lr.GetDimensionIntersection(lr.FileName, PropertyFriendlyIdftCount).First().SourceEqualsTarget(),
                "idIntlSwapFuncs resource is set to 1, but idftCOUNT resource is not localized (it should be).");

            Check(lr =>
                lr.TargetStringNoHK == "1" && //Indicates the functions should be localised
                lr.GetDimensionIntersection(lr.FileName, PropertyFriendlyIdftAverage).First().SourceEqualsTarget(),
                "idIntlSwapFuncs resource is set to 1, but idftAVERAGE resource is not localized (it should be).");
        }
    }

    /// <summary>
    /// Helper methods for simpler notation of the 'Check' expressions.
    /// </summary>
    public static class LRExtensions
    {
        /// <summary>
        /// Returns true if SourceString == TargetString. False otherwise.
        /// </summary>
        public static bool SourceEqualsTarget(this LocResource me)
        {
            return me.SourceStringNoHK.Value == me.TargetStringNoHK.Value;
        }
    }
}