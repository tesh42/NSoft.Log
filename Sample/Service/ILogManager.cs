using System.Collections.Generic;
using System.ServiceModel;
using NSoft.Log.Core;

namespace NSoft.Log.Sample.Service
{
    /// <summary>
    /// Service contract of the log manager.
    /// </summary>
    [ServiceContract(Name = "LogManager")]
    public interface ILogManager
    {
        /// <summary>
        /// Writes data to the channel.
        /// </summary>
        /// <param name="channel">Name of the channel.</param>
        /// <param name="data">Data that should be written.</param>
        [OperationContract(Name = "WriteLog", IsOneWay = true)]
        void WriteLog(string channel, string[] data);

        /// <summary>
        /// Writes log records.
        /// </summary>
        /// <param name="records">Records that should be written.</param>
        [OperationContract(Name = "WriteLogs", IsOneWay = true)]
        void WriteLogs(List<LogRecord> records);
    }
}
