using System;

namespace NSoft.Log.Core
{
    /// <summary>
    /// Contains information about the event "Write failed".
    /// </summary>
    public class WriteFailedEventArgs : EventArgs
    {
        /// <summary>
        /// Information about the error.
        /// </summary>
        public Exception Error { get; private set; }

        /// <summary>
        /// Indicates whether the error is fatal.
        /// If error isn't fatal, then next writer will be chosen.
        /// </summary>
        public bool IsFatalError { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="WriteFailedEventArgs"/> class.
        /// </summary>
        /// <param name="error">Information about the error.</param>
        /// <param name="isFatalError">Indicates whether the error is fatal.</param>
        public WriteFailedEventArgs(Exception error, bool isFatalError)
        {
            Error = error;
            IsFatalError = isFatalError;
        }
    }
}