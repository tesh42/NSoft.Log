using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using NSoft.Log.Core;

namespace NSoft.Log.Writers.DB
{
    /// <summary>
    /// Writes information about events to the Microsoft SQL Server database.
    /// </summary>
    public class DBLogWriter : LogWriterBase
    {
        /// <summary>
        /// Contains information about channel.
        /// </summary>
        class ChannelInfo
        {
            /// <summary>
            /// Empty channel.
            /// </summary>
            public static readonly ChannelInfo Empty = new ChannelInfo(null);

            /// <summary>
            /// Name of the table where channel's events will be stored.
            /// </summary>
            public string TableName { get; private set; }

            /// <summary>
            /// Table where channel's events will be stored.
            /// </summary>
            public DataTable Table { get; private set; }

            /// <summary>
            /// Initializes a new instance of the <see cref="ChannelInfo"/> class.
            /// </summary>
            /// <param name="tableName">Name of the table.</param>
            public ChannelInfo(string tableName)
            {
                TableName = tableName;
                Table = new DataTable(tableName);
            }
        }

        /// <summary>
        /// Number of seconds for the writer to complete saving of the events before it times out.
        /// </summary>
        readonly int timeout;

        /// <summary>
        /// Connection string that is used for connection to the database.
        /// </summary>
        readonly string connectionString;

        /// <summary>
        /// Template that is used for generating of the name of the channel's table.
        /// </summary>
        readonly string defaultTableNameTemplate;

        /// <summary>
        /// Custom table name templates by the channel name.
        /// </summary>
        readonly Dictionary<string, string> customTableNameTemplates;

        /// <summary>
        /// Channels that are supported by the writer.
        /// </summary>
        readonly Dictionary<string, ChannelInfo> channels = new Dictionary<string, ChannelInfo>();

        /// <summary>
        /// Initializes a new instance of the <see cref="DBLogWriter"/> class.
        /// </summary>
        /// <param name="id">Identifier of the writer.</param>
        /// <param name="settings">Settings of the writer.</param>
        public DBLogWriter(int id, DBLogWriterSettings settings) : base(id)
        {
            timeout = settings.Timeout;
            connectionString = settings.Connectionstring;
            defaultTableNameTemplate = settings.ChannelTableTemplate;
            customTableNameTemplates = settings.Mappings.Where(obj => obj.Enabled).ToDictionary(obj => obj.ChannelName, obj => obj.Value);
        }

        public override void RegisterChannel(string channelName)
        {
            string tableNameTemplate;
            if (!customTableNameTemplates.TryGetValue(channelName, out tableNameTemplate))
                tableNameTemplate = defaultTableNameTemplate;
            channels.Add(channelName, new ChannelInfo(tableNameTemplate.Replace("{ChannelName}", channelName)));
        }

        public override void Write(string channelName, params string[] data)
        {
            if (!IsValidRecord(channelName, data))
                return;
            var channelInfo = GetChannelInfo(channelName);
            if (channelInfo == ChannelInfo.Empty)
                return;
            var cmd = GetSqlCommand(channelInfo.TableName, data);
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                cmd.Connection = connection;
                cmd.ExecuteNonQuery();
            }
        }

        public override void Write(IEnumerable<LogRecord> records)
        {
            if (records == null)
                return;
            var recordsForSend = records.Where(IsValidRecord).ToList();
            if (recordsForSend.Count == 0)
                return;
            var tables = BuildTables(recordsForSend);
            if (tables.Length == 0)
                return;
            var bulkCopy = new SqlBulkCopy(connectionString, SqlBulkCopyOptions.Default);
            try
            {
                foreach (var table in tables)
                {
                    bulkCopy.BulkCopyTimeout = timeout;
                    bulkCopy.DestinationTableName = table.TableName;
                    bulkCopy.WriteToServer(table);
                }
            }
            finally
            {
                foreach (var table in tables)
                    table.Rows.Clear();
                bulkCopy.Close();
            }
        }

        /// <summary>
        /// Builds the query that is used for saving events to the database.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="argsCount">Number of arguments.</param>
        static string BuildQuery(string tableName, int argsCount)
        {
            var sb = new StringBuilder("INSERT INTO [" + tableName + "] VALUES (@Param0");
            for (var i = 1; i < argsCount; ++i)
                sb.AppendFormat(", @Param{0}", i);
            sb.Append(");");
            return sb.ToString();
        }

        /// <summary>
        /// Gets the channel's information.
        /// </summary>
        /// <param name="channelName">Name of the channel.</param>
        ChannelInfo GetChannelInfo(string channelName)
        {
            ChannelInfo channelInfo;
            return channels.TryGetValue(channelName, out channelInfo) ? channelInfo : ChannelInfo.Empty;
        }

        /// <summary>
        /// Gets the SQL command.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="data">The data.</param>
        static SqlCommand GetSqlCommand(string tableName, string[] data)
        {
            var query = BuildQuery(tableName, data.Length);
            var command = new SqlCommand(query);
            for (var i = 0; i < data.Length; ++i)
                command.Parameters.AddWithValue("@Param" + i, data[i]);
            return command;
        }

        /// <summary>
        /// Builds the tables.
        /// </summary>
        /// <param name="records">The records.</param>
        /// <returns></returns>
        DataTable[] BuildTables(IEnumerable<LogRecord> records)
        {
            foreach (var record in records)
            {
                var channelInfo = GetChannelInfo(record.ChannelName);
                if (channelInfo == ChannelInfo.Empty)
                    continue;
                if (channelInfo.Table.Columns.Count == 0)
                {
                    for (var i = record.Data.Length; i > 0; i--)
                        channelInfo.Table.Columns.Add();
                }
                channelInfo.Table.Rows.Add(record.Data);
            }
            return channels.Values.Where(channel => channel.Table.Rows.Count > 0).Select(channel => channel.Table).ToArray();
        }
    }
}
