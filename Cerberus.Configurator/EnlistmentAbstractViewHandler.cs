using System.Linq;
using System.Windows.Controls;
using Microsoft.Localization.LocSolutions.Cerberus.Core;

namespace Microsoft.Localization.LocSolutions.Cerberus.Configurator
{
    /// <summary>
    /// Base for specific views that look on enlistment (languages and projects).
    /// Contains common methods used by any enlistment-related, derived view,
    /// such as <see cref="ViewOnEnlistmentHandler"/> or <see cref="ViewOnProjectsHandler"/>.
    /// </summary>
    internal abstract class EnlistmentAbstractViewHandler : ViewHandler
    {
        /// <summary>
        /// Tree view created by this handler. This tree view is a child of the view container.
        /// </summary>
        protected TreeView _treeView;

        /// <summary>
        /// Reference to a secondary (child) view that manages tag addition to selecion.
        /// </summary>
        protected TagEditorViewHandler _tagEditorViewHandler;

        /// <summary>
        /// Textbox containing the filtering expression.
        /// </summary>
        protected TextBox _FilterTextBox;

        /// <summary>
        /// Root node of the main tree view control (view on enlistment) in this view.
        /// </summary>
        protected TreeViewItem _rootNode;

        /// <summary>
        /// Gets a reference of the tree view that contains the enlismtment.
        /// </summary>
        internal TreeView EnlistmentTreeView { get { return _treeView; } }

        public EnlistmentAbstractViewHandler(Grid viewContainer, CheckConfiguration configuration) : base(viewContainer, configuration)
        {
        }

        /// <summary>
        /// Returns true if any of language's properties match the filter.
        /// Also returns true if the filter is null or empty.
        /// </summary>
        /// <param name="language">Name of the check to see if it matches the specified filter.</param>
        /// <param name="filterText">Filter text (may be null or empty string - in which case filtering is disabled and true is returned) to see if any of language's properties match the filter.</param>
        /// <returns>True if the specified language matches the filter. False otherwise.</returns>
        protected bool LanguageMatchesFilter(string language, string filterText)
        {
            if (!string.IsNullOrEmpty(filterText))
            {
                var filter = filterText.Trim();
                if (!string.IsNullOrEmpty(filter))
                {
                    var filterItems = filter.Split(' ');
                    if (!filterItems.All(f => AnyLanguagePropertyMatchesFilter(language, f))) return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Determines if any property of the language matches the specific filter item.
        /// </summary>
        /// <param name="language">Language name</param>
        /// <param name="filter">Filter string to compare all language's properties against</param>
        /// <returns>If any property of the specified language matches the filter item, then true is returned. False is returned when none of known language properties matches the filter item.</returns>
        protected bool AnyLanguagePropertyMatchesFilter(string language, string filter)
        {
            if (language.ContainsCaseInsensitive(filter)) return true;
            if (Configuration.GetLanguageTags(language).Any(tag => tag.ContainsCaseInsensitive(filter))) return true;
            if (Configuration.GetAllProjects().Any(project => project.ContainsCaseInsensitive(filter))) return true;
            if (Configuration.GetAllProjects().Any(project => Configuration.GetProjectTags(project).Any(tag => tag.ContainsCaseInsensitive(filter)))) return true;
            return false;
        }

        /// <summary>
        /// Returns true if any of project's properties match the filter.
        /// Also returns true if the filter is null or empty.
        /// </summary>
        /// <param name="project">Name of the check to see if it matches the specified filter.</param>
        /// <param name="filterText">Filter text (may be null or empty string - in which case filtering is disabled and true is returned) to see if any of project's properties match the filter.</param>
        /// <returns>True if the specified project matches the filter. False otherwise.</returns>
        protected bool ProjectMatchesFilter(string project, string filterText)
        {
            if (!string.IsNullOrEmpty(filterText))
            {
                var filter = filterText.Trim();
                if (!string.IsNullOrEmpty(filter))
                {
                    var filterItems = filter.Split(' ');
                    if (!filterItems.All(f => AnyProjectPropertyMatchesFilter(project, f))) return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Determines if any property of the project matches the specific filter item.
        /// </summary>
        /// <param name="project">Language name</param>
        /// <param name="filter">Filter string to compare all project's properties against</param>
        /// <returns>If any property of the specified project matches the filter item, then true is returned. False is returned when none of known project properties matches the filter item.</returns>
        protected bool AnyProjectPropertyMatchesFilter(string project, string filter)
        {
            if (project.ContainsCaseInsensitive(filter)) return true;
            if (Configuration.GetProjectTags(project).Any(tag => tag.ContainsCaseInsensitive(filter))) return true;
            return false;
        }

    }
}
