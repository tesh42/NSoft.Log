using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using NSoft.Log.Core;

namespace NSoft.Log.Sample.Service
{
    /// <summary>
    /// Provides infrastructure for writing data to the log.
    /// </summary>
    public class LogMessageProcessor : BasicDisposable
    {
        /// <summary>
        /// Writing period. Measures in milliseconds.
        /// </summary>
        const int FlushPeriod = 500;

        /// <summary>
        /// Event that will be switched to the signaled state when terminationof the processing thread be requested.
        /// </summary>
        AutoResetEvent processingThreadStop = new AutoResetEvent(true);

        /// <summary>
        /// Thread that is processing records.
        /// </summary>
        Thread processingThread;

        /// <summary>
        /// Manager that is used for writing records to the log.
        /// </summary>
        readonly Core.ILogManager logManager;

        /// <summary>
        /// Records that should be saved to the log.
        /// </summary>
        readonly ConcurrentQueue<LogRecord> records = new ConcurrentQueue<LogRecord>();

        /// <summary>
        /// Date and time format for exceptions channel.
        /// </summary>
        public string DateTimeFormat { get; set; }

        /// <summary>
        /// Decimal numbers separator.
        /// </summary>
        public string NumberDecimalSeparator { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LogMessageProcessor"/> class.
        /// </summary>
        /// <param name="configuration">The configuration of the log manager.</param>
        public LogMessageProcessor(LogManagerConfiguration configuration)
        {
            logManager = new LogManagerFactory().Create(configuration);
            DateTimeFormat = "yyyy-MM-ddTHH:mm:ss.fff";
            NumberDecimalSeparator = ".";
        }

        /// <summary>
        /// Starts data processing.
        /// </summary>
        public void Start()
        {
            processingThreadStop = new AutoResetEvent(false);
            processingThread = new Thread(ProcessRecordsCallback) { IsBackground = false };
            var culture = (CultureInfo)processingThread.CurrentCulture.Clone();
            culture.NumberFormat.NumberDecimalSeparator = NumberDecimalSeparator;
            processingThread.CurrentCulture = culture;
            processingThread.Start();
        }

        /// <summary>
        /// Stops data processing.
        /// </summary>
        public void Stop()
        {
            if (processingThread == null)
                return;
            processingThreadStop.Set();
            processingThread.Join();
            processingThread = null;
        }

        /// <summary>
        /// Adds log record to the queue.
        /// </summary>
        /// <param name="channelName">The channel name.</param>
        /// <param name="data">Tha data to be written.</param>
        public void Enqueue(string channelName, params string[] data)
        {
            records.Enqueue(new LogRecord(channelName, data));
        }

        /// <summary>
        /// Adds log record to the queue.
        /// </summary>
        /// <param name="record">The log record information</param>
        public void Enqueue(LogRecord record)
        {
            records.Enqueue(record);
        }

        /// <summary>
        /// Writes records to the log.
        /// </summary>
        /// <param name="records">The records.</param>
        void WriteRecords(IEnumerable<LogRecord> records)
        {
            try
            {
                logManager.Write(records);
            }
            catch(Exception ex)
            {
                Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "> Failed writing data to the log. Exception: {0}" + ex);
            }
        }

        /// <summary>
        /// Writes records to the log.
        /// </summary>
        void ProcessRecordsCallback()
        {
            var sw = Stopwatch.StartNew();
            var waitTime = FlushPeriod;
            while (!processingThreadStop.WaitOne(waitTime))
            {
                sw.Reset();
                sw.Start();
                var localRecords = new List<LogRecord>();
                LogRecord record;
                while (records.TryDequeue(out record))
                    localRecords.Add(record);
                if (localRecords.Count > 0)
                    WriteRecords(localRecords);
                var elapsedTime = (int) sw.ElapsedMilliseconds;
                waitTime = elapsedTime < FlushPeriod ? FlushPeriod - elapsedTime : 1;
            }
        }

        protected override void DisposeManaged()
        {
            processingThreadStop.Set();
            processingThread.Join();
            var disposable = logManager as IDisposable;
            if (disposable != null)
                disposable.SafeDispose();
        }
    }
}
