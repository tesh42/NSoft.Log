using System;
using System.Collections.Generic;

namespace NSoft.Log.Core
{
    /// <summary>
    /// Provides interaction with data store.
    /// </summary>
    public interface ILogWriter
    {
        /// <summary>
        /// Unique identifier.
        /// </summary>
        int Id { get; }

        /// <summary>
        /// Current state.
        /// </summary>
        LogWriterState State { get; }

        /// <summary>
        /// Occurs when the state is changed.
        /// </summary>
        event EventHandler<LogWriterStateChangedEventArgs> StateChanged;

        /// <summary>
        /// Enables writing to the channel.
        /// </summary>
        void Enable();

        /// <summary>
        /// Disables writing to the channel.
        /// </summary>
        void Disable();

        /// <summary>
        /// Disables writing to the channel.
        /// </summary>
        /// <param name="milliseconds">Duration of the suspending. Measured in milliseconds.</param>
        void Disable(int milliseconds);

        /// <summary>
        /// Registers the channel.
        /// </summary>
        /// <param name="channelName">Name of the channel.</param>
        void RegisterChannel(string channelName);

        /// <summary>
        /// Writes data to a channel.
        /// </summary>
        /// <param name="channelName">Name of the channel.</param>
        /// <param name="data">The data to be written.</param>
        void Write(string channelName, params string[] data);

        /// <summary>
        /// Writes information about events.
        /// </summary>
        /// <param name="records">Information about events.</param>
        void Write(IEnumerable<LogRecord> records);
    }
}
