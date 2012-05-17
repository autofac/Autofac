using System;
using System.Globalization;
using System.Security;
using System.ServiceModel;
using Autofac;
using Autofac.Extras.Multitenant.Properties;
using Autofac.Extras.Multitenant.Wcf.DynamicProxy;

namespace Autofac.Extras.Multitenant.Wcf
{
    /// <summary>
    /// Service implementation data provider that returns multitenant-aware
    /// service hosting information.
    /// </summary>
    [SecurityCritical]
    public class MultitenantServiceImplementationDataProvider : IServiceImplementationDataProvider
    {
        /// <summary>
        /// Proxy generator used to create proxy types that will be substituted
        /// in during service hosting.
        /// </summary>
        private static readonly ServiceHostProxyGenerator _generator = new ServiceHostProxyGenerator();

        /// <summary>
        /// Gets data about a service implementation.
        /// </summary>
        /// <param name="value">
        /// The constructor string passed in to the service host factory
        /// that is used to determine which type to host/use as a service
        /// implementation.
        /// </param>
        /// <returns>
        /// A <see cref="Autofac.Extras.Multitenant.Wcf.ServiceImplementationData"/>
        /// object containing information about which type to use in
        /// the service host and how to resolve the implementation.
        /// </returns>
        /// <remarks>
        /// <para>
        /// This method returns a dynamic proxy object as the type to host
        /// and resolves the implementation type as a dynamic proxy that proxies
        /// to a tenant-specific implementation. This is necessary since
        /// WCF will only allow hosting of concrete classes and we need it to,
        /// effectively, host an 'interface' - in this case, a dynamic proxy type
        /// rather than an actual implementation type.
        /// </para>
        /// </remarks>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="value" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// Thrown if <paramref name="value" /> is empty.
        /// </exception>
        /// <exception cref="System.InvalidOperationException">
        /// Thrown if <paramref name="value" /> does not resolve into
        /// a known type, resolves to a type that is not an interface, or the
        /// interface it resolves to is not marked with a <see cref="System.ServiceModel.ServiceContractAttribute"/>.
        /// </exception>
        [SecuritySafeCritical]
        public ServiceImplementationData GetServiceImplementationData(string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            if (value.Length == 0)
            {
                throw new ArgumentException(String.Format(CultureInfo.CurrentUICulture, Properties.Resources.ArgumentException_StringEmpty, "value"));
            }

            Type serviceInterfaceType = Type.GetType(value, false);
            if (serviceInterfaceType == null)
            {
                throw new InvalidOperationException(String.Format(CultureInfo.CurrentUICulture, Resources.MultitenantServiceImplementationDataProvider_ServiceInterfaceTypeNotResolvable, value));
            }
            if (!serviceInterfaceType.IsInterface)
            {
                throw new InvalidOperationException(String.Format(CultureInfo.CurrentUICulture, Resources.MultitenantServiceImplementationDataProvider_ServiceInterfaceTypeNotInterface, value, serviceInterfaceType));
            }
            if (serviceInterfaceType.GetCustomAttributes(typeof(ServiceContractAttribute), false).Length == 0)
            {
                throw new InvalidOperationException(String.Format(CultureInfo.CurrentUICulture, Resources.MultitenantServiceImplementationDataProvider_ServiceInterfaceTypeNotServiceContract, value, serviceInterfaceType));
            }

            // To create the actual proxy object type that will be used to sub-in
            // tenant-specific instances, we need to create a dummy implementation
            // (which is an empty proxy without a target) and then a real proxy
            // (which will use that dummy as a target). The reason is that Castle
            // generates two different proxy types when this happens - the first
            // proxy type - the dummy object - will be something like "Castle.Proxies.ProxyType"
            // and the second proxy type - the one with a target - will be
            // something like "Castle.Proxies.ProxyType_1". Subsequent proxies
            // created for the same interface type that have a target will be
            // of the same type as the original - "Castle.Proxies.ProxyType_1"
            // (or whatever) because Castle caches the various proxy type definitions.
            var dummyHostProxyObject = _generator.CreateInterfaceProxyWithoutTarget(serviceInterfaceType);
            var actualHostProxyObject = _generator.CreateWcfProxy(serviceInterfaceType, dummyHostProxyObject);

            return new ServiceImplementationData()
            {
                ConstructorString = value,
                ServiceTypeToHost = actualHostProxyObject.GetType(),
                ImplementationResolver = l =>
                    {
                        var implementation = l.Resolve(serviceInterfaceType);
                        // The wrapped implementation will be of the same proxy
                        // type as "actualHostProxyObject" above.
                        var implementationProxy = _generator.CreateWcfProxy(serviceInterfaceType, implementation);
                        return implementationProxy;
                    }
            };
        }
    }
}
