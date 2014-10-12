using System;
using System.Threading;

namespace NSoft.Log.Core
{
    /// <summary>
    /// Provides an implementation of Disposable pattern.
    /// </summary>
    public abstract class BasicDisposable : IDisposable
    {
        /// <summary>
        /// Indicates whether object was disposed.
        /// </summary>
        int disposed;

        /// <summary>
        /// Indicates whether object was disposed.
        /// </summary>
        public bool Disposed
        {
            get { return Thread.VolatileRead(ref disposed) == 1; }
        }

        /// <summary>
        /// Finalizer.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly", Justification = "This is a correct implementation.")]
        ~BasicDisposable()
        {
            if (Interlocked.CompareExchange(ref disposed, 1, 0) != 0)
                return;
            SafeDispose(false);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly", Justification = "This is a correct implementation.")]
        public void Dispose()
        {
            if (Interlocked.CompareExchange(ref disposed, 1, 0) != 0)
                return;
            SafeDispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes resources. Doesn't throws any exception.
        /// </summary>
        /// <param name="disposing">Indicates whether this method is called from <see cref="Dispose"/>.</param>
        void SafeDispose(bool disposing)
        {
            if (disposing)
                SafeDisposeManaged();
            SafeDisposeUnmanaged();
        }

        /// <summary>
        /// Disposes unmanaged resources. Doesn't throws any exception.
        /// </summary>
        void SafeDisposeUnmanaged()
        {
            try
            {
                DisposeUnmanaged();
            }
            catch (Exception e)
            {
            }
        }

        /// <summary>
        /// Disposes managed resources. Doesn't throws any exception.
        /// </summary>
        void SafeDisposeManaged()
        {
            try
            {
                DisposeManaged();
            }
            catch (Exception e)
            {
            }
        }

        /// <summary>
        /// Disposes managed resources.
        /// </summary>
        protected virtual void DisposeManaged()
        {
        }

        /// <summary>
        /// Disposes unmanaged resources.
        /// </summary>
        protected virtual void DisposeUnmanaged()
        {
        }
    }
}
