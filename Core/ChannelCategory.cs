using System.Collections.Generic;
using System.Linq;

namespace NSoft.Log.Core
{
    /// <summary>
    /// Provides information about writers and supported channels.
    /// </summary>
    internal class ChannelCategory
    {
        /// <summary>
        /// Contains information about log writer.
        /// </summary>
        class LogWriterInfo
        {
            /// <summary>
            /// Priority of the writer.
            /// It affects position in the writers queue (an active writer have the highest priority).
            /// </summary>
            public int Priority { get; set; }

            /// <summary>
            /// Index of the writer.
            /// </summary>
            public int Index { get; set; }

            /// <summary>
            /// Log writer.
            /// </summary>
            public ILogWriter Writer { get; set; }
        }

        /// <summary>
        /// Empty category.
        /// </summary>
        public static readonly ChannelCategory Empty = new ChannelCategory(-1, -1);

        /// <summary>
        /// List of the writers that belong to that category.
        /// </summary>
        List<LogWriterInfo> writers;

        /// <summary>
        /// Map of the writers' identifiers to their indices.
        /// </summary>
        readonly Dictionary<int, int> writerIndexById;

        /// <summary>
        /// List of the known channels that belong to that category.
        /// </summary>
        readonly List<string> knownChannels;

        /// <summary>
        /// Index of the writer that is currently used for writing event's data.
        /// </summary>
        int currentWriterIndex;

        /// <summary>
        /// Duration of suspending writer in case of an error. Measured in milliseconds.
        /// </summary>
        readonly int writerSwitchTime;

        /// <summary>
        /// Unique identifier of the category.
        /// </summary>
        public int Id { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChannelCategory"/> class.
        /// </summary>
        /// <param name="id">Unique identifier of the category.</param>
        /// <param name="writerSwitchTime">Duration of suspending writer in case of an error. Measured in milliseconds.</param>
        public ChannelCategory(int id, int writerSwitchTime)
        {
            Id = id;
            writers = new List<LogWriterInfo>();
            writerIndexById = new Dictionary<int, int>();
            knownChannels = new List<string>();
            this.writerSwitchTime = writerSwitchTime;
        }

        /// <summary>
        /// Current log writer.
        /// </summary>
        public ILogWriter CurrentWriter
        {
            get { return currentWriterIndex == -1 ? null : writers[currentWriterIndex].Writer; }
        }

        /// <summary>
        /// Registers the channel.
        /// </summary>
        /// <param name="channelName">Name of the channel.</param>
        public void RegisterChannel(string channelName)
        {
            knownChannels.Add(channelName);
            foreach (var writer in writers)
                writer.Writer.RegisterChannel(channelName);
        }

        /// <summary>
        /// Registers the log writer.
        /// </summary>
        /// <param name="writer">The log writer.</param>
        /// <param name="priority">The priority. It affects position in the writers queue (an active writer have the highest priority).</param>
        public void RegisterLogWriter(ILogWriter writer, int priority)
        {
            var writerInfo = new LogWriterInfo {Priority = priority, Writer = writer};
            writer.StateChanged += OnWriterStateChanged;
            foreach (var channel in knownChannels)
                writer.RegisterChannel(channel);
            writers.Add(writerInfo);
            writers = writers.OrderByDescending(obj => obj.Priority).ToList();
            RecalculateWriterIndexes();
            currentWriterIndex = GetBestWriterIndex();
        }

        /// <summary>
        /// Moves to the next writer.
        /// </summary>
        /// <returns><c>true</c>, if next writer wasn't found; otherwise, <c>false</c>.</returns>
        public bool MoveNextWriter()
        {
            if (currentWriterIndex != -1)
                CurrentWriter.Disable(writerSwitchTime);
            return currentWriterIndex != -1;
        }

        /// <summary>
        /// Handles an event of the state changing.
        /// </summary>
        void OnWriterStateChanged(object sender, LogWriterStateChangedEventArgs e)
        {
            var writer = sender as ILogWriter;
            if (writer == null)
                return;
            if (e.NewState == LogWriterState.Disabled)
            {
                currentWriterIndex = GetBestWriterIndex();
            }
            else if (e.NewState == LogWriterState.Enabled && e.OldState == LogWriterState.Disabled)
            {
                var writerIndex = writerIndexById[writer.Id];
                if (writerIndex < currentWriterIndex || currentWriterIndex == -1)
                    currentWriterIndex = writerIndex;
            }
        }

        /// <summary>
        /// Determines a non disabled writer with the highest priority.
        /// </summary>
        /// <returns>Identifier of the non disabled writer with the highest priority if it was found; otherwise, -1.</returns>
        int GetBestWriterIndex()
        {
            var nextWriter = writers.FirstOrDefault(obj => obj.Writer.State != LogWriterState.Disabled);
            return nextWriter != null ? nextWriter.Index : -1;
        }

        /// <summary>
        /// Recalculates indexes of the writers that belong to the category.
        /// </summary>
        void RecalculateWriterIndexes()
        {
            var count = writers.Count;
            for (var i = 0; i < count; i++)
            {
                var writerInfo = writers[i];
                writerInfo.Index = i;
                writerIndexById[writerInfo.Writer.Id] = i;
            }
        }
    }
}
