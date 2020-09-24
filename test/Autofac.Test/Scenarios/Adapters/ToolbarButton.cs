// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Autofac.Test.Scenarios.Adapters
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
