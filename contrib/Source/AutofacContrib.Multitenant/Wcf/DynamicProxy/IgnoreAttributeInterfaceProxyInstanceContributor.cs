using System;
using System.Security;
using Castle.DynamicProxy;
using Castle.DynamicProxy.Contributors;
using Castle.DynamicProxy.Generators.Emitters;
using Castle.DynamicProxy.Generators.Emitters.SimpleAST;

namespace AutofacContrib.Multitenant.Wcf.DynamicProxy
{
	/// <summary>
	/// Code generator that ignores type-level non-inherited attributes.
	/// </summary>
	/// <remarks>
	/// <para>
	/// The default behavior of <see cref="Castle.DynamicProxy.Contributors.InterfaceProxyInstanceContributor"/>
	/// is to generate a type definition that copies over all of the non-inherited
	/// attributes from the target interface. This includes the <see cref="System.ServiceModel.ServiceContractAttribute"/>
	/// that would be copied over from service interfaces. Unfortunately, WCF
	/// doesn't allow a type marked with <see cref="System.ServiceModel.ServiceContractAttribute"/>
	/// to also implement an interface marked with <see cref="System.ServiceModel.ServiceContractAttribute"/>.
	/// This code generator does everything that <see cref="Castle.DynamicProxy.Contributors.InterfaceProxyInstanceContributor"/>
	/// does except it doesn't copy over type-level non-inherited attributes.
	/// </para>
	/// </remarks>
    [SecuritySafeCritical]
    public class IgnoreAttributeInterfaceProxyInstanceContributor : InterfaceProxyInstanceContributor
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="IgnoreAttributeInterfaceProxyInstanceContributor"/> class.
		/// </summary>
		/// <param name="targetType">Type of the target to proxy.</param>
		/// <param name="proxyGeneratorId">The proxy generator ID.</param>
		/// <param name="interfaces">The additional interfaces to implement.</param>
		public IgnoreAttributeInterfaceProxyInstanceContributor(Type targetType, string proxyGeneratorId, Type[] interfaces)
			: base(targetType, proxyGeneratorId, interfaces)
		{
		}

		/// <summary>
		/// Generates the class defined by the provided class emitter.
		/// </summary>
		/// <param name="class">
		/// The <see cref="Castle.DynamicProxy.Generators.Emitters.ClassEmitter"/>
		/// being used to build the target type.
		/// </param>
		/// <param name="options">The options to use during proxy generation.</param>
		/// <exception cref="System.ArgumentNullException">
		/// Thrown if <paramref name="class" /> is <see langword="null" />.
		/// </exception>
		/// <remarks>
		/// <para>
		/// This overridden version of the method does everything that the base
		/// <see cref="Castle.DynamicProxy.Contributors.ProxyInstanceContributor.Generate"/>
		/// method does but it skips the part where it checks for non-inherited
		/// attributes and copies them over from the proxy target.
		/// </para>
		/// </remarks>
        [SecuritySafeCritical]
        public override void Generate(ClassEmitter @class, ProxyGenerationOptions options)
		{
			if (@class == null)
			{
				throw new ArgumentNullException("class");
			}
			FieldReference field = @class.GetField("__interceptors");
			this.ImplementGetObjectData(@class);
			this.ImplementProxyTargetAccessor(@class, field);
		}
	}
}
