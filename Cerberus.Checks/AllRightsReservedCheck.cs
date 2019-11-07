using System;
using System.Text.RegularExpressions;
using System.Linq;
using Microsoft.Localization.OSLEBot.Core.KnowledgeBase;
using Microsoft.Localization.OSLEBot.ClassificationObjects;
using Microsoft.Localization.OSLEBot.Core.Misc;
using System.Globalization;
using System.Collections.Generic;

namespace Cerberus.Checks
{
    /// <summary>
    /// Description from the spec:
    /// Verify that the translation for "All rights reserved" is correct for selected languages.
    /// 
    /// 
    /// Criteria:
    /// If source string contains "All rights reserved" translation must contain the approved translation specific for the language.
    /// If for the current language translation is not specified, this check throws a different warning.
    /// Output message: Invalid translation for "All rights reserved". Should be: 
    /// </summary>
    public class AllRightsReservedCheck : LocResourceRule
    {
        public AllRightsReservedCheck(RuleManager owner, string filteringExpression)
            : base(owner, filteringExpression)
        {
            mc = new StringAppendMessageCreator();
        }
        /// <summary>
        /// Message creator used to provide context messages in checks.
        /// </summary>
        readonly StringAppendMessageCreator mc;


        protected override void Run()
        {
            var currentLocResource = (LocResource)CurrentCO;
            // check if source string contains the "all rights reserved" expression". if not, ignore the check.
            if (currentLocResource.SourceStringNoHK.Contains(StringComparison.OrdinalIgnoreCase, "All rights reserved"))
            {
                var targetCultureInfo = currentLocResource.TargetCulture.Value.BuiltinCultureInfo;
                string approvedTranslation = null;
                // get the approvedTranslation for the current target language.
                if (approvedTranslations.TryGetValue(targetCultureInfo, out approvedTranslation))
                {
                    // set message context to the approved translation value, so we append it to the message.
                    mc.SetContext(approvedTranslation);
                    Check(lr => 
                            !lr.TargetString.Value.Contains(StringComparison.OrdinalIgnoreCase, approvedTranslation),
                            mc.SetInit("Incorrect translation for term \"All rights reserved\". Should be: ")
                            );
                }
                else
                {
                    Check(lr => true,
                        String.Format("No approved translation for term \"All rights reserved\" defined in Cerberus for culture {0}. Cannot execute check.", targetCultureInfo.Name));
                }
            }
        }
        /// <summary>
        /// Mapping of culture infos to approved translations of term "All rights reserved"
        /// </summary>
        private readonly Dictionary<CultureInfo, string> approvedTranslations = new Dictionary<CultureInfo, string>()
        {
            {new CultureInfo("ko-kr"),@"모든 권리 보유"},
            {new CultureInfo("zh-CN"),@"保留所有权利。"},
            {new CultureInfo("zh-TW"), "All rights reserved"},
	        {new CultureInfo("bg-bg"), "Всички права запазени"},
	        {new CultureInfo("fi-fi"), "Kaikki oikeudet pidätetään"},
	        {new CultureInfo("fr-fr"), "Tous droits réservés"},
	        {new CultureInfo("de-de"), "Alle Rechte vorbehalten."},
	        {new CultureInfo("eu-es"), "Eskubide guztiak erreserbatuta"},
	        {new CultureInfo("ca-es"), "Tots els drets reservats"},
	        {new CultureInfo("hr-hr"), "Sva prava pridržana"},
	        {new CultureInfo("cs-cz"), "Všechna práva vyhrazena"},
	        {new CultureInfo("da-dk"), "Alle rettigheder forbeholdes"},
	        {new CultureInfo("nl-nl"), "Alle rechten voorbehouden"},
	        {new CultureInfo("et-ee"), "Kõik õigused on reserveeritud"},
	        {new CultureInfo("gl-es"), "Todos os dereitos reservados"},
	        {new CultureInfo("el-gr"), "Με επιφύλαξη κάθε νόμιμου δικαιώματος"},
	        {new CultureInfo("hi-in"), "सर्वाधिकार सुरक्षित."},
	        {new CultureInfo("hu-hu"), "Minden jog fenntartva"},
	        {new CultureInfo("it-it"), "Tutti i diritti riservati"},
	        {new CultureInfo("ja-jp"), "All rights reserved"},
	        {new CultureInfo("kk-kz"), "Барлық құқықтары қорғалған"},
	        {new CultureInfo("lt-lt"), "Visas tiesības paturētas"},
	        {new CultureInfo("lv-lv"), "Visos teisės ginamos"},
	        {new CultureInfo("nb-no"), "Med enerett"},
	        {new CultureInfo("pl-pl"), "Wszelkie prawa zastrzeżone"},
	        {new CultureInfo("pt-br"), "Todos os direitos reservados"},
	        {new CultureInfo("pt-pt"), "Todos os direitos reservados"},
	        {new CultureInfo("ro-ro"), "Toate drepturile rezervate"},
	        {new CultureInfo("ru-ru"), "Все права защищены"},
	        {new CultureInfo("sr-cyrl-cs"), "Сва права задржана"},
	        {new CultureInfo("sr-latn-cs"), "Sva prava zadržana"},
	        {new CultureInfo("sk-sk"), "Všetky práva vyhradené"},
	        {new CultureInfo("sl-si"), "Vse pravice pridržane"},
	        {new CultureInfo("es-es"), "Reservados todos los derechos"},
	        {new CultureInfo("sv-se"), "Med ensamrätt"},
	        {new CultureInfo("th-th"), "สงวนลิขสิทธิ์ "},
	        {new CultureInfo("tr-tr"), "Tüm hakları saklıdır"},
	        {new CultureInfo("uk-ua"), "Всі права захищено"},
	        {new CultureInfo("ar-sa"), "كافة الحقوق محفوظة"},
        };
    }
}