using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Autofac.Tests.Scenarios.Parameterisation
{
    public class Parameterised
    {
        public string A { get; private set; }
        public int B { get; private set; }

        public Parameterised(string a, int b)
        {
            A = a;
            B = b;
        }
    }
}
