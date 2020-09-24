// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;

namespace Autofac.Builder
{
    /// <summary>
    /// Parameterises the construction of a container by a <see cref="ContainerBuilder"/>.
    /// </summary>
    [Flags]
    public enum ContainerBuildOptions
    {
        /// <summary>
        /// No options - the default behavior for container building.
        /// </summary>
        None = 0,

        /// <summary>
        /// Prevents inclusion of standard modules like support for
        /// relationship types including <see cref="IEnumerable{T}"/> etc.
        /// </summary>
        ExcludeDefaultModules = 2,

        /// <summary>
        /// Does not call <see cref="IStartable.Start"/> on components implementing
        /// this interface (useful for module testing.)
        /// </summary>
        IgnoreStartableComponents = 4,
    }
}
