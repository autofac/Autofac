using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Autofac.Core;
using Autofac.Features.GeneratedFactories;

namespace Autofac.Util.WindowsPhone
{
    /// <summary>
    /// Generates context-bound closures that represent factories from
    /// a set of heuristics based on delegate type signatures.
    /// </summary>
    public class Wp7FactoryGenerator
    {
        static readonly MethodInfo[] DelegateActivators = typeof(Wp7FactoryGenerator)
            .GetMethods(BindingFlags.NonPublic | BindingFlags.Static)
            .Where(x => x.Name == "DelegateActivator")
            .ToArray();

        Func<IComponentContext, IEnumerable<Parameter>, Delegate> _generator;

        ///<summary />
        public Wp7FactoryGenerator(Type delegateType, Service service, ParameterMapping parameterMapping)
        {
            if (service == null) throw new ArgumentNullException("service");
            Enforce.ArgumentTypeIsFunction(delegateType);

            CreateGenerator(service, delegateType, parameterMapping);
        }

        ///<summary />
        public Wp7FactoryGenerator(Type delegateType, IComponentRegistration service, ParameterMapping parameterMapping)
        {
            if(service == null)
                throw new ArgumentNullException("service");
            Enforce.ArgumentTypeIsFunction(delegateType);

            CreateGenerator(service, delegateType, parameterMapping);
        }

        void CreateGenerator(object service, Type delegateType, ParameterMapping parameterMapping)
        {
            var resultType = delegateType.FunctionReturnType();
            var invoke = delegateType.GetMethod("Invoke");
            var args = invoke.GetParameters().Select(x => x.ParameterType)
                .Append(resultType)
                .Append(delegateType)
                .ToList();

            //Find a func method that matches all args
            var funcRegistration = DelegateActivators
                .FirstOrDefault(x => x.GetGenericArguments().Count() == args.Count());

            if(funcRegistration != null)
            {
                var creator = funcRegistration.MakeGenericMethod(args.ToArray());

                _generator = (a0, a1) =>
                {
                    return (Delegate)creator.Invoke(null, new[] { a0, service, parameterMapping.ResolveParameterMapping(delegateType) });
                };
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
            if (context == null) throw new ArgumentNullException("context");
            if (parameters == null) throw new ArgumentNullException("parameters");

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

        // ReSharper disable UnusedMember.Local
        // ReSharper disable UnusedParameter.Local
        
        static Delegate DelegateActivator<TResult, TDelegate>(IComponentContext context, object target, ParameterMapping mapping)
        {
            if(!(target is Service || target is IComponentRegistration))
                throw new ArgumentException("target");
            var ls = context.Resolve<ILifetimeScope>();
            Func<TResult> del1 = () =>
            {
                Debug.WriteLine("Invoking Delegate Func<TResult>");
                object resolved ;
                if(target is Service)
                    resolved = ls.ResolveService((Service)target, new List<Parameter>());
                else
                    resolved = ls.ResolveComponent((IComponentRegistration)target, new List<Parameter>());
                return (TResult)resolved;
            };
            var del = Delegate.CreateDelegate(typeof(TDelegate), del1.Target, del1.Method, true);
            return del;
        }
        // ReSharper restore UnusedParameter.Local

        static Delegate DelegateActivator<TArg0, TResult, TDelegate>(IComponentContext context, object target, ParameterMapping mapping)
        {
            if(!(target is Service || target is IComponentRegistration))
                throw new ArgumentException("target");
            var ls = context.Resolve<ILifetimeScope>();
            Func<TArg0, TResult> del1 = a0 =>
            {
                Debug.WriteLine("Invoking Delegate Func<TArg0, TResult>");
                var parameterCollection = mapping.GetParameterCollection<TDelegate>(a0);
                object resolved;
                if(target is Service)
                    resolved = ls.ResolveService((Service)target, parameterCollection);
                else
                    resolved = ls.ResolveComponent((IComponentRegistration)target, parameterCollection);
                return (TResult)resolved;
            };
            var del = Delegate.CreateDelegate(typeof(TDelegate), del1.Target, del1.Method, true);
            return del;
        }

        static Delegate DelegateActivator<TArg0, TArg1, TResult, TDelegate>(IComponentContext context, object target, ParameterMapping mapping)
        {
            if(!(target is Service || target is IComponentRegistration))
                throw new ArgumentException("target");
            var ls = context.Resolve<ILifetimeScope>();
            Func<TArg0, TArg1, TResult> del1 = (a0, a1) =>
            {
                Debug.WriteLine("Invoking Delegate Func<TArg0, TArg1, TResult>");
                var parameterCollection = mapping.GetParameterCollection<TDelegate>(a0, a1);
                object resolved;
                if(target is Service)
                    resolved = ls.ResolveService((Service)target, parameterCollection);
                else
                    resolved = ls.ResolveComponent((IComponentRegistration)target, parameterCollection);
                return (TResult)resolved;
            };
            var del = Delegate.CreateDelegate(typeof(TDelegate), del1.Target, del1.Method, true);
            return del;
        }

        static Delegate DelegateActivator<TArg0, TArg1, TArg2, TResult, TDelegate>(IComponentContext context, object target, ParameterMapping mapping)
        {
            if(!(target is Service || target is IComponentRegistration))
                throw new ArgumentException("target");
            var ls = context.Resolve<ILifetimeScope>();
            Func<TArg0, TArg1, TArg2, TResult> del1 = (a0, a1, a2) =>
            {
                Debug.WriteLine("Invoking Delegate Func<TArg0, TArg1, TArg2, TResult>");
                var parameterCollection = mapping.GetParameterCollection<TDelegate>(a0, a1, a2);
                object resolved;
                if(target is Service)
                    resolved = ls.ResolveService((Service)target, parameterCollection);
                else
                    resolved = ls.ResolveComponent((IComponentRegistration)target, parameterCollection);
                return (TResult)resolved;
            };
            var del = Delegate.CreateDelegate(typeof(TDelegate), del1.Target, del1.Method, true);
            return del;
        }

        // ReSharper restore UnusedMember.Local
    }
}
