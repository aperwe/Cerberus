using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Localization.LocSolutions.Cerberus.Core
{
    /// <summary>
    /// Base class for wrapper objects around OSLEBot that work without database (they are driven by command line input).
    /// </summary>
    public abstract class StandaloneAsimo : AsimoBase
    {
        /// <summary>
        /// Initializes an instance of Asimo with reference to enlistment to access some of its functionality.
        /// </summary>
        /// <param name="enlistment">Reference to enlistment.</param>
        protected StandaloneAsimo(Office14Enlistment enlistment) : base(enlistment)
        {
        }

        /// <summary>
        /// Selects distinct LocGroup names from the specified collection of items.
        /// </summary>
        protected static IEnumerable<string> GetDistinctLocgroups(IEnumerable<ConfigItem> items)
        {
            return from file in items
                   group file by file.LocGroup
                   into locGroupGroup
                       select locGroupGroup.Key;
        }
    }
}
