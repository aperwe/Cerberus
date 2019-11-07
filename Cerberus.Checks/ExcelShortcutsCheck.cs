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
    /// For strings living in xlintl32.rest in string table sttkeys
    /// any resource which has one of the options in the list provided
    /// should contain one of these options in the translation as well.
    /// For example if the source is F1 then the translation should be F1-9.
    /// List of values is:
    /// Cancel, Backspace, Tab, Clear, Return, Escape, Space, PageUp, PageDown,
    /// End, Home, Left, Up, Right, Down, Print, Execute, Insert, Delete, Help,
    /// Num0, Num1, Num2, Num3, Num4, Num5, Num6, Num7, Num8, Num9,
    /// Multiply, Add, Separator, Subtract, Decimal, Divide,
    /// F1, F2, F3, F4, F5, F6, F7, F8, F9, F10, F11, F12, F13, F14, F15, F16,
    /// Quote
    /// 
    /// Requested by: sarahs
    /// Project: Excel
    /// 
    /// Criteria:
    /// TBD
    /// 
    /// Check message: "The translation for this shortcut is incorrect."
    /// </summary>
    public class ExcelShortcutsCheck : LocResourceRule
    {
        /// <summary>
        /// Name of the file that this check looks at.
        /// </summary>
        private const string XlFileName = "xlintl32.rest";

        /// <summary>
        /// Name of the string table to look in inside the specified Excel file
        /// </summary>
        private const string StringTable = "sttkeys";

        /// <summary>
        /// Default constructor of the check.
        /// </summary>
        /// <param name="owner">Rule manager passed by OSLEBot engine.</param>
        /// <param name="filteringExpression">Filtering expression passed from the configuration by OSLEBot engine. Used to determine if a check should look at the supplied classification objects depending on whether they meet the filter criteria.</param>
        public ExcelShortcutsCheck(RuleManager owner, string filteringExpression)
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
            Check(lr =>
                lr.LSResID.Value.Contains(StringComparison.InvariantCultureIgnoreCase, StringTable) &&
                ValidStrings.Contains(lr.SourceStringNoHK) &&
                !ValidStrings.Contains(lr.TargetStringNoHK),
                "The translation for this shortcut is incorrect.");

        }
        /// <summary>
        /// Complete set of valid shortcut strings that ara allowed in sttkeys string table in xlitnl32.rest.lcl.
        /// <para/>Provided by: sarahs.
        /// </summary>
        HashSet<string> ValidStrings = new HashSet<string>
        {
            "Cancel", "Backspace", "Tab", "Clear", "Return", "Escape", "Space", "PageUp", "PageDown",
            "End", "Home", "Left", "Up", "Right", "Down", "Print", "Execute", "Insert", "Delete", "Help",
            "Num0", "Num1", "Num2", "Num3", "Num4", "Num5", "Num6", "Num7", "Num8", "Num9",
            "Multiply", "Add", "Separator", "Subtract", "Decimal", "Divide",
            "F1", "F2", "F3", "F4", "F5", "F6", "F7", "F8", "F9", "F10", "F11", "F12", "F13", "F14", "F15", "F16",
            "Quote"
        };
    }
}