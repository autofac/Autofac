// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Autofac.Specification.Test.Registration.Adapters
{
    public class ToolbarButton : IToolbarButton
    {
        public ToolbarButton(Command command, string name = "")
        {
            Command = command;
            Name = name;
        }

        public string Name { get; }

        public Command Command { get; }
    }
}
