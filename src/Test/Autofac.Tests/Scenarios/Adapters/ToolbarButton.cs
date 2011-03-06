using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Autofac.Tests.Scenarios.Adapters
{
    class ToolbarButton : IToolbarButton
    {
        readonly Command _command;
        readonly string _name;

#if !(NET35 || WINDOWS_PHONE)
        public ToolbarButton(Command command, string name = "")
#else
        public ToolbarButton(Command command, string name)
#endif
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
