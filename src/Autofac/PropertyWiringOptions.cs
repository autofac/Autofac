// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Autofac
{
    /// <summary>
    /// Options that can be applied when autowiring properties on a component. (Multiple options can
    /// be specified using bitwise 'or' - e.g. AllowCircularDependencies | PreserveSetValues.
    /// </summary>
    [Flags]
    public enum PropertyWiringOptions
    {
        /// <summary>
        /// Default behavior. Circular dependencies are not allowed; existing non-default
        /// property values are overwritten.
        /// </summary>
        None = 0,

        /// <summary>
        /// Allows property-property and property-constructor circular dependency wiring.
        /// This flag moves property wiring from the Activating to the Activated event.
        /// </summary>
        AllowCircularDependencies = 1,

        /// <summary>
        /// If specified, properties that already have a non-default value will be left
        /// unchanged in the wiring operation.
        /// </summary>
        PreserveSetValues = 2,
    }
}
