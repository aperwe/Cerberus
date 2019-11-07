using System;
using Microsoft.Localization.LocSolutions.Cerberus.Core;
using System.Runtime.Serialization;

namespace Microsoft.Localization.LocSolutions.Cerberus.SDUtility
{
    /// <summary>
    /// Exception thrown when a caller checks if files are opened in the enlistment and finds that there are files opened.
    /// The caller can then handle this event by displaying a message to the user that files should not be opened in their enlistment.
    /// <para/>This exception is used as a program-flow control mechanism.
    /// </summary>
    [Serializable]
    public class FilesOpenedException : EnlistmentException
    {
        /// <summary>
        /// Initializes a new instance of the FilesOpenedException class.
        /// </summary>
        public FilesOpenedException() : base() { }
        /// <summary>
        /// Initializes a new instance of the System.Exception class with a specified error message.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception</param>
        public FilesOpenedException(string message) : base(message) { }
        /// <summary>
        /// Initializes a new instance of the System.Exception class with a specified error message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference if no inner exception is specified</param>
        public FilesOpenedException(string message, Exception innerException) : base(message, innerException) { }
        /// <summary>
        /// Initializes a new instance of the System.Exception class with serialized data.
        /// </summary>
        /// <param name="info">The SerializationInfo that holds the serialized object data about the exception being thrown</param>
        /// <param name="context">The StreamingContext that contains contextual information about the source or destination</param>
        protected FilesOpenedException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }

    /// <summary>
    /// Exception thrown when a caller validates the state of the enlistment and it is found that the staging depot either doesn't exist or is in invalid/inconsistent state.
    /// The caller can then handle this event by returning an error message to its parent method.
    /// <para/>This exception is used as a program-flow control mechanism.
    /// </summary>
    [Serializable]
    public class IncorrectSDStateException : EnlistmentException
    {
        /// <summary>
        /// Indicates what kind of problem was detected when analysing results from SD sync operation.
        /// This result can be directly returned to the parent method of the caller.
        /// </summary>
        public ExecutionResult ProblemCode { get; set; }

        /// <summary>
        /// Initializes a new instance of the SDResultsWithErrorException class.
        /// </summary>
        public IncorrectSDStateException() : base() { }
        /// <summary>
        /// Initializes a new instance of the System.Exception class with a specified error message.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception</param>
        public IncorrectSDStateException(string message) : base(message) { }
        /// <summary>
        /// Initializes a new instance of the System.Exception class with a specified error message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference if no inner exception is specified</param>
        public IncorrectSDStateException(string message, Exception innerException) : base(message, innerException) { }
        /// <summary>
        /// Initializes a new instance of the System.Exception class with serialized data.
        /// </summary>
        /// <param name="info">The SerializationInfo that holds the serialized object data about the exception being thrown</param>
        /// <param name="context">The StreamingContext that contains contextual information about the source or destination</param>
        protected IncorrectSDStateException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
