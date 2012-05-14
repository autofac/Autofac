using System;
using System.Globalization;
using System.Security;
using System.ServiceModel;
using AutofacContrib.Multitenant.Properties;
using Castle.DynamicProxy;

namespace AutofacContrib.Multitenant.Wcf.DynamicProxy
{
    /// <summary>
    /// Proxy generator used in multitenant service hosting.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The WCF service host has very specific requirements around the object type that
    /// you pass in when you call <see cref="System.ServiceModel.Activation.ServiceHostFactory.CreateServiceHost(Type,Uri[])"/>.
    /// </para>
    /// <para>
    /// If you have a type that has a <see cref="System.ServiceModel.ServiceContractAttribute"/>
    /// on it and it implements an interface that has <see cref="System.ServiceModel.ServiceContractAttribute"/>
    /// on it, the WCF service host complains that you can't have two different
    /// service contracts.
    /// </para>
    /// <para>
    /// The proxy generator uses a <see cref="AutofacContrib.Multitenant.Wcf.DynamicProxy.ServiceHostProxyBuilder"/>
    /// to build the proxy types. This is specifically interesting in the
    /// <see cref="AutofacContrib.Multitenant.Wcf.DynamicProxy.ServiceHostProxyGenerator.CreateWcfProxy"/>
    /// method, which uses some special overrides and additions in the builder.
    /// </para>
    /// <para>
    /// The builder, when called through
    /// <see cref="AutofacContrib.Multitenant.Wcf.DynamicProxy.ServiceHostProxyGenerator.CreateWcfProxy"/>,
    /// generates proxy types that ignore non-inherited
    /// attributes on the service interface (e.g.,
    /// <see cref="System.ServiceModel.ServiceContractAttribute"/>)
    /// so when the proxy type is generated, it doesn't bring over anything
    /// that will cause WCF host initialization to fail or get confused.
    /// </para>
    /// </remarks>
    [SecurityCritical]
    public class ServiceHostProxyGenerator : ProxyGenerator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceHostProxyGenerator"/> class.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The proxy generator uses a <see cref="AutofacContrib.Multitenant.Wcf.DynamicProxy.ServiceHostProxyBuilder"/>
        /// to build the proxy types.
        /// </para>
        /// </remarks>
        public ServiceHostProxyGenerator()
            : base(new ServiceHostProxyBuilder())
        {
        }

        /// <summary>
        /// Creates a proxy object that can be used by the WCF service host.
        /// </summary>
        /// <param name="interfaceToProxy">
        /// The WCF service interface for service implementations.
        /// </param>
        /// <param name="target">
        /// The target of the proxy object that will receive the actual calls.
        /// </param>
        /// <returns>
        /// An object that implements the interface <paramref name="interfaceToProxy" />
        /// and proxies calls to the <paramref name="target" />.
        /// </returns>
        /// <remarks>
        /// <para>
        /// When initializing the service host, call this method with a dummy
        /// <paramref name="target" /> object, just to create the dynamic proxy
        /// type for the first time and get the service host up and running.
        /// Subsequent proxies for that interface should have a valid target
        /// implementation type to which service calls will be proxied.
        /// </para>
        /// </remarks>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="interfaceToProxy" /> or <paramref name="target" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// <para>
        /// Thrown if:
        /// </para>
        /// <list type="bullet">
        /// <item>
        /// <term><paramref name="interfaceToProxy" /> is not an interface.</term>
        /// </item>
        /// <item>
        /// <term><paramref name="interfaceToProxy" /> is an open generic.</term>
        /// </item>
        /// <item>
        /// <term><paramref name="target" /> cannot be cast to <paramref name="interfaceToProxy" />.</term>
        /// </item>
        /// </list>
        /// </exception>
        public object CreateWcfProxy(Type interfaceToProxy, object target)
        {
            if (interfaceToProxy == null)
            {
                throw new ArgumentNullException("interfaceToProxy");
            }
            if (!interfaceToProxy.IsInterface)
            {
                throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, Resources.DynamicProxy_InterfaceTypeToProxyNotInterface, interfaceToProxy.FullName), "interfaceToProxy");
            }
            if (interfaceToProxy.IsGenericTypeDefinition)
            {
                throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, Resources.DynamicProxy_InterfaceTypeToProxyIsGeneric, interfaceToProxy.FullName), "interfaceToProxy");
            }
            if (target == null)
            {
                throw new ArgumentNullException("target");
            }
            if (!interfaceToProxy.IsAssignableFrom(target.GetType()))
            {
                throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, Resources.DynamicProxy_ProxyTargetDoesNotImplementInterface, target.GetType().FullName, interfaceToProxy.FullName), "target");
            }
            if (interfaceToProxy.GetCustomAttributes(typeof(ServiceContractAttribute), false).Length == 0)
            {
                throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, Resources.DynamicProxy_InterfaceTypeToProxyNotServiceContract, interfaceToProxy.FullName));
            }

            Type type = this.CreateWcfProxyType(interfaceToProxy);
            return Activator.CreateInstance(type, new object[] { new IInterceptor[0], target });
        }

        /// <summary>
        /// Creates the WCF service interface proxy type or retrieves it from cache.
        /// </summary>
        /// <param name="interfaceToProxy">
        /// The interface type that will be proxied.
        /// </param>
        /// <returns>
        /// A generated proxy type that can be used to proxy calls to actual
        /// service implementations.
        /// </returns>
        protected virtual Type CreateWcfProxyType(Type interfaceToProxy)
        {
            return ((ServiceHostProxyBuilder)this.ProxyBuilder).CreateWcfProxyType(interfaceToProxy);
        }
    }
}
