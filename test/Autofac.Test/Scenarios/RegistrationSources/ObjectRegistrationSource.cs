// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autofac.Core;

namespace Autofac.Test.Scenarios.RegistrationSources
{
    public class ObjectRegistrationSource : IRegistrationSource
    {
        private readonly object _instance;

        public ObjectRegistrationSource()
            : this(new object())
        {
        }

        public ObjectRegistrationSource(object instance)
        {
            _instance = instance;
        }

        public IEnumerable<IComponentRegistration> RegistrationsFor(Service service, Func<Service, IEnumerable<ServiceRegistration>> registrationAccessor)
        {
            var objectService = new TypedService(typeof(object));
            if (service == objectService)
            {
                yield return Factory.CreateSingletonObjectRegistration(_instance);
            }
        }

        public bool IsAdapterForIndividualComponents
        {
            get { return false; }
        }
    }
}
