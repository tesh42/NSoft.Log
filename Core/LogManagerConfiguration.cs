using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

namespace NSoft.Log.Core
{
    public class LogManagerConfiguration
    {
        [XmlElement("logWriters")]
        public LogWritersConfiguration LogWriters { get; set; }

        [XmlArray(ElementName = "categories")]
        [XmlArrayItem(ElementName = "add")]
        public List<CategoryConfiguration> Categories { get; set;}

        public LogManagerConfiguration()
        {
            LogWriters = new LogWritersConfiguration();
            Categories = new List<CategoryConfiguration>();
        }
    }

    public class LogWritersConfiguration
    {
        private const int DefaultSwitchTime = 60000;

        [XmlAttribute("switchTime")]
        public int SwitchTime { get; set; }

        [XmlElement("add")]
        public List<LogWriterConfiguration> LogWriters { get; set; }

        public LogWritersConfiguration()
        {
            SwitchTime = DefaultSwitchTime;
            LogWriters = new List<LogWriterConfiguration>();
        }
    }

    public class LogWriterConfiguration
    {
        [XmlAttribute("id")]
        public int Id { get; set;}

        [XmlAttribute("type")]
        public string Type { get; set; }

        [XmlAttribute("configurationType")]
        public string ConfigurationType { get; set; }

        //[XmlElement(ElementName = "configuration")]
        [XmlAnyElement(Name = "configuration")]
        public XmlElement Configuration { get; set; }
    }

    public class CategoryConfiguration
    {
        [XmlAttribute("id")]
        public int Id { get; set; }

        [XmlArray(ElementName = "logWriters")]
        [XmlArrayItem(ElementName = "add")]
        public List<LogWriterDefinition> LogWriters { get; set; }

        [XmlArray(ElementName = "channels")]
        [XmlArrayItem(ElementName = "add")]
        public List<ChannelDefinition> Channels { get; set;}

        public CategoryConfiguration()
        {
            LogWriters = new List<LogWriterDefinition>();
            Channels = new List<ChannelDefinition>();
        }
    }

    public class LogWriterDefinition
    {
        [XmlAttribute("id")]
        public int Id { get; set;}

        [XmlAttribute("priority")]
        public int Priority { get; set;}
    }

    public class ChannelDefinition
    {
        [XmlAttribute("name")]
        public string Name { get; set; }
    }
}
