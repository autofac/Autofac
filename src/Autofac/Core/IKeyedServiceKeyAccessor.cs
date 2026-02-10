// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Autofac.Core;

/// <summary>
/// Provides access to the keyed service key associated with the current resolve request.
/// </summary>
internal interface IKeyedServiceKeyAccessor
{
    /// <summary>
    /// Attempts to retrieve the keyed service key associated with the resolve operation.
    /// </summary>
    /// <param name="key">When this method returns, contains the keyed service key.</param>
    /// <returns><see langword="true"/> if the key is available; otherwise, <see langword="false"/>.</returns>
    bool TryGetServiceKey([NotNullWhen(returnValue: true)] out object? key);
}
