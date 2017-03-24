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
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Autofac.Core;
using Autofac.Util;

namespace Autofac.Features.GeneratedFactories
{
    /// <summary>
    /// Generates context-bound closures that represent factories from
    /// a set of heuristics based on delegate type signatures.
    /// </summary>
    public class FactoryGenerator
    {
        private readonly Func<IComponentContext, IEnumerable<Parameter>, Delegate> _generator;

        /// <summary>
        /// Initializes a new instance of the <see cref="FactoryGenerator"/> class.
        /// </summary>
        /// <param name="service">The service that will be activated in
        /// order to create the products of the factory.</param>
        /// <param name="delegateType">The delegate to provide as a factory.</param>
        /// <param name="parameterMapping">The parameter mapping mode to use.</param>
        public FactoryGenerator(Type delegateType, Service service, ParameterMapping parameterMapping)
        {
            if (service == null) throw new ArgumentNullException(nameof(service));
            Enforce.ArgumentTypeIsFunction(delegateType);

            _generator = CreateGenerator(
                (activatorContextParam, resolveParameterArray) =>
                {
                    // c, service, [new Parameter(name, (object)dps)]*
                    var resolveParams = new[]
                    {
                        activatorContextParam,
                        Expression.Constant(service, typeof(Service)),
                        Expression.NewArrayInit(typeof(Parameter), resolveParameterArray),
                    };

                    // c.Resolve(...)
                    return Expression.Call(
                        ReflectionExtensions.GetMethod<IComponentContext>(cc => cc.ResolveService(default(Service), default(Parameter[]))),
                        resolveParams);
                },
                delegateType,
                GetParameterMapping(delegateType, parameterMapping));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FactoryGenerator"/> class.
        /// </summary>
        /// <param name="productRegistration">The component that will be activated in
        /// order to create the products of the factory.</param>
        /// <param name="delegateType">The delegate to provide as a factory.</param>
        /// <param name="parameterMapping">The parameter mapping mode to use.</param>
        public FactoryGenerator(Type delegateType, IComponentRegistration productRegistration, ParameterMapping parameterMapping)
        {
            if (productRegistration == null) throw new ArgumentNullException(nameof(productRegistration));
            Enforce.ArgumentTypeIsFunction(delegateType);

            _generator = CreateGenerator(
                (activatorContextParam, resolveParameterArray) =>
                {
                    // productRegistration, [new Parameter(name, (object)dps)]*
                    var resolveParams = new Expression[]
                    {
                        Expression.Constant(productRegistration, typeof(IComponentRegistration)),
                        Expression.NewArrayInit(typeof(Parameter), resolveParameterArray),
                    };

                    // c.Resolve(...)
                    return Expression.Call(
                        activatorContextParam,
                        ReflectionExtensions.GetMethod<IComponentContext>(cc => cc.ResolveComponent(default(IComponentRegistration), default(Parameter[]))),
                        resolveParams);
                },
                delegateType,
                GetParameterMapping(delegateType, parameterMapping));
        }

        private static ParameterMapping GetParameterMapping(Type delegateType, ParameterMapping configuredParameterMapping)
        {
            if (configuredParameterMapping == ParameterMapping.Adaptive)
                return DelegateTypeIsFunc(delegateType) ? ParameterMapping.ByType : ParameterMapping.ByName;
            return configuredParameterMapping;
        }

        private static bool DelegateTypeIsFunc(Type delegateType)
        {
            return delegateType.Name.StartsWith("Func`", StringComparison.Ordinal);
        }

        private static Func<IComponentContext, IEnumerable<Parameter>, Delegate> CreateGenerator(Func<Expression, Expression[], Expression> makeResolveCall, Type delegateType, ParameterMapping pm)
        {
            // (c, p) => ([dps]*) => (drt)Resolve(c, productRegistration, [new NamedParameter(name, (object)dps)]*)
            // (c, p)
            var activatorContextParam = Expression.Parameter(typeof(IComponentContext), "c");
            var activatorParamsParam = Expression.Parameter(typeof(IEnumerable<Parameter>), "p");
            var activatorParams = new[] { activatorContextParam, activatorParamsParam };

            var invoke = delegateType.GetTypeInfo().GetDeclaredMethod("Invoke");

            // [dps]*
            var creatorParams = invoke
                .GetParameters()
                .Select(pi => Expression.Parameter(pi.ParameterType, pi.Name))
                .ToList();

            Expression resolveCast = null;
            if (DelegateTypeIsFunc(delegateType) && pm == ParameterMapping.ByType)
            {
                // Issue #269:
                // If we're resolving a Func<X1...XN>() and there are duplicate input parameter types
                // and the parameter mapping is by type, we shouldn't be able to resolve it.
                var arguments = delegateType.GetTypeInfo().GenericTypeArguments;
                var returnType = arguments.Last();

                // Remove the return type to check the list of input types only.
                Array.Resize(ref arguments, arguments.Length - 1);
                if (arguments.Distinct().Count() != arguments.Length)
                {
                    // There are duplicate input types - that's a problem. Throw
                    // when the function is invoked.
                    var message = String.Format(CultureInfo.CurrentCulture, GeneratedFactoryRegistrationSourceResources.DuplicateTypesInTypeMappedFuncParameterList, returnType.AssemblyQualifiedName, String.Join(", ", arguments.Cast<object>().ToArray()));
                    resolveCast = Expression.Throw(Expression.Constant(new DependencyResolutionException(message)), invoke.ReturnType);
                }
            }

            if (resolveCast == null)
            {
                // Issue #269: There aren't duplicate parameter types in the generated
                // factory, so in the case of Func<X1...XN>() typed parameter mapping,
                // if there are duplicate types in the target constructor, both constructor
                // parameters will get the same value passed in. (We don't know
                // the activator, so we can't do much more about it than that.)
                //
                // (drt)
                var resolveParameterArray = MapParameters(creatorParams, pm);
                var resolveCall = makeResolveCall(activatorContextParam, resolveParameterArray);
                resolveCast = Expression.Convert(resolveCall, invoke.ReturnType);
            }

            // ([dps]*) => c.Resolve(service, [new Parameter(name, dps)]*)
            var creator = Expression.Lambda(delegateType, resolveCast, creatorParams);

            // (c, p) => (
            var activator = Expression.Lambda<Func<IComponentContext, IEnumerable<Parameter>, Delegate>>(creator, activatorParams);

            return activator.Compile();
        }

        private static Expression[] MapParameters(IEnumerable<ParameterExpression> creatorParams, ParameterMapping pm)
        {
            switch (pm)
            {
                case ParameterMapping.ByType:
                    return creatorParams
                        .Select(p => Expression.New(typeof(TypedParameter).GetMatchingConstructor(new[] { typeof(Type), typeof(object) }), Expression.Constant(p.Type, typeof(Type)), Expression.Convert(p, typeof(object))))
                        .OfType<Expression>()
                        .ToArray();

                case ParameterMapping.ByPosition:
                    return creatorParams
                        .Select((p, i) => Expression.New(typeof(PositionalParameter).GetMatchingConstructor(new[] { typeof(int), typeof(object) }), Expression.Constant(i, typeof(int)), Expression.Convert(p, typeof(object))))
                        .OfType<Expression>()
                        .ToArray();

                case ParameterMapping.ByName:
                default:
                    return creatorParams
                        .Select(p => Expression.New(typeof(NamedParameter).GetMatchingConstructor(new[] { typeof(string), typeof(object) }), Expression.Constant(p.Name, typeof(string)), Expression.Convert(p, typeof(object))))
                        .OfType<Expression>()
                        .ToArray();
            }
        }

        /// <summary>
        /// Generates a factory delegate that closes over the provided context.
        /// </summary>
        /// <param name="context">The context in which the factory will be used.</param>
        /// <param name="parameters">Parameters provided to the resolve call for the factory itself.</param>
        /// <returns>A factory delegate that will work within the context.</returns>
        public Delegate GenerateFactory(IComponentContext context, IEnumerable<Parameter> parameters)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (parameters == null) throw new ArgumentNullException(nameof(parameters));

            return _generator(context.Resolve<ILifetimeScope>(), parameters);
        }

        /// <summary>
        /// Generates a factory delegate that closes over the provided context.
        /// </summary>
        /// <param name="context">The context in which the factory will be used.</param>
        /// <param name="parameters">Parameters provided to the resolve call for the factory itself.</param>
        /// <returns>A factory delegate that will work within the context.</returns>
        public TDelegate GenerateFactory<TDelegate>(IComponentContext context, IEnumerable<Parameter> parameters)
            where TDelegate : class
        {
            return (TDelegate)(object)GenerateFactory(context, parameters);
        }
    }
}
