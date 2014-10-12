using System.Collections.Generic;
using System.Xml.Serialization;

namespace NSoft.Log.Core
{
    /// <summary>
    /// Base configuration of the writers.
    /// </summary>
    public class LogWriterSettingsBase
    {
        /// <summary>
        /// Configuration of the custom channel mappings.
        /// </summary>
        [XmlArray(ElementName = "mappings")]
        [XmlArrayItem(ElementName = "add")]
        public List<ChannelMapping> Mappings { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LogWriterSettingsBase"/> class.
        /// </summary>
        public LogWriterSettingsBase()
        {
            Mappings = new List<ChannelMapping>();
        }
    }
}
