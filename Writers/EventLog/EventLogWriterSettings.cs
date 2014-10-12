using System.Collections.Generic;
using System.Diagnostics;
using System.Xml.Serialization;
using NSoft.Log.Core;

namespace NSoft.Log.Writers.EventLog
{
    /// <summary>
    /// Contains settings of <see cref="EventLogWriter"/>.
    /// </summary>
    public class EventLogWriterSettings : LogWriterSettingsBase
    {
        /// <summary>
        /// Data delimeter that is used by default.
        /// </summary>
        [XmlElement("dataDelimeter")]
        public string DataDelimeter { get; set; }

        /// <summary>
        /// Event log entry type that is used by default.
        /// </summary>
        [XmlElement("entryType")]
        public EventLogEntryType EntryType { get; set; }

        /// <summary>
        /// Name of the event log.
        /// </summary>
        [XmlElement("logName")]
        public string LogName { get; set; }

        /// <summary>
        /// Name or network address of the computer where event log is situated.
        /// </summary>
        [XmlElement("machineName")]
        public string MachineName { get; set; }

        /// <summary>
        /// Custom settings for data mapping.
        /// </summary>
        [XmlArray(ElementName = "dataMappings")]
        [XmlArrayItem(ElementName = "add")]
        public List<DataMapping> DataMappings { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="EventLogWriterSettings"/> class.
        /// </summary>
        public EventLogWriterSettings()
        {
            MachineName = ".";
            DataDelimeter = "\t";
            EntryType = EventLogEntryType.Information;
        }
    }
}