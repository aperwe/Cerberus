namespace Microsoft.Localization.LocSolutions.Cerberus.Core
{
    /// <summary>
    /// File information about a single file in enlistment.
    /// Contains information about project, LocGroup, language that this file belongs to.
    /// </summary>
    public class ConfigItem
    {
        /// <summary>
        /// Identifies project (application) for which this check is defined.
        /// <para/>This is 2nd level tag in the hierarchy as defined by customers.
        /// </summary>
        public string Project { get; set; }
        /// <summary>
        /// Identifies language for which this check is defined.
        /// <para/>This is a top level tag in the hierarchy as defined by customers.
        /// </summary>
        public string Language { get; set; }
        /// <summary>
        /// Identifies LocGroup (project component) for which this check is defined.
        /// <para/>This is 3rd level tag in the hierarchy as defined by customers.
        /// </summary>
        public string LocGroup { get; set; }
        /// <summary>
        /// Identifies File (individual LCL file in a LocGroup) for which this check is defined.
        /// <para/>This is 4th level tag in the hierarchy as defined by customers.
        /// </summary>
        public string File { get; set; }
        /// <summary>
        /// Absolute path to the file on disk.
        /// </summary>
        public string PhysicalPath { get; set; }
        /// <summary>
        /// Build number associated with the file.
        /// </summary>
        public string BuildNumber { get; set; }
    }
}