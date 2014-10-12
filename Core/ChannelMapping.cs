using System.Xml.Serialization;

namespace NSoft.Log.Core
{
    /// <summary>
    /// Contains information about custom channel mapping.
    /// </summary>
    public class ChannelMapping
    {
        /// <summary>
        /// Name of the channel.
        /// </summary>
        [XmlAttribute("channelName")]
        public string ChannelName { get; set; }

        /// <summary>
        /// Value of the channel mapping.
        /// </summary>
        [XmlAttribute("value")]
        public string Value { get; set;}

        /// <summary>
        /// Indicates whether the channel mapping is enabled.
        /// </summary>
        [XmlAttribute("enabled")]
        public bool Enabled { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChannelMapping"/> class.
        /// </summary>
        public ChannelMapping()
        {
            Enabled = true;
        }
    }
}