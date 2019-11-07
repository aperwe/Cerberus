using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Localization.OSLEBot.ClassificationObjects;
using Microsoft.Localization.OSLEBot.Core.KnowledgeBase;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Globalization;
using System.IO;

namespace Cerberus.Checks
{
    /// <summary>
    /// Description from the spec:
    /// OSLEBot reads in a list of English branding terms from a config file,
    /// which for each language contains the correct translations for these.
    /// OSLEBot searches for each of the English terms in the source string and,
    /// if found, matches the correct translation with the translation for
    /// that language from the config file.
    /// If easier, OSLEBot can read strings contained in sttProductNames_xxxx string tables
    /// in each file and use this as the source and translations for the terms.
    /// 
    /// Criteria:
    /// [Where source contains one or more of the terms in the list,
    /// ensure that translation value contains the correct translation for that language.
    /// If not flag as an issue.]
    /// Output message: "The branding term has been incorrectly localized in this resource."
    /// </summary>
    public class BrandingTranslationCheck : LocResourceRule
    {
        /// <summary>
        /// Default constructor of the check.
        /// </summary>
        /// <param name="owner">Rule manager passed by OSLEBot engine.</param>
        /// <param name="filteringExpression">Filtering expression passed from the configuration by OSLEBot engine. Used to determine if a check should look at the supplied classification objects depending on whether they meet the filter criteria.</param>
        public BrandingTranslationCheck(RuleManager owner, string filteringExpression)
            : base(owner, filteringExpression)
        {
            mc = new IvalidBrandingMessageCreator();
            //TODO: have to figure out how to make the config file be accessible relatively to the location of the check file
            //cannot rely on current assembly since checks can be compiled at runtime
            pathToCheckConfig =
                Path.Combine(
                @"%OTOOLS%\bin\intl",
                "BrandingNamesMatrix.xml"
                );
        }

        /// <summary>
        /// Message creator used to provide context messages in checks.
        /// </summary>
        readonly IvalidBrandingMessageCreator mc;

        /// <summary>
        /// Path to the XML file that contains configuration for this check
        /// </summary>
        readonly string pathToCheckConfig;

        /// <summary>
        /// Method called by OSLEBot engine for every resource.
        /// This method contains one or more calls to Check() method that log messages to OSLEBot output when criteria specified in Check() are met.
        /// </summary>
        protected override void Run()
        {
            //TODO: check not final
            Check(lr =>
                mc.SetContext(GetInvalidBranding(lr)).Any(),
                mc.SetInit("Invalid Branding Name translations detected. Should be: "));

        }

        /// <summary>
        /// Searches the target string for each of the known branding names defined in brandingNameIndex.
        /// If found, looks for the approved translation for the target culture.
        /// </summary>
        /// <param name="lr"></param>
        /// <returns>A collection of English Branding Names for which no correct translation was found. Correct Translation is also included for reporting purposes.</returns>
        private InvalidBranding[] GetInvalidBranding(LocResource lr)
        {
            var targetCulture = lr.TargetCulture.Value.BuiltinCultureInfo;
            return (
                from indexEntry in brandingNameIndex
                where lr.SourceStringNoHK.RegExp(indexEntry.EnglishBrandingEntry.Regex)
                let locEntry = indexEntry.GetLocalizedBrandingEntry(targetCulture)
                where !lr.TargetStringNoHK.RegExp(locEntry.Regex)
                select new InvalidBranding
                {
                    BrandingName = indexEntry.EnglishBrandingEntry.BrandingName,
                    CorrectTranslation = locEntry.BrandingName
                }
                ).ToArray();
        }

        private struct InvalidBranding
        {
            public string BrandingName;
            public string CorrectTranslation;
        }

        /// <summary>
        /// Prepare the rule for first use.
        /// </summary>
        protected override void Init()
        {
            base.Init();
            //Loading check config from an XML file
            //TODO: Implement XML Schema validation when loading

            XDocument xDoc;
            try
            {
                xDoc = XDocument.Load(Environment.ExpandEnvironmentVariables(pathToCheckConfig));
            }
            catch (Exception e)
            {
                throw new Microsoft.Localization.OSLEBot.Exceptions.InitializingRuleException("Failed to load config file for check.", e);
            }
            var defaultNS = (xDoc.Root.GetDefaultNamespace()).NamespaceName;
            var brandingNames = xDoc.Root.Descendants(XName.Get("BrandingName", defaultNS)).ToArray();
            brandingNameIndex = new IndexEntry[brandingNames.Length];

            for (int i = 0; i < brandingNames.Length; i++)
            {
                var bNameNode = brandingNames[i];
                var englishName = bNameNode.Attribute(XName.Get("Value")).Value;
                var englishRegexAttribute = bNameNode.Attribute(XName.Get("RegularExpression"));
                string englishNameRegexPattern = null;
                if (englishRegexAttribute == null || String.IsNullOrEmpty(englishRegexAttribute.Value))
                {
                    // create regex based on the englishName provided by user
                    englishNameRegexPattern = GetRegexPattern(englishName);
                }
                else
                {
                    //user provided a regex pattern, so use it as is
                    englishNameRegexPattern = englishRegexAttribute.Value;
                }
                var enBrandingEntry = new BrandingEntry
                {
                    BrandingName = englishName,
                    Regex = new Regex(englishNameRegexPattern, RegexOptions.Compiled | RegexOptions.ExplicitCapture | RegexOptions.Singleline)
                };
                // do the same for all language translations
                Dictionary<CultureInfo, BrandingEntry> translationMapping = null;
                var localizedBrandingNames = bNameNode.Descendants(XName.Get("LocalizedBrandingName", defaultNS)).ToArray();
                if (localizedBrandingNames.Length > 0)
                {
                    translationMapping = new Dictionary<CultureInfo, BrandingEntry>(localizedBrandingNames.Length);
                    foreach (var lbNameNode in localizedBrandingNames)
                    {
                        var locName = lbNameNode.Attribute(XName.Get("Translation")).Value;
                        var locRegexAttribute = lbNameNode.Attribute(XName.Get("RegularExpression"));
                        string locNameRegexPattern = null;
                        if (locRegexAttribute == null || String.IsNullOrEmpty(locRegexAttribute.Value))
                        {
                            // create regex based on the localized name provided by user
                            locNameRegexPattern = GetRegexPattern(locName);
                        }
                        else
                        {
                            locNameRegexPattern = locRegexAttribute.Value;
                        }
                        var locBrandingEntry = new BrandingEntry
                        {
                            BrandingName = locName,
                            Regex = new Regex(locNameRegexPattern, RegexOptions.Compiled | RegexOptions.ExplicitCapture | RegexOptions.Singleline)
                        };
                        var targetCulture = new CultureInfo(lbNameNode.Attribute(XName.Get("Culture")).Value);
                        translationMapping.Add(targetCulture, locBrandingEntry);
                    }
                }
                brandingNameIndex[i] = new IndexEntry(enBrandingEntry, translationMapping);
            }
        }
        /// <summary>
        /// Convert the input branding name into a regular expression pattern.
        /// Mainly replaces whitespace characters with more generic \s+ pattern to avoid noise in output due to differences in spacing
        /// </summary>
        /// <param name="brandingName"></param>
        /// <returns></returns>
        private static string GetRegexPattern(string brandingName)
        {
            if (String.IsNullOrEmpty(brandingName))
            {
                return brandingName;
            }
            var regexPattern = brandingName
                .Replace(" ", @"\s+")
                .Replace("\t", @"\s+");
            return regexPattern;
        }

        /// <summary>
        /// Cleanup memory after use.
        /// </summary>
        protected override void Cleanup()
        {
            base.Cleanup();
            brandingNameIndex = null;
        }
        /// <summary>
        /// Contains all Branding Name entries defined in the config file with  translations specified by user.
        /// </summary>
        private IndexEntry[] brandingNameIndex;
        /// <summary>
        /// Index entry for one Branding Name. Contains the Engilsh branding entry and a mapping of target cultures to approved translations.
        /// </summary>
        private struct IndexEntry
        {
            public IndexEntry(BrandingEntry englishBrandingEntry, Dictionary<CultureInfo, BrandingEntry> translationMapping)
            {
                EnglishBrandingEntry = englishBrandingEntry;
                this.translationMapping = translationMapping;
            }
            /// <summary>
            /// The English branding name entry.
            /// </summary>
            public readonly BrandingEntry EnglishBrandingEntry;
            /// <summary>
            /// Mapping of all specified translations for the branding entry. Contains only entries for cultures that were
            /// specified by user in the config file.
            /// </summary>
            private readonly Dictionary<CultureInfo, BrandingEntry> translationMapping;
            /// <summary>
            /// Obtain the branding entry for the target culture.
            /// </summary>
            /// <param name="ci"></param>
            /// <returns>Returns the localized branding entry, as specified by user in the config file. If no entry is specified for the 
            /// target culture, returns the English branding name. This corresponds to the logic of the config file where no localized entry means
            /// that the target branding name is the same as English.</returns>
            public BrandingEntry GetLocalizedBrandingEntry(CultureInfo ci)
            {
                BrandingEntry ret = EnglishBrandingEntry;
                if (!Object.ReferenceEquals(translationMapping, null))
                    if (!translationMapping.TryGetValue(ci, out ret))
                    {
                        ret = EnglishBrandingEntry;
                    }
                return ret;
            }
        }
        /// <summary>
        /// Branding entry containing the Branding Name and the corresponding regular expression to be used for search
        /// </summary>
        private struct BrandingEntry
        {
            public string BrandingName { get; set; }
            public Regex Regex { get; set; }
        }


        private class IvalidBrandingMessageCreator : MessageCreator
        {
            InvalidBranding[] context;
            public InvalidBranding[] SetContext(InvalidBranding[] invalidBranding)
            {
                context = invalidBranding;
                return context;
            }
            public override string GetFullMessage()
            {
                return
                    String.Format("{0} {1}",
                        base.GetBaseMessage(),
                        String.Join("; ",
                            context.Select(ib => String.Format("{0} => {1}", ib.BrandingName, ib.CorrectTranslation)).ToArray()
                            )
                        );

            }
        }
    }


}