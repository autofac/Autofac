using System;
using System.Globalization;
using System.Linq;
using Autofac.Core;
using Autofac.Core.Lifetime;
using Autofac.Core.Registration;
using Cirrious.CrossCore.Core;
using Cirrious.CrossCore.IoC;

namespace Autofac.Extras.MvvmCross
{
    /// <summary>
    /// Inversion of control provider for the MvvmCross framework backed by Autofac.
    /// </summary>
    public class AutofacMvxIocProvider : MvxSingleton<IMvxIoCProvider>, IMvxIoCProvider
    {
        /// <summary>
        /// The container from which dependencies should be resolved.
        /// </summary>
        private readonly IContainer _container;

        /// <summary>
        /// Initializes a new instance of the <see cref="AutofacMvxIocProvider"/> class.
        /// </summary>
        /// <param name="container">
        /// The container from which dependencies should be resolved.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="container"/> is <see langword="null"/>.
        /// </exception>
        public AutofacMvxIocProvider(IContainer container)
        {
            if (container == null)
            {
                throw new ArgumentNullException("container");
            }

            this._container = container;
        }

        /// <summary>
        /// Registers an action to occur when a specific type is registered.
        /// </summary>
        /// <typeparam name="T">
        /// The <see cref="System.Type"/> that should raise the callback when registered.
        /// </typeparam>
        /// <param name="action">
        /// The <see cref="Action"/> to call when the specified type is registered.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="action"/> is <see langword="null"/>.
        /// </exception>
        public void CallbackWhenRegistered<T>(Action action)
        {
            this.CallbackWhenRegistered(typeof(T), action);
        }

        /// <summary>
        /// Registers an action to occur when a specific type is registered.
        /// </summary>
        /// <param name="type">
        /// The <see cref="System.Type"/> that should raise the callback when registered.
        /// </param>
        /// <param name="action">
        /// The <see cref="Action"/> to call when the specified type is registered.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="type"/> or <paramref name="action"/> is <see langword="null"/>.
        /// </exception>
        public void CallbackWhenRegistered(Type type, Action action)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            if (action == null)
            {
                throw new ArgumentNullException("action");
            }

            this._container.ComponentRegistry.Registered += (sender, args) =>
            {
                if (args.ComponentRegistration.Services.OfType<TypedService>().Any(x => x.ServiceType == type))
                {
                    action();
                }
            };
        }

        /// <summary>
        /// Determines whether an instance of a specified type can be resolved.
        /// </summary>
        /// <typeparam name="T">
        /// The <see cref="System.Type"/> to check for resolution.
        /// </typeparam>
        /// <returns>
        /// <see langword="true"/> if the instance can be resolved; <see langword="false"/> if not.
        /// </returns>
        /// <remarks>
        /// <para>
        /// Technically this implementation determines if the type <typeparamref name="T"/>
        /// is registered with the Autofac container. This method returning
        /// <see langword="true"/> does not guarantee that no exception will
        /// be thrown if the type is resolved but there
        /// are missing dependencies for constructing the instance.
        /// </para>
        /// </remarks>
        public bool CanResolve<T>()
            where T : class
        {
            return this.CanResolve(typeof(T));
        }

        /// <summary>
        /// Determines whether an instance of a specified type can be resolved.
        /// </summary>
        /// <param name="type">
        /// The <see cref="System.Type"/> to check for resolution.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the instance can be resolved; <see langword="false"/> if not.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="type"/> is <see langword="null"/>.
        /// </exception>
        /// <remarks>
        /// <para>
        /// Technically this implementation determines if the <paramref name="type"/>
        /// is registered with the Autofac container. This method returning
        /// <see langword="true"/> does not guarantee that no exception will
        /// be thrown if the <paramref name="type"/> is resolved but there
        /// are missing dependencies for constructing the instance.
        /// </para>
        /// </remarks>
        public bool CanResolve(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            return this._container.IsRegistered(type);
        }

        /// <summary>
        /// Resolves a service instance of a specified type.
        /// </summary>
        /// <typeparam name="T">
        /// The <see cref="System.Type"/> of the service to resolve.
        /// </typeparam>
        /// <returns>
        /// The resolved instance of type <typeparamref name="T"/>.
        /// </returns>
        public T Create<T>()
            where T : class
        {
            return (T)this.Create(typeof(T));
        }

        /// <summary>
        /// Resolves a service instance of a specified type.
        /// </summary>
        /// <param name="type">
        /// The <see cref="System.Type"/> of the service to resolve.
        /// </param>
        /// <returns>
        /// The resolved instance of type <paramref name="type"/>.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="type"/> is <see langword="null"/>.
        /// </exception>
        public object Create(Type type)
        {
            return this.Resolve(type);
        }

        /// <summary>
        /// Resolves a singleton service instance of a specified type.
        /// </summary>
        /// <typeparam name="T">
        /// The <see cref="System.Type"/> of the service to resolve.
        /// </typeparam>
        /// <returns>
        /// The resolved singleton instance of type <typeparamref name="T"/>.
        /// </returns>
        public T GetSingleton<T>()
            where T : class
        {
            return (T)this.GetSingleton(typeof(T));
        }

        /// <summary>
        /// Resolves a singleton service instance of a specified type.
        /// </summary>
        /// <param name="type">
        /// The <see cref="System.Type"/> of the service to resolve.
        /// </param>
        /// <returns>
        /// The resolved singleton instance of type <paramref name="type"/>.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="type"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="DependencyResolutionException">
        /// Thrown if the <paramref name="type"/> is not registered as a singleton.
        /// </exception>
        public object GetSingleton(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            var service = new TypedService(type);
            IComponentRegistration registration;
            if (!this._container.ComponentRegistry.TryGetRegistration(service, out registration))
            {
                throw new ComponentNotRegisteredException(service);
            }

            if (registration.Sharing != InstanceSharing.Shared || !(registration.Lifetime is RootScopeLifetime))
            {
                // Ensure the dependency is registered as a singleton
                // WITHOUT resolving the dependency twice.
                throw new DependencyResolutionException(String.Format(CultureInfo.CurrentCulture, AutofacMvxIocProviderResources.TypeNotRegisteredAsSingleton, type));
            }

            return this.Resolve(type);
        }

        /// <summary>
        /// Resolves a service instance of a specified type.
        /// </summary>
        /// <typeparam name="T">
        /// The <see cref="System.Type"/> of the service to resolve.
        /// </typeparam>
        /// <returns>
        /// The resolved instance of type <typeparamref name="T"/>.
        /// </returns>
        public T IoCConstruct<T>()
            where T : class
        {
            return (T)this.IoCConstruct(typeof(T));
        }

        /// <summary>
        /// Resolves a service instance of a specified type.
        /// </summary>
        /// <param name="type">
        /// The <see cref="System.Type"/> of the service to resolve.
        /// </param>
        /// <returns>
        /// The resolved instance of type <paramref name="type"/>.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="type"/> is <see langword="null"/>.
        /// </exception>
        public object IoCConstruct(Type type)
        {
            return this.Resolve(type);
        }

        /// <summary>
        /// Register an instance as a component.
        /// </summary>
        /// <typeparam name="TInterface">
        /// The type of the instance. This may be an interface/service that
        /// the instance implements.
        /// </typeparam>
        /// <param name="theObject">The instance to register.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="theObject"/> is <see langword="null"/>.
        /// </exception>
        public void RegisterSingleton<TInterface>(TInterface theObject)
            where TInterface : class
        {
            this.RegisterSingleton(typeof(TInterface), theObject);
        }

        /// <summary>
        /// Register a delegate as a singleton component.
        /// </summary>
        /// <typeparam name="TInterface">
        /// The type of the instance generated by the function. This may be an interface/service that
        /// the instance implements.
        /// </typeparam>
        /// <param name="theConstructor">
        /// The construction function/delegate to call to create the singleton.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="theConstructor"/> is <see langword="null"/>.
        /// </exception>
        public void RegisterSingleton<TInterface>(Func<TInterface> theConstructor)
            where TInterface : class
        {
            RegisterSingleton(typeof(TInterface), theConstructor);
        }

        /// <summary>
        /// Register an instance as a component.
        /// </summary>
        /// <param name="tInterface">
        /// The type of the instance. This may be an interface/service that
        /// the instance implements.
        /// </param>
        /// <param name="theObject">The instance to register.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="tInterface"/> or <paramref name="theObject"/> is <see langword="null"/>.
        /// </exception>
        public void RegisterSingleton(Type tInterface, object theObject)
        {
            if (tInterface == null)
            {
                throw new ArgumentNullException("tInterface");
            }

            if (theObject == null)
            {
                throw new ArgumentNullException("theObject");
            }

            var cb = new ContainerBuilder();
            cb.RegisterInstance(theObject).As(tInterface).AsSelf().SingleInstance();
            cb.Update(this._container);
        }

        /// <summary>
        /// Register a delegate as a singleton component.
        /// </summary>
        /// <param name="tInterface">
        /// The type of the instance generated by the function. This may be an interface/service that
        /// the instance implements.
        /// </param>
        /// <param name="theConstructor">
        /// The construction function/delegate to call to create the singleton.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="tInterface"/> or <paramref name="theConstructor"/> is <see langword="null"/>.
        /// </exception>
        public void RegisterSingleton(Type tInterface, Func<object> theConstructor)
        {
            if (tInterface == null)
            {
                throw new ArgumentNullException("tInterface");
            }

            if (theConstructor == null)
            {
                throw new ArgumentNullException("theConstructor");
            }

            var cb = new ContainerBuilder();
            cb.Register(cc => theConstructor()).As(tInterface).AsSelf().SingleInstance();
            cb.Update(this._container);
        }

        /// <summary>
        /// Registers a reflection-based component to service mapping.
        /// </summary>
        /// <typeparam name="TFrom">
        /// The component type that implements the service to register.
        /// </typeparam>
        /// <typeparam name="TTo">
        /// The service type that will be resolved from the container.
        /// </typeparam>
        /// <remarks>
        /// <para>
        /// This method updates the container to include a new reflection-based
        /// registration that maps <typeparamref name="TFrom"/> to its own implementing
        /// type as well as to the service type <typeparamref name="TTo"/>.
        /// </para>
        /// </remarks>
        public void RegisterType<TFrom, TTo>()
            where TFrom : class
            where TTo : class, TFrom
        {
            this.RegisterType(typeof(TFrom), typeof(TTo));
        }

        /// <summary>
        /// Register a delegate for creating a component.
        /// </summary>
        /// <typeparam name="TInterface">
        /// The type of the instance generated by the function. This may be an interface/service that
        /// the instance implements.
        /// </typeparam>
        /// <param name="constructor">
        /// The construction function/delegate to call to create the instance.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="constructor"/> is <see langword="null"/>.
        /// </exception>
        public void RegisterType<TInterface>(Func<TInterface> constructor) where TInterface : class
        {
            if (constructor == null)
                throw new ArgumentNullException("constructor");

            var cb = new ContainerBuilder();
            cb.Register(c => constructor()).AsSelf();
            cb.Update(_container);
        }

        /// <summary>
        /// Register a delegate for creating a component.
        /// </summary>
        /// <param name="t">
        /// The type of the instance generated by the function. This may be an interface/service that
        /// the instance implements.
        /// </param>
        /// <param name="constructor">
        /// The construction function/delegate to call to create the instance.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="t"/> or <paramref name="constructor"/> is <see langword="null"/>.
        /// </exception>
        public void RegisterType(Type t, Func<object> constructor)
        {
            if (t == null)
                throw new ArgumentNullException("t");

            if (constructor == null)
                throw new ArgumentNullException("constructor");

            var cb = new ContainerBuilder();
            cb.Register(c => constructor()).As(t).AsSelf();
            cb.Update(_container);
        }

        /// <summary>
        /// Registers a reflection-based component to service mapping.
        /// </summary>
        /// <param name="tFrom">
        /// The component type that implements the service to register.
        /// </param>
        /// <param name="tTo">
        /// The service type that will be resolved from the container.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="tFrom"/> or <paramref name="tTo"/> is <see langword="null"/>.
        /// </exception>
        /// <remarks>
        /// <para>
        /// This method updates the container to include a new reflection-based
        /// registration that maps <paramref name="tFrom"/> to its own implementing
        /// type as well as to the service type <paramref name="tTo"/>.
        /// </para>
        /// </remarks>
        public void RegisterType(Type tFrom, Type tTo)
        {
            if (tFrom == null)
            {
                throw new ArgumentNullException("tFrom");
            }

            if (tTo == null)
            {
                throw new ArgumentNullException("tTo");
            }

            var cb = new ContainerBuilder();
            cb.RegisterType(tTo).As(tFrom).AsSelf();
            cb.Update(this._container);
        }

        /// <summary>
        /// Resolves a service instance of a specified type.
        /// </summary>
        /// <typeparam name="T">
        /// The <see cref="System.Type"/> of the service to resolve.
        /// </typeparam>
        /// <returns>
        /// The resolved instance of type <typeparamref name="T"/>.
        /// </returns>
        public T Resolve<T>()
            where T : class
        {
            return (T)this.Resolve(typeof(T));
        }

        /// <summary>
        /// Resolves a service instance of a specified type.
        /// </summary>
        /// <param name="type">
        /// The <see cref="System.Type"/> of the service to resolve.
        /// </param>
        /// <returns>
        /// The resolved instance of type <paramref name="type"/>.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="type"/> is <see langword="null"/>.
        /// </exception>
        public object Resolve(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            return this._container.Resolve(type);
        }

        /// <summary>
        /// Tries to retrieve a service of a specified type.
        /// </summary>
        /// <typeparam name="T">
        /// The service <see cref="System.Type"/> to resolve.
        /// </typeparam>
        /// <param name="resolved">
        /// The resulting component instance providing the service, or default(T) if resolution is not possible.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if a component providing the service is available; <see langword="false"/> if not.
        /// </returns>
        public bool TryResolve<T>(out T resolved)
            where T : class
        {
            return this._container.TryResolve<T>(out resolved);
        }

        /// <summary>
        /// Tries to retrieve a service of a specified type.
        /// </summary>
        /// <param name="type">
        /// The service <see cref="System.Type"/> to resolve.
        /// </param>
        /// <param name="resolved">
        /// The resulting component instance providing the service, or <see langword="null"/> if resolution is not possible.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if a component providing the service is available; <see langword="false"/> if not.
        /// </returns>
        public bool TryResolve(Type type, out object resolved)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            return this._container.TryResolve(type, out resolved);
        }
    }
}
