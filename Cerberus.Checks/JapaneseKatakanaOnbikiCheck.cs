using System.Linq;
using Microsoft.Localization.OSLEBot.Core.KnowledgeBase;
using System.IO;
using System;
using Microsoft.Localization.OSLEBot.ClassificationObjects;
using System.Text.RegularExpressions;

namespace Cerberus.Checks
{
    /// <summary>
    /// Japan Katakana Onbiki check
    /// Description from the spec:
    //    Proposal of the new logic:
    //Preparation:
    //Check in the attached OnbikiChangeList.txt in SD so that the tool can refer to.
    //	There are 3 columns in the change list file. First column is the English term, and the second column is the Katakana term (without Onbiki). The third column, which exits only for exceptional cases, is the Katakana term with Onbiki.
    //	Katakana cho-on Onbiki almost always occurs to the end of a word. Only exceptions so far are “membership” and “supervisor” (which have the third column).

    //Execution:
    //1.	If there is the third column in the change list, check if the word of the third column exists in translation. If it doesn’t, raise a flag.
    //2.	If there is no third column, then search for the word in translation. If there is one, check if it is followed by Onbiki character ‘ー’. If it isn’t, raise a flag.

    //Note: The tool doesn’t need to check if the English word exist in the source string or not.

    /// Criteria:
    /// [Where source contains one or more of the terms in the list,
    /// and Text Loc Status equals Localised, ensure that translation value
    /// contains the correct translation for the Japanese language.
    /// If not, flag as an issue.]
    /// 
    /// Output message: "One or more of the terms in this resource are not translated correctly."
    /// </summary>
    public class JapaneseKatakanaOnbikiCheck : LocResourceRule
    {
        /// <summary>
        /// This is a data file with tabulator-separated entries of English and Japanese pairs of strings.
        /// This file has been provided by denism.
        /// </summary>
        public string DictionaryLocation = @"%OTOOLS%\bin\intl\Cerberus\checks\JPN_OnbikiChangeList_OSLEBot.txt";

        /// <summary>
        /// Default constructor of the check.
        /// </summary>
        /// <param name="owner">Rule manager passed by OSLEBot engine.</param>
        /// <param name="filteringExpression">Filtering expression passed from the configuration by OSLEBot engine. Used to determine if a check should look at the supplied classification objects depending on whether they meet the filter criteria.</param>
        public JapaneseKatakanaOnbikiCheck(RuleManager owner, string filteringExpression)
            : base(owner, filteringExpression)
        {
            mc = new OnbikiMessageCreator();
        }
        /// <summary>
        /// Message creator used to provide context messages in checks.
        /// </summary>
        readonly OnbikiMessageCreator mc;

        /// <summary>
        /// Method called by OSLEBot engine for every resource.
        /// This method contains one or more calls to Check() method that log messages to OSLEBot output when criteria specified in Check() are met.
        /// </summary>
        protected override void Run()
        {
            mc.ResetContext();
            Check(lr =>
                  lr.LocalizationStatus == Microsoft.Localization.LocStatus.Localized
                  &&
                      // set message context with incorrectly translated terms
                  mc.SetContext(
                      // scan all entries and extract cases that are incorrectly translated
                      _dictionary.Where(entry => HasIncorrectTranslation(lr, entry))
                          .ToArray()
                          )
                      // check if there was at least one incorrect entry
                          .Any(),
                  mc.SetInit("Some terms translated incorrectly, violating Onbiki rules. Should be: ")
                  );
        }
        /// <summary>
        /// Returns true if string contains incorrect translation for japanese.
        /// </summary>
        /// <param name="lr"></param>
        /// <param name="entry"></param>
        /// <returns></returns>
        private bool HasIncorrectTranslation(LocResource lr, JapaneseEntry entry)
        {
            var startingPoint = 0;
            // we are doing this in a loop because a string can contain more than 1 occurence of a given term and we want to check them all
            // using this logic we will skip over any correct translations and report the first incorrect translation
            while (true)
            {
                //check if target string contains the incorrect translation. if it doesn't, skip
                //we are using a regex in order to ignore false positives where a katakana word is part of a larger word
                var match = entry.IncorrectJapaneseRegex.Match(lr.TargetStringNoHK.Value, startingPoint);
                // the incorrect term does not exist at all.
                if (!match.Success)
                {
                    return false;
                }
                var incorrectIndex = match.Index;
                //here we are using a trick based on logic specified by user:
                //most of correct entries are the same as incorrect ones with extra ‘ー’ at the end.
                //so we look for the incorrect translation and then make sure that correct translation exists
                //at the same index.
                //in the rare case where ‘ー’ is injected in the middle of the translation, we always return true if
                //incorrect translation is present
                var correctIndex = lr.TargetStringNoHK.Value.IndexOf(entry.CorrectJapanese,startingPoint, StringComparison.Ordinal);
                // if the correct translation does not exist in the same place, the translation is not correct
                if (incorrectIndex != correctIndex)
                    return true;
                // move the starting point beyond the current substring that we have examined
                startingPoint = incorrectIndex + match.Length;
            }
        }
        int _japaneseLCID = new System.Globalization.CultureInfo("ja-jp").LCID;
        /// <summary>
        /// Prepare the rule for first use.
        /// </summary>
        protected override void Init()
        {
            base.Init();
            #region we should be throwing exceptions if the file is missing to signal that the check is not working correctly.
            //_dictionary = new JapaneseEntry[0]; //Initialize a default empty array. It will be replaced with the contents of data file if it is available.
            //var txtFile = new FileInfo(Environment.ExpandEnvironmentVariables(DictionaryLocation));
            //if (!txtFile.Exists) return; //If the file is missing, the dictionary will be empty. 
	#endregion
            
            string[] lines;
            try
            {
                lines = File.ReadAllLines(Environment.ExpandEnvironmentVariables(DictionaryLocation));
            }
            catch (Exception e)
            {
                throw new Microsoft.Localization.OSLEBot.Exceptions.InitializingRuleException("Failed to load config file for check.", e);
            }
            //from spec: if only 2 columns exist 2nd column is the incorrect translation and the correct translation is obtained by adding
            //‘ー’ to the string.
            //if there are 3 columns, 2nd column has the incorrect translation and 3rd column has the correct translation - these are exceptions.
            //1st column - English string, is always to be ignored.
            var transform = from line in lines
                            let split = line.Split(new[] { "\t" }, StringSplitOptions.RemoveEmptyEntries)
                            select new JapaneseEntry(split[1], split.Length == 3 ? split[2] : split[1] + "ー");
            _dictionary = transform.ToArray();
        }

        /// <summary>
        /// Cleanup memory after use.
        /// </summary>
        protected override void Cleanup()
        {
            base.Cleanup();
            _dictionary = null;
        }

        private JapaneseEntry[] _dictionary;
        /// <summary>
        /// Translation entry containing the English string and it's expected translation in Japanese Katakana.
        /// </summary>
        private struct JapaneseEntry
        {
            public JapaneseEntry(string incorrectJapanese, string correctJapanese)
            {
                IncorrectJapanese = incorrectJapanese;
                CorrectJapanese = correctJapanese;
                // we consider a sequence to be an incorrect japanese term only if there is no 
                // Katakana character preceding nor following the term
                // we exclude the bullet character (30FB) in order not to miss strings that have a starting bullet
                string katakanaChars = @"[\p{IsKatakana}-[\u30FB]]";
                IncorrectJapaneseRegex = new Regex(
                    String.Format(
                        @"(?<!{0}){1}(?!{0})",
                        katakanaChars, incorrectJapanese
                    )
                    , RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.ExplicitCapture);
            }
            public readonly string IncorrectJapanese;
            public readonly string CorrectJapanese;
            /// <summary>
            /// A regular expression used to search for IncorrectJapanase term.
            /// We have to use a regex in order to ignore incorrect japanase terms that exist as part
            /// of a composite Katakana word/phrase and would otherwise cause noise.
            /// </summary>
            public readonly Regex IncorrectJapaneseRegex;
        }
        /// <summary>
        /// Used to format incorrect Onbiki entries in error messages
        /// </summary>
        private class OnbikiMessageCreator : MessageCreator
        {
            private static JapaneseEntry[] emptyContext = new JapaneseEntry[0];
            JapaneseEntry[] context = emptyContext;
            public JapaneseEntry[] SetContext(JapaneseEntry[] invalidOnbiki)
            {
                context = invalidOnbiki;
                return context;
            }
            public void ResetContext()
            {
                context = emptyContext;
            }
            public override string GetFullMessage()
            {
                return
                    String.Format("{0} {1}",
                        base.GetBaseMessage(),
                        String.Join("; ",
                            context.Select(entry => String.Format("{0} => {1}", entry.IncorrectJapanese, entry.CorrectJapanese)).ToArray()
                            )
                        );

            }
        }
    }


}