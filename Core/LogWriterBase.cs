using System;
using System.Collections.Generic;
using System.Threading;

namespace NSoft.Log.Core
{
    /// <summary>
    /// Provides base functionality to all writers.
    /// </summary>
    public abstract class LogWriterBase : BasicDisposable, ILogWriter
    {
        /// <summary>
        /// Timer of suspension.
        /// </summary>
        readonly Timer enableTimer;

        /// <summary>
        /// Current state.
        /// </summary>
        LogWriterState state;

        /// <summary>
        /// Unique identifier.
        /// </summary>
        public int Id { get; private set; }

        /// <summary>
        /// Current state.
        /// </summary>
        public LogWriterState State
        {
            get { return state; }

            private set 
            { 
                if (state == value)
                    return;
                var oldState = state;
                state = value;
                OnStateChanged(oldState, value);
            }
        }

        /// <summary>
        /// Occurs when the state is changed.
        /// </summary>
        public event EventHandler<LogWriterStateChangedEventArgs> StateChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="LogWriterBase"/> class.
        /// </summary>
        /// <param name="id">Unique identifier.</param>
        protected LogWriterBase(int id)
        {
            Id = id;
            state = LogWriterState.Enabled;
            enableTimer = new Timer(EnableTimerCallback, null, Timeout.Infinite, Timeout.Infinite);
        }

        public void Enable()
        {
            if (State == LogWriterState.Disabled)
                State = LogWriterState.Enabled;
        }

        public void Disable()
        {
            State = LogWriterState.Disabled;
        }

        public void Disable(int milliseconds)
        {
            State = LogWriterState.Disabled;
            enableTimer.Change(milliseconds, Timeout.Infinite);
        }

        public virtual void RegisterChannel(string channelName)
        {
        }

        public abstract void Write(string channelName, params string[] data);

        public virtual void Write(IEnumerable<LogRecord> records)
        {
            foreach (var record in records)
                Write(record.ChannelName, record.Data);
        }

        void OnStateChanged(LogWriterState oldState, LogWriterState newState)
        {
            var eventArgs = new LogWriterStateChangedEventArgs(oldState, newState);
            var handler = StateChanged;
            if (handler != null)
                handler(this, eventArgs);
        }

        /// <summary>
        /// Handles the suspension timer's event.
        /// </summary>
        void EnableTimerCallback(object state)
        {
            if (State == LogWriterState.Disabled)
                State = LogWriterState.Enabled;
        }

        /// <summary>
        /// Validates the record that will be written.
        /// </summary>
        /// <param name="channelName">Name of the channel.</param>
        /// <param name="data">The data to be written.</param>
        /// <returns><c>true</c> if record is valid; overwise, false.</returns>
        protected virtual bool IsValidRecord(string channelName, params string[] data)
        {
            return !(string.IsNullOrEmpty(channelName) || data == null || data.Length == 0);
        }

        /// <summary>
        /// Validates the record that will be written.
        /// </summary>
        /// <param name="record">The record to be written.</param>
        /// <returns><c>true</c> if record is valid; overwise, false.</returns>
        protected virtual bool IsValidRecord(LogRecord record)
        {
            return record != null && IsValidRecord(record.ChannelName, record.Data);
        }

        protected override void DisposeManaged()
        {
            enableTimer.Dispose();
        }
    }
}
