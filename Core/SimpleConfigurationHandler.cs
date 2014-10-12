using System.Configuration;
using System.Xml;

namespace NSoft.Log.Core
{
    /// <summary>
    /// Handles the access to certain configuration section.
    /// </summary>
    /// <typeparam name="T">Type of the configuration object.</typeparam>
    public class SimpleConfigurationHandler<T> : IConfigurationSectionHandler
    {
        public object Create(object parent, object configContext, XmlNode section)
        {
            return ConfigurationHelper.Load<T>(section.Name, section.OuterXml);
        }
    }
}
