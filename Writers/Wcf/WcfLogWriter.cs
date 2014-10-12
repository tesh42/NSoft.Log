using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using NSoft.Log.Core;

namespace NSoft.Log.Writers.Wcf
{
    /// <summary>
    /// Sends information about events to the remote WCF service.
    /// </summary>
    public class WcfLogWriter : LogWriterBase
    {
        /// <summary>
        /// Channel factory.
        /// </summary>
        readonly ChannelFactory<ILogManager> factory;

        /// <summary>
        /// Channel that is used for communication with the remote service.
        /// </summary>
        ILogManager channel;

        /// <summary>
        /// Initializes a new instance of the <see cref="WcfLogWriter"/> class.
        /// </summary>
        /// <param name="id">Identifier of the writer.</param>
        /// <param name="settings">Settings of the writer.</param>
        public WcfLogWriter(int id, WcfLogWriterSettings settings) : base(id)
        {
            factory = new ChannelFactory<ILogManager>(settings.EndpointName);
            factory.Open();
        }

        public override void Write(string channelName, params string[] data)
        {
            if (!IsValidRecord(channelName, data))
                return;
            try
            {
                channel.WriteLog(channelName, data);
            }
            catch (Exception)
            {
                channel = factory.CreateChannel();
                channel.WriteLog(channelName, data);
            }
        }

        public override void Write(IEnumerable<LogRecord> records)
        {
            if (records == null)
                return;
            var recordsForSend = records.Where(IsValidRecord).ToList();
            if (recordsForSend.Count == 0)
                return;
            try
            {
                channel.WriteLogs(recordsForSend);
            }
            catch (Exception)
            {
                channel = factory.CreateChannel();
                channel.WriteLogs(recordsForSend);
            }
        }

        protected override void DisposeManaged()
        {
            base.DisposeManaged();
            try
            {
                ((ICommunicationObject)channel).Close();
            }
            catch (Exception)
            {
                ((ICommunicationObject)channel).Abort();
            }
            try
            {
                factory.Close();
            }
            catch (Exception)
            {
                factory.Abort();
            }
        }
    }
}
