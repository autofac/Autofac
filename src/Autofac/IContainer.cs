// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics;

namespace Autofac
{
    /// <summary>
    /// Creates, wires dependencies and manages lifetime for a set of components.
    /// Most instances of <see cref="IContainer"/> are created
    /// by a <see cref="ContainerBuilder"/>.
    /// </summary>
    /// <example>
    /// <code>
    /// // See ContainerBuilder for the definition of the builder variable
    /// using (var container = builder.Build())
    /// {
    ///     var program = container.Resolve&lt;Program&gt;();
    ///     program.Run();
    /// }
    /// </code>
    /// </example>
    /// <remarks>
    /// Most <see cref="IContainer"/> functionality is provided by extension methods
    /// on the inherited <see cref="IComponentContext"/> interface.
    /// </remarks>
    /// <seealso cref="ILifetimeScope"/>
    /// <seealso cref="IComponentContext"/>
    /// <seealso cref="ResolutionExtensions"/>
    /// <seealso cref="ContainerBuilder"/>
    public interface IContainer : ILifetimeScope
    {
        /// <summary>
        /// Gets the <see cref="System.Diagnostics.DiagnosticListener"/> to which
        /// trace events should be written.
        /// </summary>
        DiagnosticListener DiagnosticSource { get; }
    }
}
