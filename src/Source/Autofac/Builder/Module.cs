// This software is part of the Autofac IoC container
// Copyright (c) 2007 - 2008 Autofac Contributors
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

namespace Autofac.Builder
{
    /// <summary>
    /// Base class for user-defined modules. Implements IModule on top
    /// of ContainerBuilder.
    /// </summary>
    public abstract class Module : IModule
    {
        /// <summary>
        /// Apply the module to the component registry.
        /// </summary>
        /// <param name="componentRegistry">Component registry to apply configuration to.</param>
        public void Configure(IComponentRegistry componentRegistry)
        {
            Enforce.ArgumentNotNull(componentRegistry, "componentRegistry");
            var builder = new ContainerBuilder();
            Load(builder);
            builder.Build(componentRegistry);
            AttachToRegistrations(componentRegistry);
        }

        /// <summary>
        /// Override to add registrations to the container.
        /// Note that the ContainerBuilder parameter is not the same one
	    /// that the module is being registered by (i.e. it can have its own defaults.)
	    /// </summary>
		/// <param name="moduleBuilder">The builder.</param>
        protected virtual void Load(ContainerBuilder moduleBuilder) { }

        /// <summary>
        /// Attach the module to a registration either already existing in
        /// or being registered in the component registry.
        /// </summary>
        /// <param name="componentRegistry">The component registry.</param>
        /// <param name="registration">The registration.</param>
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
