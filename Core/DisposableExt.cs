using System;

namespace NSoft.Log.Core
{
    /// <summary>
    /// Provides a set of extension methods for working with <see cref="IDisposable"/>.
    /// </summary>
    public static class DisposableExt
    {
        /// <summary>
        /// Disposes an object. Doesn't throw any exception.
        /// </summary>
        /// <param name="disposable">Object that should be disposed.</param>
        public static void SafeDispose(this IDisposable disposable)
        {
            try
            {
                if (disposable != null)
                    disposable.Dispose();
            }
            catch (Exception e)
            {
            }
        }
    }
}
