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
using System.Globalization;

namespace Microsoft.Localization.LocSolutions.Tools.LcxOmMemoryChecker
{
    /// <summary>
    /// Contains information about the Lcx file loaded.
    /// </summary>
    public class LcxFile
    {
        public IEnumerable Data { get; set; }

        /// <summary>
        /// Event message type.
        /// </summary>
        public MessageType Type { get; set; }

        /// <summary>
        /// Current working set size at the time this item has been constructed.
        /// </summary>
        public long WorkingSet { get; set; }

        /// <summary>
        /// Size expressed as a string.
        /// </summary>
        public string WorkingSetString
        {
            get
            {
                if (WorkingSet > (long)Sizes.Terabyte)
                {
                    var size = (WorkingSet / (double)(long)Sizes.Terabyte);
                    return string.Format(CultureInfo.CurrentUICulture, "{0:n} TB", size);
                }
                if (WorkingSet > (long)Sizes.Gigabyte)
                {
                    var size = (WorkingSet / (double)(long)Sizes.Gigabyte);
                    return string.Format(CultureInfo.CurrentUICulture, "{0:n} GB", size);
                }
                if (WorkingSet > (long)Sizes.Megabyte)
                {
                    var size = (WorkingSet / (double)(long)Sizes.Megabyte);
                    return string.Format(CultureInfo.CurrentUICulture, "{0:n} MB", size);
                }
                if (WorkingSet > (long)Sizes.Kilobyte)
                {
                    var size = (WorkingSet / (double)(long)Sizes.Kilobyte);
                    return string.Format(CultureInfo.CurrentUICulture, "{0:n} KB", size);
                }
                return string.Format(CultureInfo.CurrentUICulture, "{0:n} GB", WorkingSet);
            }
        }

        /// <summary>
        /// A description of the file. Most often this is the file path.
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        public LcxFile()
        {
        }
    }

    /// <summary>
    /// Sizes for string formatting.
    /// </summary>
    public enum Sizes : long
    {
        Kilobyte = 1024,
        Megabyte = Kilobyte * 1024,
        Gigabyte = Megabyte * 1024,
        Terabyte = Gigabyte * 1024,
    }
}
