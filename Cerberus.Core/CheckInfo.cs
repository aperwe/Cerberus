namespace Microsoft.Localization.LocSolutions.Cerberus.Core
{
    /// <summary>
    /// Complete information about a single check.
    /// </summary>
    public class CheckInfo
    {
        /// <summary>
        /// Name of the check. This is generated from the file name, but the extension is stripped.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// This is the source code of the check.
        /// </summary>
        public string SourceCode { get; set; }

        /// <summary>
        /// This is the description of the check - provided by user. By default it is "New Check".
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Path to physical location on disk. This could be an absolute file location (not recommended),
        /// or a location dependent on environment variables (recommended). For example: "%OTOOLS%\bin\intl\Cerberus\Checks\CheckName.cs".
        /// </summary>
        public string FilePath { get; set; }
    }
}