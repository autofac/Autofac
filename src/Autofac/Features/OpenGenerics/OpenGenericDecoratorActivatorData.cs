// This software is part of the Autofac IoC container
// Copyright © 2011 Autofac Contributors
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
using System.Globalization;
using System.Reflection;
using Autofac.Builder;
using Autofac.Core;

namespace Autofac.Features.OpenGenerics
{
    /// <summary>
    /// Describes the activator for an open generic decorator.
    /// </summary>
    public class OpenGenericDecoratorActivatorData : ReflectionActivatorData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OpenGenericDecoratorActivatorData"/> class.
        /// </summary>
        /// <param name="implementer">The decorator type.</param>
        /// <param name="fromService">The open generic service type to decorate.</param>
        public OpenGenericDecoratorActivatorData(Type implementer, IServiceWithType fromService)
            : base(implementer)
        {
            if (fromService == null) throw new ArgumentNullException(nameof(fromService));
            if (!fromService.ServiceType.GetTypeInfo().IsGenericTypeDefinition)
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, OpenGenericDecoratorActivatorDataResources.DecoratedServiceIsNotOpenGeneric, fromService));

            FromService = fromService;
        }

        /// <summary>
        /// Gets the open generic service type to decorate.
        /// </summary>
        public IServiceWithType FromService { get; }
    }
}
