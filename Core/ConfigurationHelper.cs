using System;
using System.IO;
using System.Xml.Serialization;

namespace NSoft.Log.Core
{
    /// <summary>
    /// Provides utility methods for working with configuration.
    /// </summary>
    public static class ConfigurationHelper
    {
        /// <summary>
        /// Loads a configuration object from the specific section.
        /// </summary>
        /// <typeparam name="T">Type of the configuration object.</typeparam>
        /// <param name="sectionName">Name of the section.</param>
        /// <param name="xml">String that contains serialized object.</param>
        public static T Load<T>(string sectionName, string xml)
        {
            var rootAttribute = new XmlRootAttribute(sectionName);
            var serializer = new XmlSerializer(typeof(T), rootAttribute);
            return (T)serializer.Deserialize(new StringReader(xml));
        }

        /// <summary>
        /// Loads a configuration object from the specific section.
        /// </summary>
        /// <param name="type">Type of the configuration object.</param>
        /// <param name="sectionName">Name of the section.</param>
        /// <param name="xml">String that contains serialized object.</param>
        public static object Load(Type type, string sectionName, string xml)
        {
            var rootAttribute = new XmlRootAttribute(sectionName);
            var serializer = new XmlSerializer(type, rootAttribute);
            return serializer.Deserialize(new StringReader(xml));
        }
    }
}
