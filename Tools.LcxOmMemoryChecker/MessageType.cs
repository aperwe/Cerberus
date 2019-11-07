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
using System.Collections;

namespace Microsoft.Localization.LocSolutions.Tools.LcxOmMemoryChecker
{
    /// <summary>
    /// Type of event message.
    /// </summary>
    public enum MessageType
    {
        /// <summary>
        /// Loading of the response file has started.
        /// </summary>
        Started,

        /// <summary>
        /// Another file has been loaded.
        /// </summary>
        File,

        /// <summary>
        /// Loading of all LCX files in the response file has been finished.
        /// </summary>
        Finished

    }
}
