using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NSoft.Log.Core;

namespace NSoft.Log.Writers.EventLog
{
    /// <summary>
    /// Writes information about events to the Windows event log.
    /// </summary>
    public class EventLogWriter : LogWriterBase
    {
        /// <summary>
        /// Data delimeter that is used by default.
        /// </summary>
        readonly string defaultDataDelimeter;

        /// <summary>
        /// Data binding templates by channel name.
        /// </summary>
        readonly Dictionary<string, DataMapping> dataMappings;

        /// <summary>
        /// Map of the channels' names to the event source name.
        /// </summary>
        readonly Dictionary<string, string> channelMappings;

        /// <summary>
        /// Map of the channel's names to the event log.
        /// </summary>
        readonly Dictionary<string, System.Diagnostics.EventLog> eventLogsByChannel = new Dictionary<string, System.Diagnostics.EventLog>();

        /// <summary>
        /// Event log entry type that is used by default.
        /// </summary>
        readonly EventLogEntryType defaultEntryType;

        /// <summary>
        /// Network address or name of the computer where event log is situated.
        /// </summary>
        readonly string machineName;

        /// <summary>
        /// Name of the event log.
        /// </summary>
        readonly string logName;

        /// <summary>
        /// Initializes a new instance of the <see cref="EventLogWriter"/> class.
        /// </summary>
        /// <param name="id">Unique identifier of the writer.</param>
        /// <param name="settings">The settings of the writer.</param>
        public EventLogWriter(int id, EventLogWriterSettings settings) : base(id)
        {
            channelMappings = settings.Mappings.Where(obj => obj.Enabled).ToDictionary(obj => obj.ChannelName, obj => obj.Value);
            defaultDataDelimeter = settings.DataDelimeter;
            dataMappings = settings.DataMappings.ToDictionary(obj => obj.ChannelName);
            defaultEntryType = settings.EntryType;
            machineName = settings.MachineName;
            logName = settings.LogName;
        }

        public override void RegisterChannel(string channelName)
        {
            string source;
            if (!channelMappings.TryGetValue(channelName, out source))
                source = channelName;
            if (!System.Diagnostics.EventLog.SourceExists(source, machineName))
            {
                var eventSourceData = new EventSourceCreationData(source, logName) { MachineName = machineName };
                System.Diagnostics.EventLog.CreateEventSource(eventSourceData);
            }
            var eventLog = new System.Diagnostics.EventLog(logName, machineName, source);
            eventLogsByChannel.Add(channelName, eventLog);
        }

        public override void Write(string channelName, params string[] data)
        {
            if (!IsValidRecord(channelName, data))
                return;
            System.Diagnostics.EventLog eventLog;
            if (!eventLogsByChannel.TryGetValue(channelName, out eventLog))
                return;
            DataMapping dataMapping;
            if (dataMappings.TryGetValue(channelName, out dataMapping))
                Write(eventLog, dataMapping, data);
            else
                Write(eventLog, data);
        }

        /// <summary>
        /// Writes data to the specified event log.
        /// </summary>
        /// <param name="eventLog">The event log.</param>
        /// <param name="dataMapping">Data mapping that should be used for writing.</param>
        /// <param name="data">Data that should be written.</param>
        static void Write(System.Diagnostics.EventLog eventLog, DataMapping dataMapping, string[] data)
        {
            var message = "";
            short categoryId = 0;
            var eventId = 0;
            if (!string.IsNullOrEmpty(dataMapping.Category))
            {
                var categoryUnparsed = string.Format(dataMapping.Category, data);
                short.TryParse(categoryUnparsed, out categoryId);
            }
            if (!string.IsNullOrEmpty(dataMapping.Event))
            {
                var eventUnparsed = string.Format(dataMapping.Event, data);
                int.TryParse(eventUnparsed, out eventId);
            }
            if (!string.IsNullOrEmpty(dataMapping.Message))
                message = string.Format(dataMapping.Message, data);
            eventLog.WriteEntry(message, dataMapping.EntryType, eventId, categoryId);
        }

        /// <summary>
        /// Writes data to the specified event log.
        /// </summary>
        /// <param name="eventLog">The event log.</param>
        /// <param name="data">Data that should be written.</param>
        void Write(System.Diagnostics.EventLog eventLog, string[] data)
        {
            var message = string.Join(defaultDataDelimeter, data);
            eventLog.WriteEntry(message, defaultEntryType);
        }

        protected override void DisposeManaged()
        {
            base.DisposeManaged();
            foreach (var eventLog in eventLogsByChannel.Values)
                eventLog.SafeDispose();
        }
    }
}
