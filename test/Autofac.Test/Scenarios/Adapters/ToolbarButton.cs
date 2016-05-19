using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Autofac.Test.Scenarios.Adapters
{
    public class ToolbarButton : IToolbarButton
    {
        private readonly Command _command;
        private readonly string _name;

        public ToolbarButton(Command command, string name = "")
        {
            _command = command;
            _name = name;
        }

        public string Name
        {
            get { return _name; }
        }

        public Command Command
        {
            get { return _command; }
        }
    }
}
