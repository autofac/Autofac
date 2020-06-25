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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autofac.Util;

namespace Autofac.Core.Activators.Reflection
{
    /// <summary>
    /// Provide helper methods for injecting property values.
    /// </summary>
    internal static class AutowiringPropertyInjector
    {
        /// <summary>
        /// Name of the parameter containing the instance type provided when resolving an injected service.
        /// </summary>
        internal const string InstanceTypeNamedParameter = "Autofac.AutowiringPropertyInjector.InstanceType";

        private static readonly ConcurrentDictionary<PropertyInfo, Action<object, object?>> PropertySetters =
            new ConcurrentDictionary<PropertyInfo, Action<object, object?>>();

        private static readonly ConcurrentDictionary<Type, PropertyInfo[]> InjectableProperties =
            new ConcurrentDictionary<Type, PropertyInfo[]>();

        private static readonly MethodInfo CallPropertySetterOpenGenericMethod =
            typeof(AutowiringPropertyInjector).GetDeclaredMethod(nameof(CallPropertySetter));

        /// <summary>
        /// Inject properties onto an instance, filtered by a property selector.
        /// </summary>
        /// <param name="context">The component context to resolve dependencies from.</param>
        /// <param name="instance">The instance to inject onto.</param>
        /// <param name="propertySelector">The property selector.</param>
        /// <param name="parameters">The set of parameters for the resolve that can be used to satisfy injectable properties.</param>
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

            var resolveParameters = parameters as Parameter[] ?? parameters.ToArray();

            var instanceType = instance.GetType();
            var injectableProperties = InjectableProperties.GetOrAdd(instanceType, type => GetInjectableProperties(type).ToArray());

            for (var index = 0; index < injectableProperties.Length; index++)
            {
                var property = injectableProperties[index];

                if (!propertySelector.InjectProperty(property, instance))
                {
                    continue;
                }

                var setParameter = property.SetMethod.GetParameters()[0];
                var valueProvider = (Func<object?>?)null;
                var parameter = resolveParameters.FirstOrDefault(p => p.CanSupplyValue(setParameter, context, out valueProvider));
                if (parameter != null)
                {
                    var setter = PropertySetters.GetOrAdd(property, MakeFastPropertySetter);
                    setter(instance, valueProvider!());
                    continue;
                }

                var propertyService = new TypedService(property.PropertyType);
                var instanceTypeParameter = new NamedParameter(InstanceTypeNamedParameter, instanceType);
                if (context.TryResolveService(propertyService, new Parameter[] { instanceTypeParameter }, out var propertyValue))
                {
                    var setter = PropertySetters.GetOrAdd(property, MakeFastPropertySetter);
                    setter(instance, propertyValue);
                }
            }
        }

        private static IEnumerable<PropertyInfo> GetInjectableProperties(Type instanceType)
        {
            foreach (var property in instanceType.GetRuntimeProperties())
            {
                if (!property.CanWrite)
                {
                    continue;
                }

                // SetMethod will be non-null if CanWrite is true.
                // Don't want to inject onto static properties.
                if (property.SetMethod.IsStatic)
                {
                    continue;
                }

                var propertyType = property.PropertyType;

                if (propertyType.IsValueType && !propertyType.IsEnum)
                {
                    continue;
                }

                if (propertyType.IsArray && propertyType.GetElementType().IsValueType)
                {
                    continue;
                }

                if (propertyType.IsGenericEnumerableInterfaceType() && propertyType.GenericTypeArguments[0].IsValueType)
                {
                    continue;
                }

                if (property.GetIndexParameters().Length != 0)
                {
                    continue;
                }

                yield return property;
            }
        }

        private static Action<object, object?> MakeFastPropertySetter(PropertyInfo propertyInfo)
        {
            var setMethod = propertyInfo.SetMethod;
            var typeInput = setMethod.DeclaringType;
            var parameters = setMethod.GetParameters();
            var parameterType = parameters[0].ParameterType;

            // Create a delegate TDeclaringType -> { TDeclaringType.Property = TValue; }
            var propertySetterAsAction = setMethod.CreateDelegate(typeof(Action<,>).MakeGenericType(typeInput, parameterType));
            var callPropertySetterClosedGenericMethod = CallPropertySetterOpenGenericMethod.MakeGenericMethod(typeInput, parameterType);
            var callPropertySetterDelegate = callPropertySetterClosedGenericMethod.CreateDelegate<Action<object, object?>>(propertySetterAsAction);

            return callPropertySetterDelegate;
        }

        private static void CallPropertySetter<TDeclaringType, TValue>(
            Action<TDeclaringType, TValue> setter, object target, object value) =>
                setter((TDeclaringType)target, (TValue)value);
    }
}
