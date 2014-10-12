using System.Xml.Serialization;
using NSoft.Log.Core;

namespace NSoft.Log.Writers.Wcf
{
    /// <summary>
    /// Contains settings of <see cref="WcfLogWriter"/>.
    /// </summary>
    public class WcfLogWriterSettings : LogWriterSettingsBase
    {
        /// <summary>
        /// WCF client endpoint name.
        /// </summary>
        [XmlElement("endpointName")]
        public string EndpointName { get; set; }
    }
}
