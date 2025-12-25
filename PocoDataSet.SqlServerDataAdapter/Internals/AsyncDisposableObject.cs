using System;
using System.Threading.Tasks;

namespace PocoDataSet.SqlServerDataAdapter
{
    /// <summary>
    /// Provides disposable object functionality
    /// </summary>
    public abstract class AsyncDisposableObject : IDisposable, IAsyncDisposable
    {
        #region Data Fields
        /// <summary>
        /// Flag indicating whether object had been disposed
        /// </summary>
        bool _disposed;
        #endregion

        #region Protected Methods
        /// <summary>
        /// Core async dispose hook
        /// </summary>
        protected virtual async ValueTask DisposeAsyncCore()
        {
            await ReleaseResourcesAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Releases resources
        /// </summary>
        protected virtual void ReleaseResources()
        {
        }

        /// <summary>
        /// Releases resources asynchronously
        /// </summary>
        protected virtual ValueTask ReleaseResourcesAsync()
        {
            return ValueTask.CompletedTask;
        }
        #endregion

        #region Protected Properties
        /// <summary>
        /// Gets flag indicating whether object has been disposed
        /// </summary>
        protected bool IsDisposed
        {
            get
            {
                return _disposed;
            }
        }
        #endregion

        #region IDisposable
        /// <summary>
        /// Disposes object
        /// </summary>
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            try
            {
                Dispose(true);
            }
            finally
            {
                _disposed = true;
                GC.SuppressFinalize(this);
            }
        }

        /// <summary>
        /// Disposes object
        /// </summary>
        /// <param name="disposing">Flag indicating that object is disposing</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                ReleaseResources();
            }
        }
        #endregion

        #region IAsyncDisposable
        /// <summary>
        /// Disposes object asynchronously
        /// </summary>
        public async ValueTask DisposeAsync()
        {
            if (_disposed)
            {
                return;
            }

            try
            {
                await DisposeAsyncCore().ConfigureAwait(false);
                Dispose(false); // ensures derived Dispose(bool) runs in async path too
            }
            finally
            {
                _disposed = true;
                GC.SuppressFinalize(this);
            }
        }
        #endregion
    }
}
