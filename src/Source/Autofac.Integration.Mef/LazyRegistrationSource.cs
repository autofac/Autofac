using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Autofac.Builder;
using Autofac.Core;

namespace Autofac.Integration.Mef
{
    class LazyRegistrationSource : IRegistrationSource
    {
        static readonly MethodInfo CreateLazyRegistrationMethod = typeof(LazyRegistrationSource).GetMethod(
            "CreateLazyRegistration", BindingFlags.Static | BindingFlags.NonPublic);

        public IEnumerable<IComponentRegistration> RegistrationsFor(Service service, Func<Service, IEnumerable<IComponentRegistration>> registrationAccessor)
        {
            var swt = service as IServiceWithType;
            if (swt == null ||
                !swt.ServiceType.IsGenericType ||
                swt.ServiceType.GetGenericTypeDefinition() != typeof(Lazy<>))
                return Enumerable.Empty<IComponentRegistration>();

            var valueType = swt.ServiceType.GetGenericArguments()[0];

            var valueService = swt.ChangeType(valueType);

            var registrationCreator = CreateLazyRegistrationMethod.MakeGenericMethod(valueType);

            return registrationAccessor(valueService)
                .Select(v => registrationCreator.Invoke(null, new object[] { service, v }))
                .Cast<IComponentRegistration>();
        }

        static IComponentRegistration CreateLazyRegistration<T>(Service providedService, IComponentRegistration valueRegistration)
        {
            var rb = RegistrationBuilder.ForDelegate(
                (c, p) =>
                {
                    var context = c.Resolve<IComponentContext>();
                    return new Lazy<T>(() => (T)c.Resolve(valueRegistration, p));
                })
                .As(providedService);

            return RegistrationBuilder.CreateRegistration(rb);
        }
    }
}
