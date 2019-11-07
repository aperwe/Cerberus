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
    /// Ensure that the translation that exists for each language for resource id:
    /// "ResT", 0; "idsSheet" matches values we have in Office 12 files.
    /// We will provide the correct translation for this resource for each language.
    /// Please use this reference: 
    /// http://office/14/teams/IPE/OSIPE/OIPM/Project%20Management/OSLEBOT%20downstream/idsSheetO12.html
    /// Requested by: sarahs
    /// Project: Excel
    /// 
    /// Criteria:
    /// TBD
    /// 
    /// Check message: "The translation for this shortcut is incorrect."
    /// </summary>
    public class ExcelSheetnameCheck : LocResourceRule
    {
        /// <summary>
        /// Name of the file that this check looks at.
        /// </summary>
        private const string XlFileName = "xlintl32.rest";

        private readonly CustomMessage CheckMessage = new CustomMessage();

        /// <summary>
        /// Default constructor of the check.
        /// </summary>
        /// <param name="owner">Rule manager passed by OSLEBot engine.</param>
        /// <param name="filteringExpression">Filtering expression passed from the configuration by OSLEBot engine. Used to determine if a check should look at the supplied classification objects depending on whether they meet the filter criteria.</param>
        public ExcelSheetnameCheck(RuleManager owner, string filteringExpression)
            : base(owner, filteringExpression)
        {
        }

        /// <summary>
        /// Method called by OSLEBot engine for every resource.
        /// This method contains one or more calls to Check() method that log messages to OSLEBot output when criteria specified in Check() are met.
        /// </summary>
        protected override void Run()
        {
            CheckMessage.SetInit("Translation is different from O12");

            Check(lr =>
                lr.FileName.Equals(StringComparison.InvariantCultureIgnoreCase, XlFileName) && //Skip any file that is not this Excel file
                lr.FriendlyID.Equals("idsSheet") &&
                !lr.MatchesLegacyTranslationFromDictionary(O12Translations, CheckMessage),
                CheckMessage);

        }

        /// <summary>
        /// List of legacy translations of 'Sheet' from Office12. The requirement for Office14 is to maintain compatibility with these.
        /// <para/>List owner: sarahs.
        /// </summary>
        private Dictionary<CultureInfo, string> O12Translations = new Dictionary<CultureInfo, string>
        {
            {new CultureInfo("bg-BG"), "Лист"},
            {new CultureInfo("ca-ES"), "Full"},
            {new CultureInfo("cs-CZ"), "List"},
            {new CultureInfo("da-DK"), "Ark"},
            {new CultureInfo("de-DE"), "Tabelle"},
            {new CultureInfo("el-GR"), "Φύλλο"},
            {new CultureInfo("es-ES"), "Hoja"},
            {new CultureInfo("et-EE"), "Leht"},
            {new CultureInfo("fi-FI"), "Taul"},
            {new CultureInfo("fr-FR"), "Feuil"},
            {new CultureInfo("hr-HR"), "List"},
            {new CultureInfo("hu-HU"), "Munka"},
            {new CultureInfo("it-IT"), "Foglio"},
            {new CultureInfo("kk-KZ"), "Парақ"},
            {new CultureInfo("lt-LT"), "Lapas"},
            {new CultureInfo("lv-LV"), "Lapa"},
            {new CultureInfo("nb-NO"), "Ark"},
            {new CultureInfo("nl-NL"), "Blad"},
            {new CultureInfo("pl-PL"), "Arkusz"},
            {new CultureInfo("pt-BR"), "Plan"},
            {new CultureInfo("pt-PT"), "Folha"},
            {new CultureInfo("ro-RO"), "Foaie"},
            {new CultureInfo("ru-RU"), "Лист"},
            {new CultureInfo("sk-SK"), "Hárok"},
            {new CultureInfo("sl-SI"), "List"},
            {new CultureInfo("sr-Latn-CS"), "List"},
            {new CultureInfo("sv-SE"), "Blad"},
            {new CultureInfo("tr-TR"), "Sayfa"},
            {new CultureInfo("uk-UA"), "Аркуш"}
        };
    }

    /// <summary>
    /// Helper methods for simpler notation of the 'Check' expressions.
    /// </summary>
    public static class SheetnameExtensions
    {
        /// <summary>
        /// Returns true if the dictionary contains a translation the same culture as this resource
        /// and the TargetString matches that translation.
        /// <para/>If the dictionary doesn't contain a translation for that culture, it returns a default value of 'true'.
        /// Otherwise, false is returned.
        /// <para/>If false is returned, <paramref name="modifiableMessage"/> is updated to indicate the expected value.
        /// </summary>
        /// <param name="dictionary">Dictionary that contains legacy translations from Office 12.</param>
        /// <param name="modifiableMessage">Message that will go to OSLEBot output. Will be updated by this method when it is about to return false.</param>
        public static bool MatchesLegacyTranslationFromDictionary(this LocResource me, Dictionary<CultureInfo, string> dictionary, CustomMessage modifiableMessage)
        {
            string referenceTranslation;
            if (dictionary.TryGetValue(me.TargetCulture.Value.BuiltinCultureInfo, out referenceTranslation))
            {
                if (me.TargetString.Equals(referenceTranslation)) return true;
                modifiableMessage.SetInit("Current translation is different from Office12 version.");
                modifiableMessage.Expected = referenceTranslation;
                return false;
            }
            return true; //When the dictionary doesn't contain a reference translation for this culture, return a default of 'true'.
        }
    }

    /// <summary>
    /// Invention... Why?
    /// </summary>
    public class CustomMessage : MessageCreator
    {
        /// <summary>
        /// Expected translation.
        /// </summary>
        public string Expected { get; set; }

        /// <summary>
        /// Entry point called by OSLEBot when logging the message.
        /// </summary>
        public override string GetFullMessage()
        {
            return string.Format("{0} Expected: \"{1}\"", base.GetFullMessage(), Expected);
        }
    }
}