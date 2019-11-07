using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.OffGlobe.SourceDepot
{
    public class SourceDepotCommand
    {
        public string Name { get; set; }
        public IList<string> Arguments { get; set; }
    }
}
