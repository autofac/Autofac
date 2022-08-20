// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Autofac.Util.Cache;

public interface IReflectionCacheStore
{
    bool UsedAtRegistrationOnly { get; }

    void Clear();

    void Clear(ReflectionCacheShouldClearPredicate predicate);
}
