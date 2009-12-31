using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autofac.Core;

namespace Autofac.Integration.Mef
{
    /// <summary>
    /// Support the <see cref="System.Lazy{T}"/> and <see cref="System.Lazy{T, TMetadata}"/>
    /// types automatically whenever type T is registered with the container.
    /// When a dependency of a lazy type is used, the instantiation of the underlying
    /// component will be delayed until the Value property is first accessed.
    /// </summary>
    /// <example>
    /// To enable the module, register it with the container.
    /// <code>
    /// var builder = new ContainerBuilder();
    /// builder.RegisterModule(new LazyDependencyModule());
    /// // Register other components and modules...
    /// var container = builder.Build();
    /// </code>
    /// The container will now resolve references of any lazy type:
    /// <code>
    /// var lazyFoo = container.Resolve&lt;Lazy&ltIFoo&gt;&gt;();
    /// </code>
    /// </example>
    public class LazyDependencyModule : Module
    {
        /// <summary>
        /// Registers functionaity from the module with the container.
        /// </summary>
        /// <param name="moduleBuilder">Module builder with which to register.</param>
        protected override void Load(ContainerBuilder moduleBuilder)
        {
            base.Load(moduleBuilder);

            moduleBuilder.RegisterSource(new LazyRegistrationSource());
            moduleBuilder.RegisterSource(new LazyWithMetadataRegistrationSource());
        }
    }
}
