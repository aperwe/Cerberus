using System;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;
using Microsoft.Localization.LocSolutions.Cerberus.Configurator.Properties;
using Microsoft.Localization.LocSolutions.Logger;
using System.Linq;

namespace Microsoft.Localization.LocSolutions.Cerberus.Configurator
{
    /// <summary>
    /// View type that is currently active in the main window.
    /// </summary>
    public enum UserView
    {
        /// <summary>
        /// Default view for standard users.
        /// </summary>
        Standard,
        /// <summary>
        /// Advanced view that can be activated through the menu.
        /// </summary>
        Advanced
    }
}
