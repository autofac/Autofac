using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Reflection;

namespace Autofac.Builder
{
	/// <summary>
	/// Base class for component registrars.
	/// </summary>
	abstract class Registrar : IRegistrar
	{
		IEnumerable<Type> _services;
		InstanceOwnership _ownership = InstanceOwnership.Container;
		InstanceScope _scope = InstanceScope.Singleton;
        IList<EventHandler<ActivatingEventArgs>> _activatingHandlers = new List<EventHandler<ActivatingEventArgs>>();
        IList<EventHandler<ActivatedEventArgs>> _activatedHandlers = new List<EventHandler<ActivatedEventArgs>>();
        IList<Type> _factoryDelegates = new List<Type>();

		#region IRegistrar Members

		/// <summary>
		/// Change the service associated with the registration.
		/// </summary>
		/// <typeparam name="TService">The service that the registration will expose.</typeparam>
		/// <returns>A registrar allowing registration to continue.</returns>
		public IRegistrar As<TService>()
		{
			return As(new[] { typeof(TService) });
		}

		/// <summary>
		/// Change the services associated with the registration.
		/// </summary>
		/// <typeparam name="TService1">The first service that the registration will expose.</typeparam>
		/// <typeparam name="TService2">The second service that the registration will expose.</typeparam>
		/// <returns>A registrar allowing registration to continue.</returns>
		public IRegistrar As<TService1, TService2>()
		{
            return As(new[] { typeof(TService1), typeof(TService2) });
		}

		/// <summary>
		/// Change the services associated with the registration.
		/// </summary>
		/// <typeparam name="TService1">The first service that the registration will expose.</typeparam>
		/// <typeparam name="TService2">The second service that the registration will expose.</typeparam>
		/// <typeparam name="TService3">The third service that the registration will expose.</typeparam>
		/// <returns>A registrar allowing registration to continue.</returns>
		public IRegistrar As<TService1, TService2, TService3>()
		{
			return As(new[] { typeof(TService1), typeof(TService2), typeof(TService3) });
		}

		/// <summary>
		/// Change the service associated with the registration.
		/// </summary>
		/// <param name="services">The services that the registration will expose.</param>
		/// <returns>
		/// A registrar allowing registration to continue.
		/// </returns>
		public IRegistrar As(params Type[] services)
		{
            Enforce.ArgumentNotNull(services, "services");
			Services = services;
			return this;
		}

        public IRegistrar ThroughFactory(Type factoryDelegate)
        {
            Enforce.ArgumentNotNull(factoryDelegate, "factoryDelegate");
            MethodInfo invoke = factoryDelegate.GetMethod("Invoke");
            if (invoke == null)
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture,
                    RegistrarResources.TypeIsNotADelegate, factoryDelegate));
            else if (invoke.ReturnType == typeof(void))
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture,
                    RegistrarResources.DelegateReturnsVoid, factoryDelegate));

            FactoryDelegates.Add(factoryDelegate);
            return this;
        }

        public IRegistrar ThroughFactory<TFactoryDelegate>()
        {
            return ThroughFactory(typeof(TFactoryDelegate));
        }

		/// <summary>
		/// Change the ownership model associated with the registration.
		/// This determines when the instances are disposed and by whom.
		/// </summary>
		/// <param name="ownership">The ownership model to use.</param>
		/// <returns>
		/// A registrar allowing registration to continue.
		/// </returns>
		public IRegistrar WithOwnership(InstanceOwnership ownership)
		{
			Ownership = ownership;
			return this;
		}

		/// <summary>
		/// Change the scope associated with the registration.
		/// This determines how instances are tracked and shared.
		/// </summary>
		/// <param name="scope">The scope model to use.</param>
		/// <returns>
		/// A registrar allowing registration to continue.
		/// </returns>
		public IRegistrar WithScope(InstanceScope scope)
		{
			Scope = scope;
			return this;
		}

        /// <summary>
        /// Call the provided handler when activating an instance.
        /// </summary>
        /// <param name="handler">The handler.</param>
        /// <returns>A registrar allowing registration to continue.</returns>
        public IRegistrar OnActivating(EventHandler<ActivatingEventArgs> handler)
        {
            Enforce.ArgumentNotNull(handler, "handler");
            _activatingHandlers.Add(handler);
            return this;
        }

        /// <summary>
        /// Call the provided handler when an instance is activated.
        /// </summary>
        /// <param name="handler">The handler.</param>
        /// <returns>A registrar allowing registration to continue.</returns>
        public IRegistrar OnActivated(EventHandler<ActivatedEventArgs> handler)
        {
            Enforce.ArgumentNotNull(handler, "handler");
            _activatedHandlers.Add(handler);
            return this;
        }

		#endregion

		/// <summary>
		/// The services exposed by this registration.
		/// </summary>
		protected virtual IEnumerable<Type> Services
		{
			get
			{
				return _services;
			}
			set
			{
                Enforce.ArgumentNotNull(value, "value");
				_services = value;
			}
		}

        /// <summary>
        /// The factory delegates that can create the component.
        /// </summary>
        protected ICollection<Type> FactoryDelegates
        {
            get
            {
                return _factoryDelegates;
            }
        }

		/// <summary>
		/// The instance scope used by this registration.
		/// </summary>
		protected InstanceScope Scope
		{
			get
			{
				return _scope;
			}
			set
			{
				_scope = value;
			}
		}

		/// <summary>
		/// The instance ownership used by this registration.
		/// </summary>
		protected InstanceOwnership Ownership
		{
			get
			{
				return _ownership;
			}
			set
			{
				_ownership = value;
			}
		}

        /// <summary>
        /// The handlers for the Activating event used by this registration.
        /// </summary>
        protected IEnumerable<EventHandler<ActivatingEventArgs>> ActivatingHandlers
        {
            get
            {
                return _activatingHandlers;
            }
        }

        /// <summary>
        /// The handlers for the Activated event used by this registration.
        /// </summary>
        protected IEnumerable<EventHandler<ActivatedEventArgs>> ActivatedHandlers
        {
            get
            {
                return _activatedHandlers;
            }
        }
	}
}
