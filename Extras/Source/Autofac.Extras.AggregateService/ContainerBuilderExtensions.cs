// This software is part of the Autofac IoC container
// Copyright (c) 2007 - 2010 Autofac Contributors
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
using Autofac;

namespace AutofacContrib.AggregateService
{
    /// <summary>
    /// AggregateService extensions to <see cref="ContainerBuilder"/>.
    /// </summary>
    public static class ContainerBuilderExtensions
    {
        ///<summary>
        /// Register <typeparamref name="TInterface"/> as an aggregate service.
        ///</summary>
        ///<param name="builder">The container builder</param>
        ///<typeparam name="TInterface">The interface type to register</typeparam>
        /// <exception cref="ArgumentNullException">If <typeparamref name="TInterface"/> is null</exception>
        /// <exception cref="ArgumentException">If <typeparamref name="TInterface"/> is not an interface</exception>
        public static void RegisterAggregateService<TInterface>(this ContainerBuilder builder) where TInterface:class 
        {
            builder.RegisterAggregateService(typeof (TInterface));
        }

        ///<summary>
        /// Register <paramref name="interfaceType"/> as an aggregate service.
        ///</summary>
        ///<param name="builder">The container builder</param>
        ///<param name="interfaceType">The interface type to register</param>
        /// <exception cref="ArgumentNullException">If <paramref name="interfaceType"/> is null</exception>
        /// <exception cref="ArgumentException">If <paramref name="interfaceType"/> is not an interface</exception>
        public static void RegisterAggregateService(this ContainerBuilder builder, Type interfaceType)
        {
            if (interfaceType == null) throw new ArgumentNullException("interfaceType");
            if (!interfaceType.IsInterface) throw new ArgumentException("Aggregate service type must be an interface", "interfaceType");
            builder.Register(c => AggregateServiceGenerator.CreateInstance(interfaceType, c.Resolve<IComponentContext>()))
                .As(interfaceType)
                .InstancePerDependency();
        }
    }
}