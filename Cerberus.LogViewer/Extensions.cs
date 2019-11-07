using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Threading;
using System.Windows.Forms;

namespace Microsoft.Localization.LocSolutions.Cerberus.LogViewer
{
    /// <summary>
    /// C# extensions (helper methods) used in this project.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Invokes a method invoker on the specified dispatcher.
        /// </summary>
        /// <param name="me">Dispatcher on UI thread.</param>
        /// <param name="method">Method to be invoked.</param>
        /// <param name="args">Optional arguments to be passed into the method.</param>
        /// <returns>Some return value from Dispather.Invoker(Delegate, ...)</returns>
        public static object Invoke(this Dispatcher me, MethodInvoker method, params object[] args)
        {
            return me.Invoke(method, args);
        }
    }
}
