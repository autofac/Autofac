using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Autofac.Test.Scenarios.Dependencies.Circularity
{
    public class D : ID
    {
        public D(IA a, IC c)
        {
        }
    }
}
