using System;

namespace Microsoft.Localization.LocSolutions.Cerberus.SDUtility
{
    /// <summary>
    /// Standard class that contains an entry point into the program.
    /// </summary>
    class Program
    {
        /// <summary>
        /// Entry point into the program.
        /// </summary>
        /// <param name="args">Command line arguments.</param>
        /// <returns>Return value passed to the operating system.</returns>
        static int Main(string[] args)
        {
            var context = new SDInterface(args);
            var retVal = context.Execute();
            return (int)retVal;
        }
    }
}
