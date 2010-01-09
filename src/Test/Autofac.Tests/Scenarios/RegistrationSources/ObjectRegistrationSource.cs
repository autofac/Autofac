using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autofac.Core;

namespace Autofac.Tests.Scenarios.RegistrationSources
{
    class ObjectRegistrationSource : IRegistrationSource
    {
        public IEnumerable<IComponentRegistration> RegistrationsFor(Service service, Func<Service, IEnumerable<IComponentRegistration>> registrationAccessor)
        {
            var objectService = new TypedService(typeof(object));
            if (service == objectService)
                yield return Factory.CreateSingletonObjectRegistration();

        }
    }
}
