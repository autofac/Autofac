using Autofac;
using Autofac.Core;
using AutofacWebApiSample.Services;

namespace AutofacWebApiSample
{
    public class AutofacModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.Register(c => new Logger())
                .As<ILogger>()
                .InstancePerLifetimeScope();

            builder.Register(c => new ValuesService(c.Resolve<ILogger>()))
                .As<IValuesService>()
                .InstancePerLifetimeScope();
        }

        public override bool Equals(IModule other)
        {
            if (other == null) return false;
            return other.GetType() == GetType();
        }
    }
}
