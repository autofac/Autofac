using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Autofac.Test.Scenarios.ScannedAssembly
{
    public class HasNestedFactoryDelegate
    {
        public delegate HasNestedFactoryDelegate Factory();
    }
}
