// This software is part of the Autofac IoC container
// Copyright (c) 2007 - 2008 Autofac Contributors
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
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Autofac.Builder;
using Autofac.Component;
using Autofac.Registrars;

namespace Autofac.Extras.GeneratedFactories
{
    /// <summary>
    /// Extends ContainerBuilder with methods to create generated factories.
    /// </summary>
    public static class ContainerBuilderExtensions
    {
        /// <summary>
        /// Registers the factory delegate.
        /// </summary>
        /// <typeparam name="TDelegate">The type of the delegate.</typeparam>
        /// <param name="builder">The builder.</param>
        /// <param name="service">The service that the delegate will return instances of.</param>
        /// <returns>A registrar allowing configuration to continue.</returns>
        public static IConcreteRegistrar RegisterGeneratedFactory<TDelegate>(this ContainerBuilder builder, Service service)
        {
            if (builder == null)
                throw new ArgumentNullException("builder");

            if (service == null)
                throw new ArgumentNullException("service");

            var factoryDelegate = GenerateDelegate(typeof(TDelegate), service);

            return builder.Register<TDelegate>((c,p) => (TDelegate)(factoryDelegate(c,p))).WithScope(InstanceScope.Container);
        }
        /// <summary>
        /// Registers the factory delegate.
        /// </summary>
        /// <typeparam name="TDelegate">The type of the delegate.</typeparam>
        /// <param name="builder">The builder.</param>
        /// <param name="service">The service that the delegate will return instances of.</param>
        /// <returns>A registrar allowing configuration to continue.</returns>
        public static IConcreteRegistrar RegisterGeneratedFactory<TDelegate>(this ContainerBuilder builder)
        {
            if (builder == null)
                throw new ArgumentNullException("builder");

            EnforceTypeIsDelegate(typeof(TDelegate));
            var returnType = typeof(TDelegate).GetMethod("Invoke").ReturnType;
            return RegisterGeneratedFactory<TDelegate>(builder, new TypedService(returnType));
        }

        private static ComponentActivator GenerateDelegate(Type delegateType, Service service)
        {
            if (delegateType == null)
                throw new ArgumentNullException("delegateType");

            if (service == null)
                throw new ArgumentNullException("service");

            EnforceTypeIsDelegate(delegateType);

            // (c, p) => ([dps]*) => (drt)c.Resolve(service, [new Parameter(name, (object)dps)]*)

            // (c, p)
            var activatorContextParam = Expression.Parameter(typeof(IContext), "c");
            var activatorParamsParam = Expression.Parameter(typeof(IActivationParameters), "p");
            var activatorParams = new[] { activatorContextParam, activatorParamsParam };

            var invoke = delegateType.GetMethod("Invoke");

            // [dps]*
            var creatorParams = invoke
                .GetParameters()
                .Select(pi => Expression.Parameter(pi.ParameterType, pi.Name))
                .ToList();

            // service, [new Parameter(name, (object)dps)]*
            var resolveParams = new Expression[] {
                Expression.Constant(service),
                Expression.NewArrayInit(typeof(Parameter),
                    creatorParams
                        .Select(p => Expression.New(
                            typeof(Parameter).GetConstructor(new[] { typeof(string), typeof(object) }),
                            Expression.Constant(p.Name), Expression.Convert(p, typeof(object))))
                        .OfType<Expression>()
                        .ToArray())
            };

            // c.Resolve(...)
            var resolveCall = Expression.Call(
                activatorContextParam,
                typeof(IContext).GetMethod("Resolve", new[] { typeof(Service), typeof(Parameter[]) }),
                resolveParams);

            // (drt)
            var resolveCast = Expression.Convert(resolveCall, invoke.ReturnType);

            // ([dps]*) => c.Resolve(service, [new Parameter(name, dps)]*)
            var creator = Expression.Lambda(delegateType, resolveCast, creatorParams);

            // (c, p) => (
            var activator = Expression.Lambda<ComponentActivator>(creator, activatorParams);

            return activator.Compile();
        }

        private static void EnforceTypeIsDelegate(Type delegateType)
        {
            if (delegateType == null)
                throw new ArgumentNullException("delegateType");

            MethodInfo invoke = delegateType.GetMethod("Invoke");
            if (invoke == null)
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture,
                    ContainerBuilderExtensionsResources.TypeIsNotADelegate, delegateType));
            else if (invoke.ReturnType == typeof(void))
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture,
                    ContainerBuilderExtensionsResources.DelegateReturnsVoid, delegateType));
        }
    }
}
