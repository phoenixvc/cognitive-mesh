namespace FoundationLayer.Infrastructure.Exceptions
{
    /// <summary>
    /// Exception thrown when an error occurs in the PolicyRepository.
    /// </summary>
    public class PolicyRepositoryException : Exception
    {
        public PolicyRepositoryException()
        {
        }

        public PolicyRepositoryException(string message) : base(message)
        {
        }

        public PolicyRepositoryException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
