// This software is part of the Autofac IoC container
// Copyright © 2018 Autofac Contributors
// https://autofac.org
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

using System.Collections.Generic;
using System.Linq;
using Autofac.Builder;
using Autofac.Core;
using Autofac.Core.Registration;

namespace Autofac.Features.Decorators
{
    internal static class InstanceDecorator
    {
        internal static object TryDecorateRegistration(
            Service service,
            IComponentRegistration registration,
            object instance,
            IComponentContext context,
            IEnumerable<Parameter> parameters)
        {
            var instanceType = instance.GetType();

            if (registration.Services.OfType<DecoratorService>().Any()
                || !(service is IServiceWithType serviceWithType)
                || registration is ExternalComponentRegistration) return instance;

            var decoratorRegistrations = context.ComponentRegistry
                .RegistrationsFor(new DecoratorService(serviceWithType.ServiceType))
                .Where(r => !r.IsAdapterForIndividualComponent)
                .OrderBy(d => d.GetRegistrationOrder())
                .ToArray();

            if (decoratorRegistrations.Length == 0) return instance;

            var serviceType = serviceWithType.ServiceType;
            var resolveParameters = parameters as Parameter[] ?? parameters.ToArray();

            var decoratorContext = DecoratorContext.Create(instanceType, serviceType, instance);

            foreach (var decoratorRegistration in decoratorRegistrations)
            {
                var decoratorService = decoratorRegistration.Services.OfType<DecoratorService>().First();
                if (!decoratorService.Condition(decoratorContext)) continue;

                var serviceParameter = new TypedParameter(serviceType, instance);
                var contextParameter = new TypedParameter(typeof(IDecoratorContext), decoratorContext);
                var invokeParameters = resolveParameters.Concat(new Parameter[] { serviceParameter, contextParameter });
                instance = context.ResolveComponent(new ResolveRequest(decoratorService, decoratorRegistration, invokeParameters));

                decoratorContext = decoratorContext.UpdateContext(instance);
            }

            return instance;
        }
    }
}
