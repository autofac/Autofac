// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Autofac.Benchmarks;

/// <summary>
/// Chooses how a generated factory benchmark obtains its <c>Func</c> factories, because the two
/// sources use different generators.
/// </summary>
public enum GeneratedFactorySource
{
    /// <summary>
    /// The implicit <c>Func</c> relationship, which resolves through <c>ResolveComponent</c>.
    /// </summary>
    Implicit,

    /// <summary>
    /// <c>RegisterGeneratedFactory</c>, which resolves through <c>ResolveService</c>.
    /// </summary>
    Registered,
}
