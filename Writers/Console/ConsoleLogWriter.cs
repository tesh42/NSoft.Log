using NSoft.Log.Core;

namespace NSoft.Log.Writers.Console
{
    /// <summary>
    /// Writes information about events to standard output stream.
    /// </summary>
    public class ConsoleLogWriter : LogWriterBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConsoleLogWriter"/> class.
        /// </summary>
        /// <param name="id">Unique identifier.</param>
        public ConsoleLogWriter(int id) : base(id)
        {
        }

        public override void Write(string channelName, params string[] data)
        {
            System.Console.WriteLine(string.Join("> ", data));
        }
    }
}
