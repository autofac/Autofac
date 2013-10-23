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
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using Autofac.Features.Metadata;

namespace Autofac.Integration.WebApi
{
    /// <summary>
    /// Autofac implementation of the <see cref="ModelBinderProvider"/> class.
    /// </summary>
    [SecurityCritical]
    public class AutofacWebApiModelBinderProvider : ModelBinderProvider
    {
        /// <summary>
        /// Metadata key for the supported model types.
        /// </summary>
        internal static readonly string MetadataKey = "SupportedModelTypes";

        /// <summary>
        /// Find a binder for the given type.
        /// </summary>
        /// <param name="configuration">A configuration object.</param>
        /// <param name="modelType">The type of the model to bind against.</param>
        /// <returns>A binder, which can attempt to bind this type. Or null if the binder knows statically that it will never be able to bind the type.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="configuration" /> is <see langword="null" />.
        /// </exception>
        [SecurityCritical]
        public override IModelBinder GetBinder(HttpConfiguration configuration, Type modelType)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }
            var modelBinders = configuration.DependencyResolver
                .GetServices(typeof(Meta<Lazy<IModelBinder>>))
                .Cast<Meta<Lazy<IModelBinder>>>();

            var modelBinder = modelBinders
                .Where(binder => binder.Metadata.ContainsKey(MetadataKey))
                .FirstOrDefault(binder => ((List<Type>)binder.Metadata[MetadataKey]).Contains(modelType));
            return (modelBinder != null) ? modelBinder.Value.Value : null;
        }
    }
}