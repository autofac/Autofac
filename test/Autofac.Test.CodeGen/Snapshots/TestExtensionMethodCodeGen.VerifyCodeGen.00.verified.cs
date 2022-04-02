//HintName: Autofac.DelegateInvokers.g.cs
// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Reflection;
using Autofac.Builder;
using Autofac.Core;

namespace Autofac;

/// <summary>
/// Provides delegate invocation holding classes.
/// </summary>
[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
internal static partial class DelegateInvokers
{
    public sealed class DelegateInvoker1<TDependency1, TComponent> : BaseGenericResolveDelegateInvoker
        where TDependency1 : notnull
    {
        private readonly Func<TDependency1, TComponent> _delegate;

        public DelegateInvoker1(Func<TDependency1, TComponent> @delegate)
        {
            _delegate = @delegate;
        }

        protected override ParameterInfo[] GetDelegateParameters() => _delegate.Method.GetParameters();

        public TComponent ResolveWithDelegate(IComponentContext context, IEnumerable<Parameter> parameters)
        {
            if (AnyParameters(parameters))
            {
                return _delegate(
                    ResolveWithParametersOrRegistration<TDependency1>(context, parameters, 0));
            }

            return _delegate(
                    context.Resolve<TDependency1>());
        }
    }

    public sealed class DelegateInvoker1WithComponentContext<TDependency1, TComponent> : BaseGenericResolveDelegateInvoker
        where TDependency1 : notnull
    {        
        private readonly Func<IComponentContext, TDependency1, TComponent> _delegate;

        public DelegateInvoker1WithComponentContext(Func<IComponentContext, TDependency1, TComponent> @delegate)
        {
            _delegate = @delegate;
        }

        protected override ParameterInfo[] GetDelegateParameters() => _delegate.Method.GetParameters();

        public TComponent ResolveWithDelegate(IComponentContext context, IEnumerable<Parameter> parameters)
        {
            if (AnyParameters(parameters))
            {
                return _delegate(
                    context,
                    ResolveWithParametersOrRegistration<TDependency1>(context, parameters, 1));
            }

            return _delegate(
                    context,
                    context.Resolve<TDependency1>());
        }
    }

    public sealed class DelegateInvoker2<TDependency1, TDependency2, TComponent> : BaseGenericResolveDelegateInvoker
        where TDependency1 : notnull
        where TDependency2 : notnull
    {
        private readonly Func<TDependency1, TDependency2, TComponent> _delegate;

        public DelegateInvoker2(Func<TDependency1, TDependency2, TComponent> @delegate)
        {
            _delegate = @delegate;
        }

        protected override ParameterInfo[] GetDelegateParameters() => _delegate.Method.GetParameters();

        public TComponent ResolveWithDelegate(IComponentContext context, IEnumerable<Parameter> parameters)
        {
            if (AnyParameters(parameters))
            {
                return _delegate(
                    ResolveWithParametersOrRegistration<TDependency1>(context, parameters, 0),
                    ResolveWithParametersOrRegistration<TDependency2>(context, parameters, 1));
            }

            return _delegate(
                    context.Resolve<TDependency1>(),
                    context.Resolve<TDependency2>());
        }
    }

    public sealed class DelegateInvoker2WithComponentContext<TDependency1, TDependency2, TComponent> : BaseGenericResolveDelegateInvoker
        where TDependency1 : notnull
        where TDependency2 : notnull
    {        
        private readonly Func<IComponentContext, TDependency1, TDependency2, TComponent> _delegate;

        public DelegateInvoker2WithComponentContext(Func<IComponentContext, TDependency1, TDependency2, TComponent> @delegate)
        {
            _delegate = @delegate;
        }

        protected override ParameterInfo[] GetDelegateParameters() => _delegate.Method.GetParameters();

        public TComponent ResolveWithDelegate(IComponentContext context, IEnumerable<Parameter> parameters)
        {
            if (AnyParameters(parameters))
            {
                return _delegate(
                    context,
                    ResolveWithParametersOrRegistration<TDependency1>(context, parameters, 1),
                    ResolveWithParametersOrRegistration<TDependency2>(context, parameters, 2));
            }

            return _delegate(
                    context,
                    context.Resolve<TDependency1>(),
                    context.Resolve<TDependency2>());
        }
    }

    public sealed class DelegateInvoker3<TDependency1, TDependency2, TDependency3, TComponent> : BaseGenericResolveDelegateInvoker
        where TDependency1 : notnull
        where TDependency2 : notnull
        where TDependency3 : notnull
    {
        private readonly Func<TDependency1, TDependency2, TDependency3, TComponent> _delegate;

        public DelegateInvoker3(Func<TDependency1, TDependency2, TDependency3, TComponent> @delegate)
        {
            _delegate = @delegate;
        }

        protected override ParameterInfo[] GetDelegateParameters() => _delegate.Method.GetParameters();

        public TComponent ResolveWithDelegate(IComponentContext context, IEnumerable<Parameter> parameters)
        {
            if (AnyParameters(parameters))
            {
                return _delegate(
                    ResolveWithParametersOrRegistration<TDependency1>(context, parameters, 0),
                    ResolveWithParametersOrRegistration<TDependency2>(context, parameters, 1),
                    ResolveWithParametersOrRegistration<TDependency3>(context, parameters, 2));
            }

            return _delegate(
                    context.Resolve<TDependency1>(),
                    context.Resolve<TDependency2>(),
                    context.Resolve<TDependency3>());
        }
    }

    public sealed class DelegateInvoker3WithComponentContext<TDependency1, TDependency2, TDependency3, TComponent> : BaseGenericResolveDelegateInvoker
        where TDependency1 : notnull
        where TDependency2 : notnull
        where TDependency3 : notnull
    {        
        private readonly Func<IComponentContext, TDependency1, TDependency2, TDependency3, TComponent> _delegate;

        public DelegateInvoker3WithComponentContext(Func<IComponentContext, TDependency1, TDependency2, TDependency3, TComponent> @delegate)
        {
            _delegate = @delegate;
        }

        protected override ParameterInfo[] GetDelegateParameters() => _delegate.Method.GetParameters();

        public TComponent ResolveWithDelegate(IComponentContext context, IEnumerable<Parameter> parameters)
        {
            if (AnyParameters(parameters))
            {
                return _delegate(
                    context,
                    ResolveWithParametersOrRegistration<TDependency1>(context, parameters, 1),
                    ResolveWithParametersOrRegistration<TDependency2>(context, parameters, 2),
                    ResolveWithParametersOrRegistration<TDependency3>(context, parameters, 3));
            }

            return _delegate(
                    context,
                    context.Resolve<TDependency1>(),
                    context.Resolve<TDependency2>(),
                    context.Resolve<TDependency3>());
        }
    }

    public sealed class DelegateInvoker4<TDependency1, TDependency2, TDependency3, TDependency4, TComponent> : BaseGenericResolveDelegateInvoker
        where TDependency1 : notnull
        where TDependency2 : notnull
        where TDependency3 : notnull
        where TDependency4 : notnull
    {
        private readonly Func<TDependency1, TDependency2, TDependency3, TDependency4, TComponent> _delegate;

        public DelegateInvoker4(Func<TDependency1, TDependency2, TDependency3, TDependency4, TComponent> @delegate)
        {
            _delegate = @delegate;
        }

        protected override ParameterInfo[] GetDelegateParameters() => _delegate.Method.GetParameters();

        public TComponent ResolveWithDelegate(IComponentContext context, IEnumerable<Parameter> parameters)
        {
            if (AnyParameters(parameters))
            {
                return _delegate(
                    ResolveWithParametersOrRegistration<TDependency1>(context, parameters, 0),
                    ResolveWithParametersOrRegistration<TDependency2>(context, parameters, 1),
                    ResolveWithParametersOrRegistration<TDependency3>(context, parameters, 2),
                    ResolveWithParametersOrRegistration<TDependency4>(context, parameters, 3));
            }

            return _delegate(
                    context.Resolve<TDependency1>(),
                    context.Resolve<TDependency2>(),
                    context.Resolve<TDependency3>(),
                    context.Resolve<TDependency4>());
        }
    }

    public sealed class DelegateInvoker4WithComponentContext<TDependency1, TDependency2, TDependency3, TDependency4, TComponent> : BaseGenericResolveDelegateInvoker
        where TDependency1 : notnull
        where TDependency2 : notnull
        where TDependency3 : notnull
        where TDependency4 : notnull
    {        
        private readonly Func<IComponentContext, TDependency1, TDependency2, TDependency3, TDependency4, TComponent> _delegate;

        public DelegateInvoker4WithComponentContext(Func<IComponentContext, TDependency1, TDependency2, TDependency3, TDependency4, TComponent> @delegate)
        {
            _delegate = @delegate;
        }

        protected override ParameterInfo[] GetDelegateParameters() => _delegate.Method.GetParameters();

        public TComponent ResolveWithDelegate(IComponentContext context, IEnumerable<Parameter> parameters)
        {
            if (AnyParameters(parameters))
            {
                return _delegate(
                    context,
                    ResolveWithParametersOrRegistration<TDependency1>(context, parameters, 1),
                    ResolveWithParametersOrRegistration<TDependency2>(context, parameters, 2),
                    ResolveWithParametersOrRegistration<TDependency3>(context, parameters, 3),
                    ResolveWithParametersOrRegistration<TDependency4>(context, parameters, 4));
            }

            return _delegate(
                    context,
                    context.Resolve<TDependency1>(),
                    context.Resolve<TDependency2>(),
                    context.Resolve<TDependency3>(),
                    context.Resolve<TDependency4>());
        }
    }

    public sealed class DelegateInvoker5<TDependency1, TDependency2, TDependency3, TDependency4, TDependency5, TComponent> : BaseGenericResolveDelegateInvoker
        where TDependency1 : notnull
        where TDependency2 : notnull
        where TDependency3 : notnull
        where TDependency4 : notnull
        where TDependency5 : notnull
    {
        private readonly Func<TDependency1, TDependency2, TDependency3, TDependency4, TDependency5, TComponent> _delegate;

        public DelegateInvoker5(Func<TDependency1, TDependency2, TDependency3, TDependency4, TDependency5, TComponent> @delegate)
        {
            _delegate = @delegate;
        }

        protected override ParameterInfo[] GetDelegateParameters() => _delegate.Method.GetParameters();

        public TComponent ResolveWithDelegate(IComponentContext context, IEnumerable<Parameter> parameters)
        {
            if (AnyParameters(parameters))
            {
                return _delegate(
                    ResolveWithParametersOrRegistration<TDependency1>(context, parameters, 0),
                    ResolveWithParametersOrRegistration<TDependency2>(context, parameters, 1),
                    ResolveWithParametersOrRegistration<TDependency3>(context, parameters, 2),
                    ResolveWithParametersOrRegistration<TDependency4>(context, parameters, 3),
                    ResolveWithParametersOrRegistration<TDependency5>(context, parameters, 4));
            }

            return _delegate(
                    context.Resolve<TDependency1>(),
                    context.Resolve<TDependency2>(),
                    context.Resolve<TDependency3>(),
                    context.Resolve<TDependency4>(),
                    context.Resolve<TDependency5>());
        }
    }

    public sealed class DelegateInvoker5WithComponentContext<TDependency1, TDependency2, TDependency3, TDependency4, TDependency5, TComponent> : BaseGenericResolveDelegateInvoker
        where TDependency1 : notnull
        where TDependency2 : notnull
        where TDependency3 : notnull
        where TDependency4 : notnull
        where TDependency5 : notnull
    {        
        private readonly Func<IComponentContext, TDependency1, TDependency2, TDependency3, TDependency4, TDependency5, TComponent> _delegate;

        public DelegateInvoker5WithComponentContext(Func<IComponentContext, TDependency1, TDependency2, TDependency3, TDependency4, TDependency5, TComponent> @delegate)
        {
            _delegate = @delegate;
        }

        protected override ParameterInfo[] GetDelegateParameters() => _delegate.Method.GetParameters();

        public TComponent ResolveWithDelegate(IComponentContext context, IEnumerable<Parameter> parameters)
        {
            if (AnyParameters(parameters))
            {
                return _delegate(
                    context,
                    ResolveWithParametersOrRegistration<TDependency1>(context, parameters, 1),
                    ResolveWithParametersOrRegistration<TDependency2>(context, parameters, 2),
                    ResolveWithParametersOrRegistration<TDependency3>(context, parameters, 3),
                    ResolveWithParametersOrRegistration<TDependency4>(context, parameters, 4),
                    ResolveWithParametersOrRegistration<TDependency5>(context, parameters, 5));
            }

            return _delegate(
                    context,
                    context.Resolve<TDependency1>(),
                    context.Resolve<TDependency2>(),
                    context.Resolve<TDependency3>(),
                    context.Resolve<TDependency4>(),
                    context.Resolve<TDependency5>());
        }
    }

    public sealed class DelegateInvoker6<TDependency1, TDependency2, TDependency3, TDependency4, TDependency5, TDependency6, TComponent> : BaseGenericResolveDelegateInvoker
        where TDependency1 : notnull
        where TDependency2 : notnull
        where TDependency3 : notnull
        where TDependency4 : notnull
        where TDependency5 : notnull
        where TDependency6 : notnull
    {
        private readonly Func<TDependency1, TDependency2, TDependency3, TDependency4, TDependency5, TDependency6, TComponent> _delegate;

        public DelegateInvoker6(Func<TDependency1, TDependency2, TDependency3, TDependency4, TDependency5, TDependency6, TComponent> @delegate)
        {
            _delegate = @delegate;
        }

        protected override ParameterInfo[] GetDelegateParameters() => _delegate.Method.GetParameters();

        public TComponent ResolveWithDelegate(IComponentContext context, IEnumerable<Parameter> parameters)
        {
            if (AnyParameters(parameters))
            {
                return _delegate(
                    ResolveWithParametersOrRegistration<TDependency1>(context, parameters, 0),
                    ResolveWithParametersOrRegistration<TDependency2>(context, parameters, 1),
                    ResolveWithParametersOrRegistration<TDependency3>(context, parameters, 2),
                    ResolveWithParametersOrRegistration<TDependency4>(context, parameters, 3),
                    ResolveWithParametersOrRegistration<TDependency5>(context, parameters, 4),
                    ResolveWithParametersOrRegistration<TDependency6>(context, parameters, 5));
            }

            return _delegate(
                    context.Resolve<TDependency1>(),
                    context.Resolve<TDependency2>(),
                    context.Resolve<TDependency3>(),
                    context.Resolve<TDependency4>(),
                    context.Resolve<TDependency5>(),
                    context.Resolve<TDependency6>());
        }
    }

    public sealed class DelegateInvoker6WithComponentContext<TDependency1, TDependency2, TDependency3, TDependency4, TDependency5, TDependency6, TComponent> : BaseGenericResolveDelegateInvoker
        where TDependency1 : notnull
        where TDependency2 : notnull
        where TDependency3 : notnull
        where TDependency4 : notnull
        where TDependency5 : notnull
        where TDependency6 : notnull
    {        
        private readonly Func<IComponentContext, TDependency1, TDependency2, TDependency3, TDependency4, TDependency5, TDependency6, TComponent> _delegate;

        public DelegateInvoker6WithComponentContext(Func<IComponentContext, TDependency1, TDependency2, TDependency3, TDependency4, TDependency5, TDependency6, TComponent> @delegate)
        {
            _delegate = @delegate;
        }

        protected override ParameterInfo[] GetDelegateParameters() => _delegate.Method.GetParameters();

        public TComponent ResolveWithDelegate(IComponentContext context, IEnumerable<Parameter> parameters)
        {
            if (AnyParameters(parameters))
            {
                return _delegate(
                    context,
                    ResolveWithParametersOrRegistration<TDependency1>(context, parameters, 1),
                    ResolveWithParametersOrRegistration<TDependency2>(context, parameters, 2),
                    ResolveWithParametersOrRegistration<TDependency3>(context, parameters, 3),
                    ResolveWithParametersOrRegistration<TDependency4>(context, parameters, 4),
                    ResolveWithParametersOrRegistration<TDependency5>(context, parameters, 5),
                    ResolveWithParametersOrRegistration<TDependency6>(context, parameters, 6));
            }

            return _delegate(
                    context,
                    context.Resolve<TDependency1>(),
                    context.Resolve<TDependency2>(),
                    context.Resolve<TDependency3>(),
                    context.Resolve<TDependency4>(),
                    context.Resolve<TDependency5>(),
                    context.Resolve<TDependency6>());
        }
    }

    public sealed class DelegateInvoker7<TDependency1, TDependency2, TDependency3, TDependency4, TDependency5, TDependency6, TDependency7, TComponent> : BaseGenericResolveDelegateInvoker
        where TDependency1 : notnull
        where TDependency2 : notnull
        where TDependency3 : notnull
        where TDependency4 : notnull
        where TDependency5 : notnull
        where TDependency6 : notnull
        where TDependency7 : notnull
    {
        private readonly Func<TDependency1, TDependency2, TDependency3, TDependency4, TDependency5, TDependency6, TDependency7, TComponent> _delegate;

        public DelegateInvoker7(Func<TDependency1, TDependency2, TDependency3, TDependency4, TDependency5, TDependency6, TDependency7, TComponent> @delegate)
        {
            _delegate = @delegate;
        }

        protected override ParameterInfo[] GetDelegateParameters() => _delegate.Method.GetParameters();

        public TComponent ResolveWithDelegate(IComponentContext context, IEnumerable<Parameter> parameters)
        {
            if (AnyParameters(parameters))
            {
                return _delegate(
                    ResolveWithParametersOrRegistration<TDependency1>(context, parameters, 0),
                    ResolveWithParametersOrRegistration<TDependency2>(context, parameters, 1),
                    ResolveWithParametersOrRegistration<TDependency3>(context, parameters, 2),
                    ResolveWithParametersOrRegistration<TDependency4>(context, parameters, 3),
                    ResolveWithParametersOrRegistration<TDependency5>(context, parameters, 4),
                    ResolveWithParametersOrRegistration<TDependency6>(context, parameters, 5),
                    ResolveWithParametersOrRegistration<TDependency7>(context, parameters, 6));
            }

            return _delegate(
                    context.Resolve<TDependency1>(),
                    context.Resolve<TDependency2>(),
                    context.Resolve<TDependency3>(),
                    context.Resolve<TDependency4>(),
                    context.Resolve<TDependency5>(),
                    context.Resolve<TDependency6>(),
                    context.Resolve<TDependency7>());
        }
    }

    public sealed class DelegateInvoker7WithComponentContext<TDependency1, TDependency2, TDependency3, TDependency4, TDependency5, TDependency6, TDependency7, TComponent> : BaseGenericResolveDelegateInvoker
        where TDependency1 : notnull
        where TDependency2 : notnull
        where TDependency3 : notnull
        where TDependency4 : notnull
        where TDependency5 : notnull
        where TDependency6 : notnull
        where TDependency7 : notnull
    {        
        private readonly Func<IComponentContext, TDependency1, TDependency2, TDependency3, TDependency4, TDependency5, TDependency6, TDependency7, TComponent> _delegate;

        public DelegateInvoker7WithComponentContext(Func<IComponentContext, TDependency1, TDependency2, TDependency3, TDependency4, TDependency5, TDependency6, TDependency7, TComponent> @delegate)
        {
            _delegate = @delegate;
        }

        protected override ParameterInfo[] GetDelegateParameters() => _delegate.Method.GetParameters();

        public TComponent ResolveWithDelegate(IComponentContext context, IEnumerable<Parameter> parameters)
        {
            if (AnyParameters(parameters))
            {
                return _delegate(
                    context,
                    ResolveWithParametersOrRegistration<TDependency1>(context, parameters, 1),
                    ResolveWithParametersOrRegistration<TDependency2>(context, parameters, 2),
                    ResolveWithParametersOrRegistration<TDependency3>(context, parameters, 3),
                    ResolveWithParametersOrRegistration<TDependency4>(context, parameters, 4),
                    ResolveWithParametersOrRegistration<TDependency5>(context, parameters, 5),
                    ResolveWithParametersOrRegistration<TDependency6>(context, parameters, 6),
                    ResolveWithParametersOrRegistration<TDependency7>(context, parameters, 7));
            }

            return _delegate(
                    context,
                    context.Resolve<TDependency1>(),
                    context.Resolve<TDependency2>(),
                    context.Resolve<TDependency3>(),
                    context.Resolve<TDependency4>(),
                    context.Resolve<TDependency5>(),
                    context.Resolve<TDependency6>(),
                    context.Resolve<TDependency7>());
        }
    }

    public sealed class DelegateInvoker8<TDependency1, TDependency2, TDependency3, TDependency4, TDependency5, TDependency6, TDependency7, TDependency8, TComponent> : BaseGenericResolveDelegateInvoker
        where TDependency1 : notnull
        where TDependency2 : notnull
        where TDependency3 : notnull
        where TDependency4 : notnull
        where TDependency5 : notnull
        where TDependency6 : notnull
        where TDependency7 : notnull
        where TDependency8 : notnull
    {
        private readonly Func<TDependency1, TDependency2, TDependency3, TDependency4, TDependency5, TDependency6, TDependency7, TDependency8, TComponent> _delegate;

        public DelegateInvoker8(Func<TDependency1, TDependency2, TDependency3, TDependency4, TDependency5, TDependency6, TDependency7, TDependency8, TComponent> @delegate)
        {
            _delegate = @delegate;
        }

        protected override ParameterInfo[] GetDelegateParameters() => _delegate.Method.GetParameters();

        public TComponent ResolveWithDelegate(IComponentContext context, IEnumerable<Parameter> parameters)
        {
            if (AnyParameters(parameters))
            {
                return _delegate(
                    ResolveWithParametersOrRegistration<TDependency1>(context, parameters, 0),
                    ResolveWithParametersOrRegistration<TDependency2>(context, parameters, 1),
                    ResolveWithParametersOrRegistration<TDependency3>(context, parameters, 2),
                    ResolveWithParametersOrRegistration<TDependency4>(context, parameters, 3),
                    ResolveWithParametersOrRegistration<TDependency5>(context, parameters, 4),
                    ResolveWithParametersOrRegistration<TDependency6>(context, parameters, 5),
                    ResolveWithParametersOrRegistration<TDependency7>(context, parameters, 6),
                    ResolveWithParametersOrRegistration<TDependency8>(context, parameters, 7));
            }

            return _delegate(
                    context.Resolve<TDependency1>(),
                    context.Resolve<TDependency2>(),
                    context.Resolve<TDependency3>(),
                    context.Resolve<TDependency4>(),
                    context.Resolve<TDependency5>(),
                    context.Resolve<TDependency6>(),
                    context.Resolve<TDependency7>(),
                    context.Resolve<TDependency8>());
        }
    }

    public sealed class DelegateInvoker8WithComponentContext<TDependency1, TDependency2, TDependency3, TDependency4, TDependency5, TDependency6, TDependency7, TDependency8, TComponent> : BaseGenericResolveDelegateInvoker
        where TDependency1 : notnull
        where TDependency2 : notnull
        where TDependency3 : notnull
        where TDependency4 : notnull
        where TDependency5 : notnull
        where TDependency6 : notnull
        where TDependency7 : notnull
        where TDependency8 : notnull
    {        
        private readonly Func<IComponentContext, TDependency1, TDependency2, TDependency3, TDependency4, TDependency5, TDependency6, TDependency7, TDependency8, TComponent> _delegate;

        public DelegateInvoker8WithComponentContext(Func<IComponentContext, TDependency1, TDependency2, TDependency3, TDependency4, TDependency5, TDependency6, TDependency7, TDependency8, TComponent> @delegate)
        {
            _delegate = @delegate;
        }

        protected override ParameterInfo[] GetDelegateParameters() => _delegate.Method.GetParameters();

        public TComponent ResolveWithDelegate(IComponentContext context, IEnumerable<Parameter> parameters)
        {
            if (AnyParameters(parameters))
            {
                return _delegate(
                    context,
                    ResolveWithParametersOrRegistration<TDependency1>(context, parameters, 1),
                    ResolveWithParametersOrRegistration<TDependency2>(context, parameters, 2),
                    ResolveWithParametersOrRegistration<TDependency3>(context, parameters, 3),
                    ResolveWithParametersOrRegistration<TDependency4>(context, parameters, 4),
                    ResolveWithParametersOrRegistration<TDependency5>(context, parameters, 5),
                    ResolveWithParametersOrRegistration<TDependency6>(context, parameters, 6),
                    ResolveWithParametersOrRegistration<TDependency7>(context, parameters, 7),
                    ResolveWithParametersOrRegistration<TDependency8>(context, parameters, 8));
            }

            return _delegate(
                    context,
                    context.Resolve<TDependency1>(),
                    context.Resolve<TDependency2>(),
                    context.Resolve<TDependency3>(),
                    context.Resolve<TDependency4>(),
                    context.Resolve<TDependency5>(),
                    context.Resolve<TDependency6>(),
                    context.Resolve<TDependency7>(),
                    context.Resolve<TDependency8>());
        }
    }

    public sealed class DelegateInvoker9<TDependency1, TDependency2, TDependency3, TDependency4, TDependency5, TDependency6, TDependency7, TDependency8, TDependency9, TComponent> : BaseGenericResolveDelegateInvoker
        where TDependency1 : notnull
        where TDependency2 : notnull
        where TDependency3 : notnull
        where TDependency4 : notnull
        where TDependency5 : notnull
        where TDependency6 : notnull
        where TDependency7 : notnull
        where TDependency8 : notnull
        where TDependency9 : notnull
    {
        private readonly Func<TDependency1, TDependency2, TDependency3, TDependency4, TDependency5, TDependency6, TDependency7, TDependency8, TDependency9, TComponent> _delegate;

        public DelegateInvoker9(Func<TDependency1, TDependency2, TDependency3, TDependency4, TDependency5, TDependency6, TDependency7, TDependency8, TDependency9, TComponent> @delegate)
        {
            _delegate = @delegate;
        }

        protected override ParameterInfo[] GetDelegateParameters() => _delegate.Method.GetParameters();

        public TComponent ResolveWithDelegate(IComponentContext context, IEnumerable<Parameter> parameters)
        {
            if (AnyParameters(parameters))
            {
                return _delegate(
                    ResolveWithParametersOrRegistration<TDependency1>(context, parameters, 0),
                    ResolveWithParametersOrRegistration<TDependency2>(context, parameters, 1),
                    ResolveWithParametersOrRegistration<TDependency3>(context, parameters, 2),
                    ResolveWithParametersOrRegistration<TDependency4>(context, parameters, 3),
                    ResolveWithParametersOrRegistration<TDependency5>(context, parameters, 4),
                    ResolveWithParametersOrRegistration<TDependency6>(context, parameters, 5),
                    ResolveWithParametersOrRegistration<TDependency7>(context, parameters, 6),
                    ResolveWithParametersOrRegistration<TDependency8>(context, parameters, 7),
                    ResolveWithParametersOrRegistration<TDependency9>(context, parameters, 8));
            }

            return _delegate(
                    context.Resolve<TDependency1>(),
                    context.Resolve<TDependency2>(),
                    context.Resolve<TDependency3>(),
                    context.Resolve<TDependency4>(),
                    context.Resolve<TDependency5>(),
                    context.Resolve<TDependency6>(),
                    context.Resolve<TDependency7>(),
                    context.Resolve<TDependency8>(),
                    context.Resolve<TDependency9>());
        }
    }

    public sealed class DelegateInvoker9WithComponentContext<TDependency1, TDependency2, TDependency3, TDependency4, TDependency5, TDependency6, TDependency7, TDependency8, TDependency9, TComponent> : BaseGenericResolveDelegateInvoker
        where TDependency1 : notnull
        where TDependency2 : notnull
        where TDependency3 : notnull
        where TDependency4 : notnull
        where TDependency5 : notnull
        where TDependency6 : notnull
        where TDependency7 : notnull
        where TDependency8 : notnull
        where TDependency9 : notnull
    {        
        private readonly Func<IComponentContext, TDependency1, TDependency2, TDependency3, TDependency4, TDependency5, TDependency6, TDependency7, TDependency8, TDependency9, TComponent> _delegate;

        public DelegateInvoker9WithComponentContext(Func<IComponentContext, TDependency1, TDependency2, TDependency3, TDependency4, TDependency5, TDependency6, TDependency7, TDependency8, TDependency9, TComponent> @delegate)
        {
            _delegate = @delegate;
        }

        protected override ParameterInfo[] GetDelegateParameters() => _delegate.Method.GetParameters();

        public TComponent ResolveWithDelegate(IComponentContext context, IEnumerable<Parameter> parameters)
        {
            if (AnyParameters(parameters))
            {
                return _delegate(
                    context,
                    ResolveWithParametersOrRegistration<TDependency1>(context, parameters, 1),
                    ResolveWithParametersOrRegistration<TDependency2>(context, parameters, 2),
                    ResolveWithParametersOrRegistration<TDependency3>(context, parameters, 3),
                    ResolveWithParametersOrRegistration<TDependency4>(context, parameters, 4),
                    ResolveWithParametersOrRegistration<TDependency5>(context, parameters, 5),
                    ResolveWithParametersOrRegistration<TDependency6>(context, parameters, 6),
                    ResolveWithParametersOrRegistration<TDependency7>(context, parameters, 7),
                    ResolveWithParametersOrRegistration<TDependency8>(context, parameters, 8),
                    ResolveWithParametersOrRegistration<TDependency9>(context, parameters, 9));
            }

            return _delegate(
                    context,
                    context.Resolve<TDependency1>(),
                    context.Resolve<TDependency2>(),
                    context.Resolve<TDependency3>(),
                    context.Resolve<TDependency4>(),
                    context.Resolve<TDependency5>(),
                    context.Resolve<TDependency6>(),
                    context.Resolve<TDependency7>(),
                    context.Resolve<TDependency8>(),
                    context.Resolve<TDependency9>());
        }
    }

    public sealed class DelegateInvoker10<TDependency1, TDependency2, TDependency3, TDependency4, TDependency5, TDependency6, TDependency7, TDependency8, TDependency9, TDependency10, TComponent> : BaseGenericResolveDelegateInvoker
        where TDependency1 : notnull
        where TDependency2 : notnull
        where TDependency3 : notnull
        where TDependency4 : notnull
        where TDependency5 : notnull
        where TDependency6 : notnull
        where TDependency7 : notnull
        where TDependency8 : notnull
        where TDependency9 : notnull
        where TDependency10 : notnull
    {
        private readonly Func<TDependency1, TDependency2, TDependency3, TDependency4, TDependency5, TDependency6, TDependency7, TDependency8, TDependency9, TDependency10, TComponent> _delegate;

        public DelegateInvoker10(Func<TDependency1, TDependency2, TDependency3, TDependency4, TDependency5, TDependency6, TDependency7, TDependency8, TDependency9, TDependency10, TComponent> @delegate)
        {
            _delegate = @delegate;
        }

        protected override ParameterInfo[] GetDelegateParameters() => _delegate.Method.GetParameters();

        public TComponent ResolveWithDelegate(IComponentContext context, IEnumerable<Parameter> parameters)
        {
            if (AnyParameters(parameters))
            {
                return _delegate(
                    ResolveWithParametersOrRegistration<TDependency1>(context, parameters, 0),
                    ResolveWithParametersOrRegistration<TDependency2>(context, parameters, 1),
                    ResolveWithParametersOrRegistration<TDependency3>(context, parameters, 2),
                    ResolveWithParametersOrRegistration<TDependency4>(context, parameters, 3),
                    ResolveWithParametersOrRegistration<TDependency5>(context, parameters, 4),
                    ResolveWithParametersOrRegistration<TDependency6>(context, parameters, 5),
                    ResolveWithParametersOrRegistration<TDependency7>(context, parameters, 6),
                    ResolveWithParametersOrRegistration<TDependency8>(context, parameters, 7),
                    ResolveWithParametersOrRegistration<TDependency9>(context, parameters, 8),
                    ResolveWithParametersOrRegistration<TDependency10>(context, parameters, 9));
            }

            return _delegate(
                    context.Resolve<TDependency1>(),
                    context.Resolve<TDependency2>(),
                    context.Resolve<TDependency3>(),
                    context.Resolve<TDependency4>(),
                    context.Resolve<TDependency5>(),
                    context.Resolve<TDependency6>(),
                    context.Resolve<TDependency7>(),
                    context.Resolve<TDependency8>(),
                    context.Resolve<TDependency9>(),
                    context.Resolve<TDependency10>());
        }
    }

    public sealed class DelegateInvoker10WithComponentContext<TDependency1, TDependency2, TDependency3, TDependency4, TDependency5, TDependency6, TDependency7, TDependency8, TDependency9, TDependency10, TComponent> : BaseGenericResolveDelegateInvoker
        where TDependency1 : notnull
        where TDependency2 : notnull
        where TDependency3 : notnull
        where TDependency4 : notnull
        where TDependency5 : notnull
        where TDependency6 : notnull
        where TDependency7 : notnull
        where TDependency8 : notnull
        where TDependency9 : notnull
        where TDependency10 : notnull
    {        
        private readonly Func<IComponentContext, TDependency1, TDependency2, TDependency3, TDependency4, TDependency5, TDependency6, TDependency7, TDependency8, TDependency9, TDependency10, TComponent> _delegate;

        public DelegateInvoker10WithComponentContext(Func<IComponentContext, TDependency1, TDependency2, TDependency3, TDependency4, TDependency5, TDependency6, TDependency7, TDependency8, TDependency9, TDependency10, TComponent> @delegate)
        {
            _delegate = @delegate;
        }

        protected override ParameterInfo[] GetDelegateParameters() => _delegate.Method.GetParameters();

        public TComponent ResolveWithDelegate(IComponentContext context, IEnumerable<Parameter> parameters)
        {
            if (AnyParameters(parameters))
            {
                return _delegate(
                    context,
                    ResolveWithParametersOrRegistration<TDependency1>(context, parameters, 1),
                    ResolveWithParametersOrRegistration<TDependency2>(context, parameters, 2),
                    ResolveWithParametersOrRegistration<TDependency3>(context, parameters, 3),
                    ResolveWithParametersOrRegistration<TDependency4>(context, parameters, 4),
                    ResolveWithParametersOrRegistration<TDependency5>(context, parameters, 5),
                    ResolveWithParametersOrRegistration<TDependency6>(context, parameters, 6),
                    ResolveWithParametersOrRegistration<TDependency7>(context, parameters, 7),
                    ResolveWithParametersOrRegistration<TDependency8>(context, parameters, 8),
                    ResolveWithParametersOrRegistration<TDependency9>(context, parameters, 9),
                    ResolveWithParametersOrRegistration<TDependency10>(context, parameters, 10));
            }

            return _delegate(
                    context,
                    context.Resolve<TDependency1>(),
                    context.Resolve<TDependency2>(),
                    context.Resolve<TDependency3>(),
                    context.Resolve<TDependency4>(),
                    context.Resolve<TDependency5>(),
                    context.Resolve<TDependency6>(),
                    context.Resolve<TDependency7>(),
                    context.Resolve<TDependency8>(),
                    context.Resolve<TDependency9>(),
                    context.Resolve<TDependency10>());
        }
    }

}
