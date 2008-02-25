// This software is part of the Autofac IoC container
// Copyright (c) 2007 Nicholas Blumhardt
// nicholas.blumhardt@gmail.com
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
using System.Collections.Generic;
using System.Text;

namespace Autofac.Registrars
{
    /// <summary>
    /// Provides an interface through which a component registration can
    /// be incrementally built. Calling any of the methods on this interface
    /// after 'Build' has been called on the parent ContainerBuilder will have
    /// no effect.
    /// </summary>
    /// <typeparam name="TSyntax">Self-type.</typeparam>
	public interface IRegistrar<TSyntax>
        where TSyntax : IRegistrar<TSyntax>
	{
        /// <summary>
        /// Change the service associated with the registration.
        /// </summary>
        /// <typeparam name="TService">The service that the registration will expose.</typeparam>
        /// <returns>
        /// A registrar allowing registration to continue.
        /// </returns>
        TSyntax As<TService>();

        /// <summary>
        /// Change the services associated with the registration.
        /// </summary>
        /// <typeparam name="TService1">The first service that the registration will expose.</typeparam>
        /// <typeparam name="TService2">The second service that the registration will expose.</typeparam>
        /// <returns>A registrar allowing registration to continue.</returns>
        TSyntax As<TService1, TService2>();

        /// <summary>
        /// Change the services associated with the registration.
        /// </summary>
        /// <typeparam name="TService1">The first service that the registration will expose.</typeparam>
        /// <typeparam name="TService2">The second service that the registration will expose.</typeparam>
        /// <typeparam name="TService3">The third service that the registration will expose.</typeparam>
        /// <returns>A registrar allowing registration to continue.</returns>
        TSyntax As<TService1, TService2, TService3>();

        /// <summary>
        /// Change the service associated with the registration.
        /// </summary>
        /// <param name="services">The services that the registration will expose.</param>
        /// <returns>A registrar allowing registration to continue.</returns>
        TSyntax As(params Type[] services);

        /// <summary>
		/// Change the ownership model associated with the registration.
		/// This determines when the instances are disposed and by whom.
		/// </summary>
		/// <param name="ownership">The ownership model to use.</param>
        /// <returns>A registrar allowing registration to continue.</returns>
        TSyntax WithOwnership(InstanceOwnership ownership);
        
        /// <summary>
        /// The instance(s) will not be disposed when the container is disposed.
        /// </summary>
        TSyntax ExternallyOwned();
        
        /// <summary>
        /// The instance(s) will be disposed with the container.
        /// </summary>
        TSyntax OwnedByContainer();

		/// <summary>
		/// Change the scope associated with the registration.
		/// This determines how instances are tracked and shared.
		/// </summary>
		/// <param name="scope">The scope model to use.</param>
        /// <returns>A registrar allowing registration to continue.</returns>
        TSyntax WithScope(InstanceScope scope);
        
        /// <summary>
        /// An instance will be created every time one is requested.
        /// </summary>
        TSyntax FactoryScoped();
        
        /// <summary>
        /// An instance will be created once per container.
        /// </summary>
        /// <seealso cref="IContainer.CreateInnerContainer" />
        TSyntax ContainerScoped();
        
        /// <summary>
        /// Only one instance will ever be created.
        /// </summary>
        TSyntax SingletonScoped();

        /// <summary>
        /// Calls the provided handler when the registration is made on the
        /// container being built.
        /// </summary>
        /// <param name="handler">The handler.</param>
        /// <returns>A registrar allowing registration to continue.</returns>
        TSyntax OnRegistered(EventHandler<RegisteredEventArgs> handler);

        /// <summary>
        /// Call the provided handler when activating an instance. OnActivating
        /// is the place to do work to get an instance into a usable state.
        /// </summary>
        /// <param name="handler">The handler.</param>
        /// <returns>A registrar allowing registration to continue.</returns>
        TSyntax OnActivating(EventHandler<ActivatingEventArgs> handler);

        /// <summary>
        /// Call the provided handler when an instance is activated. This is the
        /// place to perform operations on the activated instance that will use that
        /// instance's behaviour/features.
        /// </summary>
        /// <param name="handler">The handler.</param>
        /// <returns>A registrar allowing registration to continue.</returns>
        TSyntax OnActivated(EventHandler<ActivatedEventArgs> handler);
	}
}
