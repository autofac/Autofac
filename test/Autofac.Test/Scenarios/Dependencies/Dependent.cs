using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Autofac.Test.Scenarios.Dependencies
{
    public class Dependent
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
