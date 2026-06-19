// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac.Builder;
using Autofac.Core;
using Autofac.Util;

namespace Autofac.Features.GeneratedFactories;

/// <summary>
/// Registration source for generated factory methods (i.e. when resolving <see cref="Func{T}"/> or some variant).
/// </summary>
internal class GeneratedFactoryRegistrationSource : IRegistrationSource
{
    /// <inheritdoc/>
    public bool IsAdapterForIndividualComponents => true;

    /// <summary>
    /// Retrieve registrations for an unregistered service, to be used
    /// by the container.
    /// </summary>
    /// <param name="service">The service that was requested.</param>
    /// <param name="registrationAccessor">A function that will return existing registrations for a service.</param>
    /// <returns>Registrations providing the service.</returns>
    [UnconditionalSuppressMessage(
        "AOT",
        "IL3050:RequiresDynamicCode",
        Justification = "The generated-factory source is registered for every container but only compiles a factory delegate (via FactoryGenerator's expression tree) when a consumer actually resolves a delegate factory type. The closed-type resolve path never reaches this. Consumers that resolve generated factories take on the dynamic-code requirement.")]
    [UnconditionalSuppressMessage(
        "Trimming",
        "IL2072:UnrecognizedReflectionPattern",
        Justification = "The delegate ServiceType is the factory type the consumer requested; reading its Invoke return type is intrinsic to a delegate and the consumer that resolves the factory is responsible for that type.")]
    public IEnumerable<IComponentRegistration> RegistrationsFor(Service service, Func<Service, IEnumerable<ServiceRegistration>> registrationAccessor)
    {
        if (service == null)
        {
            throw new ArgumentNullException(nameof(service));
        }

        if (registrationAccessor == null)
        {
            throw new ArgumentNullException(nameof(registrationAccessor));
        }

        if (service is not IServiceWithType ts || !ts.ServiceType.IsDelegate())
        {
            return Enumerable.Empty<IComponentRegistration>();
        }

        var resultType = ts.ServiceType.FunctionReturnType();
        var resultTypeService = ts.ChangeType(resultType);

        return registrationAccessor(resultTypeService)
            .Select(r =>
            {
                var factory = new FactoryGenerator(ts.ServiceType, resultTypeService, r, ParameterMapping.Adaptive);
                var rb = RegistrationBuilder.ForDelegate(ts.ServiceType, factory.GenerateFactory)
                    .InstancePerLifetimeScope()
                    .ExternallyOwned()
                    .As(service)
                    .Targeting(r.Registration)
                    .InheritRegistrationOrderFrom(r.Registration);

                return rb.CreateRegistration();
            });
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return GeneratedFactoryRegistrationSourceResources.GeneratedFactoryRegistrationSourceDescription;
    }
}
