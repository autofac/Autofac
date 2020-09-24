// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Autofac.Test.Scenarios.ScannedAssembly
{
    /// <summary>
    /// An abstract base class that implements the open generic
    /// interface type.
    /// </summary>
    public abstract class CommandBase<T> : ICommand<T>
    {
        public abstract void Execute(T data);
    }
}
