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
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;

namespace Autofac.Core.Activators.Reflection
{
    public readonly struct BoundConstructor
    {
        private readonly Func<object?[], object>? _factory;
        private readonly Func<object?>[]? _valueRetrievers;
        private readonly ParameterInfo? _firstNonBindableParameter;

        public BoundConstructor(ConstructorInfo constructor, Func<object?[], object> factory, Func<object?>[] valueRetrievers)
        {
            CanInstantiate = true;
            TargetConstructor = constructor;
            _factory = factory;
            _valueRetrievers = valueRetrievers;
            _firstNonBindableParameter = null;
        }

        public BoundConstructor(ConstructorInfo constructor, ParameterInfo firstNonBindableParameter)
        {
            CanInstantiate = false;
            TargetConstructor = constructor;
            _firstNonBindableParameter = firstNonBindableParameter;
            _factory = null;
            _valueRetrievers = null;
        }

        /// <summary>
        /// Gets the constructor on the target type. The actual constructor used
        /// might differ, e.g. if using a dynamic proxy.
        /// </summary>
        public ConstructorInfo TargetConstructor { get; }

        /// <summary>
        /// Gets a value indicating whether the binding is valid.
        /// </summary>
        public bool CanInstantiate { get; }

        /// <summary>
        /// Invoke the constructor with the parameter bindings.
        /// </summary>
        /// <returns>The constructed instance.</returns>
        public object Instantiate()
        {
            if (!CanInstantiate)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, ConstructorParameterBindingResources.CannotInstantitate, this.Description));
            }

            var values = new object?[_valueRetrievers!.Length];
            for (var i = 0; i < _valueRetrievers.Length; ++i)
            {
                values[i] = _valueRetrievers[i]();
            }

            try
            {
                return _factory!(values);
            }
            catch (TargetInvocationException ex)
            {
                throw new DependencyResolutionException(string.Format(CultureInfo.CurrentCulture, ConstructorParameterBindingResources.ExceptionDuringInstantiation, TargetConstructor, TargetConstructor.DeclaringType.Name), ex.InnerException);
            }
            catch (Exception ex)
            {
                throw new DependencyResolutionException(string.Format(CultureInfo.CurrentCulture, ConstructorParameterBindingResources.ExceptionDuringInstantiation, TargetConstructor, TargetConstructor.DeclaringType.Name), ex);
            }
        }

        /// <summary>
        /// Gets a description of the constructor parameter binding.
        /// </summary>
        public string Description => CanInstantiate
            ? string.Format(CultureInfo.CurrentCulture, ConstructorParameterBindingResources.BoundConstructor, TargetConstructor)
            : string.Format(CultureInfo.CurrentCulture, ConstructorParameterBindingResources.NonBindableConstructor, TargetConstructor, _firstNonBindableParameter);

        /// <summary>Returns a System.String that represents the current System.Object.</summary>
        /// <returns>A System.String that represents the current System.Object.</returns>
        public override string ToString()
        {
            return Description;
        }
    }

    public class ConstructorBinder
    {
        private readonly ConstructorInfo _constructor;
        private readonly ParameterInfo[] _constructorArgs;
        private readonly Func<object?[], object>? _factory;
        private readonly ParameterInfo? _illegalParameter;

        public ConstructorBinder(ConstructorInfo constructorInfo)
        {
            _constructor = constructorInfo ?? throw new ArgumentNullException(nameof(constructorInfo));
            _constructorArgs = constructorInfo.GetParameters();

            // If any of the parameters are unsafe, do not create an invoker, and store the parameter
            // that broke the rule.
            _illegalParameter = DetectIllegalParameter(_constructorArgs);

            if (_illegalParameter is null)
            {
                // Build the invoker.
                _factory = GetConstructorInvoker(constructorInfo);
            }
        }

        private static ParameterInfo? DetectIllegalParameter(ParameterInfo[] constructorArgs)
        {
            for (var idx = 0; idx < constructorArgs.Length; idx++)
            {
                if (constructorArgs[idx].ParameterType.IsPointer)
                {
                    // Boo.
                    return constructorArgs[idx];
                }
            }

            return null;
        }

        public BoundConstructor TryBind(IEnumerable<Parameter> availableParameters, IComponentContext context)
        {
            if (availableParameters is null)
            {
                throw new ArgumentNullException(nameof(availableParameters));
            }

            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var constructorArgs = _constructorArgs;

            if (_constructorArgs.Length == 0)
            {
                // No args, auto-bind with an empty value-retriever array to avoid the allocation.
                return new BoundConstructor(_constructor, _factory, Array.Empty<Func<object?>>());
            }

            if (_illegalParameter is object)
            {
                return new BoundConstructor(_constructor, _illegalParameter);
            }

            var valueRetrievers = new Func<object?>[constructorArgs.Length];

            for (var idx = 0; idx < constructorArgs.Length; idx++)
            {
                var pi = constructorArgs[idx];
                var foundValue = false;

                foreach (var param in availableParameters)
                {
                    if (param.CanSupplyValue(pi, context, out var valueRetriever))
                    {
                        valueRetrievers[idx] = valueRetriever;
                        foundValue = true;
                        break;
                    }
                }

                if (!foundValue)
                {
                    return new BoundConstructor(_constructor, pi);
                }
            }

            return new BoundConstructor(_constructor, _factory, valueRetrievers);
        }

        private static Func<object?[], object> GetConstructorInvoker(ConstructorInfo constructorInfo)
        {
            var paramsInfo = constructorInfo.GetParameters();

            var parametersExpression = Expression.Parameter(typeof(object[]), "args");
            var argumentsExpression = new Expression[paramsInfo.Length];

            for (int paramIndex = 0; paramIndex < paramsInfo.Length; paramIndex++)
            {
                var indexExpression = Expression.Constant(paramIndex);
                var parameterType = paramsInfo[paramIndex].ParameterType;

                var parameterIndexExpression = Expression.ArrayIndex(parametersExpression, indexExpression);

                if (parameterType.IsPointer)
                {
                    // We can't do anything with pointer arguments.
                    argumentsExpression[paramIndex] = Expression.Default(parameterType);
                    continue;
                }

                var convertExpression = parameterType.IsPrimitive
                    ? Expression.Convert(ConvertPrimitiveType(parameterIndexExpression, parameterType), parameterType)
                    : Expression.Convert(parameterIndexExpression, parameterType);

                if (!parameterType.IsValueType)
                {
                    argumentsExpression[paramIndex] = convertExpression;
                    continue;
                }

                var nullConditionExpression = Expression.Equal(
                    parameterIndexExpression, Expression.Constant(null));
                argumentsExpression[paramIndex] = Expression.Condition(
                    nullConditionExpression, Expression.Default(parameterType), convertExpression);
            }

            var newExpression = Expression.New(constructorInfo, argumentsExpression);
            var lambdaExpression = Expression.Lambda<Func<object?[], object>>(newExpression, parametersExpression);

            return lambdaExpression.Compile();
        }

        private static MethodCallExpression ConvertPrimitiveType(Expression valueExpression, Type conversionType)
        {
            var changeTypeMethod = typeof(Convert).GetRuntimeMethod(nameof(Convert.ChangeType), new[] { typeof(object), typeof(Type) });
            return Expression.Call(changeTypeMethod, valueExpression, Expression.Constant(conversionType));
        }
    }
}
