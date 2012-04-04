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
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.ModelBinding;
using Autofac.Features.Metadata;

namespace Autofac.Integration.WebApi
{
    /// <summary>
    /// Autofac implementation of the <see cref="ModelBinderProvider"/> class.
    /// </summary>
    public class AutofacModelBinderProvider : ModelBinderProvider
    {
        readonly HttpConfiguration _configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="AutofacModelBinderProvider"/> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        public AutofacModelBinderProvider(HttpConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Metadata key for the supported model types.
        /// </summary>
        internal static readonly string MetadataKey = "SupportedModelTypes";

        /// <summary>
        /// Gets the model binder associated with the model type found in the binding context.
        /// </summary>
        /// <param name="actionContext">The action context.</param>
        /// <param name="bindingContext">The binding context.</param>
        /// <returns>An <see cref="IModelBinder"/> instance if found; otherwise, <c>null</c>.</returns>
        public override IModelBinder GetBinder(HttpActionContext actionContext, ModelBindingContext bindingContext)
        {
            var modelBinders = _configuration.ServiceResolver
                .GetServices(typeof(Meta<Lazy<IModelBinder>>))
                .Cast<Meta<Lazy<IModelBinder>>>();
            var modelType = bindingContext.ModelType;

            var modelBinder = modelBinders
                .Where(binder => binder.Metadata.ContainsKey(MetadataKey))
                .FirstOrDefault(binder => ((List<Type>)binder.Metadata[MetadataKey]).Contains(modelType));
            return (modelBinder != null) ? modelBinder.Value.Value : null;
        }
    }
}