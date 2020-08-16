// This software is part of the Autofac IoC container
// Copyright © 2011 Autofac Contributors
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

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Autofac.Builder;
using Autofac.Core;
using Autofac.Core.Activators.Reflection;
using Autofac.Core.Resolving.Pipeline;

namespace Autofac.Features.OpenGenerics
{
    /// <summary>
    /// Generates activators for open generic types.
    /// </summary>
    internal class OpenGenericRegistrationSource : IRegistrationSource
    {
        private readonly RegistrationData _registrationData;
        private readonly IResolvePipelineBuilder _existingPipelineBuilder;
        private readonly ReflectionActivatorData _activatorData;

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenGenericRegistrationSource"/> class.
        /// </summary>
        /// <param name="registrationData">The registration data for the open generic.</param>
        /// <param name="existingPipelineBuilder">The pipeline for the existing open generic registration.</param>
        /// <param name="activatorData">The activator data.</param>
        public OpenGenericRegistrationSource(
            RegistrationData registrationData,
            IResolvePipelineBuilder existingPipelineBuilder,
            ReflectionActivatorData activatorData)
        {
            if (registrationData == null)
            {
                throw new ArgumentNullException(nameof(registrationData));
            }

            if (activatorData == null)
            {
                throw new ArgumentNullException(nameof(activatorData));
            }

            OpenGenericServiceBinder.EnforceBindable(activatorData.ImplementationType, registrationData.Services);

            _registrationData = registrationData;
            _existingPipelineBuilder = existingPipelineBuilder;
            _activatorData = activatorData;
        }

        /// <inheritdoc/>
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

            if (OpenGenericServiceBinder.TryBindOpenGenericService(service, _registrationData.Services, _activatorData.ImplementationType, out Type? constructedImplementationType, out Service[]? services))
            {
                // Pass the pipeline builder from the original registration to the 'CreateRegistration'.
                // So the original registration will contain all of the pipeline stages originally added, plus anything we want to add.
                yield return RegistrationBuilder.CreateRegistration(
                    Guid.NewGuid(),
                    _registrationData,
                    new ReflectionActivator(constructedImplementationType, _activatorData.ConstructorFinder, _activatorData.ConstructorSelector, _activatorData.ConfiguredParameters, _activatorData.ConfiguredProperties),
                    _existingPipelineBuilder,
                    services);
            }
        }

        /// <inheritdoc/>
        public bool IsAdapterForIndividualComponents => false;

        /// <inheritdoc/>
        public override string ToString()
        {
            return string.Format(
                CultureInfo.CurrentCulture,
                OpenGenericRegistrationSourceResources.OpenGenericRegistrationSourceDescription,
                _activatorData.ImplementationType.FullName,
                string.Join(", ", _registrationData.Services.Select(s => s.Description).ToArray()));
        }
    }
}
