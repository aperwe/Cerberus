using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Localization.OSLEBot.ClassificationObjects;
using Microsoft.Localization.OSLEBot.Core.KnowledgeBase;
using System.Text.RegularExpressions;

namespace Cerberus.Checks
{
    /// <summary>
    /// <list type="ol">
    /// <listheader>This check detects most common corruptions of ResT branding tokens. Branding token have the following form: (!xxx_xxx_...) and are replaced with 
    /// actual product names at ResT compile time. If corrupted, they can result in unresolved tokens appearing in UI or incorrect product names being used in translation.
    /// This check implements logic to detect several different potential token name corruptions.</listheader>
    /// <item>Broken token syntax</item>
    /// Description:
    /// Translation contains a token name with corrupt token syntax.
    /// Criteria:
    /// For each token name (i.e. Word_Full_2010) existing in source string, lookup the name in translation, and if it exists, make sure it is surrounded
    /// with the correct token markups.
    /// Output message: 
    /// "Translation contains the following token names with incorrect markups: &lt;list of token names detected&gt;"
    /// 
    /// <item>Misspelled token names</item>
    /// Description:
    /// Translation contains what may be a misspelled token name. 
    /// Criteria:
    /// For each token name existing in source string check if translation contains a similar word that is slightly different. Sensitivity of the check can be 
    /// adjusted as percentage of string length - we only want to detect minor differences to avoid false positives.
    /// Output message:  
    /// "Translation contains posssibly misspelled token names: &lt;list of token names&gt;"
    /// 
    /// <item>Tokens added</item>
    /// Description:
    /// Translation contais extra tokens that do not exist in Source string.
    /// Criteria:
    /// Extract tokens from translation and compare against the list of original tokens. Report extra tokens.
    /// Output message:
    /// "Translation contains the following extra tokens that do not exist in source string: &lt;list of token names&gt;
    /// 
    /// <item>Tokens removed</item>
    /// Description:
    /// Tokens have been removed from translation. Note: this may not be a problem for some languages that tend to de-personify error messages by removing product names.
    /// This check may need to be disabled for a list of languages or perhaps extracted into a separate Class to allow users to control it centrally.
    /// Criteria:
    /// Extract tokens from translation and compare against the list of original tokens. Report missing tokens.
    /// Output message:
    /// "Translation is missing the following tokens that exist in source string: &lt;list of token names&gt;
    /// </list>
    /// </summary>
    public class BrandingTokenCheck : LocResourceRule
    {
        /// <summary>
        /// Message creator used to provide context messages in checks.
        /// </summary>
        readonly StringAppendMessageCreator mc;

        /// <summary>
        /// Default constructor of the check.
        /// </summary>
        /// <param name="owner">Rule manager passed by OSLEBot engine.</param>
        /// <param name="filteringExpression">Filtering expression passed from the configuration by OSLEBot engine. Used to determine if a check should look at the supplied classification objects depending on whether they meet the filter criteria.</param>
        public BrandingTokenCheck(RuleManager owner, string filteringExpression)
            : base(owner, filteringExpression)
        {
            tokenChecker = new BrandingTokenLogic(0.2);
            mc = new StringAppendMessageCreator();
        }

        /// <summary>
        /// Method called by OSLEBot engine for every resource.
        /// This method contains one or more calls to Check() method that log messages to OSLEBot output when criteria specified in Check() are met.
        /// </summary>
        protected override void Run()
        {
            // current resource object, for easy access.
            var currentResource = (LocResource)CurrentCO;
            // all token names present in the Source String
            var srcTokenNames = tokenChecker.ExtractTokenNames(currentResource.SourceString);
            // all token names present in the Target String
            var tgtTokenNames = tokenChecker.ExtractTokenNames(currentResource.TargetString);

            ///<check>
            ///Broken token syntax
            ///</check>
            Check(lr => mc.SetContext(tokenChecker.ReportSyntaxErrorsForTokens(srcTokenNames, currentResource.TargetString).Select(issue => issue.Message)).Any(),
                mc.SetInit("Translation contains the following token names with incorrect markups: "));
            /// possible misspelled token names in target. we will be reusing this in other checks later to filter out some results.
            /// helper methods returns a dictionary that maps each source token name to a list of possible misspellings of the token - we are 
            /// using this information in Tokens Added and Tokens Removed checks to filer out misspelled tokens from the results.
            var misspelledTokenNames = tokenChecker.ExtractMisspelledTokens(srcTokenNames, currentResource.TargetString);
            ///<check>
            ///Misspelled token names
            ///</check>
            // flatten the information about what tokens were potentially misspelled into.
            mc.SetContext(misspelledTokenNames.Select(entry => String.Format("{0} => {1}", entry.Key, String.Join(", ", entry.Value.ToArray()))));
            Check(lr => misspelledTokenNames.Any(), mc.SetInit("Translation contains posssibly misspelled token names: "));

            ///<check>
            ///Tokens added.
            ///</check>
            Check(lr => mc.SetContext(tgtTokenNames.Except(srcTokenNames).Except(misspelledTokenNames.Values.SelectMany(list => list))).Any(),
                mc.SetInit("Translation contains the following extra tokens that do not exist in source string: "));

            ///<check>
            ///Tokens removed.
            ///</check>
            Check(lr => mc.SetContext(srcTokenNames.Except(tgtTokenNames).Except(misspelledTokenNames.Keys)).Any(),
                mc.SetInit("Translation is missing the following tokens that exist in source string: "));
        }


        BrandingTokenLogic tokenChecker;
    }

    /// <summary>
    /// Logic that finds branding tokens.
    /// </summary>
    internal class BrandingTokenLogic
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="tokenComparisonSensitivity">Percentage value that will be used when comparing token names for misspellings. The higher the value the more sensitive the check and more potential false positives will be reported.</param>
        public BrandingTokenLogic(double tokenComparisonSensitivity)
        {
            maxDiffPercentage = tokenComparisonSensitivity;
        }
        public IList<TokenIssue> ReportProblems(string source, string target, out ICollection<string> srcTokens, out ICollection<string> tgtTokens)
        {
            var allIssueReports = new List<IEnumerable<TokenIssue>>();

            srcTokens = ExtractTokenNames(source);

            allIssueReports.Add(ReportSyntaxErrorsForTokens(srcTokens, target));
            allIssueReports.Add(ReportMisspelledTokens(srcTokens, target));

            tgtTokens = ExtractTokenNames(target);
            var tokensAdded = tgtTokens.Except(srcTokens);
            allIssueReports.Add(tokensAdded.Select(t => new TokenIssue { Type = IssueType.TokenAdded, Message = String.Format("Token \"{0}\" has been added to the target string.", t) }));
            allIssueReports.Add(srcTokens.Except(tgtTokens).Select(t => new TokenIssue { Type = IssueType.TokenRemoved, Message = String.Format("Token \"{0}\" has been removed from the target string.", t) }));

            return allIssueReports.Aggregate((x, y) => x.Concat(y)).ToList();
        }
        public IList<TokenIssue> ReportSyntaxErrorsForTokens(IEnumerable<string> srcTokens, string target)
        {
            var ret = new List<TokenIssue>();
            // extract all token names from target string and map them to their starting position in string
            // pay attention not to confuse tokens that can be a substring of a longer token, for example:
            // idspnOfficeConsumer and idspnOfficeConsumer_Long
            // this is why we are mapping out all tokens going from longest to shortest and checking if token has already
            // been identified at a given position

            // mapping of location to token
            var tokenMapping = new Dictionary<int, string>();
            // go through all tokens, longest to shortest
            foreach (var token in srcTokens.OrderByDescending(t => t.Length))
            {
                int tokenStart = 0;
                while (tokenStart < target.Length - 1 && (tokenStart = target.IndexOf(token, tokenStart, StringComparison.Ordinal)) > -1)
                {
                    // only add token if no other longer token exists at this position.
                    string longerToken = null;
                    if (!tokenMapping.TryGetValue(tokenStart, out longerToken))
                    {
                        tokenMapping.Add(tokenStart, token);
                        tokenStart += token.Length;
                    }
                    else
                    {
                        // make sure we jump over the entire longer token
                        tokenStart += longerToken.Length;
                    }
                }
            }
            // go through all identified tokens in target and validate the markups
            foreach (var tokenLocation in tokenMapping)
            {
                int tokenStart = tokenLocation.Key;
                string token = tokenLocation.Value;

                var tokenEnd = tokenStart + token.Length - 1;
                // check that token name is preceeded by (!
                if (!target.Substring(Math.Max(0, tokenStart - 2), 2).Equals("(!", StringComparison.Ordinal))
                {
                    ret.Add(new TokenIssue
                    {
                        Message = String.Format("Branding token opening (! is corrupt at postition {0} for token \"{1}\"", (tokenStart + 1).ToString(), token),
                        Type = IssueType.TokenSyntaxCorrupt,
                        TokenName = token
                    });
                }
                //check that tag is closed
                if (tokenEnd == target.Length - 1 || !target[tokenEnd + 1].Equals(')'))
                {
                    ret.Add(new TokenIssue
                    {
                        Message = String.Format("Branding token closing ) is corrupt at postition {0} for token \"{1}\"", (tokenEnd + 1).ToString(), token),
                        Type = IssueType.TokenSyntaxCorrupt,
                        TokenName = token
                    });
                }
            }
            return ret;
        }
        public IList<TokenIssue> ReportMisspelledTokens(IEnumerable<string> srcTokens, string target)
        {
            var misspelled = ExtractMisspelledTokens(srcTokens, target);
            return misspelled.Select(kvp => new TokenIssue
            {
                Type = IssueType.TokenNameMisspelled,
                Message = String.Format("Source token name \"{0}\" possibly misspelled into: \"{1}\"", kvp.Key, String.Join("; ", kvp.Value.ToArray()))
            }
                ).ToList();
        }
        public ICollection<string> ExtractTokenNames(string source)
        {
            var matches = extractTokenNames.Matches(source);
            var tokens = new HashSet<string>();
            foreach (Match match in matches)
            {
                tokens.Add(match.Groups["tokenName"].Value);
            }
            return tokens;
        }
        readonly Regex extractTokenNames = new Regex(@"\(!(?<tokenName>\w+)\)", RegexOptions.Compiled | RegexOptions.ExplicitCapture);
        /// <summary>
        /// A percentage value used when comparing token names for misspellings.
        /// </summary>
        double maxDiffPercentage;

        public IDictionary<string, IList<string>> ExtractMisspelledTokens(IEnumerable<string> srcTokens, string target)
        {
            var ret = new Dictionary<string, IList<string>>(StringComparer.Ordinal);
            /// split the target string into words
            var tokenCandidates = splitStringIntoTokenCandidates.Split(target);
            foreach (var tokenName in srcTokens)
            {

                var maxDistance = Math.Ceiling(tokenName.Length * maxDiffPercentage);

                var tokensToCompare =
                    from t in tokenCandidates
                    let lengthDiff = Math.Abs(t.Length - tokenName.Length)
                    where lengthDiff <= Math.Ceiling(maxDiffPercentage * tokenName.Length)
                    select t;

                foreach (var candidate in tokensToCompare)
                {
                    var distance = DamerauLevenshteinDistance(tokenName, candidate);
                    if (distance > 0 && distance <= Math.Ceiling(tokenName.Length * maxDiffPercentage))
                    {

                        // make sure we are not reporting other valid tokens as corrupt versions of this token
                        if (!srcTokens.Contains(candidate))
                        {
                            IList<string> misspelled;
                            if (ret.TryGetValue(tokenName, out misspelled))
                            {
                                misspelled.Add(candidate);
                            }
                            else
                            {
                                misspelled = new List<string> { candidate };
                                ret.Add(tokenName, misspelled);
                            }
                        }
                    }
                }
            }
            return ret;
        }
        readonly Regex splitStringIntoTokenCandidates = new Regex(@"\W+", RegexOptions.ExplicitCapture | RegexOptions.Compiled);

        private static Int32 DamerauLevenshteinDistance(string a, string b)
        {
            Int32 cost;
            Int32[,] d = new int[a.Length + 1, b.Length + 1];
            Int32 min1;
            Int32 min2;
            Int32 min3;

            for (Int32 i = 0; i <= d.GetUpperBound(0); i += 1)
            {
                d[i, 0] = i;
            }

            for (Int32 i = 0; i <= d.GetUpperBound(1); i += 1)
            {
                d[0, i] = i;
            }

            for (Int32 i = 1; i <= d.GetUpperBound(0); i += 1)
            {
                for (Int32 j = 1; j <= d.GetUpperBound(1); j += 1)
                {
                    cost = Convert.ToInt32(!(a[i - 1] == b[j - 1]));

                    min1 = d[i - 1, j] + 1;
                    min2 = d[i, j - 1] + 1;
                    min3 = d[i - 1, j - 1] + cost;
                    d[i, j] = Math.Min(Math.Min(min1, min2), min3);

                    if (i > 1 && j > 1 && a[i - 1] == b[j - 1 - 1] && a[i - 1 - 1] == b[j - 1])
                    {
                        d[i, j] = Math.Min(
                                    d[i, j],
                                    d[i - 2, j - 2] + cost
                                 );
                    }
                }
            }

            return d[d.GetUpperBound(0), d.GetUpperBound(1)];
        }
        public struct TokenIssue
        {
            public IssueType Type;
            public string Message;
            public string TokenName;
        }
        public enum IssueType
        {
            TokenSyntaxCorrupt,
            TokenNameMisspelled,
            TokenRemoved,
            TokenAdded
        }
    }
}