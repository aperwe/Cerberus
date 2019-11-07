using System;
using System.Windows.Controls;
using Microsoft.Localization.LocSolutions.Cerberus.Core;

namespace Microsoft.Localization.LocSolutions.Cerberus.Configurator
{
    /// <summary>
    /// Contains methods for creating different views, provided the specified view type.
    /// </summary>
    internal static class ViewFactory
    {
        /// <summary>
        /// Creates the specified view on data.
        /// </summary>
        /// <param name="viewType">Requested type of view to create.</param>
        /// <param name="container">Container that will hold the view in display.</param>
        /// <param name="database">Database that feeds the contents of the view.</param>
        /// <returns>Initialized view of the specified type.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the specified view type is not supported by this factory.</exception>
        public static ViewHandler CreateView(ViewType viewType, Grid container, CheckConfiguration database)
        {
            switch (viewType)
            {
                case ViewType.ViewOnEnlistment: return new ViewOnEnlistmentHandler(container, database);
                case ViewType.ViewOnChecks: return new ViewOnChecksHandler(container, database);
                case ViewType.ViewOnChecksWithFilter: return new ViewOnChecksWithFilterHandler(container, database);
                case ViewType.ViewOnProjects: return new ViewOnProjectsHandler(container, database);
                case ViewType.CheckAssignment: return new CheckAssignmentViewHandler(container, database);
                case ViewType.ConfigurationSummary: return new ConfigurationSummaryViewHandler(container, database);
                case ViewType.TagEditorView: return new TagEditorViewHandler(container, database);
                case ViewType.NewCheckAssignment: return new MainCheckConfigurationViewHandler(container, database);
                case ViewType.LanguageGroups: return new LanguageGroupsViewHandler(container, database);
                case ViewType.NewSummaryView: return new NewSummaryViewHandler(container, database);
                case ViewType.CheckEditor: return new CheckEditorViewHandler(container, database);
                default: return new NotImplementedViewHandler(container, database);
            }
        }
    }
}
