using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Autofac.Tests.Scenarios.Dependencies
{
    class Dependent
    {
        public object TheObject;
        public string TheString;

        public Dependent(object o, string s)
        {
            TheObject = o;
            TheString = s;
        }
    }
}
