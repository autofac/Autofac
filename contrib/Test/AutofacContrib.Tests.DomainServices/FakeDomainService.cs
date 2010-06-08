using System.ServiceModel.DomainServices.Server;

namespace AutofacContrib.Tests.DomainServices
{
    public class FakeDomainService : DomainService
    {
        public bool IsDisposed { get; protected set; }

        protected override void Dispose(bool disposing)
        {
            IsDisposed = true;
        }
    }
}
