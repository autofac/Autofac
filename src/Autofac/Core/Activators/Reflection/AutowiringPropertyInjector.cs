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
using System.Reflection;
using Autofac.Util;

namespace Autofac.Core.Activators.Reflection
{
    internal class AutowiringPropertyInjector
    {
        public const string InstanceTypeNamedParameter = "Autofac.AutowiringPropertyInjector.InstanceType";

        public static void InjectProperties(IComponentContext context, object instance, IPropertySelector propertySelector, IEnumerable<Parameter> parameters)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            if (propertySelector == null)
            {
                throw new ArgumentNullException(nameof(propertySelector));
            }

            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            var instanceType = instance.GetType();

            foreach (var property in instanceType
                .GetRuntimeProperties()
                .Where(pi => pi.CanWrite))
            {
                var propertyType = property.PropertyType;

                if (propertyType.GetTypeInfo().IsValueType && !propertyType.GetTypeInfo().IsEnum)
                {
                    continue;
                }

                if (propertyType.IsArray && propertyType.GetElementType().GetTypeInfo().IsValueType)
                {
                    continue;
                }

                if (propertyType.IsGenericEnumerableInterfaceType() && propertyType.GetTypeInfo().GenericTypeArguments[0].GetTypeInfo().IsValueType)
                {
                    continue;
                }

                if (property.GetIndexParameters().Length != 0)
                {
                    continue;
                }

                if (!propertySelector.InjectProperty(property, instance))
                {
                    continue;
                }

                var setParameter = property.SetMethod.GetParameters().First();
                var valueProvider = (Func<object>)null;
                var parameter = parameters.FirstOrDefault(p => p.CanSupplyValue(setParameter, context, out valueProvider));
                if (parameter != null)
                {
                    property.SetValue(instance, valueProvider(), null);
                    continue;
                }

                object propertyValue;
                var propertyService = new TypedService(propertyType);
                var instanceTypeParameter = new NamedParameter(InstanceTypeNamedParameter, instanceType);
                if (context.TryResolveService(propertyService, new Parameter[] { instanceTypeParameter }, out propertyValue))
                {
                    property.SetValue(instance, propertyValue, null);
                }
            }
        }
    }
}
