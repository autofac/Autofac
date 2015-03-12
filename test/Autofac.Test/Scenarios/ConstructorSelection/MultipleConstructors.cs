using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Autofac.Test.Scenarios.ConstructorSelection
{
    public class MultipleConstructors
    {
        public MultipleConstructors(object o, string s)
        {
        }

        public MultipleConstructors(object o)
        {
        }
    }
}
