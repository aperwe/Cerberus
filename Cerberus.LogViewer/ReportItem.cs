using System.Linq;
using System.Collections;
using System.Data;
using System;

namespace Microsoft.Localization.LocSolutions.Cerberus.LogViewer
{
    /// <summary>
    /// A single item extracted from OSLEBot report
    /// </summary>
    public class ReportItem
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public ReportItem() { }

        /// <summary>
        /// File name.
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Resource ID
        /// </summary>
        public string ResourceID { get; set; }

        /// <summary>
        /// Name of the check.
        /// </summary>
        public string CheckName { get; set; }

        /// <summary>
        /// Language of the loc item.
        /// </summary>
        public string Language { get; set; }

        /// <summary>
        /// Project the loc item belongs to.
        /// </summary>
        public string Project { get; set; }

        /// <summary>
        /// Locgroup the loc item belongs to.
        /// </summary>
        public string Locgroup { get; set; }

        /// <summary>
        /// Comments of the loc item.
        /// </summary>
        public string Comments { get; set; }

        /// <summary>
        /// Source string of the loc item.
        /// </summary>
        public string SourceString { get; set; }

        /// <summary>
        /// Target string of the loc item.
        /// </summary>
        public string TargetString { get; set; }

        /// <summary>
        /// Message produced by the check.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Severity of the check.
        /// </summary>
        public string Severity { get; set; }
    }
}
