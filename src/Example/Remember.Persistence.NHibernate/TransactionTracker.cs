using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            if (!_disposed)
            {
                _disposed = true;

                if (CurrentTransaction != null)
                {
                    if (HttpContext.Current.Error != null)
                        CurrentTransaction.Rollback();
                    else
                        CurrentTransaction.Commit();
                }
            }
        }
    }
}
