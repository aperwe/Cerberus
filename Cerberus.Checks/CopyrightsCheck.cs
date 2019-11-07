using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Localization.OSLEBot.ClassificationObjects;
using Microsoft.Localization.OSLEBot.Core.KnowledgeBase;

namespace Cerberus.Checks
{
    /// <summary>
    /// Copyrights check
    /// Description from the spec:
    /// OSLEBot reads in a list of copyright terms from a config file,
    /// which for each language contains the correct translations for these.
    /// OSLEBot searches for each of the English terms in the source string and if found,
    /// matches the correct translation with the translation for that language from the config file.
    /// Link to copyright information:
    /// "https://sharepoint.partners.extranet.microsoft.com/sites/Office Lockit 14/SW/Instructions1/General_LocGuideline_Copyrights.aspx"
    /// 
    /// Criteria:
    /// [Where source contains one or more of the terms in the list,
    /// ensure that translation value contains the correct translation for that language.
    /// If not, flag an issue.]
    /// Output message: "The copyright information is not translated correctly in this resource."
    /// </summary>
    public class CopyrightsCheck : LocResourceRule
    {
        /// <summary>
        /// Default constructor of the check.
        /// </summary>
        /// <param name="owner">Rule manager passed by OSLEBot engine.</param>
        /// <param name="filteringExpression">Filtering expression passed from the configuration by OSLEBot engine. Used to determine if a check should look at the supplied classification objects depending on whether they meet the filter criteria.</param>
        public CopyrightsCheck(RuleManager owner, string filteringExpression) : base(owner, filteringExpression) { }

        /// <summary>
        /// Method called by OSLEBot engine for every resource.
        /// This method contains one or more calls to Check() method that log messages to OSLEBot output when criteria specified in Check() are met.
        /// </summary>
        protected override void Run()
        {
            var copyrightStrings = GetLanguageCopyrightStrings(((LocResource) CurrentCO).TargetCulture.Value.Name);

            Check(lr =>
                  copyrightStrings.Any(copyrightEntry =>
                                       lr.SourceStringNoHK.Contains(copyrightEntry.EnglishCopyright) &&
                                       !lr.TargetStringNoHK.Contains(copyrightEntry.LanguageCopyright)
                      )
                  , "The copyright information is not translated correctly in this resource.");
        }
        /// <summary>
        /// Prepare the rule for first use.
        /// </summary>
        protected override void Init()
        {
            base.Init();
            _cache = new Dictionary<string, IEnumerable<CopyrightEntry>>();
        }
        /// <summary>
        /// Cleanup memory after use.
        /// </summary>
        protected override void Cleanup()
        {
            base.Cleanup();
            _cache = null;
        }
        /// <summary>
        /// Gets a language-specific list of copyright strings that have to be translated in the approved way.
        /// </summary>
        /// <param name="cultureName">Culture name in form of ll-cc.</param>
        private IEnumerable<CopyrightEntry> GetLanguageCopyrightStrings(string cultureName)
        {
            if (cultureName == null) throw new ArgumentNullException("cultureName");
            if (_cache.ContainsKey(cultureName)) return _cache[cultureName];

            var list = new List<CopyrightEntry>
                           {
                               //Add language agnostic entries here (applicable to all languages).
                           };

            #region Addition of language-specific copyright entries
            switch (cultureName)
            {
                #region Culture: fr-FR
                case "fr-FR":
                    list.AddRange(new[]
                                      {
                                          new CopyrightEntry
                                              {
                                                  EnglishCopyright =
                                                      "© 2010 Microsoft Corporation.  All rights reserved.",
                                                  LanguageCopyright =
                                                      "© 2009 Microsoft Corporation. Tous droits réservés."
                                              },
                               //Add language specific entries here (applicable to this language only).
                                      });
                    break;
                #endregion
                #region Culture: de-DE
                case "de-DE":
                    list.AddRange(new[]
                                      {
                                          new CopyrightEntry
                                              {
                                                  EnglishCopyright =
                                                      "© 2010 Microsoft Corporation.  All rights reserved.",
                                                  LanguageCopyright =
                                                      "© 2009 Microsoft Corporation. Alle Rechte vorbehalten."
                                              },
                                      });
                    break;
                #endregion
                #region Culture: ru-RU
                case "ru-RU":
                    list.AddRange(new[]
                                      {
                                          new CopyrightEntry
                                              {
                                                  EnglishCopyright =
                                                      "© 2010 Microsoft Corporation.  All rights reserved.",
                                                  LanguageCopyright =
                                                      "© Корпорация Майкрософт, 2009. Все права защищены."
                                              },
                                      });
                    break;
                #endregion
                #region Culture: es-ES
                case "es-ES":
                    list.AddRange(new[]
                                      {
                                          new CopyrightEntry
                                              {
                                                  EnglishCopyright =
                                                      "© 2010 Microsoft Corporation.  All rights reserved.",
                                                  LanguageCopyright =
                                                      "© 2009 Microsoft Corporation. Reservados todos los derechos."
                                              },
                                      });
                    break;
                #endregion
                #region Culture: eu-ES
                case "eu-ES":
                    list.AddRange(new[]
                                      {
                                          new CopyrightEntry
                                              {
                                                  EnglishCopyright =
                                                      "© 2010 Microsoft Corporation.  All rights reserved.",
                                                  LanguageCopyright =
                                                      "© 2010 Microsoft Corporation.  Eskubide guztiak erreserbatuta."
                                              },
                                      });
                    break;
                #endregion
                #region Culture: pt-BR
                case "pt-BR":
                    list.AddRange(new[]
                                      {
                                          new CopyrightEntry
                                              {
                                                  EnglishCopyright =
                                                      "© 2010 Microsoft Corporation.  All rights reserved.",
                                                  LanguageCopyright =
                                                      "© 2010 Microsoft Corporation. Todos os direitos reservados."
                                              },
                                      });
                    break;
                #endregion
                #region Culture: bg-BG
                case "bg-BG":
                    list.AddRange(new[]
                                      {
                                          new CopyrightEntry
                                              {
                                                  EnglishCopyright =
                                                      "© 2010 Microsoft Corporation.  All rights reserved.",
                                                  LanguageCopyright =
                                                      "© 2010 Microsoft Corporation. Всички права запазени."
                                              },
                                      });
                    break;
                #endregion
                #region Culture: ca-ES
                case "ca-ES":
                    list.AddRange(new[]
                                      {
                                          new CopyrightEntry
                                              {
                                                  EnglishCopyright =
                                                      "© 2010 Microsoft Corporation.  All rights reserved.",
                                                  LanguageCopyright =
                                                      "© 2010 Microsoft Corporation. Tots els drets reservats."
                                              },
                                      });
                    break;
                #endregion
                #region Culture: hr-HR
                case "hr-HR":
                    list.AddRange(new[]
                                      {
                                          new CopyrightEntry
                                              {
                                                  EnglishCopyright =
                                                      "© 2010 Microsoft Corporation.  All rights reserved.",
                                                  LanguageCopyright =
                                                      "© 2010 Microsoft Corporation. Sva prava pridržana."
                                              },
                                      });
                    break;
                #endregion
                #region Culture: cs-CZ
                case "cs-CZ":
                    list.AddRange(new[]
                                      {
                                          new CopyrightEntry
                                              {
                                                  EnglishCopyright =
                                                      "© 2010 Microsoft Corporation.  All rights reserved.",
                                                  LanguageCopyright =
                                                      "© 2010 Microsoft Corporation. Všechna práva vyhrazena."
                                              },
                                      });
                    break;
                #endregion
                #region Culture: da-DK
                case "da-DK":
                    list.AddRange(new[]
                                      {
                                          new CopyrightEntry
                                              {
                                                  EnglishCopyright =
                                                      "© 2010 Microsoft Corporation.  All rights reserved.",
                                                  LanguageCopyright =
                                                      "© 2010 Microsoft Corporation. Alle rettigheder forbeholdes."
                                              },
                                      });
                    break;
                #endregion
                #region Culture: nl-NL
                case "nl-NL":
                    list.AddRange(new[]
                                      {
                                          new CopyrightEntry
                                              {
                                                  EnglishCopyright =
                                                      "© 2010 Microsoft Corporation.  All rights reserved.",
                                                  LanguageCopyright =
                                                      "© 2010 Microsoft Corporation. Alle rechten voorbehouden."
                                              },
                                      });
                    break;
                #endregion
                #region Culture: et-EE
                case "et-EE":
                    list.AddRange(new[]
                                      {
                                          new CopyrightEntry
                                              {
                                                  EnglishCopyright =
                                                      "© 2010 Microsoft Corporation.  All rights reserved.",
                                                  LanguageCopyright =
                                                      "© 2010, Microsoft Corporation. Kõik õigused on reserveeritud."
                                              },
                                      });
                    break;
                #endregion
                #region Culture: fi-FI
                case "fi-FI":
                    list.AddRange(new[]
                                      {
                                          new CopyrightEntry
                                              {
                                                  EnglishCopyright =
                                                      "© 2010 Microsoft Corporation.  All rights reserved.",
                                                  LanguageCopyright =
                                                      "© 2010 Microsoft Corporation.  Kaikki oikeudet pidätetään."
                                              },
                                      });
                    break;
                #endregion
                #region Culture: gl-ES
                case "gl-ES":
                    list.AddRange(new[]
                                      {
                                          new CopyrightEntry
                                              {
                                                  EnglishCopyright =
                                                      "© 2010 Microsoft Corporation.  All rights reserved.",
                                                  LanguageCopyright =
                                                      "© 2010 Microsoft Corporation. Todos os dereitos reservados."
                                              },
                                      });
                    break;
                #endregion
                #region Culture: el-GR
                case "el-GR":
                    list.AddRange(new[]
                                      {
                                          new CopyrightEntry
                                              {
                                                  EnglishCopyright =
                                                      "© 2010 Microsoft Corporation.  All rights reserved.",
                                                  LanguageCopyright =
                                                      "© 2010 Microsoft Corporation.  Με επιφύλαξη κάθε νόμιμου δικαιώματος."
                                              },
                                      });
                    break;
                #endregion
                #region Culture: hu-HU
                case "hu-HU":
                    list.AddRange(new[]
                                      {
                                          new CopyrightEntry
                                              {
                                                  EnglishCopyright =
                                                      "© 2010 Microsoft Corporation.  All rights reserved.",
                                                  LanguageCopyright =
                                                      "© 2010 Microsoft Corporation. Minden jog fenntartva."
                                              },
                                      });
                    break;
                #endregion
                #region Culture: it-IT
                case "it-IT":
                    list.AddRange(new[]
                                      {
                                          new CopyrightEntry
                                              {
                                                  EnglishCopyright =
                                                      "© 2010 Microsoft Corporation.  All rights reserved.",
                                                  LanguageCopyright =
                                                      "© 2010 Microsoft Corporation. Tutti i diritti riservati."
                                              },
                                      });
                    break;
                #endregion
                #region Culture: kk-KZ
                case "kk-KZ":
                    list.AddRange(new[]
                                      {
                                          new CopyrightEntry
                                              {
                                                  EnglishCopyright =
                                                      "© 2010 Microsoft Corporation.  All rights reserved.",
                                                  LanguageCopyright =
                                                      "© 2010 Майкрософт корпорациясы (Microsoft Corporation). Барлық құқықтары қорғалған."
                                              },
                                      });
                    break;
                #endregion
                #region Culture: lv-LV
                case "lv-LV":
                    list.AddRange(new[]
                                      {
                                          new CopyrightEntry
                                              {
                                                  EnglishCopyright =
                                                      "© 2010 Microsoft Corporation.  All rights reserved.",
                                                  LanguageCopyright =
                                                      "© 2010 Microsoft Corporation. Visas tiesības paturētas."
                                              },
                                      });
                    break;
                #endregion
                #region Culture: lt-LT
                case "lt-LT":
                    list.AddRange(new[]
                                      {
                                          new CopyrightEntry
                                              {
                                                  EnglishCopyright =
                                                      "© 2010 Microsoft Corporation.  All rights reserved.",
                                                  LanguageCopyright =
                                                      "© Microsoft Corporation, 2010. Visos teisės ginamos."
                                              },
                                      });
                    break;
                #endregion
                #region Culture: nb-NO
                case "nb-NO":
                    list.AddRange(new[]
                                      {
                                          new CopyrightEntry
                                              {
                                                  EnglishCopyright =
                                                      "© 2010 Microsoft Corporation.  All rights reserved.",
                                                  LanguageCopyright =
                                                      "© 2010 Microsoft Corporation. Med enerett."
                                              },
                                      });
                    break;
                #endregion
                #region Culture: pl-PL
                case "pl-PL":
                    list.AddRange(new[]
                                      {
                                          new CopyrightEntry
                                              {
                                                  EnglishCopyright =
                                                      "© 2010 Microsoft Corporation.  All rights reserved.",
                                                  LanguageCopyright =
                                                      "© 2010 Microsoft Corporation. Wszelkie prawa zastrzeżone."
                                              },
                                      });
                    break;
                #endregion
                #region Culture: pt-PT
                case "pt-PT":
                    list.AddRange(new[]
                                      {
                                          new CopyrightEntry
                                              {
                                                  EnglishCopyright =
                                                      "© 2010 Microsoft Corporation.  All rights reserved.",
                                                  LanguageCopyright =
                                                      "© 2010 Microsoft Corporation. Todos os direitos reservados."
                                              },
                                      });
                    break;
                #endregion
                #region Culture: ro-RO
                case "ro-RO":
                    list.AddRange(new[]
                                      {
                                          new CopyrightEntry
                                              {
                                                  EnglishCopyright =
                                                      "© 2010 Microsoft Corporation.  All rights reserved.",
                                                  LanguageCopyright =
                                                      "© 2010 Microsoft Corporation. Toate drepturile rezervate."
                                              },
                                      });
                    break;
                #endregion
                #region Culture: sr-Cyrl-cs
                case "sr-Cyrl-CS":
                    list.AddRange(new[]
                                      {
                                          new CopyrightEntry
                                              {
                                                  EnglishCopyright =
                                                      "© 2010 Microsoft Corporation.  All rights reserved.",
                                                  LanguageCopyright =
                                                      "© 2010 Microsoft Corporation. Сва права задржана."
                                              },
                                      });
                    break;
                #endregion
                #region Culture: sr-Latn-CS
                case "sr-Latn-CS":
                    list.AddRange(new[]
                                      {
                                          new CopyrightEntry
                                              {
                                                  EnglishCopyright =
                                                      "© 2010 Microsoft Corporation.  All rights reserved.",
                                                  LanguageCopyright =
                                                      "© 2010 Microsoft Corporation. Sva prava zadržana."
                                              },
                                      });
                    break;
                #endregion
                #region Culture: sk-SK
                case "sk-SK":
                    list.AddRange(new[]
                                      {
                                          new CopyrightEntry
                                              {
                                                  EnglishCopyright =
                                                      "© 2010 Microsoft Corporation.  All rights reserved.",
                                                  LanguageCopyright =
                                                      "© 2010 Microsoft Corporation. Všetky práva vyhradené."
                                              },
                                      });
                    break;
                #endregion
                #region Culture: sl-SI
                case "sl-SI":
                    list.AddRange(new[]
                                      {
                                          new CopyrightEntry
                                              {
                                                  EnglishCopyright =
                                                      "© 2010 Microsoft Corporation.  All rights reserved.",
                                                  LanguageCopyright =
                                                      "© 2010 Microsoft Corporation. Vse pravice pridržane."
                                              },
                                      });
                    break;
                #endregion
                #region Culture: sv-SE
                case "sv-SE":
                    list.AddRange(new[]
                                      {
                                          new CopyrightEntry
                                              {
                                                  EnglishCopyright =
                                                      "© 2010 Microsoft Corporation.  All rights reserved.",
                                                  LanguageCopyright =
                                                      "© 2010 Microsoft Corporation. Med ensamrätt."
                                              },
                                      });
                    break;
                #endregion
                #region Culture: tr-TR
                case "tr-TR":
                    list.AddRange(new[]
                                      {
                                          new CopyrightEntry
                                              {
                                                  EnglishCopyright =
                                                      "© 2010 Microsoft Corporation.  All rights reserved.",
                                                  LanguageCopyright =
                                                      "© 2010 Microsoft Corporation. Tüm hakları saklıdır."
                                              },
                                      });
                    break;
                #endregion
                #region Culture: uk-UA
                case "uk-UA":
                    list.AddRange(new[]
                                      {
                                          new CopyrightEntry
                                              {
                                                  EnglishCopyright =
                                                      "© 2010 Microsoft Corporation.  All rights reserved.",
                                                  LanguageCopyright =
                                                      "© Корпорація Майкрософт (Microsoft Corporation), 2010. Всі права захищено."
                                              },
                                      });
                    break;
                #endregion
            }

            #endregion

            //Add to cache so that next call doesn't need to recreate the list for the same language and return to the caller
            _cache.Add(cultureName, list);
            return list;
        }

        private Dictionary<string, IEnumerable<CopyrightEntry>> _cache;
    }
    /// <summary>
    /// Copyright entry containing the English copyright string and it's expected translation for the target language.
    /// </summary>
    internal class CopyrightEntry
    {
        internal string EnglishCopyright { get; set; }
        internal string LanguageCopyright { get; set; }
    }

}