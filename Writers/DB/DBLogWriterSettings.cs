using System.Xml.Serialization;
using NSoft.Log.Core;

namespace NSoft.Log.Writers.DB
{
    /// <summary>
    /// Contains settings of <see cref="DBLogWriter"/>.
    /// </summary>
    public class DBLogWriterSettings : LogWriterSettingsBase
    {
        /// <summary>
        /// Common channel name template.
        /// </summary>
        [XmlElement(ElementName = "channelTableTemplate")]
        public string ChannelTableTemplate { get; set; }

        /// <summary>
        /// Database connection string.
        /// </summary>
        [XmlElement(ElementName = "connectionstring")]
        public string Connectionstring { get; set; }

        /// <summary>
        /// Number of settings for the writer to complete saving of the events before it times out.
        /// </summary>
        [XmlElement(ElementName = "timeout")]
        public int Timeout { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DBLogWriterSettings"/> class.
        /// </summary>
        public DBLogWriterSettings()
        {
            Timeout = 240;
        }
    }
}
