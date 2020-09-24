// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Autofac.Test.Scenarios.ScannedAssembly
{
    /// <summary>
    /// A command class that implements the open generic interface
    /// type by inheriting from the abstract base class.
    /// </summary>
    public class DeleteCommand : CommandBase<DeleteCommandData>
    {
        public override void Execute(DeleteCommandData data)
        {
        }
    }
}
