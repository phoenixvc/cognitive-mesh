using System;
using System.Runtime.Serialization;

namespace CognitiveMesh.FoundationLayer.AuditLogging.Exceptions
{
    /// <summary>
    /// Represents errors that occur during audit logging operations.
    /// </summary>
    [Serializable]
    public class AuditLoggingException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AuditLoggingException"/> class.
        /// </summary>
        public AuditLoggingException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AuditLoggingException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public AuditLoggingException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AuditLoggingException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
        public AuditLoggingException(string message, Exception innerException) : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AuditLoggingException"/> class with serialized data.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information about the source or destination.</param>
        protected AuditLoggingException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
