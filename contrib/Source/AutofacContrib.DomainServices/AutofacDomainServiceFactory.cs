using System;
using System.ServiceModel.DomainServices.Server;
using Autofac;

namespace AutofacContrib.DomainServices
{
    public class AutofacDomainServiceFactory : IDomainServiceFactory
    {
        private readonly IContainer container;

        public AutofacDomainServiceFactory(IContainer container)
        {
            this.container = container;
        }

        public DomainService CreateDomainService(Type domainServiceType, DomainServiceContext context)
        {
            return (DomainService)container.Resolve(domainServiceType, TypedParameter.From(context));            
        }

        public void ReleaseDomainService(DomainService domainService)
        {
            container.Disposer.AddInstanceForDisposal(domainService);
        }
    }
}
