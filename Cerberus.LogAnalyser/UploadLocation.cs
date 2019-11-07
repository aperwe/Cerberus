using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cerberus.LogAnalyser
{
    /// <summary>
    /// UploadLocation is what gets done to the file
    /// </summary>
    public enum UploadLocation
    {
        Local,
        FastTrack,
        CleanUp,
        BackUp,
        Help,
        FastTrackSingle,
        /// <summary>
        /// Same as help but this is set when invalid arguments are specified on command line.
        /// </summary>
        ShowHelpOnBadArguments
    }
}
