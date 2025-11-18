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

        #region Destructors
        /// <summary>
        /// Disposes object
        /// </summary>
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            Dispose(true);
            _disposed = true;
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes object asynchronously
        /// </summary>
        public async ValueTask DisposeAsync()
        {
            if (_disposed)
            {
                return;
            }

            await ReleaseResourcesAsync();
            ReleaseResources();
            _disposed = true;
            GC.SuppressFinalize(this);
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
    }
}
