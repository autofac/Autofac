using Autofac;
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
    }
}