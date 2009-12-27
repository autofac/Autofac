// This software is part of the Autofac IoC container
// Copyright (c) 2007 - 2009 Autofac Contributors
// http://autofac.org
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.

using Autofac.Core;
using Autofac.Util;

namespace Autofac
{
    /// <summary>
    /// Base class for user-defined modules. Modules can add a set of releated components
    /// to a container (<see cref="Module.Load"/>) or attach cross-cutting functionality
    /// to other components (<see cref="Module.AttachToComponentRegistration"/>.
    /// Modules are given special support in the XML configuration feature - see
    /// http://code.google.com/p/autofac/wiki/StructuringWithModules.
    /// </summary>
    /// <remarks>Provides a user-friendly way to implement <see cref="Autofac.Core.IModule"/>
    /// via <see cref="ContainerBuilder"/>.</remarks>
    /// <example>
    /// Defining a module:
    /// <code>
    /// public class DataAccessModule : Module
    /// {
    ///     public string ConnectionString { get; set; }
    ///     
    ///     public override void Load(ContainerBuilder moduleBuilder)
    ///     {
    ///         moduleBuilder.RegisterGeneric(typeof(MyRepository&lt;&gt;))
    ///             .As(typeof(IRepository&lt;&gt;))
    ///             .InstancePerMatchingLifetimeScope(WebLifetime.Request);
    ///         
    ///         moduleBuilder.Register(c =&gt; new MyDbConnection(ConnectionString))
    ///             .As&lt;IDbConnection&gt;()
    ///             .InstancePerMatchingLifetimeScope(WebLifetime.Request);
    ///     }
    /// }
    /// </code>
    /// Using the module:
    /// <code>
    /// var builder = new ContainerBuilder();
    /// builder.RegisterModule(new DataAccessModule { ConnectionString = "..." });
    /// var container = builder.Build();
    /// var customers = container.Resolve&lt;IRepository&lt;Customer&gt;&gt;();
    /// </code>
    /// </example>
    public abstract class Module : IModule
    {
        /// <summary>
        /// Apply the module to the component registry.
        /// </summary>
        /// <param name="componentRegistry">Component registry to apply configuration to.</param>
        public void Configure(IComponentRegistry componentRegistry)
        {
            Enforce.ArgumentNotNull(componentRegistry, "componentRegistry");
            componentRegistry.Configure(builder =>
            {
                Load(builder);
            });
            AttachToRegistrations(componentRegistry);
        }

        /// <summary>
        /// Override to add registrations to the container.
        /// </summary>
        /// <remarks>
        /// Note that the ContainerBuilder parameter is not the same one
	    /// that the module is being registered by (i.e. it can have its own defaults.)
        /// </remarks>
		/// <param name="moduleBuilder">The builder through which components can be
        /// registered.</param>
        protected virtual void Load(ContainerBuilder moduleBuilder) { }

        /// <summary>
        /// Override to attach module-specific functionality to a
        /// component registration.
        /// </summary>
        /// <remarks>This method will be called for all existing <i>and future</i> component
        /// registrations - ordering is not important.</remarks>
        /// <param name="componentRegistry">The component registry.</param>
        /// <param name="registration">The registration to attach functionality to.</param>
        protected virtual void AttachToComponentRegistration(
            IComponentRegistry componentRegistry,
            IComponentRegistration registration)
        {
        }

        void AttachToRegistrations(IComponentRegistry componentRegistry)
        {
            Enforce.ArgumentNotNull(componentRegistry, "componentRegistry");
            foreach (IComponentRegistration registration in componentRegistry.Registrations)
                AttachToComponentRegistration(componentRegistry, registration);
            componentRegistry.Registered +=
                (sender, e) => AttachToComponentRegistration(e.ComponentRegistry, e.ComponentRegistration);
        }
    }
}
