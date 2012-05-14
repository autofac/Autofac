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

using System;
using System.Linq;
using Autofac;
using Autofac.Builder;
using Autofac.Core;

namespace AutofacContrib.Startable
{
    /// <summary>
    /// Calls a method ('Start') on a particular interface every time a component exposing
    /// that service is activated. Optionally can create and 'start' an instance of all
    /// such components when the container is 'Started' through the IStarter interface.
    /// </summary>
    /// <typeparam name="TService"></typeparam>
    public class StartableModule<TService> : Module
    {
        Service _myService = new TypedService(typeof(TService));
        Action<TService> _starter;

        /// <summary>
        /// Initializes a new instance of the <see cref="StartableModule&lt;TService&gt;"/> class.
        /// </summary>
        /// <param name="starter">The starter.</param>
        public StartableModule(Action<TService> starter)
        {
            if (starter == null)
                throw new ArgumentNullException("starter");

            _starter = starter;
        }

        /// <summary>
        /// Apply the module to the container.
        /// </summary>
        /// <param name="moduleBuilder">Builder to apply configuration to.</param>
        protected override void Load(ContainerBuilder moduleBuilder)
        {
            if (moduleBuilder == null)
                throw new ArgumentNullException("moduleBuilder");

            base.Load(moduleBuilder);

            moduleBuilder.RegisterType<Starter>()
                .As<IStarter>()
                .InstancePerLifetimeScope()
                .PreserveExistingDefaults();
        }

        /// <summary>
        /// Attach the module to a registration either already existing in
        /// or being registered in the container.
        /// </summary>
        /// <param name="registry">The component registry.</param>
        /// <param name="registration">The registration.</param>
        protected override void AttachToComponentRegistration(IComponentRegistry registry, IComponentRegistration registration)
        {
            if (registry == null)
                throw new ArgumentNullException("registry");

            if (registration == null)
                throw new ArgumentNullException("registration");

            if (registration.Services.Contains(_myService))
            {
                registration.Metadata[Starter.IsStartablePropertyName] = true;
                registration.Activated += OnComponentActivated;
            }
        }

        void OnComponentActivated(object sender, ActivatedEventArgs<object> e)
        {
            if (e == null)
                throw new ArgumentNullException("e");

            _starter.Invoke((TService)e.Instance);
        }
    }
}
