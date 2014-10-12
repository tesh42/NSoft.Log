using System;
using System.Text;
using NSoft.Log.Core;

namespace NSoft.Log.ConsoleClient
{
    /// <summary>
    /// Provides a set of methods for writing events to the log.
    /// </summary>
    public class SimpleLogger : BackgroundLogger
    {
        /// <summary>
        /// Default date and time format.
        /// </summary>
        const string DefaultDateTimeFormat = "yyyy-MM-ddTHH:mm:ss.fff";

        /// <summary>
        /// Message that is used in case an error during writing message to the log.
        /// </summary>
        const string ErrorWriterIsChanged = "Error occured during processing data. Writer is changed.";

        /// <summary>
        /// Date and time format.
        /// </summary>
        public string DateTimeFormat { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleLogger"/> class.
        /// </summary>
        /// <param name="logManager">The log manager.</param>
        public SimpleLogger(ILogManager logManager) : base(logManager)
        {
            DateTimeFormat = DefaultDateTimeFormat;
            WriteFailed += OnWriteFailed;
        }

        /// <summary>
        /// Prepares the specified string.
        /// </summary>
        /// <param name="str">The string.</param>
        static string PrepareString(string str)
        {
            return string.IsNullOrEmpty(str) ? "" : str.Replace('\t', '_').Replace('\r', '_').Replace('\n', '_');
        }

        /// <summary>
        /// Writes information about exception to the log.
        /// </summary>
        /// <param name="text">Description of the exception.</param>
        /// <param name="exception">Information about exception.</param>
        void LogException(string text, Exception exception)
        {
            string[] data;
            if (exception != null)
            {
                var preparedText = PrepareString(text + " (" + exception.Message + ")");
                var preparedStack = exception.StackTrace != null ? PrepareString(exception.StackTrace) : "";
                data = new[] {DateTime.Now.ToString(DateTimeFormat), preparedText, preparedStack};
            }
            else
            {
                var preparedText = PrepareString(text);
                data = new[] {DateTime.Now.ToString(DateTimeFormat), preparedText, ""};
            }
            Write(ChannelNames.Exception, data);
        }

        /// <summary>
        /// Writes information about event to the log.
        /// </summary>
        /// <param name="format">Format of the message.</param>
        /// <param name="args">Message arguments.</param>
        public virtual void LogMain(string format, params object[] args)
        {
            var data = new[] {DateTime.Now.ToString(DateTimeFormat), PrepareString(args == null ? format : string.Format(format, args))};
            Write(ChannelNames.Console, data);
            Write(ChannelNames.Main, data);
        }

        /// <summary>
        /// Writes information about error to the log.
        /// </summary>
        /// <param name="errorDescription">Description of the exception.</param>
        public virtual void LogError(string errorDescription)
        {
            LogMain(errorDescription);
            LogException(errorDescription, null);
        }

        /// <summary>
        /// Writes information about an exception to the log.
        /// </summary>
        /// <param name="text">Description of the exception.</param>
        /// <param name="exception">Information about the exception.</param>
        public virtual void LogError(string text, Exception exception)
        {
            var str = new StringBuilder();
            str.Append(text);
            if (exception != null)
                str.Append(" (" + exception + ")");
            LogMain(str.ToString());
            LogException(text, exception);
        }

        /// <summary>
        /// Called when writing to the log was failed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="WriteFailedEventArgs"/> instance containing the event data.</param>
        void OnWriteFailed(object sender, WriteFailedEventArgs e)
        {
            if (!e.IsFatalError)
                LogException(ErrorWriterIsChanged, e.Error);
        }
    }
}
