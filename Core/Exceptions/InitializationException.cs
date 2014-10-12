using System;

namespace NSoft.Log.Core.Exceptions
{
    /// <summary>
    /// Exception that is thrown when initialization error occurs.
    /// </summary>
    [Serializable]
    public class InitializationException : ApplicationException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InitializationException"/> class.
        /// </summary>
        public InitializationException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InitializationException"/> class.
        /// </summary>
        /// <param name="message">A message that describes the error.</param>
        public InitializationException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InitializationException"/> class.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="args">The arguments.</param>
        public InitializationException(string format, params object[] args) : this(string.Format(format, args))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InitializationException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception. If the <paramref name="innerException" /> parameter is not a null reference, the current exception is raised in a catch block that handles the inner exception.</param>
        public InitializationException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
