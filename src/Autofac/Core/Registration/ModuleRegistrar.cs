// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Autofac.Core.Registration;

/// <summary>
/// Basic implementation of the <see cref="IModuleRegistrar"/>
/// interface allowing registration of modules into a <see cref="ContainerBuilder"/>
/// in a fluent format.
/// </summary>
internal class ModuleRegistrar : IModuleRegistrar
{
    // Holds the chain of modules to register.
    private Action<IComponentRegistryBuilder>? _moduleConfigureChain;

    /// <summary>
    /// Initializes a new instance of the <see cref="ModuleRegistrar"/> class.
    /// </summary>
    /// <param name="builder">
    /// The <see cref="ContainerBuilder"/> into which registrations will be made.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="builder" /> is <see langword="null" />.
    /// </exception>
    public ModuleRegistrar(ContainerBuilder builder)
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        // We create a normal register callback that will in turn invoke our module config chain.
        // This allows predicates to wrap the entire set of modules, rather than calling
        // RegisterCallback per-module.
        var callback = builder.RegisterCallback(reg => _moduleConfigureChain?.Invoke(reg));

        RegistrarData = new ModuleRegistrarData(callback);
    }

    /// <inheritdoc />
    public ModuleRegistrarData RegistrarData { get; }

    /// <summary>
    /// Add a module to the container.
    /// </summary>
    /// <param name="module">The module to add.</param>
    /// <returns>
    /// The <see cref="IModuleRegistrar"/> to allow
    /// additional chained module registrations.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="module" /> is <see langword="null" />.
    /// </exception>
    public IModuleRegistrar RegisterModule(IModule module)
    {
        if (module == null)
        {
            throw new ArgumentNullException(nameof(module));
        }

        if (_moduleConfigureChain is null)
        {
            _moduleConfigureChain = module.Configure;
        }
        else
        {
            // Override the original callback to chain the module configuration.
            var original = _moduleConfigureChain;
            _moduleConfigureChain = reg =>
            {
                // Call the original.
                original(reg);

                // Call the new module register.
                module.Configure(reg);
            };
        }

        return this;
    }
}
