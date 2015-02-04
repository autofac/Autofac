using System;
using System.Collections.Generic;

namespace Autofac.Core.Registration
{
    /// <summary>
    /// Delegates registration lookups to a specified registry. When write operations are applied,
    /// initialises a new 'writeable' registry.
    /// </summary>
    /// <remarks>
    /// Safe for concurrent access by multiple readers. Write operations are single-threaded.
    /// </remarks>
    class CopyOnWriteRegistry : IComponentRegistry
    {
        readonly IComponentRegistry _readRegistry;
        readonly Func<IComponentRegistry> _createWriteRegistry;
        IComponentRegistry _writeRegistry;

        public CopyOnWriteRegistry(IComponentRegistry readRegistry, Func<IComponentRegistry> createWriteRegistry)
        {
            if (readRegistry == null) throw new ArgumentNullException("readRegistry");
            if (createWriteRegistry == null) throw new ArgumentNullException("createWriteRegistry");
            _readRegistry = readRegistry;
            _createWriteRegistry = createWriteRegistry;
        }

        IComponentRegistry Registry { get { return _writeRegistry ?? _readRegistry; } }

        IComponentRegistry WriteRegistry
        {
            get
            {
                if (_writeRegistry == null)
                    _writeRegistry = _createWriteRegistry();
                return _writeRegistry;
            }
        }

        public void Dispose()
        {
            if (_readRegistry != null)
                _readRegistry.Dispose();
        }

        public bool TryGetRegistration(Service service, out IComponentRegistration registration)
        {
            return Registry.TryGetRegistration(service, out registration);
        }

        public bool IsRegistered(Service service)
        {
            return Registry.IsRegistered(service);
        }

        public void Register(IComponentRegistration registration)
        {
            WriteRegistry.Register(registration);
        }

        public void Register(IComponentRegistration registration, bool preserveDefaults)
        {
            WriteRegistry.Register(registration, preserveDefaults);
        }

        public IEnumerable<IComponentRegistration> Registrations
        {
            get { return Registry.Registrations; }
        }

        public IEnumerable<IComponentRegistration> RegistrationsFor(Service service)
        {
            return Registry.RegistrationsFor(service);
        }

        public event EventHandler<ComponentRegisteredEventArgs> Registered
        {
            add { WriteRegistry.Registered += value; }
            remove { WriteRegistry.Registered -= value; }
        }

        public void AddRegistrationSource(IRegistrationSource source)
        {
            WriteRegistry.AddRegistrationSource(source);
        }

        public IEnumerable<IRegistrationSource> Sources
        {
            get { return Registry.Sources; }
        }

        public bool HasLocalComponents
        {
            get { return _writeRegistry != null; }
        }

        public event EventHandler<RegistrationSourceAddedEventArgs> RegistrationSourceAdded
        {
            add { WriteRegistry.RegistrationSourceAdded += value; }
            remove { WriteRegistry.RegistrationSourceAdded -= value; }
        }
    }
}