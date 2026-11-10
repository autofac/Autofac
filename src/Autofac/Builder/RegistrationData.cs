// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac.Core;
using Autofac.Core.Lifetime;
using Autofac.Core.Registration;
using Autofac.Util;

namespace Autofac.Builder;

/// <summary>
/// Data common to all registrations made in the container, both direct (IComponentRegistration)
/// and dynamic (IRegistrationSource).
/// </summary>
public class RegistrationData
{
    // The services form a set kept in the order they were added. Most registrations have one,
    // so it is held in a field, and the set is only created for a second.
    private Service? _firstService;
    private HashSet<Service>? _moreServices;
    private bool _defaultServiceOverridden;
    private Service _defaultService;
    private IComponentLifetime _lifetime = CurrentScopeLifetime.Instance;

    /// <summary>
    /// Initializes a new instance of the <see cref="RegistrationData"/> class.
    /// </summary>
    /// <param name="defaultService">The default service that will be used if no others
    /// are added.</param>
    public RegistrationData(Service defaultService)
    {
        _defaultService = defaultService ?? throw new ArgumentNullException(nameof(defaultService));

        Metadata = new Dictionary<string, object?>
        {
            { MetadataKeys.RegistrationOrderMetadataKey, SequenceGenerator.GetNextUniqueSequence() },
        };
    }

    /// <summary>
    /// Gets or sets the instance ownership assigned to the component.
    /// </summary>
    public InstanceOwnership Ownership { get; set; } = InstanceOwnership.OwnedByLifetimeScope;

    /// <summary>
    /// Gets the services explicitly assigned to the component.
    /// </summary>
    public IEnumerable<Service> Services
    {
        get
        {
            if (!_defaultServiceOverridden && !ContainsService(_defaultService))
            {
                yield return _defaultService;
            }

            if (_firstService is not null)
            {
                yield return _firstService;
            }

            if (_moreServices is not null)
            {
                foreach (var service in _moreServices)
                {
                    yield return service;
                }
            }
        }
    }

    /// <summary>
    /// Gets or sets the lifetime assigned to the component.
    /// </summary>
    public IComponentLifetime Lifetime
    {
        get
        {
            return _lifetime;
        }

        set
        {
            _lifetime = value ?? throw new ArgumentNullException(nameof(value));
        }
    }

    /// <summary>
    /// Gets or sets the sharing mode assigned to the component.
    /// </summary>
    public InstanceSharing Sharing { get; set; } = InstanceSharing.None;

    /// <summary>
    /// Gets the extended properties assigned to the component.
    /// </summary>
    public IDictionary<string, object?> Metadata
    {
        get;
    }

    /// <summary>
    /// Gets or sets the options for the registration.
    /// </summary>
    public RegistrationOptions Options
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the callback used to register this component.
    /// </summary>
    /// <value>
    /// A <see cref="Builder.DeferredCallback"/> that contains the delegate
    /// used to register this component with an <see cref="IComponentRegistry"/>.
    /// </value>
    public DeferredCallback? DeferredCallback
    {
        get; set;
    }

    /// <summary>
    /// Add multiple services for the registration, overriding the default.
    /// </summary>
    /// <param name="services">The services to add.</param>
    /// <remarks>If an empty collection is specified, this will still
    /// clear the default service.</remarks>
    public void AddServices(IEnumerable<Service> services)
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        var empty = true;
        foreach (var service in services)
        {
            empty = false;
            AddService(service);
        }

        // `As([])` clears the default service.
        _defaultServiceOverridden = _defaultServiceOverridden || empty;
    }

    /// <summary>
    /// Add a service to the registration, overriding the default.
    /// </summary>
    /// <param name="service">The service to add.</param>
    public void AddService(Service service)
    {
        if (service == null)
        {
            throw new ArgumentNullException(nameof(service));
        }

        // `AutoActivateService` is internal plumbing; `AutoActivate()` isn't expected to modify user-visible
        // services.
        _defaultServiceOverridden = _defaultServiceOverridden || service is not AutoActivateService;
        AddToServices(service);
    }

    /// <summary>
    /// Copies the contents of another RegistrationData object into this one.
    /// </summary>
    /// <param name="that">The data to copy.</param>
    /// <param name="includeDefaultService">When true, the default service
    /// will be changed to that of the other.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="that" /> is <see langword="null" />.
    /// </exception>
    public void CopyFrom(RegistrationData that, bool includeDefaultService)
    {
        if (that == null)
        {
            throw new ArgumentNullException(nameof(that));
        }

        Ownership = that.Ownership;
        Sharing = that.Sharing;
        Lifetime = that.Lifetime;

        _defaultServiceOverridden |= that._defaultServiceOverridden;
        if (includeDefaultService)
        {
            _defaultService = that._defaultService;
        }

        if (that._firstService is not null)
        {
            AddToServices(that._firstService);
        }

        if (that._moreServices is not null)
        {
            foreach (var service in that._moreServices)
            {
                AddToServices(service);
            }
        }

        AddAll(Metadata, that.Metadata.Where(m => m.Key != MetadataKeys.RegistrationOrderMetadataKey));
    }

    /// <summary>
    /// Empties the configured services.
    /// </summary>
    public void ClearServices()
    {
        _firstService = null;
        _moreServices?.Clear();
        _defaultServiceOverridden = true;
    }

    /// <summary>
    /// Copies <see cref="Services"/> into a new array, in the same order.
    /// </summary>
    /// <returns>The services.</returns>
    internal Service[] ServicesToArray()
    {
        var includeDefault = !_defaultServiceOverridden && !ContainsService(_defaultService);
        var count = (includeDefault ? 1 : 0) + (_firstService is null ? 0 : 1) + (_moreServices?.Count ?? 0);
        var services = new Service[count];
        var index = 0;

        if (includeDefault)
        {
            services[index++] = _defaultService;
        }

        if (_firstService is not null)
        {
            services[index++] = _firstService;
        }

        _moreServices?.CopyTo(services, index);
        return services;
    }

    private static void AddAll<T>(ICollection<T> to, IEnumerable<T> from)
    {
        foreach (var item in from)
        {
            to.Add(item);
        }
    }

    /// <summary>
    /// Adds a service unless an equal one is already present, as <see cref="HashSet{T}.Add"/> does.
    /// </summary>
    /// <param name="service">The service to add.</param>
    private void AddToServices(Service service)
    {
        if (_firstService is null)
        {
            // Rejects a service that can't compute a hash code, as the set would.
            _ = service.GetHashCode();
            _firstService = service;
        }
        else if (!EqualityComparer<Service>.Default.Equals(_firstService, service))
        {
            (_moreServices ??= new HashSet<Service>()).Add(service);
        }
    }

    /// <summary>
    /// Checks for an equal service, as <see cref="HashSet{T}.Contains"/> does.
    /// </summary>
    /// <param name="service">The service to look for.</param>
    /// <returns>True if an equal service is present.</returns>
    private bool ContainsService(Service service)
    {
        if (_firstService is null)
        {
            return false;
        }

        return EqualityComparer<Service>.Default.Equals(_firstService, service) || (_moreServices?.Contains(service) ?? false);
    }
}
