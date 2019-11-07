using System;
using System.Text.RegularExpressions;
using System.Linq;
using Microsoft.Localization.OSLEBot.Core.KnowledgeBase;
using Microsoft.Localization.OSLEBot.ClassificationObjects;
using Microsoft.Localization.OSLEBot.Core.Misc;

namespace Cerberus.Checks
{
    /// <summary>
    /// Description from the spec:
    /// For XML files parsed using LocalBindingParser make sure that there are no unescaped characters that can cause XML validation errors at runtime.
    /// 
    /// Criteria:
    /// 1. File is parsed using LocalBindingParser.
    /// 2. File is of certain type (by extension): xml, resx, xsl, aspx, wxl, accdt, gtx, etc. ...
    /// 3. String contains one of the invalid characters: &, &lt; (chevron), ". Note: & is only invalid if it occurs on its own, not as part of an entity reference.
    /// 4. Additional check: check that string does not contain undefined entities. Only the following entity references are allowed: &amp;, &quot; &lt;, &gt;, &apos;
    /// 5. Additional check: look for a possible double-escaping, like &amp;quot;
    /// Output message: An appropriate message for each sub check
    /// </summary>
    public class EscapeCharsForXMLLocalBindingCheck : LocResourceRule
    {
        /// <summary>
        /// Default constructor of the check.
        /// </summary>
        /// <param name="owner">Rule manager passed by OSLEBot engine.</param>
        /// <param name="filteringExpression">Filtering expression passed from the configuration by OSLEBot engine. Used to determine if a check should look at the supplied classification objects depending on whether they meet the filter criteria.</param>
        public EscapeCharsForXMLLocalBindingCheck(RuleManager owner, string filteringExpression)
            : base(owner, filteringExpression)
        {
            mc = new StringAppendMessageCreator();
        }
        /// <summary>
        /// Message creator used to provide context messages in checks.
        /// </summary>
        readonly StringAppendMessageCreator mc;

        /// <summary>
        /// Method called by OSLEBot engine for every resource.
        /// This method contains one or more calls to Check() method that log messages to OSLEBot output when criteria specified in Check() are met.
        /// </summary>
        protected override void Run()
        {
            // check initial condition for file type and parser and skip the check altogether if they are not met.
            var currentResource = (LocResource)CurrentCO;
            if (!(currentResource.Document.Value.ParserId.Equals(localBindingParserID, StringComparison.OrdinalIgnoreCase)
                &&
                currentResource.FileName.Value.EndsWith(StringComparison.OrdinalIgnoreCase, fileExtensions)))
            {
                return;
            }

            // check for unescaped characters, excluding double-quote to reduce noise
            Check(lr => mc.SetContext(lr.TargetString.RegExpMatches(unescapedCharacters)).Any(),
                mc.SetInit("Translation contains invalid characters that need to be escaped using character entity references: "));

            // check for double-quote in target, but only if source does not contain a double-quote:
            // exclude .resx files
            Check(lr => !lr.FileName.Value.EndsWith(StringComparison.OrdinalIgnoreCase, ".resx") &&
                        lr.TargetString.Contains("\"") && 
                        !lr.SourceString.Contains("\""),
                "Translation contains a double-quote character, but source doesn't. Use entity reference &quot; instead of this character.");

            // check for invalid XML references
            Check(lr => mc.SetContext(lr.TargetString.RegExpMatches(invalidEntityReferences)).Any(),
                mc.SetInit("Translation contains invalid XML references: "));

            // check for possible double-escaping
            Check(lr => mc.SetContext(lr.TargetString.RegExpMatches(possibleDoubleEscaping)).Any(),
                mc.SetInit("Translation contains possible double-escaping of ampersand character: "));

        }
        /// <summary>
        /// Local binding parser ID
        /// </summary>
        private const string localBindingParserID = "227";
        /// <summary>
        /// List of file extensions we want to check against.
        /// </summary>
        private readonly string[] fileExtensions = new string[] {
            ".xml", ".resx", ".xsl", ".aspx", ".wxl", ".accdt", ".gtx", ".xoml", ".xoml.rules", ".xoml.wfconfig.xml"

        };
        /// <summary>
        /// Unescaped characters that we want to detect. Double-quote is excluded and checked using separate logic to reduce noise
        /// </summary>
        private readonly Regex unescapedCharacters = new Regex(
            @"
            <                               #a chevron
            |                               #OR
            &(?!(                           #a standalone ampersand - not followed by:
                (quot|amp|apos|lt|gt)       #a valid entity reference
                |                           #OR
                ([#](\d{4}|x[0-9a-fA-F]{4}))#a valid numeric character reference, using &nnnn; or &xhhhh;
                );
            )
            ",
            RegexOptions.Compiled | RegexOptions.ExplicitCapture | RegexOptions.IgnorePatternWhitespace | RegexOptions.Singleline
            );
        /// <summary>
        /// Invalid entity references that we want to detect
        /// </summary>
        private readonly Regex invalidEntityReferences = new Regex(
           @"
            &                                   #beginning of entity or character reference
                [^;]+                           #reference
                (?<!                            #look behind to see if the reference is NOT one of the valid formats
                    (quot|amp|apos|lt|gt)       #a valid entity reference
                    |                           #OR
                    ([#](\d{4}|x[0-9a-fA-F]{4}))#a valid numeric character reference, using &nnnn; or &xhhhh;
                )
            ;                                   #ending of entity or character reference
            ",
           RegexOptions.Compiled | RegexOptions.ExplicitCapture | RegexOptions.IgnorePatternWhitespace | RegexOptions.Singleline
           );
        /// <summary>
        /// Possible double escaping
        /// </summary>
        private readonly Regex possibleDoubleEscaping = new Regex(
           @"
            &amp;\w+;
            ",
           RegexOptions.Compiled | RegexOptions.ExplicitCapture | RegexOptions.IgnorePatternWhitespace | RegexOptions.Singleline
           );
        
    }
}