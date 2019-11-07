using System;
using Microsoft.Localization.OSLEBot.Core.KnowledgeBase;
using System.Text.RegularExpressions;
using System.Linq;

namespace Cerberus.Checks
{
    /// <summary>
    /// Korean Post Positions Check
    /// Description:
    /// Korean grammar requires "post position" characters to define the subject of a sentence.
    /// When a placeholder is used in a string the following post position character has to include both possible variations - it is incorrect to have
    /// only one of the pair of post position characters after a placeholder.
    /// 
    /// Criteria:
    /// Each placeholder in the target string must be followed by a pair of post position characters, for example:
    /// “%s은(는) ”. It should not be followed by only one character, for example “%s은 ”
    /// Also, there should be no space between the placeholder and the next character - if there is, we can ignore this as the following character is a separate word.
    /// Also, in the incorrect case (i.e. “%s는 ”) we rely on the space following the character: if it's there, we know we are dealing with a post postition character, if it's not
    /// present we ignore this occurance
    ///
    /// Output message: 
    /// "Incorrect Korean post position sequence after placeholder. Use one of the correct sequences: &lt;...&gt; Incorrect sequences detected: "
    /// </summary>
    public class KoreanPostPositionsCheck : LocResourceRule
    {
        /// <summary>
        /// Default constructor of the check.
        /// </summary>
        /// <param name="owner">Rule manager passed by OSLEBot engine.</param>
        /// <param name="filteringExpression">Filtering expression passed from the configuration by OSLEBot engine. Used to determine if a check should look at the supplied classification objects depending on whether they meet the filter criteria.</param>
        public KoreanPostPositionsCheck(RuleManager owner, string filteringExpression)
            : base(owner, filteringExpression)
        {
            mc = new StringAppendMessageCreator();
            //create the Regular expression to be used by check
            //create regex string to detect all known placeholders
            var placeholderRegex = String.Format(
                    @"({0})",                               //wrap the entire regex in parenthesis
                    String.Join("|", knownPlaceholders.Select(s => String.Concat("(", s, ")")).ToArray())  //wrap each individual  regex in parenthesis, and OR all expressions
                ); 
            //create regex string to detect all post position sequences
            var postPositionSequencesRegex = String.Format(
                    @"({0})",                               //wrap the entire regex in parenthesis
                    String.Join("|", postPositionSequences.Select(s => String.Concat("(", s, ")")).ToArray())  //wrap each individual  regex in parenthesis, and OR all expressions
                ); 
            // create the Regex object by concatenating sub regexes.
            detectIncorrectKoreanPostPositionSequence = new Regex(
                String.Format(
                @"{0}{1}\s",    // placeholder regex, followed by post position regex, followed by a space - this may need to be changed in the future but for now we assume there will always be a space
                placeholderRegex,
                postPositionSequencesRegex
                ),
                RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace | RegexOptions.Singleline | RegexOptions.ExplicitCapture
                );

            // set the default message
            defaultMessage = String.Format(@"Incorrect Korean post position sequence after placeholder. Use one of the correct sequences: {0}. Incorrect sequences detected: ",
                String.Join(";", correctPostPositionSequences));
        }
        /// <summary>
        /// Message creator used to provide context messages in checks.
        /// </summary>
        readonly StringAppendMessageCreator mc;

        private string defaultMessage;

        /// <summary>
        /// Method called by OSLEBot engine for every resource.
        /// This method contains one or more calls to Check() method that log messages to OSLEBot output when criteria specified in Check() are met.
        /// </summary>
        protected override void Run()
        {

            Check(lr =>
               mc.SetContext(lr.TargetString.RegExpMatches(detectIncorrectKoreanPostPositionSequence)).Any(),
               mc.SetInit(defaultMessage));
        }

        /// <summary>
        /// The regular expression used by the check to detect incorrect korean post positions.
        /// The regex string is built in the constructor code by using the knownPlaceholders and postPositionSequences string arrays.
        /// </summary>
        private readonly Regex detectIncorrectKoreanPostPositionSequence;
        /// <summary>
        /// Each strings describes a regular expression used to detect one type of placeholders used in Office.
        /// If a new placeholder needs to be supported by this check, its regex representation should be added to the list and the
        /// main Regular Expression used by the check will be updated automatically.
        /// </summary>
        private readonly string[] knownPlaceholders = new string[] 
        {
            @"{\d+(?>,\d+)?(?>:[A-Za-z]\d*)?}",              //managed code placeholders, like {0}, {0,0C}, etc.
            @"%[a-zA-Z][a-zA-Z0-9]*%(?![a-zA-Z0-9])",        //some Office placeholders, like %s%, %Z%, %Productname%, %ApplicationName% etc.
            @"<\@?[0-9]+[a-zA-Z]+(\#[0-9]+)?>",              //Office placeholders like <1d>, <20S>, <@1d>, <@0S>, <@20A>, <@499Z>, <0w#25> etc.
            @"<[a-zA-Z]>",                                   //Office placeholders like <a> or <Z>
            @"[|][0-9]+",                                    //Office placeholders like |0, |1, |24, |299 etc.
            @"[\^][0-9]+",                                   //Office placeholders like ^0, ^1, ^24, ^299 etc.
            @"\(![\w_-]+\)",                                 //ResT branding tokens
            @"\{0:\w+\}",                                    //Excel branding tokens
            @"[\[][a-zA-Z0-9]+[\]]",                         //Office Setup placeholders like [1], [9], [ProductName], [Time], etc.
            @"%[-+]?[0-9]*\.?[0-9]*(h|l|I|I32|I64)?[cCdiouxXeEfgGnpsS]", //printf variables
        };
        /// <summary>
        /// List of Korean post position sequences.
        /// This is used to build the main Regular Expression in the check's constructor
        /// </summary>
        private readonly string[] postPositionSequences = new string[]
        {
            "는",
            "은",
            "이",
            "가 ",
            "을",
            "를",
            "과",
            "와",
            "로",
            "으로"
        };
        /// <summary>
        /// A list of correct post position characters. Used in the warning message only
        /// </summary>
        private readonly string[] correctPostPositionSequences = new string[] {
        "은(는)",
        "이(가)",
        "을(를)",
        "과(와)",
        "(으)로"
        };
    }
}