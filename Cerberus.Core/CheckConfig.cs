using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Collections.ObjectModel;
using Microsoft.Localization.OSLEBot.Core.Engine;

namespace Microsoft.Localization.LocSolutions.Cerberus.Core
{
    /// <summary>
    /// Complete information about a check, including physical file location, and the execution condition.
    /// </summary>
    public class CheckConfig
    {
        /// <summary>
        /// Default constructor of the check configuration entry.
        /// </summary>
        /// <param name="name">Friendly name of the check. This should be the name of source file (*.cs) with extension (.cs) stripped off.</param>
        /// <param name="pathToFile">Path to physical file containing source code of this check. This can and should (if possible) contain environment variables.</param>
        /// <param name="allLanguages">A flag that specifies if the check is enabled for all known languages.</param>
        /// <param name="languages">A collection of languages for which the check will be executed.</param>
        /// <param name="allProjects">A flag that specifies if the check is enabled for all known projects.</param>
        /// <param name="projects">A collection of projects for which the check will be executed</param>
        /// <param name="containerType">Specifies whether the check is stored in a .cs file that needs to be compiled or if it is a compiled module (.dll).</param>
        [CLSCompliant(false)]
        public CheckConfig(string name, string pathToFile, bool allLanguages, IList<CultureInfo> languages, bool allProjects, IList<string> projects, RuleContainerType containerType)
        {
            if (name == null) throw new ArgumentNullException("name");
            if (pathToFile == null) throw new ArgumentNullException("pathToFile");

            Name = name;
            PhysicalFile = pathToFile;
            IsEnabledForAllLanguages = allLanguages;
            Languages = new ReadOnlyCollection<CultureInfo>(languages ?? new CultureInfo[0]);
            IsEnabledForAllProjects = allProjects;
            Projects = new ReadOnlyCollection<string>(projects ?? new string[0]);
            ContainerType = containerType;
        }

        /// <summary>
        /// Friendly name
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Physical location of the file
        /// </summary>
        public string PhysicalFile { get; private set; }

        /// <summary>
        /// Specifies if the check is enabled for all known languages.
        /// </summary>
        public bool IsEnabledForAllLanguages { get; private set; }

        /// <summary>
        /// Specifies if the check is enabled for all known projects.
        /// </summary>
        public bool IsEnabledForAllProjects { get; private set; }

        /// <summary>
        /// A collection of languages for which the check will be executed.
        /// </summary>
        public ReadOnlyCollection<CultureInfo> Languages { get; private set; }

        /// <summary>
        /// A collection of projects for which the check will be executed.
        /// </summary>
        public ReadOnlyCollection<string> Projects { get; private set; }

        /// <summary>
        /// Specifies whether the check is stored in a .cs file that needs to be compiled or if it is a compiled module.
        /// </summary>
        [CLSCompliant(false)]
        public RuleContainerType ContainerType { get; private set; }

        /// <summary>
        /// True means that check is disabled for all languages or for all projects (or perhaps for all other dimensions, in the future).
        /// Essentially that means that the check will never execute. This property is meant to be used when building execution config so the check
        /// is not even loaded into the OSLEBot execution.
        /// </summary>
        public bool IsGloballyDisabled
        {
            get
            {
                return (!IsEnabledForAllLanguages && Languages.Count == 0)
                        || (!IsEnabledForAllProjects && Projects.Count == 0);
            }
        }

        /// <summary>
        /// Gets an OSLEBot Rule filtering expression. This is a string representing a lambda expression of type (LocResource lr => bool)
        /// that will be used by OSLEBot to determine under what conditions check should be executed.
        /// This expression combines the Languages and Projects properties to be used internally by OSLEBot. When new CheckConfig properties are added, this code has to be udpated.
        /// </summary>
        /// <returns>String to be compiled by OSLEBot into a filtering expression. Empty string if no filtering expression is needed.</returns>
        public string GetOSLEBotFilteringExpression()
        {
            if (IsGloballyDisabled) return "false";
            // contains filtering expressions that are to be joined using AND operator
            var filteringStrings = new List<string>();
            // create language filter expression
            if (!IsEnabledForAllLanguages)
            {
                filteringStrings.Add(String.Format("({0})",
                    String.Join(" or ", Languages.Select(lang => String.Format("TargetCulture.Value.BuiltinCultureInfo.LCID == {0}", lang.LCID)).ToArray())
                    ));
            }
            // create project filter expression
            if (!IsEnabledForAllProjects)
            {
                filteringStrings.Add(String.Format("({0})",
                    //the below does not compile correctly into a lambda expression - StringComparison cannot be resolved by lambda expr parser
                    //using an inefficient tolower approach
                    //String.Join(" or ", Projects.Select(proj => String.Format("Project.Value.Equals({0}, StringComparison.OrdinalIgnoreCase)", proj)).ToArray())
                    String.Join(" or ", Projects.Select(proj => String.Format("Project.Value.ToLower().Equals(\"{0}\")", proj.ToLower())).ToArray())
                    ));
            }
            return String.Join(" and ", filteringStrings.Where(s => !String.IsNullOrEmpty(s)).ToArray());
        }
    }
}
