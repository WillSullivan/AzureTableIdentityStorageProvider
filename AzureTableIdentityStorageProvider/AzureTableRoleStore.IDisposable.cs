using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateStreetGang.AspNet.Identity.AzureTable
{


    public partial class AzureTableRoleStore<T>
    {
        #region IDisposable pattern

        /// <summary>
        /// When <c>true</c>, this instance has been disposed
        /// </summary>
        public bool Disposed { get; private set; }

        /// <summary>
        /// Throws an <see cref="ObjectDisposedException"/> if this instance has been disposed.
        /// </summary>
        protected void AssertNotDisposed()
        {
            if (Disposed)
            {
                throw new ObjectDisposedException(this.GetType().FullName);
            }
        }

        /// <summary>
        /// Dispose of (clean up and deallocate) resources used by this class.
        /// </summary>
        /// <param name="fromUser">
        /// True if called directly or indirectly from user code.
        /// False if called from the finalizer (i.e. from the class' destructor).
        /// </param>
        /// <remarks>
        /// When called from user code, it is safe to clean up both managed and unmanaged objects.
        /// When called from the finalizer, it is only safe to dispose of unmanaged objects.
        /// This method should expect to be called multiple times without causing an exception.
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1816:CallGCSuppressFinalizeCorrectly", Justification="It is, derp.")]
        protected virtual void Dispose(bool fromUser)
        {
            if (fromUser)	// Called from user code rather than the garbage collector
            {
                if (Disposed)
                {
                    return;
                }
                // Dispose of managed resources (only safe if called directly or indirectly from user code).
                try
                {
                    DisposeManagedResources();
                    GC.SuppressFinalize(this);	// No need for the Finalizer to do all this again.
                }
                finally
                {
                    Disposed = true;
                    //ToDo: Call the base class' Dispose() method if one exists.
                    //base.Dispose();
                }
            }
            DisposeUnmanagedResources();
        }

        /// <summary>
        /// Called when it is time to dispose of all managed resources
        /// </summary>
        protected virtual void DisposeManagedResources()
        {
        }

        /// <summary>
        /// Called when it is time to dispose of all unmanaged resources
        /// </summary>
        protected virtual void DisposeUnmanagedResources()
        {
        }

        /// <summary>
        /// Dispose of all resources (both managed and unmanaged) used by this class.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1816:CallGCSuppressFinalizeCorrectly", Justification="It will.  Derp.")]
        public void Dispose()
        {
            // Call our private Dispose method, indicating that the call originated from user code.
            // Diagnostics.TraceInfo("Disposed by user code.");
            this.Dispose(true);
        }

        /// <summary>
        /// Destructor, called by the finalizer during garbage collection.
        /// There is no guarantee that this method will be called. For example, if <see cref="Dispose()"/> has already
        /// been called in user code for this object, then finalization may have been suppressed.
        /// </summary>
        ~AzureTableRoleStore()
        {
            // Call our private Dispose method, indicating that the call originated from the finalizer.
            // Diagnostics.TraceInfo("Finalizer is disposing AzureTableRoleStore instance");
            this.Dispose(false);
        }

        #endregion
    }
}
