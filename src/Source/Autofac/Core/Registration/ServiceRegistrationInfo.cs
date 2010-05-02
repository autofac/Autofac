using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Autofac.Core.Registration
{
    /// <summary>
    /// Tracks the services known to the registry.
    /// </summary>
    class ServiceRegistrationInfo
    {
        readonly Service _service;

        readonly LinkedList<IComponentRegistration> _implementations = new LinkedList<IComponentRegistration>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceRegistrationInfo"/> class.
        /// </summary>
        /// <param name="service">The tracked service.</param>
        public ServiceRegistrationInfo(Service service)
        {
            _service = service;
        }

        /// <summary>
        /// The first time a service is requested, initialization (e.g. reading from sources)
        /// happens. This value will then be set to true. Calling many methods on this type before
        /// initialisation is an error.
        /// </summary>
        public bool IsInitialized { get; set; }

        /// <summary>
        /// The known implementations.
        /// </summary>
        public IEnumerable<IComponentRegistration> Implementations
        { 
            get
            {
                RequiresInitialization();
                return _implementations; 
            }
        }

        void RequiresInitialization()
        {
            if (!IsInitialized)
                throw new InvalidOperationException();
        }

        /// <summary>
        /// True if any implementations are known.
        /// </summary>
        public bool IsRegistered
        { 
            get 
            {
                RequiresInitialization();
                return Any; 
            } 
        }

        bool Any { get { return _implementations.First != null; } }

        /// <summary>
        /// Used for bookkeeping so that the same source is not queried twice (may be null.)
        /// </summary>
        public Queue<IRegistrationSource> SourcesToQuery { get; set; }

        public void AddImplementation(IComponentRegistration registration, bool preserveDefaults)
        {
            if (preserveDefaults)
            {
                _implementations.AddLast(registration);
            }
            else
            {
                if (Any)
                    Debug.WriteLine(String.Format(
                        "[Autofac] Overriding default for: '{0}' with: '{1}' (was '{2}')",
                        _service, registration, _implementations.First));

                _implementations.AddFirst(registration);
            }
        }

        public bool TryGetRegistration(out IComponentRegistration registration)
        {
            RequiresInitialization();

            if (Any)
            {
                registration = _implementations.First.Value;
                return true;
            }

            registration = null;
            return false;
        }
    }
}