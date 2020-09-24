// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;

namespace Autofac.Test.Scenarios.ScannedAssembly
{
    public interface IHaveDeferredEnumerable
    {
        IEnumerable<IHaveDeferredEnumerable> Get();
    }
}
