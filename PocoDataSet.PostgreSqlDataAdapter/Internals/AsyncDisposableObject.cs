using System;
using System.Threading.Tasks;

namespace PocoDataSet.PostgreSqlDataAdapter
{
    /// <summary>
    /// Provides disposable object functionality.
    /// </summary>
    public abstract class AsyncDisposableObject : IDisposable, IAsyncDisposable
    {
        #region Data Fields
        bool _disposed;
        #endregion

        #region Protected Methods
        protected virtual async ValueTask DisposeAsyncCore()
        {
            await ReleaseResourcesAsync().ConfigureAwait(false);
        }

        protected virtual void ReleaseResources()
        {
        }

        protected virtual ValueTask ReleaseResourcesAsync()
        {
            return ValueTask.CompletedTask;
        }
        #endregion

        #region Protected Properties
        protected bool IsDisposed
        {
            get
            {
                return _disposed;
            }
        }
        #endregion

        #region IDisposable
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

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                ReleaseResources();
            }
        }
        #endregion

        #region IAsyncDisposable
        public async ValueTask DisposeAsync()
        {
            if (_disposed)
            {
                return;
            }

            try
            {
                await DisposeAsyncCore().ConfigureAwait(false);
                Dispose(false);
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
