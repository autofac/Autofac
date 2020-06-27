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
using System.Linq.Expressions;
using System.Reflection;

namespace Autofac.Core.Activators.Reflection
{
    /// <summary>
    /// Provides the functionality to bind a single constructor at resolve time.
    /// </summary>
    public class ConstructorBinder
    {
        private static readonly Func<ConstructorInfo, Func<object?[], object>> _factoryBuilder = GetConstructorInvoker;

        private static ConcurrentDictionary<ConstructorInfo, Func<object?[], object>> _factoryCache = new ConcurrentDictionary<ConstructorInfo, Func<object?[], object>>();

        private readonly ParameterInfo[] _constructorArgs;
        private readonly Func<object?[], object>? _factory;
        private readonly ParameterInfo? _illegalParameter;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConstructorBinder"/> class.
        /// </summary>
        /// <param name="constructorInfo">The constructor.</param>
        public ConstructorBinder(ConstructorInfo constructorInfo)
        {
            Constructor = constructorInfo ?? throw new ArgumentNullException(nameof(constructorInfo));
            _constructorArgs = constructorInfo.GetParameters();

            // If any of the parameters are unsafe, do not create an invoker, and store the parameter
            // that broke the rule.
            _illegalParameter = DetectIllegalParameter(_constructorArgs);

            if (_illegalParameter is null)
            {
                // Build the invoker.
                _factory = _factoryCache.GetOrAdd(constructorInfo, _factoryBuilder);
            }
        }

        /// <summary>
        /// Gets the constructor this binder is responsible for binding.
        /// </summary>
        public ConstructorInfo Constructor { get; }

        /// <summary>
        /// Gets the set of parameters to bind against.
        /// </summary>
        public IReadOnlyList<ParameterInfo> Parameters => _constructorArgs;

        /// <summary>
        /// Gets the number of parameters.
        /// </summary>
        public int ParameterCount => _constructorArgs.Length;

        /// <summary>
        /// Binds the set of parameters to the constructor. <see cref="BoundConstructor.CanInstantiate"/> indicates success.
        /// </summary>
        /// <param name="availableParameters">The set of all parameters.</param>
        /// <param name="context">The current component context.</param>
        /// <returns>The bind result.</returns>
        public BoundConstructor Bind(IEnumerable<Parameter> availableParameters, IComponentContext context)
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
            var constructorArgLength = constructorArgs.Length;

            if (constructorArgLength == 0)
            {
                // No args, auto-bind with an empty value-retriever array to avoid the allocation.
                return BoundConstructor.ForBindSuccess(this, _factory!, Array.Empty<Func<object?>>());
            }

            if (_illegalParameter is object)
            {
                return BoundConstructor.ForBindFailure(this, _illegalParameter);
            }

            var valueRetrievers = new Func<object?>[constructorArgLength];

            for (var idx = 0; idx < constructorArgLength; idx++)
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
                    return BoundConstructor.ForBindFailure(this, pi);
                }
            }

            return BoundConstructor.ForBindSuccess(this, _factory!, valueRetrievers);
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
    }
}
