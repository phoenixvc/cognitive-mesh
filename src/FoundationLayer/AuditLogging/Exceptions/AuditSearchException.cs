using System;

namespace CognitiveMesh.FoundationLayer.AuditLogging.Exceptions
{
    /// <summary>
    /// The exception that is thrown when an audit search operation fails.
    /// </summary>
    public class AuditSearchException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AuditSearchException"/> class.
        /// </summary>
        public AuditSearchException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AuditSearchException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public AuditSearchException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AuditSearchException"/> class with a specified error message
        /// and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public AuditSearchException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
