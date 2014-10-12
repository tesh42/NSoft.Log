using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace NSoft.Log.Core
{
    /// <summary>
    /// Contains common logic for background loggers.
    /// </summary>
    public class BackgroundLogger : BasicDisposable
    {
        /// <summary>
        /// Log manager.
        /// </summary>
        readonly ILogManager logManager;

        /// <summary>
        /// Records that will be saved.
        /// </summary>
        readonly ConcurrentQueue<LogRecord> processingRecords = new ConcurrentQueue<LogRecord>();

        /// <summary>
        /// Records to send.
        /// </summary>
        readonly List<LogRecord> recordsForWriting = new List<LogRecord>(1000);

        /// <summary>
        /// Thread that performes writing to the log.
        /// </summary>
        readonly Thread loggingThread;

        /// <summary>
        /// Event that will be switched to the signaled state, when termination of the logging thread be requested.
        /// </summary>
        readonly AutoResetEvent stopEvent = new AutoResetEvent(false);

        /// <summary>
        /// Initializes a new instance of the <see cref="BackgroundLogger"/> class.
        /// </summary>
        /// <param name="logManager">The log manager.</param>
        public BackgroundLogger(ILogManager logManager)
        {
            this.logManager = logManager;
            loggingThread = new Thread(LoggingThreadCallback) { IsBackground = true };
            loggingThread.Start();
        }

        /// <summary>
        /// Occurs when a record wasn't able to store by current writer.
        /// </summary>
        public event EventHandler<WriteFailedEventArgs> WriteFailed
        {
            add { logManager.WriteFailed += value; }
            remove { logManager.WriteFailed -= value; }
        }

        /// <summary>
        /// Writes data to the the specified channel name.
        /// </summary>
        /// <param name="channelName">Name of the channel.</param>
        /// <param name="data">Information about event.</param>
        public void Write(string channelName, params string[] data)
        {
            processingRecords.Enqueue(new LogRecord(channelName, data));
        }

        /// <summary>
        /// Performes writing to the log.
        /// </summary>
        void LoggingThreadCallback()
        {
            var timeout = 0;
            while (!stopEvent.WaitOne(timeout, false))
            {
                timeout = 0;
                LogRecord record;
                while (processingRecords.TryDequeue(out record) && recordsForWriting.Count < 1000)
                    recordsForWriting.Add(record);
                if (recordsForWriting.Count == 0)
                {
                    timeout = 10;
                    continue;
                }
                WriteRecords(recordsForWriting);
                recordsForWriting.Clear();
            }
        }

        /// <summary>
        /// Writes records to the log.
        /// </summary>
        /// <param name="records">Records that should be written.</param>
        void WriteRecords(List<LogRecord> records)
        {
            try
            {
                if (recordsForWriting.Count == 1)
                    logManager.Write(records[0].ChannelName, records[0].Data);
                else
                    logManager.Write(records);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        protected override void DisposeManaged()
        {
            stopEvent.Set();
            loggingThread.Join();
            var manager = logManager as IDisposable;
            if (manager != null)
                manager.Dispose();
        }
    }
}
