using System;
using System.Collections.Generic;
using System.Linq;
using Autofac.Builder;
using Autofac.Core;
using Autofac.Util;

namespace Autofac.Features.Decorators
{
    public class DecoratorRegistrationSource : IRegistrationSource
    {
        private readonly RegistrationData _registrationData;
        private readonly DecoratorActivatorData _activatorData;

        public DecoratorRegistrationSource(RegistrationData registrationData, DecoratorActivatorData activatorData)
        {
            _registrationData = registrationData;
            _activatorData = activatorData;

            // TODO: Add additional checks that all services appropriate for decoratoration
            var implementationService = new TypedService(_activatorData.ImplementationType);
            if (_registrationData.Services.Contains(implementationService))
            {
                throw new ArgumentException(
                    $"The decorated service {_activatorData.ServiceType} cannot expose its implementation type {_activatorData.ImplementationType} as a service");
            }
        }

        public IEnumerable<IComponentRegistration> RegistrationsFor(Service service, Func<Service, IEnumerable<IComponentRegistration>> registrationAccessor)
        {
            if (service == null) throw new ArgumentNullException(nameof(service));
            if (registrationAccessor == null) throw new ArgumentNullException(nameof(registrationAccessor));

            if (!(service is IServiceWithType serviceWithType))
                return Enumerable.Empty<IComponentRegistration>();

            if (serviceWithType.ServiceType != _activatorData.ServiceType
                && !_registrationData.Services.Contains(service))
                return Enumerable.Empty<IComponentRegistration>();

            if (service.GetType() == typeof(DecoratorService))
                return Enumerable.Empty<IComponentRegistration>();

            var decoratedRegistration = BuildDecoratedRegistration();

            var decoratorService = new DecoratorService(_activatorData.ServiceType);
            var decorators = registrationAccessor(decoratorService)
                .OrderBy(r => r.GetRegistrationOrder())
                .Select(r => new { Registration = r, Service = r.Services.OfType<DecoratorService>().FirstOrDefault() })
                .ToArray();

            if (decorators.Length == 0)
                return new[] { decoratedRegistration };

            var registrationBuilder = RegistrationBuilder.ForDelegate(
                (c, p) =>
                {
                    var parameters = p.ToArray();
                    var instance = c.ResolveComponent(decoratedRegistration, parameters);

                    var context = DecoratorContext.Create(_activatorData.ImplementationType, _activatorData.ServiceType, instance);

                    foreach (var decorator in decorators)
                    {
                        if (!decorator.Service.Condition(context)) continue;

                        var serviceParameter = new TypedParameter(_activatorData.ServiceType, instance);
                        var contextParameter = new TypedParameter(typeof(IDecoratorContext), context);
                        var invokeParameters = parameters.Concat(new Parameter[] { serviceParameter, contextParameter });
                        instance = c.ResolveComponent(decorator.Registration, invokeParameters);

                        context = context.UpdateContext(instance);
                    }

                    return instance;
                })
                .As(_activatorData.ServiceType)
                .Targeting(decoratedRegistration)
                .InheritRegistrationOrderFrom(decoratedRegistration);

            registrationBuilder.RegistrationData.CopyFrom(_registrationData, true);

            var registration = registrationBuilder.CreateRegistration();

            return new[] { registration };
        }

        private IComponentRegistration BuildDecoratedRegistration()
        {
            var builder = RegistrationBuilder.ForType(_activatorData.ImplementationType);
            builder.RegistrationData.CopyFrom(_registrationData, true);

            builder.ActivatorData.ConfiguredParameters.AddRange(_activatorData.ConfiguredParameters);

            var registrationOrder = _registrationData.Metadata[MetadataKeys.RegistrationOrderMetadataKey];
            builder.RegistrationData.Metadata[MetadataKeys.RegistrationOrderMetadataKey] = registrationOrder;

            var decoratorService = new TypedService(_activatorData.ServiceType);
            if (!builder.RegistrationData.Services.Contains(decoratorService))
                builder.RegistrationData.AddService(decoratorService);

            var registration = builder.CreateRegistration();
            return registration;
        }

        public bool IsAdapterForIndividualComponents => true;
    }
}
