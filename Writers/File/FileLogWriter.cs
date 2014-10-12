using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using NSoft.Log.Core;

namespace NSoft.Log.Writers.File
{
    /// <summary>
    /// Writes information about events to the file system.
    /// </summary>
    public class FileLogWriter : LogWriterBase
    {
        /// <summary>
        /// Set of the invalid characters.
        /// </summary>
        readonly HashSet<char> invalidChars = new HashSet<char>(Path.GetInvalidFileNameChars());

        /// <summary>
        /// Path to the files storage.
        /// </summary>
        readonly string outputDirectory;

        /// <summary>
        /// File name template that is used by default.
        /// </summary>
        readonly string defaultFileNameTemplate;

        /// <summary>
        /// Customized file name templates by the channel's name.
        /// </summary>
        readonly Dictionary<string, string> customFileNameTemplates;

        /// <summary>
        /// Data delimeter.
        /// </summary>
        readonly string dataDelimeter;

        /// <summary>
        /// Registered channels. Key - channel name, value - file name template.
        /// </summary>
        readonly Dictionary<string, string> registeredChannels = new Dictionary<string, string>();

        /// <summary>
        /// Map of the channels' names to the writers that is used for saving of the events' data.
        /// </summary>
        readonly Dictionary<string, StreamWriter> writers = new Dictionary<string, StreamWriter>();

        /// <summary>
        /// Event that is used to signal 
        /// </summary>
        readonly AutoResetEvent recreateThreadStop = new AutoResetEvent(false);

        /// <summary>
        /// Number of milliseconds before writing will be switched to another file.
        /// </summary>
        readonly int recreateTime;

        /// <summary>
        /// Indicates that at least one writer has been used.
        /// </summary>
        bool anyWriterUsed;

        /// <summary>
        /// Object that is used for synchronization of writing.
        /// </summary>
        readonly object writeSync = new object();

        /// <summary>
        /// Thread that manages recreation of the writers.
        /// </summary>
        readonly Thread recreateThread;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileLogWriter"/> class.
        /// </summary>
        /// <param name="id">Identifier of the writer.</param>
        /// <param name="settings">Settings of the writer.</param>
        public FileLogWriter(int id, FileLogWriterSettings settings) : base(id)
        {
            dataDelimeter = settings.DataDelimeter;
            defaultFileNameTemplate = settings.DefaultFileNameTemplate;
            outputDirectory = settings.OutputDirectory;
            customFileNameTemplates = settings.Mappings.Where(obj => obj.Enabled).ToDictionary(obj => obj.ChannelName, obj => obj.Value);
            recreateTime = settings.RecreateTime;
            recreateThread = new Thread(RecreateWritersCallback) {IsBackground = true};
            recreateThread.Start();
        }

        public override void RegisterChannel(string channelName)
        {
            string fileNameTemplate;
            if (!customFileNameTemplates.TryGetValue(channelName, out fileNameTemplate))
                fileNameTemplate = defaultFileNameTemplate;
            var writer = CreateWriter(channelName, fileNameTemplate);
            writers.Add(channelName, writer);
            registeredChannels.Add(channelName, fileNameTemplate);
        }

        public override void Write(string channelName, params string[] data)
        {
            if (!IsValidRecord(channelName, data))
                return;
            WriteInternal(channelName, string.Join(dataDelimeter, data));
        }

        /// <summary>
        /// Prepares the name of the file.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        string PrepareFileName(string fileName)
        {
            var validFileName = new StringBuilder();
            foreach (var ch in fileName)
            {
                var checkedChar = invalidChars.Contains(ch) ? '_' : ch;
                validFileName.Append(checkedChar);
            }
            return validFileName.ToString();
        }

        /// <summary>
        /// Creates the writer.
        /// </summary>
        /// <param name="channelName">Name of the channel.</param>
        /// <param name="fileNameTemplate">The file name template.</param>
        StreamWriter CreateWriter(string channelName, string fileNameTemplate)
        {
            var fileName = fileNameTemplate.Replace("{ChannelName}", channelName).Replace("{TimeStamp}", DateTime.Now.Ticks.ToString());
            fileName = PrepareFileName(fileName);
            var filePath = Path.Combine(outputDirectory, fileName);
            var stream = new FileStream(filePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
            return new StreamWriter(stream, Encoding.Unicode);
        }

        /// <summary>
        /// Gets the writer that is connected with channel.
        /// </summary>
        /// <param name="channelName">Name of the channel.</param>
        /// <returns>Writer if it was found; otherwise, <c>null</c>.</returns>
        StreamWriter GetWriter(string channelName)
        {
            StreamWriter writer;
            return writers.TryGetValue(channelName, out writer) ? writer : null;
        }

        /// <summary>
        /// Closes the writers.
        /// </summary>
        /// <param name="writers">Writers, that should be closed.</param>
        static void CloseWriters(IEnumerable<StreamWriter> writers)
        {
            foreach (var writer in writers)
            {
                writer.Flush();
                writer.Close();
            }
        }

        /// <summary>
        /// Writes data to the channel.
        /// </summary>
        /// <param name="channelName">Name of the channel.</param>
        /// <param name="data">The data.</param>
        void WriteInternal(string channelName, string data)
        {
            lock (writeSync)
            {
                var writer = GetWriter(channelName);
                if (writer == null)
                    return;
                writer.WriteLine(data);
                anyWriterUsed = true;
            }
        }

        /// <summary>
        /// Recreates writers if needed.
        /// </summary>
        void RecreateWritersCallback()
        {
            while (!recreateThreadStop.WaitOne(recreateTime))
            {
                lock (writeSync)
                {
                    if (!anyWriterUsed) 
                        continue;
                    CloseWriters(writers.Values);
                    writers.Clear();
                    foreach (var pair in registeredChannels)
                    {
                        var writer = CreateWriter(pair.Key, pair.Value);
                        writers.Add(pair.Key, writer);
                    }
                    anyWriterUsed = false;
                }
            }
        }

        protected override void DisposeManaged()
        {
            base.DisposeManaged();
            recreateThreadStop.Set();
            recreateThread.Join();
            CloseWriters(writers.Values);
        }
    }
}
