// This software is part of the Autofac IoC container
// Copyright (c) 2010 Autofac Contributors
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
using Autofac.Builder;
using Autofac.Core;
using Autofac.Util;
using System.Diagnostics;

namespace Autofac.Features.GeneratedFactories
{
    class GeneratedFactoryRegistrationSource : IRegistrationSource
    {
#if !WINDOWS_PHONE
        /// <summary>
        /// Retrieve registrations for an unregistered service, to be used
        /// by the container.
        /// </summary>
        /// <param name="service">The service that was requested.</param>
        /// <param name="registrationAccessor">A function that will return existing registrations for a service.</param>
        /// <returns>Registrations providing the service.</returns>
        public IEnumerable<IComponentRegistration> RegistrationsFor(Service service, Func<Service, IEnumerable<IComponentRegistration>> registrationAccessor)
        {
            Enforce.ArgumentNotNull(service, "service");
            Enforce.ArgumentNotNull(registrationAccessor, "registrationAccessor");

            var ts = service as IServiceWithType;
            if (ts != null && ts.ServiceType.IsDelegate())
            {
                var resultType = ts.ServiceType.FunctionReturnType();
                var resultTypeService = ts.ChangeType(resultType);

                return registrationAccessor(resultTypeService)
                    .Select(r =>
                    {
                        var factory = new FactoryGenerator(ts.ServiceType, r, ParameterMapping.Adaptive);

                        var rb = RegistrationBuilder.ForDelegate(ts.ServiceType, factory.GenerateFactory)
                            .InstancePerLifetimeScope()
                            .ExternallyOwned()
                            .As(service)
                            .Targeting(r.Target);

                        return rb.CreateRegistration();
                    });
            }

            return Enumerable.Empty<IComponentRegistration>();
        }

        public bool IsAdapterForIndividualComponents
        {
            get { return true; }
        }
#else
        static readonly MethodInfo[] createFuncRegistration =
            typeof(GeneratedFactoryRegistrationSource)
            .GetMethods(BindingFlags.NonPublic | BindingFlags.Static)
            .Where(x => x.Name == "CreateFuncRegistration")
            .ToArray();

        static Type[] _funcTypes = new Type[]  { typeof(Func<>), typeof(Func<,>), typeof(Func<,,>) };

        public IEnumerable<IComponentRegistration> RegistrationsFor(
            Service service,
            Func<Service, IEnumerable<IComponentRegistration>> registrationAccessor)
        {
            Debug.WriteLine("Querying source.");

            var swt = service as IServiceWithType;
            if (swt != null &&
                swt.ServiceType.IsGenericType &&
                _funcTypes.Contains(swt.ServiceType.GetGenericTypeDefinition()))
            {
                var args = swt.ServiceType.GetGenericArguments();

                //Find a func method that matches all args
                var funcRegistration = createFuncRegistration
                    .First(x => x.GetGenericArguments().Count() == args.Count());

                var creator = funcRegistration.MakeGenericMethod(args);

                return registrationAccessor(new TypedService(args.Last()))
                     .Select(cr => (IComponentRegistration)creator.Invoke(null, new object[] { cr }));

            }
            else if (swt != null && swt.ServiceType.IsDelegate())
            {
                var delegateMethod = swt.ServiceType.GetMethods().First();

                var args = delegateMethod
                    .GetParameters()
                    .Select(x => x.ParameterType)
                    .ToList();
                args.Add(delegateMethod.ReturnType);

                //Find a func method that matches all args
                var funcRegistration = createFuncRegistration
                    .First(x => x.GetGenericArguments().Count() == args.Count());

                var creator = funcRegistration.MakeGenericMethod(args.ToArray());

                return registrationAccessor(new TypedService(args.Last()))
                     .Select(cr => (IComponentRegistration)creator.Invoke(null, new object[] { cr }));
            }

            return Enumerable.Empty<IComponentRegistration>();
        }

        public bool IsAdapterForIndividualComponents
        {
            get { return true; }
        }

        //static IComponentRegistration CreateFuncRegistration<TDelegate, TResult>(IComponentRegistration target, MethodInfo method)
        //{
        //    Debug.WriteLine("Creating registration.");

        //    return RegistrationBuilder
        //        .ForDelegate(typeof(TDelegate),  (c, p) =>
        //                                    {
        //                                        var del = Delegate.CreateDelegate(thingthatusesdelegate, method);
        //                                        var ls = c.Resolve<ILifetimeScope>();

        //                                        Func<TResult> del1 = () =>
        //                                        {
        //                                            Debug.WriteLine("Invoking Func");
        //                                            var r = (TResult)ls.ResolveComponent(target, p);
        //                                            return r;
        //                                        };

        //                                        return del1;
        //                                    }).Targeting(target)
        //                                      .CreateRegistration();


        //    //(c, p) =>
        //    //{
        //    //    Debug.WriteLine("Building Func activator.");
        //    //    var ls = c.Resolve<ILifetimeScope>();

        //    //    TDelegate del  = (a0, a1) =>
        //    //    {
        //    //        Debug.WriteLine("Invoking Func");
        //    //        var r = (TResult)ls.ResolveComponent(target, new Parameter[]{});
        //    //        return r;
        //    //    };

        //    //    return del;
        //    //})
        //    //.Targeting(target)
        //    //.CreateRegistration();
        //}

        static IComponentRegistration CreateFuncRegistration<TArg0, TResult>(IComponentRegistration target)
        {
            Debug.WriteLine("Creating registration.");

            return RegistrationBuilder
                .ForDelegate<Func<TArg0, TResult>>((c, p) =>
                {
                    Debug.WriteLine("Building Func activator.");
                    var ls = c.Resolve<ILifetimeScope>();
                    return a0 =>
                    {
                        Debug.WriteLine("Invoking Func with {0}", a0);
                        var r = (TResult)ls.ResolveComponent(target, new[] { TypedParameter.From(a0) });
                        return r;
                    };
                })
                .Targeting(target)
                .CreateRegistration();
        }

        static IComponentRegistration CreateFuncRegistration<TArg0, TArg1, TResult>(IComponentRegistration target)
        {
            Debug.WriteLine("Creating registration.");

            return RegistrationBuilder
                .ForDelegate<Func<TArg0, TArg1, TResult>>((c, p) =>
                {
                    Debug.WriteLine("Building Func activator.");
                    var ls = c.Resolve<ILifetimeScope>();
                    return (a0, a1) =>
                    {
                        Debug.WriteLine("Invoking Func with {0} and {1}", a0, a1);
                        var r = (TResult)ls.ResolveComponent(target, new[] { TypedParameter.From(a0), TypedParameter.From(a1) });
                        return r;
                    };
                })
                .Targeting(target)
                .CreateRegistration();
        }
#endif
    }
}
