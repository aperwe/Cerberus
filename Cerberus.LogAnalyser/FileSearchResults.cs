using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cerberus.LogAnalyser
{
    /// <summary>
    /// Quick object for holding basic info about a file
    /// </summary>
    public class FileSearchResults
    {
        public string Project { get; set; }
        public string LocGroup { get; set; }
        public string Language { get; set; }
        public string LogLocation { get; set; }

    }
}
