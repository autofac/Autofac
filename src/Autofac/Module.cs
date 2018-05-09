// This software is part of the Autofac IoC container
// Copyright © 2011 Autofac Contributors
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

using System;
using System.Globalization;
using System.Reflection;
using Autofac.Core;

namespace Autofac
{
    /// <summary>
    /// Base class for user-defined modules. Modules can add a set of related components
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
    /// Using the module...
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
            if (componentRegistry == null) throw new ArgumentNullException(nameof(componentRegistry));

            var moduleBuilder = new ContainerBuilder(componentRegistry.Properties);

            Load(moduleBuilder);
            moduleBuilder.UpdateRegistry(componentRegistry);
            AttachToRegistrations(componentRegistry);
            AttachToSources(componentRegistry);
        }

        /// <summary>
        /// Override to add registrations to the container.
        /// </summary>
        /// <remarks>
        /// Note that the ContainerBuilder parameter is unique to this module.
        /// </remarks>
        /// <param name="builder">The builder through which components can be
        /// registered.</param>
        protected virtual void Load(ContainerBuilder builder)
        {
        }

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

        /// <summary>
        /// Override to perform module-specific processing on a registration source.
        /// </summary>
        /// <remarks>This method will be called for all existing <i>and future</i> sources
        /// - ordering is not important.</remarks>
        /// <param name="componentRegistry">The component registry into which the source was added.</param>
        /// <param name="registrationSource">The registration source.</param>
        protected virtual void AttachToRegistrationSource(
            IComponentRegistry componentRegistry,
            IRegistrationSource registrationSource)
        {
        }

        private void AttachToRegistrations(IComponentRegistry componentRegistry)
        {
            if (componentRegistry == null) throw new ArgumentNullException(nameof(componentRegistry));
            foreach (var registration in componentRegistry.Registrations)
                AttachToComponentRegistration(componentRegistry, registration);
            componentRegistry.Registered +=
                (sender, e) => AttachToComponentRegistration(e.ComponentRegistry, e.ComponentRegistration);
        }

        private void AttachToSources(IComponentRegistry componentRegistry)
        {
            if (componentRegistry == null) throw new ArgumentNullException(nameof(componentRegistry));
            foreach (var source in componentRegistry.Sources)
                AttachToRegistrationSource(componentRegistry, source);
            componentRegistry.RegistrationSourceAdded +=
                (sender, e) => AttachToRegistrationSource(e.ComponentRegistry, e.RegistrationSource);
        }

        /// <summary>
        /// Gets the assembly in which the concrete module type is located. To avoid bugs whereby deriving from a module will
        /// change the target assembly, this property can only be used by modules that inherit directly from
        /// <see cref="Module"/>.
        /// </summary>
        protected virtual Assembly ThisAssembly
        {
            get
            {
                var thisType = GetType();
                var baseType = thisType.GetTypeInfo().BaseType;
                if (baseType != typeof(Module))
                    throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, ModuleResources.ThisAssemblyUnavailable, thisType, baseType));

                return thisType.GetTypeInfo().Assembly;
            }
        }
    }
}
