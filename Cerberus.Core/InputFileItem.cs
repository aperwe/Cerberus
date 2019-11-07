namespace Microsoft.Localization.LocSolutions.Cerberus.Core
{
    /// <summary>
    /// File information about a single input file to be processed (LCL file).
    /// Contains metainformation about this file that allow construction of proper OSLEBot config.
    /// <para/>Compare with <seealso cref="ConfigItem"/> class. This is a trimmed-down version, because it requires less information than <seealso cref="ConfigItem"/>.
    /// </summary>
    public class InputFileItem
    {
        /// <summary>
        /// Identifies project (application) for which this check is defined.
        /// <para/>This is 2nd level tag in the hierarchy as defined by customers.
        /// </summary>
        public string Project { get; set; }
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
        /// Build number associated with the file.
        /// </summary>
        public string BuildNumber { get; set; }
    }
}