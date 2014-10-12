using System;

namespace NSoft.Log.Core
{
    /// <summary>
    /// Provides information about changed state.
    /// </summary>
    public class LogWriterStateChangedEventArgs : EventArgs
    {
        /// <summary>
        /// New state.
        /// </summary>
        public LogWriterState NewState { get; private set; }

        /// <summary>
        /// Old state.
        /// </summary>
        public LogWriterState OldState { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LogWriterStateChangedEventArgs"/> class.
        /// </summary>
        /// <param name="oldState">The old state.</param>
        /// <param name="newState">The new state.</param>
        public LogWriterStateChangedEventArgs(LogWriterState oldState, LogWriterState newState)
        {
            OldState = oldState;
            NewState = newState;
        }
    }
}