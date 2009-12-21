// This software is part of the Autofac IoC container
// Copyright (c) 2007 - 2009 Autofac Contributors
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
using Autofac.Builder;
using Autofac.Core;
using Autofac.Core.Activators.Delegate;

namespace Autofac.Features.GeneratedFactories
{
    static class GeneratedFactoryRegistrationExtensions
    {
        internal static RegistrationBuilder<TLimit, GeneratedFactoryActivatorData, SingleRegistrationStyle>
            RegisterGeneratedFactory<TLimit>(ContainerBuilder builder, Type delegateType, Service service)
        {
            var activatorData = new GeneratedFactoryActivatorData();

            var rb = new RegistrationBuilder<TLimit, GeneratedFactoryActivatorData, SingleRegistrationStyle>(
                activatorData, new SingleRegistrationStyle());

            builder.RegisterCallback(cr =>
            {
                var factory = new FactoryGenerator(delegateType, service, activatorData.ParameterMapping);
                var activator = new DelegateActivator(delegateType, (c, p) => factory.GenerateFactory(c, p));
                RegistrationHelpers.RegisterSingleComponent(cr, rb, activator);
            });

            return rb.InstancePerLifetimeScope();
        }
    }
}
