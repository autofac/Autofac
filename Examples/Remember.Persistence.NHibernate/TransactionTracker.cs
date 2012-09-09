using System;
using NHibernate;
using System.Web;

namespace Remember.Persistence.NHibernate
{
    class TransactionTracker : IDisposable
    {
        bool _disposed;

        public ITransaction CurrentTransaction { get; set; }

        public void Dispose()
        {
            if (_disposed) return;

            _disposed = true;

            if (CurrentTransaction == null) return;

            if (HttpContext.Current.Error != null)
                CurrentTransaction.Rollback();
            else
                CurrentTransaction.Commit();
        }
    }
}
