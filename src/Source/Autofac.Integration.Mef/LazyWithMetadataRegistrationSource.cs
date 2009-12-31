using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reflection;
using Autofac;
using Autofac.Builder;
using Autofac.Core;

namespace Autofac.Integration.Mef
{
    class LazyWithMetadataRegistrationSource : IRegistrationSource
    {
        static readonly MethodInfo CreateLazyRegistrationMethod = typeof(LazyWithMetadataRegistrationSource).GetMethod(
            "CreateLazyRegistration", BindingFlags.Static | BindingFlags.NonPublic);

        public IEnumerable<IComponentRegistration> RegistrationsFor(Service service, Func<Service, IEnumerable<IComponentRegistration>> registrationAccessor)
        {
            var swt = service as IServiceWithType;
            if (swt == null ||
                !swt.ServiceType.IsGenericType ||
                swt.ServiceType.GetGenericTypeDefinition() != typeof (Lazy<,>))
                return Enumerable.Empty<IComponentRegistration>();

            var valueType = swt.ServiceType.GetGenericArguments()[0];
            var metaType = swt.ServiceType.GetGenericArguments()[1];

            var valueService = swt.ChangeType(valueType);

            var registrationCreator = CreateLazyRegistrationMethod.MakeGenericMethod(valueType, metaType);

            return registrationAccessor(valueService)
                .Select(v => registrationCreator.Invoke(null, new object[] { service, v }))
                .Cast<IComponentRegistration>();
        }

        static IComponentRegistration CreateLazyRegistration<T, TMeta>(Service providedService, IComponentRegistration valueRegistration)
        {
            var rb = RegistrationBuilder.ForDelegate(
                (c, p) =>
                {
                    var context = c.Resolve<IComponentContext>();
                    return new Lazy<T, TMeta>(
                        () => (T) c.Resolve(valueRegistration, p),
                        AttributedModelServices.GetMetadataView<TMeta>(valueRegistration.ExtendedProperties));
                })
                .As(providedService);

            return RegistrationBuilder.CreateRegistration(rb);
        }
    }
}
