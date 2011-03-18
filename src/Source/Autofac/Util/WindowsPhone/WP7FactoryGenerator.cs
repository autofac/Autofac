using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
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
        static readonly MethodInfo[] delegateFuncRegistrations = typeof(FactoryGenerator)
            .GetMethods(BindingFlags.NonPublic | BindingFlags.Static)
            .Where(x => x.Name == "DelegateFuncRegistration")
            .ToArray();

        readonly Func<IComponentContext, IEnumerable<Parameter>, Delegate> _generator;

        ///<summary>
        ///</summary>
        public FactoryGenerator(Type delegateType, Service service, ParameterMapping parameterMapping)
        {
            if (service == null) throw new ArgumentNullException("service");
            Enforce.ArgumentTypeIsFunction(delegateType);
            List<Type> args = null;
            MethodInfo funcRegistration = null;

            var resultType = delegateType.FunctionReturnType();
            var invoke = delegateType.GetMethod("Invoke");
            args = invoke.GetParameters().Select(x => x.ParameterType)
                .Append(resultType)
                .Append(delegateType)
                .ToList();

            //Find a func method that matches all args
            funcRegistration = delegateFuncRegistrations
                .FirstOrDefault(x => x.GetGenericArguments().Count() == args.Count());

            if (funcRegistration != null)
            {
                var creator = funcRegistration
                    .MakeGenericMethod(args.ToArray());

                _generator = (a0, a1) =>
                {
                    return (Delegate)creator.Invoke(null, new object[] { a0, service, a1, GetParameterMapping(delegateType, parameterMapping) });
                };
            }
        }

        /// <summary>
        /// Create a factory generator.
        /// </summary>
        /// <param name="productRegistration">The component that will be activated in
        /// order to create the products of the factory.</param>
        /// <param name="delegateType">The delegate to provide as a factory.</param>
        /// <param name="parameterMapping">The parameter mapping mode to use.</param>
        public FactoryGenerator(Type delegateType, IComponentRegistration productRegistration, ParameterMapping parameterMapping)
        {
            if (productRegistration == null) throw new ArgumentNullException("productRegistration");
            Enforce.ArgumentTypeIsFunction(delegateType);

            
        }

        internal static ParameterMapping GetParameterMapping(Type delegateType, ParameterMapping configuredParameterMapping)
        {
            if (configuredParameterMapping == ParameterMapping.Adaptive)
                return delegateType.Name.StartsWith("Func`") ? ParameterMapping.ByType : ParameterMapping.ByName;
            return configuredParameterMapping;
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

        static Delegate DelegateFuncRegistration<TResult, TDelegate>(IComponentContext context, Service target, IEnumerable<Parameter> parameters, ParameterMapping mapping)
        {
            var ls = context.Resolve<ILifetimeScope>();
            Func<TResult> del1 = () =>
            {
                Debug.WriteLine("Invoking Delegate Func<TResult>");
                var r = ls.ResolveService(target, parameters);
                return (TResult)r;
            };
            var del = Delegate.CreateDelegate(typeof(TDelegate), del1.Target, del1.Method, true);
            return del;
        }

        static Delegate DelegateFuncRegistration<TArg0, TResult, TDelegate>(IComponentContext context, Service target, IEnumerable<Parameter> parameters, ParameterMapping mapping)
        {
            var ls = context.Resolve<ILifetimeScope>();
            Func<TArg0, TResult> del1 = a0 =>
            {
                Debug.WriteLine("Invoking Delegate Func<TArg0, TResult>");
                var parameterCollection = GetParameterCollection<TDelegate>(mapping, a0);
                var r = ls.ResolveService(target, parameterCollection);
                return (TResult)r;
            };
            var del = Delegate.CreateDelegate(typeof(TDelegate), del1.Target, del1.Method, true);
            return del;
        }

        static Delegate DelegateFuncRegistration<TArg0, TArg1, TResult, TDelegate>(IComponentContext context, Service target, IEnumerable<Parameter> parameters, ParameterMapping mapping)
        {
                var ls = context.Resolve<ILifetimeScope>();
                Func<TArg0, TArg1, TResult> del1 = (a0, a1) =>
                {
                    Debug.WriteLine("Invoking Delegate Func<TArg0, TArg1, TResult>");
                    var parameterCollection = GetParameterCollection<TDelegate>(mapping, a0, a1);
                    var r = ls.ResolveService(target, parameterCollection);
                    return (TResult)r;
                };
                var del = Delegate.CreateDelegate(typeof(TDelegate), del1.Target, del1.Method, true);
                return del;
        }

        static Delegate DelegateFuncRegistration<TArg0, TArg1, TArg2, TResult, TDelegate>(IComponentContext context, Service target, IEnumerable<Parameter> parameters, ParameterMapping mapping)
        {
            var ls = context.Resolve<ILifetimeScope>();
            Func<TArg0, TArg1, TArg2, TResult> del1 = (a0, a1, a2) =>
            {
                Debug.WriteLine("Invoking Delegate Func<TArg0, TArg1, TArg2, TResult>");
                var parameterCollection = GetParameterCollection<TDelegate>(mapping, a0, a1, a2);
                var r = ls.ResolveService(target, parameterCollection);
                return (TResult)r;
            };
            var del = Delegate.CreateDelegate(typeof(TDelegate), del1.Target, del1.Method, true);
            return del;
        }

        internal static IEnumerable<Parameter> GetParameterCollection<TDelegate>(ParameterMapping mapping, params object[] param)
        {
            IEnumerable<Parameter> parameterCollection;

            switch (mapping)
            {
                case ParameterMapping.ByType:
                    parameterCollection = param.Select(x => (Parameter)new TypedParameter(x.GetType(), x));
                    break;
                case ParameterMapping.ByName:
                    {
                        var parameterInfo = typeof(TDelegate).GetMethods().First().GetParameters();
                        parameterCollection = new List<Parameter>();
                        for (var i = 0; i < param.Length; i++)
                        {
                            ((List<Parameter>)parameterCollection).Add(new NamedParameter(parameterInfo[i].Name, param[i]));
                        }
                    }
                    break;
                case ParameterMapping.ByPosition:
                    parameterCollection = new List<Parameter>();
                    for (var i = 0; i < param.Length; i++)
                    {
                        ((List<Parameter>)parameterCollection).Add(new PositionalParameter(i, param[i]));
                    }
                    break;
                default:
                    throw new NotSupportedException("Parameter mapping not supported");
            }

            return parameterCollection;
        }
    }
}
