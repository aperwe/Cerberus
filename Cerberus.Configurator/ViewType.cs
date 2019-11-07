namespace Microsoft.Localization.LocSolutions.Cerberus.Configurator
{
    /// <summary>
    /// Possible types of view that Cerberus.Configurator is using.
    /// </summary>
    internal enum ViewType
    {
        /// <summary>
        /// Default view. Shows Languages as root items, under them shows projects.
        /// </summary>
        ViewOnEnlistment,
        /// <summary>
        /// View that shows checks in a flat list and allows assigning tags to checks.
        /// </summary>
        ViewOnChecks,
        /// <summary>
        /// View that shows checks in a flat list and allows assigning tags to checks;
        /// also allows for filtering the checks based on the filter text box.
        /// </summary>
        ViewOnChecksWithFilter,
        /// <summary>
        /// Variant of the default view (<see cref="ViewOnEnlistment"/>) where only projects are shown, without their parent languages
        /// </summary>
        ViewOnProjects,
        /// <summary>
        /// View type that allows enabling checks to languages or projects.
        /// </summary>
        CheckAssignment,
        /// <summary>
        /// View type that shows to the user what enlistment items each check is configured on.
        /// Check is the data selector, and languages and projects on which the selected check is enabled, are shown in secondary view (list or tree).
        /// </summary>
        ConfigurationSummary,
        /// <summary>
        /// View that allows typing a new tag name and apply the tag to selection on the parent view.
        /// </summary>
        TagEditorView,

        /// <summary>
        /// New version of the main view where checks can be assigned to languages and projects.
        /// List of checks should be on the left.
        /// List of languages (with checkboxes) should be on the right at the top.
        /// List of projects (with checkboxes) should be on the right at the bottom.
        /// </summary>
        NewCheckAssignment,

        /// <summary>
        /// New view where languages can be assigned to language groups, such as EA, Complex script, etc.
        /// This view shows language tags (groups) in one list, and on the second list all languages with checkboxes will be present
        /// and the user can select checkboxes for each language to assign/unassign it to/from language group.
        /// </summary>
        LanguageGroups,

        /// <summary>
        /// New view (advanced) where the user can click through languages and language groups to see what checks
        /// are assigned to the selection.
        /// </summary>
        NewSummaryView,

        /// <summary>
        /// Allows adding and removing checks from the configuration.
        /// </summary>
        CheckEditor,
    }
}