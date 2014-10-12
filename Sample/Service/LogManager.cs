using System;
using System.Collections.Generic;
using System.Configuration;
using System.ServiceModel;
using NSoft.Log.Core;

namespace NSoft.Log.Sample.Service
{
    /// <summary>
    /// Log manager service.
    /// </summary>
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, InstanceContextMode = InstanceContextMode.Single)]
    public class LogManager : BasicDisposable, ILogManager
    {
        /// <summary>
        /// Date and time format parameter name.
        /// </summary>
        const string DateTimeFormatParameterName = "DateTimeFormat";
        /// <summary>
        /// Decimal numbers separator parameter name.
        /// </summary>
        const string NumberDecimalSeparatorParameterName = "NumberDecimalSeparator";

        /// <summary>
        /// Object that is used for processing of the messages.
        /// </summary>
        readonly LogMessageProcessor messageProcessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="LogManager"/> class.
        /// </summary>
        public LogManager()
        {
            var configuration = LoadLogManagerConfiguration();
            messageProcessor = new LogMessageProcessor(configuration);
            var dateTimeFormat = ConfigurationManager.AppSettings[DateTimeFormatParameterName];
            if (!string.IsNullOrEmpty(dateTimeFormat))
                messageProcessor.DateTimeFormat = dateTimeFormat;    
            var numberDecimalSeparator = ConfigurationManager.AppSettings[NumberDecimalSeparatorParameterName];
            if (!string.IsNullOrEmpty(numberDecimalSeparator))
                messageProcessor.NumberDecimalSeparator = numberDecimalSeparator;
            messageProcessor.Start();
        }

        public void WriteLog(string channel, string[] data)
        {
            if (!IsValidRecord(channel, data))
                return;
            messageProcessor.Enqueue(channel, data);
        }

        public void WriteLogs(List<LogRecord> records)
        {
            if (records == null || records.Count == 0)
                return;
            foreach (var record in records)
            {
                if (!IsValidRecord(record.ChannelName, record.Data))
                    continue;
                messageProcessor.Enqueue(record);
            }
        }

        /// <summary>
        /// Loads configuration of the log manager.
        /// </summary>
        static LogManagerConfiguration LoadLogManagerConfiguration()
        {
            try
            {
                return (LogManagerConfiguration) ConfigurationManager.GetSection("logManager");
            }
            catch (Exception)
            {
                Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "> Failed to load database writer settings from the 'logManager' section.");
                throw;
            }
        }

        /// <summary>
        /// Determines whether record that should be written is valid.
        /// </summary>
        /// <param name="channel">Channel, where record should be written.</param>
        /// <param name="data">Data that should be written.</param>
        /// <returns><c>true</c> if record is valid; otherwise, <c>false</c>.</returns>
        static bool IsValidRecord(string channel, string[] data)
        {
            return !(string.IsNullOrEmpty(channel) || data == null || data.Length == 0);
        }

        protected override void DisposeManaged()
        {
            messageProcessor.Stop();
            messageProcessor.Dispose();
        }
    }
}
