using System;
using System.Text.RegularExpressions;
using System.Linq;
using Microsoft.Localization.OSLEBot.Core.KnowledgeBase;
using Microsoft.Localization.OSLEBot.ClassificationObjects;
using Microsoft.Localization.OSLEBot.Core.Misc;
using System.Globalization;
using System.Collections.Generic;
using Microsoft.Localization;

namespace Cerberus.Checks
{
    /// <summary>
    /// Description from the spec:
    /// Check for invalid combination of approval flags
    /// 
    /// Criteria:
    /// Text loc status = Updated and Approval Status = Approved/Pre-Approved 
    /// OR 
    /// Text Loc Status = not localized and Approval status = Approved/Pre-Approved
    /// OR
    /// Usr src lock = yes and Approval status not equal Not Applicable
    /// </summary>
    public class WrongApprovalStatusCheck : LocResourceRule
    {
        /// <summary>
        /// Default constructor of the check.
        /// </summary>
        /// <param name="owner">Rule manager passed by OSLEBot engine.</param>
        /// <param name="filteringExpression">Filtering expression passed from the configuration by OSLEBot engine. Used to determine if a check should look at the supplied classification objects depending on whether they meet the filter criteria.</param>
        public WrongApprovalStatusCheck(RuleManager owner, string filteringExpression)
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
                lr.LocalizationStatus == LocStatus.Updated && (lr.ApprovalStatus == ApprovalStatus.Approved || lr.ApprovalStatus == ApprovalStatus.PreApproved),
                "Incorrect approval status: string cannot be Updated and Approved/Pre-Approved.");

            Check(lr =>
               lr.LocalizationStatus == LocStatus.NotLocalized && (lr.ApprovalStatus == ApprovalStatus.Approved || lr.ApprovalStatus == ApprovalStatus.PreApproved),
               "Incorrect approval status: string cannot be NotLocalized and Approved/Pre-Approved.");

            #region Currently disabled as it produces huge amount of noise
            //Check(lr =>
            //   lr.UserSrcLocked && (lr.ApprovalStatus != ApprovalStatus.NotApplicable),
            //   "The approval status on locked string has not been set to Not Applicable.");
            #endregion
        }
    }
}