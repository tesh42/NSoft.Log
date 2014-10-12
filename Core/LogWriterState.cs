namespace NSoft.Log.Core
{
    /// <summary>
    /// State of the log writer.
    /// </summary>
    public enum LogWriterState
    {
        /// <summary>
        /// Log writer is disabled and can't be used for writing information about events.
        /// </summary>
        Disabled,

        /// <summary>
        /// Log writer is enabled and can be used for writing information about events.
        /// </summary>
        Enabled
    }
}