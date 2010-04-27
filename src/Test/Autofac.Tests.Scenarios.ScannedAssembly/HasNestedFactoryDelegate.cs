using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Autofac.Tests.Scenarios.ScannedAssembly
{
    public class HasNestedFactoryDelegate
    {
        public delegate HasNestedFactoryDelegate Factory();
    }
}
