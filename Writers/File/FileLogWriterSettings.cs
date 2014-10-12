using System.Xml.Serialization;
using NSoft.Log.Core;

namespace NSoft.Log.Writers.File
{
    /// <summary>
    ///  Contains settings of <see cref="FileLogWriter"/>.
    /// </summary>
    public class FileLogWriterSettings : LogWriterSettingsBase
    {
        /// <summary>
        /// Data delimeter.
        /// </summary>
        [XmlElement("dataDelimeter")]
        public string DataDelimeter { get; set;}

        /// <summary>
        /// Common file name template.
        /// </summary>
        [XmlElement("channelFileTemplate")]
        public string DefaultFileNameTemplate { get; set; }

        /// <summary>
        /// Path to the files storage.
        /// </summary>
        [XmlElement("outputDirectory")]
        public string OutputDirectory { get; set; }

        /// <summary>
        /// Number of milliseconds before writing will be switched to another file.
        /// </summary>
        [XmlElement(ElementName = "recreateTime")]
        public int RecreateTime { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileLogWriterSettings"/> class.
        /// </summary>
        public FileLogWriterSettings()
        {
            RecreateTime = 10 * 60 * 1000;
            DataDelimeter = "\t";
        }
    }
}
