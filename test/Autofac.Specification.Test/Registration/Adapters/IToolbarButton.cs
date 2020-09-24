// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Autofac.Specification.Test.Registration.Adapters
{
    public interface IToolbarButton
    {
        string Name { get; }

        Command Command { get; }
    }
}