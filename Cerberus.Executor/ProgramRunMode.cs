namespace Microsoft.Localization.LocSolutions.Cerberus.Executor
{
    /// <summary>
    /// Indicates execution mode selected by the user of Cerberus.Executor.
    /// </summary>
    public enum ProgramRunMode
    {
        /// <summary>
        /// Default mode, means running in standalone mode (command-line driven), where 
        /// OSLEBot engine is instantiated separately for each locgroup.
        /// </summary>
        StandaloneLocGroups,
        /// <summary>
        /// Similar to the default mode, but
        /// OSLEBot engine is instantiated separately for each project (less granular).
        /// </summary>
        StandaloneProjects,
        /// <summary>
        /// Information about which checks are enabled for each {language, project} is obtained from a database
        /// that is at the heart of Cerberus. Database is configured via the GUI tool: Cerberus.Configurator.
        /// </summary>
        DatabaseCentralized,
        /// <summary>
        /// Either -h or /h or ? was specified. The program should show usage info and quit.
        /// </summary>
        ShowHelpAndQuit
    }
}