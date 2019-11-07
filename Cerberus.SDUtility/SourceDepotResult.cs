using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using SourceDepotClient;
using Microsoft.OffGlobe.SourceDepot;

namespace Microsoft.Localization.LocSolutions.Cerberus.SDUtility
{
    /// <summary>
    /// Further translation of Source Depot synchronization result.
    /// </summary>
    public enum SourceDepotResult
    {
        /// <summary>
        /// General error.
        /// </summary>
        Error,
        /// <summary>
        /// Syncing was actually successful.
        /// </summary>
        Success,
        /// <summary>
        /// Attempt to sync an invalid set of files (not in enlistment file-set) has failed. Probably an incorrect language name was specified.
        /// </summary>
        NoSuchFiles,
        /// <summary>
        /// Unhandled kind of error. This indicates a problem in the exe's logic. Please log the bug against arturp@microsoft.com.
        /// </summary>
        UnspecifiedError
    }
}
