// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Autofac.Test.Scenarios.ScannedAssembly
{
    /// <summary>
    /// A command class that directly implements the open
    /// generic interface type.
    /// </summary>
    public class SaveCommand : ICommand<SaveCommandData>
    {
        public void Execute(SaveCommandData data)
        {
        }
    }
}
