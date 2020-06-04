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
using Autofac.Builder;
using Autofac.Core;

namespace Autofac.Features.GeneratedFactories
{
    /// <summary>
    /// Helper methods for registering factories.
    /// </summary>
    internal static class GeneratedFactoryRegistrationExtensions
    {
        /// <summary>
        /// Registers a factory delegate.
        /// </summary>
        /// <param name="builder">Container builder.</param>
        /// <param name="delegateType">Factory type to generate.</param>
        /// <param name="service">The service that the delegate will return instances of.</param>
        /// <returns>Registration builder allowing the registration to be configured.</returns>
        /// <remarks>Factory delegates are provided automatically in Autofac 2, and
        /// this method is generally not required.</remarks>
        internal static IRegistrationBuilder<TLimit, GeneratedFactoryActivatorData, SingleRegistrationStyle>
            RegisterGeneratedFactory<TLimit>(ContainerBuilder builder, Type delegateType, Service service)
            where TLimit : notnull
        {
            var activatorData = new GeneratedFactoryActivatorData(delegateType, service);

            var rb = new RegistrationBuilder<TLimit, GeneratedFactoryActivatorData, SingleRegistrationStyle>(
                new TypedService(delegateType),
                activatorData,
                new SingleRegistrationStyle());

            rb.RegistrationData.DeferredCallback = builder.RegisterCallback(cr => RegistrationBuilder.RegisterSingleComponent(cr, rb));

            return rb.InstancePerLifetimeScope();
        }
    }
}
