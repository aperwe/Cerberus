using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Xml.Linq;
using System.Collections;
using Microsoft.Localization.Lcx;

namespace Microsoft.Localization.LocSolutions.Tools.LcxOmMemoryChecker
{
    public class LcxLoaderEventData : EventArgs
    {
        /// <summary>
        /// Data items.
        /// </summary>
        public IEnumerable Data { get; set; }

        /// <summary>
        /// Name of the file from which <see cref="Data"/> was loaded.
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        public LcxLoaderEventData()
        {
        }
    }
}
