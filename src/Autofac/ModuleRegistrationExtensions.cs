// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using Autofac.Core;
using Autofac.Core.Registration;

namespace Autofac
{
    /// <summary>
    /// Extension methods for registering <see cref="IModule"/> instances with a container.
    /// </summary>
    public static class ModuleRegistrationExtensions
    {
        /// <summary>
        /// Registers modules found in an assembly.
        /// </summary>
        /// <param name="builder">The builder to register the modules with.</param>
        /// <param name="assemblies">The assemblies from which to register modules.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="builder"/> is <see langword="null"/>.
        /// </exception>
        /// <returns>
        /// The <see cref="IModuleRegistrar"/> to allow
        /// additional chained module registrations.
        /// </returns>
        public static IModuleRegistrar RegisterAssemblyModules(this ContainerBuilder builder, params Assembly[] assemblies)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            var registrar = new ModuleRegistrar(builder);
            return registrar.RegisterAssemblyModules<IModule>(assemblies);
        }

        /// <summary>
        /// Registers modules found in an assembly.
        /// </summary>
        /// <param name="registrar">The module registrar that will make the registrations into the container.</param>
        /// <param name="assemblies">The assemblies from which to register modules.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="registrar"/> is <see langword="null"/>.
        /// </exception>
        /// <returns>
        /// The <see cref="IModuleRegistrar"/> to allow
        /// additional chained module registrations.
        /// </returns>
        public static IModuleRegistrar RegisterAssemblyModules(this IModuleRegistrar registrar, params Assembly[] assemblies)
        {
            if (registrar == null)
            {
                throw new ArgumentNullException(nameof(registrar));
            }

            return registrar.RegisterAssemblyModules<IModule>(assemblies);
        }

        /// <summary>
        /// Registers modules found in an assembly.
        /// </summary>
        /// <param name="builder">The builder to register the modules with.</param>
        /// <param name="assemblies">The assemblies from which to register modules.</param>
        /// <typeparam name="TModule">The type of the module to add.</typeparam>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="builder"/> is <see langword="null"/>.
        /// </exception>
        /// <returns>
        /// The <see cref="IModuleRegistrar"/> to allow
        /// additional chained module registrations.
        /// </returns>
        public static IModuleRegistrar RegisterAssemblyModules<TModule>(this ContainerBuilder builder, params Assembly[] assemblies)
            where TModule : IModule
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            var registrar = new ModuleRegistrar(builder);
            return registrar.RegisterAssemblyModules(typeof(TModule), assemblies);
        }

        /// <summary>
        /// Registers modules found in an assembly.
        /// </summary>
        /// <param name="registrar">The module registrar that will make the registrations into the container.</param>
        /// <param name="assemblies">The assemblies from which to register modules.</param>
        /// <typeparam name="TModule">The type of the module to add.</typeparam>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="registrar"/> is <see langword="null"/>.
        /// </exception>
        /// <returns>
        /// The <see cref="IModuleRegistrar"/> to allow
        /// additional chained module registrations.
        /// </returns>
        public static IModuleRegistrar RegisterAssemblyModules<TModule>(this IModuleRegistrar registrar, params Assembly[] assemblies)
            where TModule : IModule
        {
            if (registrar == null)
            {
                throw new ArgumentNullException(nameof(registrar));
            }

            return registrar.RegisterAssemblyModules(typeof(TModule), assemblies);
        }

        /// <summary>
        /// Registers modules found in an assembly.
        /// </summary>
        /// <param name="builder">The builder to register the modules with.</param>
        /// <param name="moduleType">The <see cref="Type"/> of the module to add.</param>
        /// <param name="assemblies">The assemblies from which to register modules.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="builder"/> or <paramref name="moduleType"/> is <see langword="null"/>.
        /// </exception>
        /// <returns>
        /// The <see cref="IModuleRegistrar"/> to allow
        /// additional chained module registrations.
        /// </returns>
        public static IModuleRegistrar RegisterAssemblyModules(this ContainerBuilder builder, Type moduleType, params Assembly[] assemblies)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (moduleType == null)
            {
                throw new ArgumentNullException(nameof(moduleType));
            }

            var registrar = new ModuleRegistrar(builder);
            return registrar.RegisterAssemblyModules(moduleType, assemblies);
        }

        /// <summary>
        /// Registers modules found in an assembly.
        /// </summary>
        /// <param name="registrar">The module registrar that will make the registrations into the container.</param>
        /// <param name="moduleType">The <see cref="Type"/> of the module to add.</param>
        /// <param name="assemblies">The assemblies from which to register modules.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="registrar"/> or <paramref name="moduleType"/> is <see langword="null"/>.
        /// </exception>
        /// <returns>
        /// The <see cref="IModuleRegistrar"/> to allow
        /// additional chained module registrations.
        /// </returns>
        public static IModuleRegistrar RegisterAssemblyModules(this IModuleRegistrar registrar, Type moduleType, params Assembly[] assemblies)
        {
            if (registrar == null)
            {
                throw new ArgumentNullException(nameof(registrar));
            }

            if (moduleType == null)
            {
                throw new ArgumentNullException(nameof(moduleType));
            }

            var moduleFinder = new ContainerBuilder();

            moduleFinder.RegisterAssemblyTypes(assemblies)
                .Where(t => moduleType.IsAssignableFrom(t))
                .As<IModule>();

            using (var moduleContainer = moduleFinder.Build())
            {
                foreach (var module in moduleContainer.Resolve<IEnumerable<IModule>>())
                {
                    registrar.RegisterModule(module);
                }
            }

            return registrar;
        }

        /// <summary>
        /// Add a module to the container.
        /// </summary>
        /// <param name="builder">The builder to register the module with.</param>
        /// <typeparam name="TModule">The module to add.</typeparam>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="builder"/> is <see langword="null"/>.
        /// </exception>
        /// <returns>
        /// The <see cref="IModuleRegistrar"/> to allow
        /// additional chained module registrations.
        /// </returns>
        public static IModuleRegistrar RegisterModule<TModule>(this ContainerBuilder builder)
            where TModule : IModule, new()
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            var registrar = new ModuleRegistrar(builder);
            return registrar.RegisterModule<TModule>();
        }

        /// <summary>
        /// Add a module to the container.
        /// </summary>
        /// <param name="registrar">The module registrar that will make the registration into the container.</param>
        /// <typeparam name="TModule">The module to add.</typeparam>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="registrar"/> is <see langword="null"/>.
        /// </exception>
        /// <returns>
        /// The <see cref="IModuleRegistrar"/> to allow
        /// additional chained module registrations.
        /// </returns>
        public static IModuleRegistrar RegisterModule<TModule>(this IModuleRegistrar registrar)
            where TModule : IModule, new()
        {
            if (registrar == null)
            {
                throw new ArgumentNullException(nameof(registrar));
            }

            return registrar.RegisterModule(new TModule());
        }

        /// <summary>
        /// Add a module to the container.
        /// </summary>
        /// <param name="builder">The builder to register the module with.</param>
        /// <param name="module">The module to add.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="builder"/> or <paramref name="module"/> is <see langword="null"/>.
        /// </exception>
        /// <returns>
        /// The <see cref="IModuleRegistrar"/> to allow
        /// additional chained module registrations.
        /// </returns>
        public static IModuleRegistrar RegisterModule(this ContainerBuilder builder, IModule module)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (module == null)
            {
                throw new ArgumentNullException(nameof(module));
            }

            var registrar = new ModuleRegistrar(builder);
            return registrar.RegisterModule(module);
        }
    }
}
