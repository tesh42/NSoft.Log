using System;
using System.Collections.Generic;

namespace NSoft.Log.Core
{
    /// <summary>
    /// Provides a manager that is used for writing events to the log.
    /// </summary>
    public interface ILogManager
    {
        /// <summary>
        /// Occurs when a record is failed  to store by current writer.
        /// </summary>
        event EventHandler<WriteFailedEventArgs> WriteFailed;

        /// <summary>
        /// Writes data to the specific channel.
        /// </summary>
        /// <param name="channelName">Name of the channel.</param>
        /// <param name="data">Information about event.</param>
        void Write(string channelName, params string[] data);

        /// <summary>
        /// Writes information about events.
        /// </summary>
        /// <param name="records">Information about events.</param>
        void Write(IEnumerable<LogRecord> records);
    }
}
