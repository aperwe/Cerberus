﻿using System.Text.RegularExpressions;
using Microsoft.Localization.OSLEBot.Core.KnowledgeBase;

namespace Cerberus.Checks
{
    /// <summary>
    /// Description from the spec:
    /// Where translation value contains any character from the list provided, flag as issue.
    /// Output message: "Hybrid pseudo has been turned off, but some resources are still getting Hybrid pseudo'ed."
    /// </summary>
    public class HybridPseudoCheck : LocResourceRule
    {
        /// <summary>
        /// Default constructor of the check.
        /// </summary>
        /// <param name="owner">Rule manager passed by OSLEBot engine.</param>
        /// <param name="filteringExpression">Filtering expression passed from the configuration by OSLEBot engine. Used to determine if a check should look at the supplied classification objects depending on whether they meet the filter criteria.</param>
        public HybridPseudoCheck(RuleManager owner, string filteringExpression) : base(owner, filteringExpression) { }

        /// <summary>
        /// Message generated by this check into OSLEBot log.
        /// </summary>
        private readonly string message = "Hybrid pseudo has been turned off, but some resources are still getting Hybrid pseudo'ed.";

        /// <summary>
        /// Method called by OSLEBot engine for every resource.
        /// This method contains one or more calls to Check() method that log messages to OSLEBot output when criteria specified in Check() are met.
        /// </summary>
        protected override void Run()
        {
            //Flag a resource if it matches any of the specified hybrid-like patterns.
            Check(lr =>
                  lr.TargetStringNoHK.RegExp(hybridPseudoCharacterDetector1,
                                             hybridPseudoCharacterDetector2,
                                             hybridPseudoCharacterDetector3,
                                             hybridPseudoCharacterDetector4,
                                             hybridPseudoCharacterDetector5,
                                             hybridPseudoCharacterDetector6,
                                             hybridPseudoCharacterDetector7,
                                             hybridPseudoCharacterDetector8,
                                             hybridPseudoCharacterDetector9,
                                             hybridPseudoCharacterDetectorA)
                  , message);

            //Special treatment. Because scanning only target produces a lot of noise, also check if source doesn't match this generic regex.
            Check(lr =>
                  lr.TargetStringNoHK.RegExp(noisyPseudoDetector) &&
                 !lr.SourceStringNoHK.RegExp(noisyPseudoDetector),
                 message);

            //Special treatment of this one.
            //If the translation matches the specified regex, then flag the resource BUT only if the resID is not in the list of exceptions.
            Check(lr =>
                  lr.TargetStringNoHK.RegExp(hybridWithExceptions)
                  && !lr.LSResID.Value.RegExp(exceptions)
                  , message);
        }
        /// <summary>
        /// This is a hybrid pattern specific to 1025 culture.
        /// </summary>
        private readonly Regex hybridPseudoCharacterDetector1 = new Regex(@"^لإ.+أبعَد2؟$", RegexOptions.Compiled | RegexOptions.ExplicitCapture);
        /// <summary>
        /// This is a hybrid pattern specific to 1028 culture.
        /// </summary>
        private readonly Regex hybridPseudoCharacterDetector2 = new Regex(@"^［.+］$", RegexOptions.Compiled | RegexOptions.ExplicitCapture);
        /// <summary>
        /// This is a hybrid pattern specific to 1037 culture.
        /// </summary>
        private readonly Regex hybridPseudoCharacterDetector3 = new Regex(@"^ף.+שלך$", RegexOptions.Compiled | RegexOptions.ExplicitCapture);
        /// <summary>
        /// This is a hybrid pattern specific to 1041 (Japanese) culture.
        /// </summary>
        private readonly Regex hybridPseudoCharacterDetector4 = new Regex(@"^\[:.+థ్క Iiلإَّ$", RegexOptions.Compiled | RegexOptions.ExplicitCapture);
        /// <summary>
        /// This is a hybrid pattern specific to 1054 culture.
        /// </summary>
        private readonly Regex hybridPseudoCharacterDetector5 = new Regex(@"^น้ำ.+ท้ายสุด$", RegexOptions.Compiled | RegexOptions.ExplicitCapture);
        /// <summary>
        /// This is a hybrid pattern specific to 1055 (Turkish) culture.
        /// </summary>
        private readonly Regex hybridPseudoCharacterDetector6 = new Regex(@"^İ.+ıi$", RegexOptions.Compiled | RegexOptions.ExplicitCapture);
        /// <summary>
        /// This is a hybrid pattern specific to 1058 culture. Note the space in the ending sequence.
        /// </summary>
        private readonly Regex hybridPseudoCharacterDetector7 = new Regex(@"^म्दिं९.+ ७त्रृँ$", RegexOptions.Compiled | RegexOptions.ExplicitCapture);
        /// <summary>
        /// This is a hybrid pattern specific to ar-sa-pointloc culture.
        /// </summary>
        private readonly Regex hybridPseudoCharacterDetector8 = new Regex(@"^ل.+؟$", RegexOptions.Compiled | RegexOptions.ExplicitCapture);
        /// <summary>
        /// This is a hybrid pattern specific to he-il-pointloc culture.
        /// </summary>
        private readonly Regex hybridPseudoCharacterDetector9 = new Regex(@"^ף.+ש$", RegexOptions.Compiled | RegexOptions.ExplicitCapture);
        /// <summary>
        /// This is a hybrid pattern specific to hi-in-hybrid culture. Note the space in the ending sequence.
        /// </summary>
        private readonly Regex hybridPseudoCharacterDetectorA = new Regex(@"^म्दिं९.+ ७त्रृँ$", RegexOptions.Compiled | RegexOptions.ExplicitCapture);
        /// <summary>
        /// This is a hybrid pattern specific to many cultures. It produces a lot of noise, so requires a special treatment.
        /// </summary>
        private readonly Regex noisyPseudoDetector = new Regex(@"^\[.+\]$", RegexOptions.Compiled | RegexOptions.ExplicitCapture);

        /// <summary>
        /// This will need to undergo special treatment. It applies to many cultures.
        /// </summary>
        private readonly Regex hybridWithExceptions = new Regex(@"^_.+_$", RegexOptions.Compiled | RegexOptions.ExplicitCapture);
        /// <summary>
        /// This is a list of all resource ids which should be ignored when <see cref="_hybridWithExceptions"/> catches a target string of a resource.
        /// </summary>
        private readonly Regex exceptions = new Regex("(\"idzQryParamPrefix\")|(\"idzQryTblAlias\")|(STR; 15723)|(\"msoidsInsDear\")|(\"msoidsInsPunct\")|(\"msoidsInsVerb\")|(\"IDS_INSERT_MEETING_DETAIL_ENDHEADER\")|(\"WIN_DLG_CTRL_\", 130; 10[689])|(\"idcUnderScore\")|(\"idzQryTblAlias\")|(\"WIN_MENU\"; 65535)|(\"STR\"; (5176|2959))|(\"TablePreviewChar\")|(5; 21)|(\"_foz1j9_28_2\")|(\"_idsUnderlineLeader\")|(\"0x049D\")|(\"_idsHtmlAnchor\")|(\"_idsHtmlComment\")|(\"_idsHtmlMsoAnchor\")|(\"_idsHtmlMsoComment\")|(0; 35)|(\"idsTocLeaderUScore\")", RegexOptions.Compiled | RegexOptions.ExplicitCapture);
    }

    public static class HybridExtensions
    {
        public static bool RegExp(this string me, Regex regex)
        {
            return regex.IsMatch(me);
        }
    }
}