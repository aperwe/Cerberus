using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;

namespace Microsoft.OffGlobe.SourceDepot
{
    [Serializable]
    public class SourceDepotException : Exception
    {
        public SourceDepotException() { }
        public SourceDepotException(string message) : base(message) { }
        public SourceDepotException(string message, Exception innerException) : base(message, innerException) { }
        protected SourceDepotException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
