// This software is part of the Autofac IoC container
// Copyright © 2018 Autofac Contributors
// http://autofac.org
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Linq;
using Autofac.Builder;
using Autofac.Core;
using Autofac.Features.OpenGenerics;

namespace Autofac.Features.Decorators
{
    internal static class DecoratorExtensions
    {
        private const string DecoratedServiceTypesPropertyKey = "__DecoratedServiceTypes";

        internal static HashSet<Type> GetDecoratedServiceTypes(this IComponentRegistry registry)
        {
            if (registry.Properties.ContainsKey(DecoratedServiceTypesPropertyKey))
                return (HashSet<Type>)registry.Properties[DecoratedServiceTypesPropertyKey];

            var decoratedServiceTypes = new HashSet<Type>();
            registry.Properties.Add(DecoratedServiceTypesPropertyKey, decoratedServiceTypes);
            return decoratedServiceTypes;
        }

        internal static void RegisterForDecoration(this IComponentRegistry registry, Service service)
        {
            if (service is DecoratorService decoratorService)
                registry.GetDecoratedServiceTypes().Add(decoratorService.ServiceType);
        }

        internal static void RegisterForDecoration(
            this IComponentRegistry registry,
            IRegistrationSource source)
        {
            if (!(source is OpenGenericRegistrationSource openGenericRegistrationSource)) return;

            var decoratorService = openGenericRegistrationSource.Services
                .OfType<DecoratorService>()
                .Select(s => s.ServiceType)
                .FirstOrDefault();

            if (decoratorService != null)
                registry.GetDecoratedServiceTypes().Add(decoratorService);
        }

        internal static bool TryGetDecoratedService(
            this IComponentRegistry registry,
            IComponentRegistration registration,
            out IServiceWithType service)
        {
            service = null;

            var decoratedServiceTypes = registry.GetDecoratedServiceTypes();

            foreach (var serviceWithType in registration.Services.OfType<IServiceWithType>())
            {
                if (serviceWithType is DecoratorService) break;

                if (decoratedServiceTypes.Contains(serviceWithType.ServiceType))
                {
                    service = serviceWithType;
                    return true;
                }

                if (!serviceWithType.ServiceType.IsConstructedGenericType) continue;

                var openGenericType = serviceWithType.ServiceType.GetGenericTypeDefinition();
                if (!decoratedServiceTypes.Contains(openGenericType)) continue;

                service = serviceWithType;
                return true;
            }

            return false;
        }

        internal static object DecorateService(
            this IServiceWithType typedService,
            object instance,
            IComponentContext context,
            IEnumerable<Parameter> parameters)
        {
            var registry = context.ComponentRegistry;
            var resolveParameters = parameters as Parameter[] ?? parameters.ToArray();
            var serviceType = typedService.ServiceType;

            var decoratorService = new DecoratorService(serviceType);

            var decorators = registry.RegistrationsFor(decoratorService)
                .OrderBy(r => r.GetRegistrationOrder())
                .Select(r => new
                {
                    Registration = r,
                    Service = r.Services.OfType<DecoratorService>().FirstOrDefault()
                })
                .ToArray();

            if (decorators.Length == 0) return instance;

            var decoratorContext = DecoratorContext.Create(instance.GetType(), serviceType, instance);

            foreach (var decorator in decorators)
            {
                if (!decorator.Service.Condition(decoratorContext)) continue;

                var serviceParameter = new TypedParameter(serviceType, instance);
                var contextParameter = new TypedParameter(typeof(IDecoratorContext), decoratorContext);
                var invokeParameters = resolveParameters.Concat(new Parameter[] { serviceParameter, contextParameter });
                instance = context.ResolveComponent(decorator.Registration, invokeParameters);

                decoratorContext = decoratorContext.UpdateContext(instance);
            }

            return instance;
        }
    }
}
