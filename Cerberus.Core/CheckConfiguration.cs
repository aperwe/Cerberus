using System.Collections;

namespace Microsoft.Localization.LocSolutions.Cerberus.Core
{
    using System.Linq;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using Logger;

    public partial class CheckConfiguration
    {
        partial class ChecksDataTable
        {
            /// <summary>
            /// A macro overload that can be used to add a check record using incomplete data.
            /// </summary>
            /// <param name="Name">Name of the check file. Usually this is the *.cs file name with extension stripped.</param>
            /// <param name="Description">Check description as provided by check owner.</param>
            /// <param name="FilePath">Path to physical file that contains this check.</param>
            protected internal ChecksRow AddChecksRow(string Name, string Description, string FilePath)
            {
                return AddChecksRow(Name, Description, FilePath, "<Comments>", "<Owners>");
            }
        }

        ///// <summary>
        ///// Retrieves source code of the check specified by check name.
        ///// This source code can be later compiled into binary assembly by OSLEBot.
        ///// </summary>
        ///// <param name="checkName">Name of the check whose source code needs to be retrieved.</param>
        ///// <returns>Source code string. This should be written into a temporary *.cs file if you want to have OSLEBot compile it.</returns>
        ///// <exception cref="ArgumentOutOfRangeException">Thrown when a check with the specified name is not found.</exception>
        ///// <exception cref="ArgumentException">Thrown when more than 1 check matches the specified name. This indicates data inconsistency.</exception>
        //public string GetCheckSourceCode(string checkName)
        //{
        //    var matches = from check in Checks
        //                  where check.Name == checkName
        //                  select check;
        //    if (matches.Count() == 0)
        //        throw new ArgumentOutOfRangeException("checkName", "The specified check not found");
        //    if (matches.Count() > 1)
        //        throw new ArgumentException("Data inconsistency. Found more than 1 check with the same name.",
        //                                    "checkName");
        //    return matches.Single().SourceCode;
        //}

        /// <summary>
        /// Fills this data set with test (sample) data.
        /// </summary>
        public void FillWithTestData()
        {
            Clear();
            var check1 = Checks.AddChecksRow("FedericaTest.cs", "Test for Federica", "");
            var check2 = Checks.AddChecksRow("Branding.cs", "This is check 2", "");
            var check3 = Checks.AddChecksRow("PseudoLoc.cs", "This is check 3", "");
            var check4 = Checks.AddChecksRow("InvalidLocVer.cs", "This is global check for testing globality detection", "");
            var check5 = Checks.AddChecksRow("EACheck.cs", "This is a check that is global for all languages", "");
            var check6 = Checks.AddChecksRow("WordTemplatesMailMerge.cs", "This is a second check that is global for all languages", "");

            var enUS = Languages.AddLanguagesRow("en-us");
            var frFR = Languages.AddLanguagesRow("fr-fr");
            var deDE = Languages.AddLanguagesRow("de-de");
            var pseudo = Languages.AddLanguagesRow("ja-jp.pseudo");
            var plPL = Languages.AddLanguagesRow("pl-pl");
            var jaJP = Languages.AddLanguagesRow("ja-jp");
            var brBR = Languages.AddLanguagesRow("br-BR");

            var msoProject = Projects.AddProjectsRow("mso");
            var wordProject = Projects.AddProjectsRow("word");
            var excelProject = Projects.AddProjectsRow("excel");
            var accessProject = Projects.AddProjectsRow("access");

            LocGroups.AddLocGroupsRow("main_dll", msoProject);
            LocGroups.AddLocGroupsRow("main_dll", wordProject);
            LocGroups.AddLocGroupsRow("main_dll", excelProject);
            var wordTemplates = LocGroups.AddLocGroupsRow("wordtemplates", wordProject);
            var xlintl = LocGroups.AddLocGroupsRow("xlintl", excelProject);
            var accintl = LocGroups.AddLocGroupsRow("acc_intl", accessProject);

            ConfigItems.AddConfigItemsRow(pseudo, check1, wordTemplates);
            ConfigItems.AddConfigItemsRow(pseudo, check1, xlintl);
            ConfigItems.AddConfigItemsRow(plPL, check1, wordTemplates);
            ConfigItems.AddConfigItemsRow(enUS, check2, xlintl);
            ConfigItems.AddConfigItemsRow(frFR, check3, accintl);
            ConfigItems.AddConfigItemsRow(frFR, check2, wordTemplates);

            foreach (var lg in LocGroups)
            {
                ConfigItems.AddConfigItemsRow(plPL, check4, lg);
                ConfigItems.AddConfigItemsRow(brBR, check4, lg);
                ConfigItems.AddConfigItemsRow(jaJP, check1, lg);
            }
            SetCheckGlobalForAllLanguages(check5);
            SetCheckGlobalForAllLanguages(check6);

            var allChecksTag = Tags.AddTagsRow("All checks");
            Checks.ToList().ForEach(check => CheckTags.AddCheckTagsRow(check, allChecksTag));

            var eaLanguagesTag = Tags.AddTagsRow("EA languages");
            Languages.ToList().ForEach(language => LanguageTags.AddLanguageTagsRow(language, eaLanguagesTag));

            var allProjectsTag = Tags.AddTagsRow("All projects");
            Projects.ToList().ForEach(project => ProjectTags.AddProjectTagsRow(project, allProjectsTag));
        }

        /// <summary>
        /// Fills this data set with sample testable in a dataset. Dataset can be then modified.
        /// </summary>
        public void SetCheckGlobalForAllLanguages(CheckConfiguration.ChecksRow checkRow)
        {
            foreach (var language in Languages)
            {
                foreach (var lg in LocGroups)
                {
                    ConfigItems.AddConfigItemsRow(language, checkRow, lg);
                }
            }
        }

        /// <summary>
        /// Makes sure the check is enabled for every logroup in a language.
        /// <para/>No exception is thrown.
        /// </summary>
        /// <param name="checkName">Check to enable</param>
        /// <param name="languageName">Language to enable the check for</param>
        [Obsolete]
        public void OldEnableCheckForLanguage(string checkName, string languageName)
        {
            var languageRow = (from language in Languages
                               where language.Name == languageName
                               select language).Single();
            var checkRow = (from check in Checks
                            where check.Name == checkName
                            select check).Single();
            foreach (var lg in LocGroups)
            {
                try
                {
                    ConfigItems.AddConfigItemsRow(languageRow, checkRow, lg);
                }
                catch (ConstraintException ex)
                {
                    LoggerSAP.Trace(ex.Message);
                }
            }
        }

        /// <summary>
        /// Retrieves the description text of the specified check.
        /// </summary>
        public string GetCheckDescription(string checkName)
        {
            var checkRow = Checks.Single(c => c.Name.Equals(checkName));
            return checkRow.Description;
        }

        /// <summary>
        /// Sets the description text for the specified check.
        /// </summary>
        public void SetCheckDescription(string checkName, string description)
        {
            var checkRow = Checks.Single(c => c.Name.Equals(checkName));
            checkRow.Description = description;
        }

        /// <summary>
        /// Retrieves the owners text of the specified check.
        /// </summary>
        public string GetCheckOwners(string checkName)
        {
            var checkRow = Checks.Single(c => c.Name.Equals(checkName));
            return checkRow.Owners;
        }

        /// <summary>
        /// Sets the owners text for the specified check.
        /// </summary>
        public void SetCheckOwners(string checkName, string owners)
        {
            var checkRow = Checks.Single(c => c.Name.Equals(checkName));
            checkRow.Owners = owners;
        }

        /// <summary>
        /// Establishes an association between a check and a language.
        /// </summary>
        /// <param name="checkName">Check to enable on language.</param>
        /// <param name="language">Language to enable check on.</param>
        public void EnableCheckForLanguage(string checkName, string language)
        {
            var checkRow = Checks.Single(c => c.Name.Equals(checkName));
            var languageRow = Languages.Single(l => l.Name.Equals(language));
            if (LanguageChecks.Any(row => row.CheckID == checkRow.ID && row.LanguageID == languageRow.ID)) return;
            LanguageChecks.AddLanguageChecksRow(checkRow, languageRow);
        }

        /// <summary>
        /// Removes the association between a check and a language.
        /// If the association doesn't exist, nothing happens.
        /// </summary>
        /// <param name="checkName">Check to disable on language.</param>
        /// <param name="language">Language to disable check on.</param>
        public void DisableCheckForLanguage(string checkName, string language)
        {
            var candidates = LanguageChecks.Where(lc => lc.LanguagesRow.Name.Equals(language) && lc.ChecksRow.Name.Equals(checkName));
            if (candidates.Count() == 0) return;

            LanguageChecks.RemoveLanguageChecksRow(candidates.First());
        }

        /// <summary>
        /// Establishes or removes an association between a check and a language.
        /// </summary>
        /// <param name="checkName">Check to enable or disable on language.</param>
        /// <param name="language">Language to enable or disable check on.</param>
        /// <param name="enable">If true, an association will be established. If false, the association will be removed if any.</param>
        public void EnableCheckForLanguage(string checkName, string language, bool enable)
        {
            if (enable) EnableCheckForLanguage(checkName, language);
            else DisableCheckForLanguage(checkName, language);
        }

        /// <summary>
        /// Establishes or removes an association between a check and all languages.
        /// </summary>
        /// <param name="checkName">Check to enable or disable on all languages.</param>
        /// <param name="enable">If true, an association will be established. If false, the association will be removed if any.</param>
        public void EnableCheckForAllLanguages(string checkName, bool enable)
        {
            var checkRow = Checks.Single(c => c.Name.Equals(checkName));
            if (enable)
            {
                //Add any missing entries for this check
                foreach (var languageRow in Languages)
                {
                    if (LanguageChecks.Any(row => row.CheckID == checkRow.ID && row.LanguageID == languageRow.ID)) continue;
                    LanguageChecks.AddLanguageChecksRow(checkRow, languageRow);
                }
            }
            else
            {
                //Remove existing entries for this check
                LanguageChecks.Where(c => c.ChecksRow.Name.Equals(checkName)).ToList().ForEach(LanguageChecks.RemoveLanguageChecksRow);
            }
        }

        /// <summary>
        /// Establishes an association between a check and a project.
        /// </summary>
        /// <param name="checkName">Check to enable on project.</param>
        /// <param name="project">Project to enable check on.</param>
        public void EnableCheckForProject(string checkName, string project)
        {
            var checkRow = Checks.Single(c => c.Name.Equals(checkName));
            var projectRow = Projects.Single(p => p.Name.Equals(project));
            if (ProjectChecks.Any(row => row.CheckID == checkRow.ID && row.ProjectID == projectRow.ID)) return;
            ProjectChecks.AddProjectChecksRow(checkRow, projectRow);
        }

        /// <summary>
        /// Removes the association between a check and a project.
        /// If the association doesn't exist, nothing happens.
        /// </summary>
        /// <param name="checkName">Check to disable on project.</param>
        /// <param name="project">Project to disable check on.</param>
        public void DisableCheckForProject(string checkName, string project)
        {
            var candidates = from pc in ProjectChecks
                              join prj in Projects on pc.ProjectID equals prj.ID
                              join check in Checks on pc.CheckID equals check.ID
                              where check.Name.Equals(checkName)
                              where prj.Name.Equals(project)
                              select pc;
            if (candidates.Count() == 0) return;

            ProjectChecks.RemoveProjectChecksRow(candidates.First());
        }

        /// <summary>
        /// Establishes or removes an association between a check and a project.
        /// </summary>
        /// <param name="checkName">Check to enable or disable on project.</param>
        /// <param name="project">Project to enable or disable check on.</param>
        /// <param name="enable">If true, an association will be established. If false, the association will be removed if any.</param>
        public void EnableCheckForProject(string checkName, string project, bool enable)
        {
            if (enable) EnableCheckForProject(checkName, project);
            else DisableCheckForProject(checkName, project);
        }

        /// <summary>
        /// Establishes or removes an association between a check and all projects.
        /// </summary>
        /// <param name="checkName">Check to enable or disable on all projects.</param>
        /// <param name="enable">If true, an association will be established. If false, the association will be removed if any.</param>
        public void EnableCheckForAllProjects(object checkName, bool enable)
        {
            var checkRow = Checks.Single(c => c.Name.Equals(checkName));
            if (enable)
            {
                //Add any missing entries for this check
                foreach (var projectRow in Projects)
                {
                    if (ProjectChecks.Any(row => row.CheckID == checkRow.ID && row.ProjectID == projectRow.ID)) continue;
                    ProjectChecks.AddProjectChecksRow(checkRow, projectRow);
                }
            }
            else
            {
                //Remove existing entries for this check
                ProjectChecks.Where(c => c.ChecksRow.Name.Equals(checkName)).ToList().ForEach(ProjectChecks.RemoveProjectChecksRow);
            }
        }

        /// <summary>
        /// Answers the question, whether the specified check is global for the specified language.
        /// The check is global when it is defined for every possible locgroup in that language.
        /// </summary>
        /// <param name="checkName">Name of the check</param>
        /// <param name="languageName">Name of the language to examine</param>
        /// <returns>True if check is defined for all locGroups in this language.</returns>
        public bool IsCheckGlobalForLanguage(string checkName, string languageName)
        {
            var allLocGroups = from locGroup in LocGroups
                               select locGroup;
            var allCheckConfigs = from config in ConfigItems
                                  join check in Checks on config.CheckID equals check.ID
                                  join language in Languages on config.LanguageID equals language.ID
                                  where check.Name == checkName
                                  where language.Name == languageName
                                  select check;
            var z = allLocGroups.Count();
            var y = allCheckConfigs.Count();
            return z == y;
        }

        /// <summary>
        /// Answers the question, whether the specified check is enabled for any locgroup for the specified language.
        /// </summary>
        /// <param name="checkName">Name of the check</param>
        /// <param name="languageName">Name of the language to examine</param>
        /// <returns>True if check is defined for at least one locGroup in this language.</returns>
        public bool IsCheckPartiallyEnabledForLanguage(string checkName, string languageName)
        {
            var allCheckConfigs = from config in ConfigItems
                                  join check in Checks on config.CheckID equals check.ID
                                  join language in Languages on config.LanguageID equals language.ID
                                  where check.Name == checkName
                                  where language.Name == languageName
                                  select check;
            var y = allCheckConfigs.Count();
            return y > 0;
        }

        /// <summary>
        /// Gets a collection of all language names defined in this data set, sorted alphabetically.
        /// </summary>
        public IEnumerable<string> GetAllLanguages()
        {
            return from language in Languages
                   orderby language.Name
                   select language.Name;
        }

        /// <summary>
        /// Gets a collection of all projects defined in this data set, sorted alphabetically.
        /// </summary>
        public IEnumerable<string> GetAllProjects()
        {
            return from project in Projects
                   orderby project.Name
                   select project.Name;
        }

        /// <summary>
        /// Gets a collection of all check names defined in this data set, sorted alphabetically.
        /// </summary>
        public IEnumerable<string> GetAllChecks()
        {
            return from check in Checks
                   orderby check.Name
                   select check.Name;
        }

        /// <summary>
        /// Gets a collection of all tag names defined in this data set, sorted alphabetically.
        /// </summary>
        public IEnumerable<string> GetAllTags()
        {
            return from tag in Tags
                   orderby tag.Tag
                   select tag.Tag;
        }

        /// <summary>
        /// Selects names of those projects, for which there are config items of the specified check for the specified language.
        /// </summary>
        /// <param name="checkName">Name of the check which should be active for a project</param>
        /// <param name="languageName">Language, for which the check should be enabled</param>
        public IEnumerable<string> GetProjectsWithCheckEnabledForLanguage(string checkName, string languageName)
        {
            return from config in ConfigItems
                   join language in Languages on config.LanguageID equals language.ID
                   join locGroup in LocGroups on config.LocGroupID equals locGroup.ID
                   join project in Projects on locGroup.ProjectID equals project.ID
                   join check in Checks on config.CheckID equals check.ID
                   where language.Name == languageName
                   where check.Name == checkName
                   group project by project.Name
                   into projectGroup
                       select projectGroup.Key;
        }

        /// <summary>
        /// Selects names of LocGroups for which (within the specified project) there are config items of the specified check for the specified language.
        /// </summary>
        /// <param name="checkName">Name of the check which should be active for a project</param>
        /// <param name="projectName">Project, whose child LocGroups should be searched</param>
        /// <param name="languageName">Language, for which the check should be enabled</param>
        public IEnumerable<string> GetLocgroupsWithCheckEnabledForProjectForLanguage(string checkName, string projectName, string languageName)
        {
            return from config in ConfigItems
                   join locGroup in LocGroups on config.LocGroupID equals locGroup.ID
                   join project in Projects on locGroup.ProjectID equals project.ID
                   join language in Languages on config.LanguageID equals language.ID
                   join check in Checks on config.CheckID equals check.ID
                   where project.Name == projectName
                   where language.Name == languageName
                   where check.Name == checkName
                   group locGroup by locGroup.Name
                   into locGroupGroup
                       select locGroupGroup.Key;
        }

        /// <summary>
        /// Updates the <see cref="Languages"/> table with any items from the specified <paramref name="languages"/> collection
        /// that are missing in the <see cref="Languages"/> table.
        /// </summary>
        /// <param name="languages">List of languages to add (if missing) to the <see cref="Languages"/> table.</param>
        public void AddMissingLanguages(IEnumerable<string> languages)
        {
            foreach (var language in languages)
            {
                if (!Languages.Any(row => row.Name.Equals(language)))
                {
                    Languages.AddLanguagesRow(language);
                }
            }
        }

        /// <summary>
        /// Updates the <see cref="Projects"/> table with any items from the specified <paramref name="projects"/> collection
        /// that are missing in the <see cref="Projects"/> table.
        /// </summary>
        /// <param name="projects">List of projects to add (if missing) to the <see cref="Projects"/> table.</param>
        public void AddMissingProjects(IEnumerable<string> projects)
        {
            foreach (var project in projects)
            {
                if (!Projects.Any(row => row.Name.Equals(project)))
                {
                    Projects.AddProjectsRow(project);
                }
            }
        }

        /// <summary>
        /// Updates the <see cref="Checks"/> table with any items from the specified <paramref name="checks"/> collection
        /// that are missing in the <see cref="Checks"/> table.
        /// </summary>
        /// <param name="checks">List of check names to add (if missing) to the <see cref="Checks"/> table.</param>
        public void AddMissingChecks(IEnumerable<CheckInfo> checks)
        {
            foreach (var check in checks)
            {
                if (!Checks.Any(row => row.Name.Equals(check.Name)))
                {
                    Checks.AddChecksRow(check.Name, check.Description, check.FilePath);
                }
            }
        }

        /// <summary>
        /// Updates the <see cref="Tags"/> table with any items from the specified <paramref name="tags"/> collection
        /// that are missing in the <see cref="Tags"/> table.
        /// <para/>Doesn't add empty strings.
        /// </summary>
        /// <param name="tags">List of tag names to add (if missing) to the <see cref="Tags"/> table.</param>
        public void AddMissingTags(IEnumerable<string> tags)
        {
            foreach (var tag in tags)
            {
                if (string.IsNullOrEmpty(tag)) continue;
                if (Tags.Any(row => row.Tag.Equals(tag, StringComparison.InvariantCultureIgnoreCase))) continue;
                Tags.AddTagsRow(tag);
            }
        }

        /// <summary>
        /// Adds or removes an association between the specified tag name and all of the specified languages.
        /// Makes sure the tag exists (creates it if it doesn't exist).
        /// <para/>NOTE: When removing a tag from language, it doesn't remove the tag when no more languages are associated with that tag.
        /// </summary>
        /// <param name="tag">Tag to be applied to languages</param>
        /// <param name="languages">Collection of languages to apply the tag to.</param>
        /// <param name="apply">When true, the tag is applied to languages, when false, the tag is removed from languages.</param>
        public void ApplyTagToLanguages(string tag, IEnumerable<string> languages, bool apply)
        {
            if (apply)
            {
                ApplyTagToLanguages(tag, languages);
            }
            else
            {
                RemoveTagFomLanguages(tag, languages);
            }
        }

        /// <summary>
        /// Adds an association between the specified tag name and all of the specified languages.
        /// Makes sure the tag exists (creates it if it doesn't exist)
        /// </summary>
        /// <param name="tag">Tag to be applied to languages</param>
        /// <param name="languages">Collection of languages to apply the tag to.</param>
        public void ApplyTagToLanguages(string tag, IEnumerable<string> languages)
        {
            AddMissingTags(new[] {tag});
            var tagRow = Tags.Single(r => r.Tag.Equals(tag));
            foreach (var language in languages)
            {
                var languageRow = Languages.Single(l => l.Name.Equals(language));
                if (LanguageTags.Any(row => row.TagID == tagRow.ID && row.LanguageID == languageRow.ID)) continue;
                LanguageTags.AddLanguageTagsRow(languageRow, tagRow);
            }
        }

        /// <summary>
        /// Removes the specified tag from the selected langauge elements of enlistment.
        /// </summary>
        /// <param name="tag">Tag to be revoved from languages.</param>
        /// <param name="languages">List of languages to remove the tag from.</param>
        public void RemoveTagFomLanguages(string tag, IEnumerable<string> languages)
        {
            var tagID = Tags.Single(r => r.Tag.Equals(tag)).ID;
            foreach (var language in languages)
            {
                var languageID = Languages.Single(l => l.Name.Equals(language)).ID;
                var candidates = LanguageTags.Where(x => x.TagID == tagID && x.LanguageID == languageID).ToList();
                if (candidates.Count != 1) continue; //Skip, in case the caller requested removal from a language that doesn't have this tag.
                var lt = candidates.Single();
                LanguageTags.RemoveLanguageTagsRow(lt);
            }
        }

        /// <summary>
        /// Adds an association between the specified tag name and all of the specied projects.
        /// Makes sure the tag exists (creates it if it doesn't exist)
        /// </summary>
        /// <param name="tag">Tag to be applied to projects</param>
        /// <param name="projects">Collection of projects to apply the tag to.</param>
        public void ApplyTagToProjects(string tag, IEnumerable<string> projects)
        {
            AddMissingTags(new[] { tag });
            var tagRow = Tags.Single(r => r.Tag.Equals(tag));
            foreach (var project in projects)
            {
                var projectRow = Projects.Single(p => p.Name.Equals(project));
                if (ProjectTags.Any(row => row.TagID == tagRow.ID && row.ProjectID == projectRow.ID)) continue;
                ProjectTags.AddProjectTagsRow(projectRow, tagRow);
            }
        }

        /// <summary>
        /// Removes the specified tag from the selected project elements of enlistment.
        /// </summary>
        /// <param name="tag">Tag to be removed from projects.</param>
        /// <param name="projects">List of projects to remove the tag from.</param>
        public void RemoveTagFomProjects(string tag, IEnumerable<string> projects)
        {
            var tagID = Tags.Single(r => r.Tag.Equals(tag)).ID;
            foreach (var project in projects)
            {
                var projectID = Projects.Single(p => p.Name.Equals(project)).ID;
                var pt = ProjectTags.Single(x => x.TagID == tagID && x.ProjectID == projectID);
                ProjectTags.RemoveProjectTagsRow(pt);
            }
        }

        /// <summary>
        /// Adds an association between the specified tag name and all of the specied checks.
        /// Makes sure the tag exists (creates it if it doesn't exist)
        /// </summary>
        /// <param name="tag">Tag to be applied to checks</param>
        /// <param name="checks">Collection of checks to apply the tag to.</param>
        public void ApplyTagToChecks(string tag, IEnumerable<string> checks)
        {
            AddMissingTags(new[] {tag});
            var tagRow = Tags.Single(r => r.Tag.Equals(tag));
            foreach (var check in checks)
            {
                var checkRow = Checks.Single(c => c.Name.Equals(check));
                if (CheckTags.Any(row => row.TagID == tagRow.ID && row.CheckID == checkRow.ID)) continue;
                CheckTags.AddCheckTagsRow(checkRow, tagRow);
            }
        }

        /// <summary>
        /// Removes the specified tag from the selected checks.
        /// </summary>
        /// <param name="tag">Tag to be removed from checks.</param>
        /// <param name="checks">List of checks to remove the tag from.</param>
        public void RemoveTagFomChecks(string tag, IEnumerable<string> checks)
        {
            var tagID = Tags.Single(r => r.Tag.Equals(tag)).ID;
            foreach (var check in checks)
            {
                var checkID = Checks.Single(c => c.Name.Equals(check)).ID;
                var ct = CheckTags.Single(x => x.TagID == tagID && x.CheckID == checkID);
                CheckTags.RemoveCheckTagsRow(ct);
            }
        }

        /// <summary>
        /// Gets all tags that are associated with the specified language.
        /// If no such tags exist, empty collection is returned.
        /// </summary>
        /// <param name="language">Language whose associated tags are to be found.</param>
        public IEnumerable<string> GetLanguageTags(string language)
        {
            return from languageTagRow in LanguageTags
                   join languageRow in Languages on languageTagRow.LanguageID equals languageRow.ID
                   where languageRow.Name.Equals(language)
                   join tagRow in Tags on languageTagRow.TagID equals tagRow.ID
                   select tagRow.Tag;
        }

        /// <summary>
        /// Gets all tags that are associated with the specified project.
        /// If no such tags exist, empty collection is returned.
        /// </summary>
        /// <param name="project">Project whose associated tags are to be found.</param>
        public IEnumerable<string> GetProjectTags(string project)
        {
            return from projectTagRow in ProjectTags
                   join projectRow in Projects on projectTagRow.ProjectID equals projectRow.ID
                   where projectRow.Name.Equals(project)
                   join tagRow in Tags on projectTagRow.TagID equals tagRow.ID
                   select tagRow.Tag;
        }

        /// <summary>
        /// Gets all tags that are associated with the specified check.
        /// If no such tags exist, empty collection is returned.
        /// </summary>
        /// <param name="checkName">Check whose associated tags are to be found.</param>
        public IEnumerable<string> GetCheckTags(string checkName)
        {
            return from checkTagRow in CheckTags
                   join checkRow in Checks on checkTagRow.CheckID equals checkRow.ID
                   where checkRow.Name.Equals(checkName)
                   join tagRow in Tags on checkTagRow.TagID equals tagRow.ID
                   select tagRow.Tag;
        }

        /// <summary>
        /// Gets a collection of all checks that are enabled for the specified language and project.
        /// The logic is:
        /// <para/>If a check is enabled for the specified language, it is added to returned set (because logically a project is a child of the language, therefore by definition a check enabled for a language is enabled for all projects).
        /// <para/>If a check is not enabled for language, but is enabled for the specified project, it is also added to the set.
        /// </summary>
        /// <param name="language">Language for which to search for enabled checks.</param>
        /// <param name="project">Project for which to search for enabled checks.</param>
        /// <returns>A list of checks that should be run on the specified pair of {language, project}.</returns>
        public IEnumerable<string> GetEnabledChecks(string language, string project)
        {

            return GetChecksEnabledForLanguage(language).Union(GetChecksEnabledForProject(project));
        }

        /// <summary>
        /// Gets a collection of all checks that are enabled for the specified project.
        /// The logic is:
        /// <para/>If a check is enabled for the specified project, it is added to the set.
        /// </summary>
        /// <param name="project">Project for which to search for enabled checks.</param>
        /// <returns>A list of checks that should be run on the specified project.</returns>
        public IEnumerable<string> GetChecksEnabledForProject(string project)
        {
            return from projectCheckRow in ProjectChecks
                   join projectRow in Projects on projectCheckRow.ProjectID equals projectRow.ID 
                   where projectRow.Name.Equals(project)
                   join checkRow in Checks on projectCheckRow.CheckID equals checkRow.ID
                   select checkRow.Name;
        }

        /// <summary>
        /// Gets a collection of all checks that are enabled for the specified language.
        /// The logic is:
        /// <para/>If a check is enabled for the specified language, it is added to returned set.
        /// </summary>
        /// <param name="language">Language for which to search for enabled checks.</param>
        /// <returns>A list of checks that should be run on all projects in the specified language.</returns>
        public IEnumerable<string> GetChecksEnabledForLanguage(string language)
        {
            return from languageCheckRow in LanguageChecks
                   join languageRow in Languages on languageCheckRow.LanguageID equals languageRow.ID
                   where languageRow.Name.Equals(language)
                   join checkRow in Checks on languageCheckRow.CheckID equals checkRow.ID
                   select checkRow.Name;
        }

        /// <summary>
        /// Gets a collection of all languages for which the specified check is enabled.
        /// </summary>
        /// <param name="checkName">Name of the check.</param>
        /// <returns>Collection of languages for which the specified check is enabled.</returns>
        public IEnumerable<string> GetLanguagesWithCheckEnabled(string checkName)
        {
            return from checkRow in Checks
                   where checkRow.Name.Equals(checkName)
                   join languageCheckRow in LanguageChecks on checkRow.ID equals languageCheckRow.CheckID
                   join languageRow in Languages on languageCheckRow.LanguageID equals languageRow.ID
                   select languageRow.Name;
        }

        /// <summary>
        /// Gets a collection of all projects for which the specified check is enabled.
        /// </summary>
        /// <param name="checkName">Name of the check.</param>
        /// <returns>Collection of projects for which the specified check is enabled.</returns>
        public IEnumerable<string> GetProjectsWithCheckEnabled(string checkName)
        {
            return from checkRow in Checks
                   where checkRow.Name.Equals(checkName)
                   join projectCheckRow in ProjectChecks on checkRow.ID equals projectCheckRow.CheckID
                   join projectRow in Projects on projectCheckRow.ProjectID equals projectRow.ID
                   select projectRow.Name;
        }

        /// <summary>
        /// Answers the question: Is this check enabled for every language defined in this database.
        /// </summary>
        /// <param name="checkName">Name of the check</param>
        /// <returns>False if there is at least 1 language, for which the check is not enabled. True otherwise.</returns>
        public bool IsCheckGlobalForAllLanguages(string checkName)
        {
            var languagesOnWhichCheckIsEnabled = from checkRow in Checks
                                                 where checkRow.Name == checkName
                                                 join languageCheckRow in LanguageChecks on checkRow.ID equals languageCheckRow.CheckID
                                                 join languageRow in Languages on languageCheckRow.LanguageID equals languageRow.ID
                                                 select languageRow;
            return languagesOnWhichCheckIsEnabled.Count() == GetAllLanguages().Count();
        }

        /// <summary>
        /// Answers the question: Is this check enabled for every project defined in this database.
        /// </summary>
        /// <param name="checkName">Name of the check</param>
        /// <returns>False if there is at least 1 project, for which the check is not enabled. True otherwise.</returns>
        public bool IsCheckGlobalForAllProjects(string checkName)
        {
            var projectsOnWhichCheckIsEnabled = from checkRow in Checks
                                                where checkRow.Name == checkName
                                                join projectCheckRow in ProjectChecks on checkRow.ID equals projectCheckRow.CheckID
                                                join projectRow in Projects on projectCheckRow.ProjectID equals projectRow.ID
                                                select projectRow;
            return projectsOnWhichCheckIsEnabled.Count() == GetAllProjects().Count();
        }

        /// <summary>
        /// Gets file path of the check that is stored in the database.
        /// If there is no check with the specified name, returns empty string.
        /// </summary>
        /// <param name="checkName">Name of the check</param>
        /// <returns>Path of disk file containing the source code of the check.</returns>
        public string GetCheckFilePath(string checkName)
        {
            return (from checkRow in Checks
                    where checkRow.Name == checkName
                    select checkRow.FilePath).FirstOrDefault();
        }


        /// <summary>
        /// Gets a collection of all languages which have defined the specified tag.
        /// </summary>
        /// <param name="tagName">Tag name.</param>
        /// <returns>Collection of languages for which an association with the specified tag exists.</returns>
        public IEnumerable<string> GetTagLanguages(string tagName)
        {
            return from tag in Tags
                   where tag.Tag == tagName
                   join languageTag in LanguageTags on tag.ID equals languageTag.TagID
                   join language in Languages on languageTag.LanguageID equals language.ID
                   select language.Name;
        }

    }
}
