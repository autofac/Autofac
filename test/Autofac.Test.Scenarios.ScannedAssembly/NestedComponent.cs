using System;
using System.Linq;

namespace Autofac.Test.Scenarios.ScannedAssembly
{
    public class NestedComponent
    {
        private class PrivateComponent
        {
        }

        internal class InternalComponent
        {
        }

        public class PublicComponent
        {
        }
    }
}
