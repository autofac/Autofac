using Autofac;
using AutofacTestWebApplication.Services;

namespace AutofacTestWebApplication
{
    public class AutofacModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.Register(c => new Logger()).As<ILogger>();
        }
    }
}