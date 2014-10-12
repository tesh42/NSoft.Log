using System;
using System.Collections.Generic;
using System.Linq;

namespace NSoft.Log.Core
{
    /// <summary>
    /// Provides a manager that is used for writing events to the log.
    /// </summary>
    public class LogManager : BasicDisposable, ILogManager, ILogManagerConfigurator
    {
        /// <summary>
        /// An empty list of categories.
        /// </summary>
        static readonly List<ChannelCategory> EmptyCategoryList = new List<ChannelCategory>(0);

        /// <summary>
        /// Map of the writers' identifiers to the writers.
        /// </summary>
        readonly Dictionary<int, ILogWriter> logWriters;

        /// <summary>
        /// Map of the categories' identifiers to the categories.
        /// </summary>
        readonly Dictionary<int, ChannelCategory> categoryById;

        /// <summary>
        /// Map of the channels' names to the list of categories.
        /// </summary>
        readonly Dictionary<string, List<ChannelCategory>> categoriesByChannel;

        /// <summary>
        /// Occurs when a record is failed to store by current writer.
        /// </summary>
        public event EventHandler<WriteFailedEventArgs> WriteFailed;

        /// <summary>
        /// Initializes a new instance of the <see cref="LogManager"/> class.
        /// </summary>
        public LogManager()
        {
            logWriters = new Dictionary<int, ILogWriter>();
            categoriesByChannel = new Dictionary<string, List<ChannelCategory>>();
            categoryById = new Dictionary<int, ChannelCategory>();
        }

        public void Write(string channelName, params string[] data)
        {
            var categories = GetChannelCategories(channelName);
            if (categories == EmptyCategoryList)
                return;
            foreach (var category in categories)
            {
                var logWriter = category.CurrentWriter;
                while (true)
                {
                    try
                    {
                        logWriter.Write(channelName, data);
                        break;
                    }
                    catch (Exception ex)
                    {
                        var fatalError = !category.MoveNextWriter();
                        var eventArgs = new WriteFailedEventArgs(ex, fatalError);
                        OnWriteFailed(eventArgs);
                        // We can't suppress exceptions when we have no active writers because it leads to log records lost.
                        if (fatalError)
                            throw;
                        logWriter = category.CurrentWriter;
                    }
                }
            }
        }

        public void Write(IEnumerable<LogRecord> records)
        {
            var recordsByCategoryId = GroupRecordsByCategoryId(records);
            foreach (var pair in recordsByCategoryId)
            {
                var category = GetChannelCategory(pair.Key);
                var logWriter = category.CurrentWriter;
                while (logWriter != null)
                {
                    try
                    {
                        logWriter.Write(pair.Value);
                        break;
                    }
                    catch (Exception ex)
                    {
                        var fatalError = !category.MoveNextWriter();
                        var eventArgs = new WriteFailedEventArgs(ex, fatalError);
                        OnWriteFailed(eventArgs);
                        // We can't suppress exceptions when we have no active writers because it leads to log records lost.
                        if (fatalError)
                            throw;
                        logWriter = category.CurrentWriter;
                    }
                }
            }
        }

        /// <summary>
        /// Handles an event of the write failure.
        /// </summary>
        /// <param name="e">Information about the event.</param>
        void OnWriteFailed(WriteFailedEventArgs e)
        {
            var handler = WriteFailed;
            if (handler == null) 
                return;
            try
            {
                handler(this, e);
            }
            catch(Exception)
            {
                // Suppress any exception in the handler.
            }
        }

        /// <summary>
        /// Determines the list of categories that relate to the specific channel.
        /// </summary>
        /// <param name="channelName">Name of the channel.</param>
        IEnumerable<ChannelCategory> GetChannelCategories(string channelName)
        {
            List<ChannelCategory> category;
            return categoriesByChannel.TryGetValue(channelName, out category) ? category : EmptyCategoryList;
        }

        /// <summary>
        /// Determines the category by the identifier.
        /// </summary>
        /// <param name="categoryId">Identifier of the category.</param>
        /// <returns>The category if it was found; otherwise, <see cref="ChannelCategory.Empty"/>.</returns>
        ChannelCategory GetChannelCategory(int categoryId)
        {
            ChannelCategory category;
            return !categoryById.TryGetValue(categoryId, out category) ? ChannelCategory.Empty : category;
        }

        /// <summary>
        /// Groupes records by the category identifier.
        /// </summary>
        /// <param name="records">The records to be grouped.</param>
        Dictionary<int, List<LogRecord>> GroupRecordsByCategoryId(IEnumerable<LogRecord> records)
        {
            var recordsByCategoryId = new Dictionary<int, List<LogRecord>>(categoryById.Count);
            foreach (var record in records)
            {
                foreach (var category in GetChannelCategories(record.ChannelName))
                {
                    List<LogRecord> categoryRecords;
                    if (!recordsByCategoryId.TryGetValue(category.Id, out categoryRecords))
                    {
                        categoryRecords = new List<LogRecord>();
                        recordsByCategoryId.Add(category.Id, categoryRecords);
                    }
                    categoryRecords.Add(record);
                }
            }
            return recordsByCategoryId;
        }

        void ILogManagerConfigurator.CreateCategory(int id, int writersSwitchTime)
        {
            var category = new ChannelCategory(id, writersSwitchTime);
            categoryById.Add(id, category);
        }

        void ILogManagerConfigurator.BindWriter(int categoryId, ILogWriter writer, int priority)
        {
            var category = categoryById[categoryId];
            category.RegisterLogWriter(writer, priority);
            if (!logWriters.ContainsKey(writer.Id))
                logWriters.Add(writer.Id, writer);
        }

        void ILogManagerConfigurator.BindChannel(int categoryId, string channelName)
        {
            var category = categoryById[categoryId];
            category.RegisterChannel(channelName);
            List<ChannelCategory> channelCategories;
            if (!categoriesByChannel.TryGetValue(channelName, out channelCategories))
            {
                channelCategories = new List<ChannelCategory>();
                categoriesByChannel.Add(channelName, channelCategories);
            }
            if (channelCategories.All(obj => obj.Id != categoryId))
                channelCategories.Add(category);
        }

        protected override void DisposeManaged()
        {
            foreach (var writer in logWriters.Select(w => w.Value).OfType<IDisposable>())
                writer.Dispose();
        }
    }
}
