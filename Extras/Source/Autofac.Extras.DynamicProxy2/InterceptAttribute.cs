// This software is part of the Autofac IoC container
// Copyright © 2013 Autofac Contributors
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
using System.Diagnostics.CodeAnalysis;
using System.Security;
using Autofac.Core;
using Castle.DynamicProxy;

namespace Autofac.Extras.DynamicProxy2
{
    /// <summary>
    /// Indicates that a type should be intercepted.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1019:DefineAccessorsForAttributeArguments")]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
    public sealed class InterceptAttribute : Attribute
    {
        /// <summary>
        /// Gets the interceptor service.
        /// </summary>
        public Service InterceptorService { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="InterceptAttribute"/> class.
        /// </summary>
        /// <param name="interceptorService">The interceptor service.</param>
        /// <exception cref="System.ArgumentNullException">interceptorService</exception>
        public InterceptAttribute(Service interceptorService)
        {
            if (interceptorService == null)
                throw new ArgumentNullException("interceptorService");

            InterceptorService = interceptorService;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InterceptAttribute"/> class.
        /// </summary>
        /// <param name="interceptorServiceName">Name of the interceptor service.</param>
        [SecuritySafeCritical]
        public InterceptAttribute(string interceptorServiceName)
            : this(new KeyedService(interceptorServiceName, typeof(IInterceptor)))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InterceptAttribute"/> class.
        /// </summary>
        /// <param name="interceptorServiceType">The typed interceptor service.</param>
        public InterceptAttribute(Type interceptorServiceType)
            : this(new TypedService(interceptorServiceType))
        {
        }
    }
}
