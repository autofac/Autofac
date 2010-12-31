using System;
using System.Collections.Generic;
using Castle.DynamicProxy;
using Castle.DynamicProxy.Contributors;
using Castle.DynamicProxy.Generators;
using Castle.DynamicProxy.Generators.Emitters;

namespace AutofacContrib.Multitenant.Wcf.DynamicProxy
{
    /// <summary>
    /// Interface proxy generator that builds a proxy that has a default constructor
    /// and does not copy over non-inherited type attributes.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The standard <see cref="Castle.DynamicProxy.Generators.InterfaceProxyWithTargetInterfaceGenerator"/>
    /// builds a proxy object that has no default constructor. While a default
    /// constructor is not useful from an actual proxying standpoint, the WCF
    /// service host will only host object types that have default constructors.
    /// As such, if we want to start the service host with a proxy type, the
    /// proxy type has to have a default constructor.
    /// </para>
    /// <para>
    /// Also, the standard <see cref="Castle.DynamicProxy.Generators.InterfaceProxyWithTargetInterfaceGenerator"/>
    /// generates a type that copies all of the non-inherited attributes over
    /// from the target interface, which causes WCF to choke on the
    /// <see cref="System.ServiceModel.ServiceContractAttribute"/>, which is
    /// already on the service contract interface. This generator overrides
    /// <see cref="AutofacContrib.Multitenant.Wcf.DynamicProxy.ServiceHostInterfaceProxyGenerator.GetTypeImplementerMapping"/>
    /// to change the set of code generating contributors to make a slimmer
    /// proxy that WCF hosting will accept.
    /// </para>
    /// </remarks>
    /// <seealso cref="AutofacContrib.Multitenant.Wcf.DynamicProxy.IgnoreAttributeInterfaceProxyInstanceContributor"/>
    public class ServiceHostInterfaceProxyGenerator : InterfaceProxyWithTargetInterfaceGenerator
    {
        /// <summary>
        /// Holds the metadata buddy class type for the service interface.
        /// </summary>
        private readonly Type _metadataBuddyType;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceHostInterfaceProxyGenerator"/> class.
        /// </summary>
        /// <param name="scope">The scope of the module being built.</param>
        /// <param name="interface">The interface that will be proxied.</param>
        public ServiceHostInterfaceProxyGenerator(ModuleScope scope, Type @interface)
            : base(scope, @interface)
        {
            _metadataBuddyType = @interface.GetMetadataClassType();
        }

        // If it turns out the proxy type needs a parameterless constructor,
        // override the BuildClassEmitter method here, call base, and then
        // add a constructor to the ClassEmitter like this:
        // emitter.CreateConstructor(new ArgumentReference[0]);

        // If it turns out custom attributes also need to be copied at the
        // method/property/field level, the appropriate overrides need to happen
        // just like the CreateTypeAttributes override, below.

        /// <summary>
        /// Adds custom attributes to the generated type.
        /// </summary>
        /// <param name="emitter">The class emitter.</param>
        /// <remarks>
        /// <para>
        /// This override calls the base functionality and then uses the metadata
        /// buddy class (as specified by a <see cref="AutofacContrib.Multitenant.Wcf.ServiceMetadataTypeAttribute"/>
        /// on the service interface) to copy over class-level attributes to the
        /// dynamic hosting proxy.
        /// </para>
        /// </remarks>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="emitter" /> is <see langword="null" />.
        /// </exception>
        protected override void CreateTypeAttributes(ClassEmitter emitter)
        {
            if (emitter == null)
            {
                throw new ArgumentNullException("emitter");
            }
            base.CreateTypeAttributes(emitter);
            if (_metadataBuddyType == null)
            {
                return;
            }
            var attribs = _metadataBuddyType.GetCustomAttributesData();
            foreach (var attrib in attribs)
            {
                var builder = attrib.ToAttributeBuilder();
                emitter.DefineCustomAttribute(builder);
            }
        }

        /// <summary>
        /// Gets the contributors for generating the type definition.
        /// </summary>
        /// <param name="interfaces">Additional interfaces to implement.</param>
        /// <param name="proxyTargetType">The target type for the proxy.</param>
        /// <param name="contributors">The list of contributors that will be used to generate the type.</param>
        /// <param name="namingScope">The proxy type naming scope.</param>
        /// <returns>
        /// The list of types being implemented.
        /// </returns>
        /// <remarks>
        /// <para>
        /// This version of the method basically does the same thing as the
        /// original/base implementation but with these key differences:
        /// </para>
        /// <list type="bullet">
        /// <item>
        /// <term>No mixin support</term>
        /// <description>
        /// The original version of the method looks at the <see cref="Castle.DynamicProxy.Generators.BaseProxyGenerator.ProxyGenerationOptions"/>
        /// to see if there are any mixins to be added to the generated proxy.
        /// There is no need for mixin support in WCF service hosting so mixins
        /// aren't even checked for and won't be added.
        /// </description>
        /// </item>
        /// <item>
        /// <term>No additional interfaces</term>
        /// <description>
        /// The original version of the method goes through each of the additional
        /// interfaces that the are to be implemented, checks them against collisions
        /// with mixin definitions, and adds type mappings for the additional interfaces.
        /// The only interface that needs to be implemented for the WCF hosting
        /// proxy is the service interface, so all of that additional interface
        /// checking is skipped.
        /// </description>
        /// </item>
        /// <item>
        /// <term>Custom instance contributor used</term>
        /// <description>
        /// The original version of the method uses the
        /// <see cref="Castle.DynamicProxy.Contributors.InterfaceProxyInstanceContributor"/>
        /// as the code generator for the proxy type. Unfortunately, that contributor
        /// copies over all non-inherited attributes on the interface including
        /// the <see cref="System.ServiceModel.ServiceContractAttribute"/>. The
        /// concrete proxy type can't have that attribute because the interface
        /// already has it, so WCF hosting dies. This version of the method uses
        /// the <see cref="AutofacContrib.Multitenant.Wcf.DynamicProxy.IgnoreAttributeInterfaceProxyInstanceContributor"/>
        /// which does not copy over non-inherited attributes.
        /// </description>
        /// </item>
        /// </list>
        /// </remarks>
        /// <seealso cref="AutofacContrib.Multitenant.Wcf.DynamicProxy.IgnoreAttributeInterfaceProxyInstanceContributor"/>
        protected override IEnumerable<Type> GetTypeImplementerMapping(Type[] interfaces, Type proxyTargetType, out IEnumerable<ITypeContributor> contributors, INamingScope namingScope)
        {
            var typeImplementerMapping = new Dictionary<Type, ITypeContributor>();
            var allInterfaces = TypeUtil.GetAllInterfaces(new[] { proxyTargetType });
            var additionalInterfaces = TypeUtil.GetAllInterfaces(interfaces);
            var implementer = AddMappingForTargetType(typeImplementerMapping, proxyTargetType, allInterfaces, additionalInterfaces, namingScope);
            var instance = new IgnoreAttributeInterfaceProxyInstanceContributor(targetType, GeneratorType, interfaces);
            AddMappingForISerializable(typeImplementerMapping, instance);
            try
            {
                AddMappingNoCheck(typeof(IProxyTargetAccessor), instance, typeImplementerMapping);
            }
            catch (ArgumentException)
            {
                HandleExplicitlyPassedProxyTargetAccessor(allInterfaces, additionalInterfaces);
            }
            contributors = new List<ITypeContributor> {implementer, instance};
            return typeImplementerMapping.Keys;
        }
    }
}
