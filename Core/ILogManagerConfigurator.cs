namespace NSoft.Log.Core
{
    /// <summary>
    /// Provides methods for log manager configuration.
    /// </summary>
    public interface ILogManagerConfigurator
    {
        /// <summary>
        /// Creates the category.
        /// </summary>
        /// <param name="id">The category id.</param>
        /// <param name="writersSwitchTime">The writers switch time.</param>
        void CreateCategory(int id, int writersSwitchTime);

        /// <summary>
        /// Binds the writer to the category.
        /// </summary>
        /// <param name="categoryId">The category id.</param>
        /// <param name="writer">The writer.</param>
        /// <param name="priority">The priority.</param>
        void BindWriter(int categoryId, ILogWriter writer, int priority);

        /// <summary>
        /// Binds the channel to the category.
        /// </summary>
        /// <param name="categoryId">The category id.</param>
        /// <param name="channelName">Name of the channel.</param>
        void BindChannel(int categoryId, string channelName);
    }
}
