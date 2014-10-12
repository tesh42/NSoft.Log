using System.Diagnostics;
using System.Xml.Serialization;

namespace NSoft.Log.Writers.EventLog
{
    /// <summary>
    /// Contains settings of the data mapping.
    /// </summary>
    public class DataMapping
    {
        /// <summary>
        /// Channel name.
        /// </summary>
        [XmlAttribute("channelName")]
        public string ChannelName { get; set; }

        /// <summary>
        /// Category identifier format.
        /// </summary>
        [XmlAttribute("category")]
        public string Category { get; set; }

        /// <summary>
        /// Event identifier format.
        /// </summary>
        [XmlAttribute("event")]
        public string Event { get; set; }

        /// <summary>
        /// Event log entry type.
        /// </summary>
        [XmlAttribute("entryType")]
        public EventLogEntryType EntryType { get; set; }

        /// <summary>
        /// Message format.
        /// </summary>
        [XmlAttribute("message")]
        public string Message { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataMapping"/> class.
        /// </summary>
        public DataMapping()
        {
            EntryType = EventLogEntryType.Information;
        }
    }
}