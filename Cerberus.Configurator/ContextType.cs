using System;

namespace Microsoft.Localization.LocSolutions.Cerberus.Configurator
{
    /// <summary>
    /// Context type determines what items are to be shown in a ListView control in MainCheckConfigurationView.
    /// </summary>
    internal enum ContextType
    {
        /// <summary>
        /// Identifies a language item.
        /// </summary>
        Language,
        /// <summary>
        /// Identifies a project item.
        /// </summary>
        Project,
        /// <summary>
        /// Identifies a language group item.
        /// </summary>
        LanguageGroup
    }
}
