// This software is part of the Autofac IoC container
// Copyright (c) 2007 - 2009 Autofac Contributors
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
using System.Linq.Expressions;
using Autofac.Core;
using Autofac.Util;

namespace Autofac.Features.GeneratedFactories
{
    /// <summary>
    /// Determines how the parameters of the delegate type are passed on
    /// to the generated Resolve() call as Parameter objects.
    /// </summary>
    public enum ParameterMapping
    {
        /// <summary>
        /// Chooses parameter mapping based on the factory type.
        /// For Func-based factories this is equivalent to ByType, for all
        /// others ByName will be used.
        /// </summary>
        Adaptive,

        /// <summary>
        /// Pass the parameters supplied to the delegate through to the
        /// underlying registration as NamedParameters based on the parameter
        /// names in the delegate type's formal argument list.
        /// </summary>
        ByName,

        /// <summary>
        /// Pass the parameters supplied to the delegate through to the
        /// underlying registration as TypedParameters based on the parameter
        /// types in the delegate type's formal argument list.
        /// </summary>
        ByType,

        /// <summary>
        /// Pass the parameters supplied to the delegate through to the
        /// underlying registration as PositionalParameters based on the parameter
        /// indices in the delegate type's formal argument list.
        /// </summary>
        ByPosition
    };

    /// <summary>
    /// Generates context-bound closures that represent factories from
    /// a set of heuristics based on delegate type signatures.
    /// </summary>
    public class FactoryGenerator
    {
        readonly Func<IComponentContext, IEnumerable<Parameter>, Delegate> _generator;

        /// <summary>
        /// Create a factory generator.
        /// </summary>
        /// <param name="service">The service that will be resolved in
        /// order to create the products of the factory.</param>
        /// <param name="delegateType">The delegate to provide as a factory.</param>
        /// <param name="parameterMapping">The parameter mapping mode to use.</param>
        public FactoryGenerator(Type delegateType, Service service, ParameterMapping parameterMapping)
        {
            Enforce.ArgumentNotNull(service, "service");
            Enforce.ArgumentTypeIsFunction(delegateType);

            var pm = parameterMapping;
            if (parameterMapping == ParameterMapping.Adaptive)
                pm = delegateType.Name.StartsWith("Func`") ? ParameterMapping.ByType : ParameterMapping.ByName;

            _generator = CreateGenerator(service, delegateType, pm);
        }

        Func<IComponentContext, IEnumerable<Parameter>, Delegate> CreateGenerator(Service service, Type delegateType, ParameterMapping pm)
        {
            // (c, p) => ([dps]*) => (drt)Resolve(c, service, [new NamedParameter(name, (object)dps)]*)

            // (c, p)
            var activatorContextParam = Expression.Parameter(typeof(IComponentContext), "c");
            var activatorParamsParam = Expression.Parameter(typeof(IEnumerable<Parameter>), "p");
            var activatorParams = new[] { activatorContextParam, activatorParamsParam };

            var invoke = delegateType.GetMethod("Invoke");

            // [dps]*
            var creatorParams = invoke
                .GetParameters()
                .Select(pi => Expression.Parameter(pi.ParameterType, pi.Name))
                .ToList();

            Expression[] resolveParameterArray = MapParameters(delegateType, creatorParams, pm);

            // service, [new Parameter(name, (object)dps)]*
            var resolveParams = new Expression[] {
                activatorContextParam,
                Expression.Constant(service),
                Expression.NewArrayInit(typeof(Parameter), resolveParameterArray)
            };

            // c.Resolve(...)
            var resolveCall = Expression.Call(
                typeof(ResolutionExtensions).GetMethod("Resolve", new[] { typeof(IComponentContext), typeof(Service), typeof(Parameter[]) }),
                resolveParams);

            // (drt)
            var resolveCast = Expression.Convert(resolveCall, invoke.ReturnType);

            // ([dps]*) => c.Resolve(service, [new Parameter(name, dps)]*)
            var creator = Expression.Lambda(delegateType, resolveCast, creatorParams);

            // (c, p) => (
            var activator = Expression.Lambda<Func<IComponentContext, IEnumerable<Parameter>, Delegate>>(creator, activatorParams);

            return activator.Compile();
        }

        static Expression[] MapParameters(Type delegateType, List<ParameterExpression> creatorParams, ParameterMapping pm)
        {
            switch (pm)
            {
                case ParameterMapping.ByType:
                    return creatorParams
                            .Select(p => Expression.New(
                                typeof(TypedParameter).GetConstructor(new[] { typeof(Type), typeof(object) }),
                                Expression.Constant(p.Type), Expression.Convert(p, typeof(object))))
                            .OfType<Expression>()
                            .ToArray();

                case ParameterMapping.ByPosition:
                    return creatorParams
                        .Select((p, i) => Expression.New(
                                typeof(PositionalParameter).GetConstructor(new[] { typeof(int), typeof(object) }),
                                Expression.Constant(i), Expression.Convert(p, typeof(object))))
                            .OfType<Expression>()
                            .ToArray();

                case ParameterMapping.ByName:
                default:
                    return creatorParams
                            .Select(p => Expression.New(
                                typeof(NamedParameter).GetConstructor(new[] { typeof(string), typeof(object) }),
                                Expression.Constant(p.Name), Expression.Convert(p, typeof(object))))
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
            Enforce.ArgumentNotNull(context, "context");
            Enforce.ArgumentNotNull(parameters, "parameters");

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
