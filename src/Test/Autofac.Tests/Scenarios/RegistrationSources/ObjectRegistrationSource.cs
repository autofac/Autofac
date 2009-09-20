using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autofac.Core;

namespace Autofac.Tests.Scenarios.RegistrationSources
{
    class ObjectRegistrationSource : IRegistrationSource
    {
        public bool TryGetRegistration(Service service, Func<Service, bool> registeredServicesTest, out IComponentRegistration registration)
        {
            var objectService = new TypedService(typeof(object));
            if (service == objectService)
            {
                registration = Factory.CreateSingletonObjectRegistration();
                return true;
            }
            else
            {
                registration = null;
                return false;
            }
        }
    }
}
