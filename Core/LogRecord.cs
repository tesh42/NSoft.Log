using System;

namespace NSoft.Log.Core
{
    /// <summary>
    /// Contains information about an event.
    /// </summary>
    [Serializable]
    public class LogRecord
    {
        /// <summary>
        /// Name of the target channel.
        /// </summary>
        public string ChannelName { get; set; }

        /// <summary>
        /// The data to be written.
        /// </summary>
        public string[] Data { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LogRecord"/> class.
        /// </summary>
        public LogRecord()
        {
            
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LogRecord"/> class.
        /// </summary>
        /// <param name="channelName">Name of the channel.</param>
        /// <param name="data">The data to be written.</param>
        public LogRecord(string channelName, string[] data)
        {
            ChannelName = channelName;
            Data = data;
        }
    }
}
