using System;
using Cirrious.CrossCore.Core;
using Cirrious.CrossCore.IoC;
using System.Linq;
using System.Globalization;
using Autofac.Core.Registration;
using Autofac.Core;

namespace Autofac.Extras.MvvmCross
{
    public class AutofacMvxIocProvider : MvxSingleton<IMvxIoCProvider>, IMvxIoCProvider {
        private static readonly string TypeNotRegisteredAsSingleton = "Type '{0}' not registered as singleton.";

        private readonly IContainer _container;

        public AutofacMvxIocProvider(IContainer container) {
            if (container == null) throw new ArgumentNullException("container");
            _container = container;
        }

        public bool CanResolve<T>() where T : class {
            return CanResolve(typeof(T));
        }

        public bool CanResolve(Type type) {
            return _container.IsRegistered(type);
        }

        public T Resolve<T>() where T : class {
            return (T)Resolve(typeof(T));
        }

        public object Resolve(Type type) {
            return _container.Resolve(type);
        }

        public T Create<T>() where T : class {
            return (T)Create(typeof(T));
        }

        public object Create(Type type) {
            return Resolve(type);
        }

        public T GetSingleton<T>() where T : class {
            return (T)GetSingleton(typeof(T));
        }

        public object GetSingleton(Type type) {
            if (!ReferenceEquals(Resolve(type), Resolve(type)))
            {
                throw new DependencyResolutionException(String.Format(CultureInfo.CurrentCulture, TypeNotRegisteredAsSingleton, type));
            }
            return Resolve(type);
        }

        public bool TryResolve<T>(out T resolved) where T : class {
            return _container.TryResolve<T>(out resolved);
        }

        public bool TryResolve(Type type, out object resolved) {
            return _container.TryResolve(type, out resolved);
        }

        public void RegisterType<TFrom, TTo>()
            where TFrom : class
            where TTo : class, TFrom {
            RegisterType(typeof(TFrom), typeof(TTo));
        }

        public void RegisterType(Type from, Type to) {
            var cb = new ContainerBuilder();
            cb.RegisterType(to).As(from).AsSelf();
            cb.Update(_container);
        }

        public void RegisterSingleton<TInterface>(TInterface instance) where TInterface : class {
            RegisterSingleton(typeof(TInterface), instance);
        }

        public void RegisterSingleton(Type type, object instance) {
            var cb = new ContainerBuilder();
            cb.RegisterInstance(instance).As(type).AsSelf().SingleInstance();
            cb.Update(_container);
        }

        public void RegisterSingleton<TInterface>(Func<TInterface> constructor) where TInterface : class {
            RegisterSingleton(typeof(TInterface), constructor);
        }

        public void RegisterSingleton(Type tInterface, Func<object> constructor) {
            var cb = new ContainerBuilder();
            cb.Register(cc => constructor()).As(tInterface).AsSelf().SingleInstance();
            cb.Update(_container);
        }

        public T IoCConstruct<T>() where T : class {
            return (T)IoCConstruct(typeof(T));
        }

        public object IoCConstruct(Type type) {
            return Resolve(type);
        }

        public void CallbackWhenRegistered<T>(Action action) {
            CallbackWhenRegistered(typeof(T), action);
        }

        public void CallbackWhenRegistered(Type type, Action action) {
            _container.ComponentRegistry.Registered += (sender, args) => {
                if (args.ComponentRegistration.Services.OfType<TypedService>().Any(x => x.ServiceType == type)) {
                    action();
                }
            };
        }
    }
}

