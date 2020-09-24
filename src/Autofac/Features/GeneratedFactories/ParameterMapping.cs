// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Autofac.Features.GeneratedFactories
{
    /// <summary>
    /// Determines how the parameters of the delegate type are passed on
    /// to the generated Resolve() call as Parameter objects.
    /// </summary>
    public enum ParameterMapping
    {
        /// <summary>
        /// Chooses parameter mapping based on the factory type.
        /// For Func-based factories this is equivalent to ByType, for all
        /// others ByName will be used.
        /// </summary>
        Adaptive,

        /// <summary>
        /// Pass the parameters supplied to the delegate through to the
        /// underlying registration as NamedParameters based on the parameter
        /// names in the delegate type's formal argument list.
        /// </summary>
        ByName,

        /// <summary>
        /// Pass the parameters supplied to the delegate through to the
        /// underlying registration as TypedParameters based on the parameter
        /// types in the delegate type's formal argument list.
        /// </summary>
        ByType,

        /// <summary>
        /// Pass the parameters supplied to the delegate through to the
        /// underlying registration as PositionalParameters based on the parameter
        /// indices in the delegate type's formal argument list.
        /// </summary>
        ByPosition,
    }
}