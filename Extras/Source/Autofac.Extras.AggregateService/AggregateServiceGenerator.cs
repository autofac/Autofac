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
using System.Security;
using Autofac;
using Castle.DynamicProxy;

namespace Autofac.Extras.AggregateService
{
    ///<summary>
    /// Generate aggregate service instances from interface types.
    ///</summary>
    [SecuritySafeCritical]
    public static class AggregateServiceGenerator
    {
        private static readonly ProxyGenerator Generator = new ProxyGenerator();

        ///<summary>
        /// Generate an aggregate service instance that will resolve its types from <paramref name="context"/>.
        ///</summary>
        ///<param name="context">The component context from where types will be resolved</param>
        ///<typeparam name="TAggregateServiceInterface">The interface type for the aggregate service</typeparam>
        ///<returns>The aggregate service instance</returns>
        /// <exception cref="ArgumentException">Thrown if <typeparamref name="TAggregateServiceInterface"/> is not an interface</exception>
        public static object CreateInstance<TAggregateServiceInterface>(IComponentContext context)
        {
            return CreateInstance(typeof (TAggregateServiceInterface), context);
        }

        ///<summary>
        /// Generate an aggregate service instance that will resolve its types from <paramref name="context"/>.
        ///</summary>
        ///<param name="context">The component context from where types will be resolved</param>
        ///<param name="interfaceType">The interface type for the aggregate service</param>
        ///<returns>The aggregate service instance</returns>
        /// <exception cref="ArgumentException">Thrown if <paramref name="interfaceType"/> is not an interface</exception>
        public static object CreateInstance(Type interfaceType, IComponentContext context)
        {
            if (context == null) throw new ArgumentNullException("context");
            if (interfaceType == null) throw new ArgumentNullException("interfaceType");
            if (!interfaceType.IsInterface) throw new ArgumentException("Type must be an interface", "interfaceType");

            var resolverInterceptor = new ResolvingInterceptor(interfaceType, context);
            return Generator.CreateInterfaceProxyWithoutTarget(interfaceType, resolverInterceptor);
        }
    }
}