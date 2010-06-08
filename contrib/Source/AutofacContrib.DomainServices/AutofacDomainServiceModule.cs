using System.Reflection;
using System.ServiceModel.DomainServices.Server;
using Autofac;

namespace AutofacContrib.DomainServices
{
    public class AutofacDomainServiceModule : Autofac.Module
    {
        private readonly Assembly[] assemblies;

        /// <summary>
        /// Registers all classes deriving from DomainService in the current assembly.
        /// </summary>
        public AutofacDomainServiceModule()
        {
            assemblies = new[] { Assembly.GetCallingAssembly() };
        }

        /// <summary>
        /// Registers all classes deriving from DomainService in the specified assemblies.
        /// </summary>
        public AutofacDomainServiceModule(params Assembly[] assemblies)
        {
            this.assemblies = assemblies;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterAssemblyTypes(assemblies).AssignableTo<DomainService>()
                .OnActivating(args =>
                {
                    var ds = (DomainService)args.Instance;
                    var context = args.Parameters.TypedAs<DomainServiceContext>();
                    if (context != null)
                        ds.Initialize(context);
                });
        }
    }
}
