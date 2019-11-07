using System;
using System.Runtime.Serialization;

namespace Microsoft.Localization.LocSolutions.Cerberus.Core
{
    /// <summary>
    /// Base exeption for Cerberus core. All internal specific exeptions must derive from this exception
    /// so that they are organized into a hierarchy.
    /// </summary>
    [Serializable]
    public class CerberusException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the CerberusException class.
        /// </summary>
        public CerberusException() : base() { }
        /// <summary>
        /// Initializes a new instance of the System.Exception class with a specified error message.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception</param>
        public CerberusException(string message) : base(message) { }
        /// <summary>
        /// Initializes a new instance of the System.Exception class with a specified error message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference if no inner exception is specified</param>
        public CerberusException(string message, Exception innerException) : base(message, innerException) { }
        /// <summary>
        /// Initializes a new instance of the System.Exception class with serialized data.
        /// </summary>
        /// <param name="info">The SerializationInfo that holds the serialized object data about the exception being thrown</param>
        /// <param name="context">The StreamingContext that contains contextual information about the source or destination</param>
        protected CerberusException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }

    /// <summary>
    /// Exception thrown when a caller attempts to use Office14Entlistment methods with no enlistment detected.
    /// </summary>
    [Serializable]
    public class EnlistmentException : CerberusException
    {
        /// <summary>
        /// Initializes a new instance of the EnlistmentException class.
        /// </summary>
        public EnlistmentException() : base() { }
        /// <summary>
        /// Initializes a new instance of the System.Exception class with a specified error message.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception</param>
        public EnlistmentException(string message) : base(message) { }
        /// <summary>
        /// Initializes a new instance of the System.Exception class with a specified error message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference if no inner exception is specified</param>
        public EnlistmentException(string message, Exception innerException) : base(message, innerException) { }
        /// <summary>
        /// Initializes a new instance of the System.Exception class with serialized data.
        /// </summary>
        /// <param name="info">The SerializationInfo that holds the serialized object data about the exception being thrown</param>
        /// <param name="context">The StreamingContext that contains contextual information about the source or destination</param>
        protected EnlistmentException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }

}