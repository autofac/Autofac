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
        /// Apply the module to the container.
        /// </summary>
        /// <param name="container">Container to apply configuration to.</param>
        public virtual void Configure(IContainer container)
        {
            Enforce.ArgumentNotNull(container, "container");
            var builder = new ContainerBuilder();
            Load(builder);
            builder.Build(container);
            AttachToRegistrations(container);
        }

        /// <summary>
        /// Override to add registrations to the container.
        /// Note that the ContainerBuilder parameter is not the same one
	/// that the module is being registered by (i.e. it can have its own defaults.)
	/// </summary>
        /// <param name="builder">The builder.</param>
        protected virtual void Load(ContainerBuilder moduleBuilder) { }

        /// <summary>
        /// Attach the module to a registration either already existing in
        /// or being registered in the container.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <param name="registration">The registration.</param>
        protected virtual void AttachToComponentRegistration(
            IContainer container,
            IComponentRegistration registration)
        {
        }

        void AttachToRegistrations(IContainer container)
        {
            Enforce.ArgumentNotNull(container, "container");
            foreach (IComponentRegistration registration in container.ComponentRegistrations)
                AttachToComponentRegistration(container, registration);
            container.ComponentRegistered +=
                (sender, e) => AttachToComponentRegistration(e.Container, e.ComponentRegistration);
        }
    }
}
