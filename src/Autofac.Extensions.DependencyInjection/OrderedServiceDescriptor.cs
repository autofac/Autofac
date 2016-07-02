using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Autofac.Extensions.DependencyInjection
{
    internal class OrderedServiceDescriptor
    {
        public int Index { get; set; }

        public object ImplementationInstance { get; set; }

        public Type ImplementationType { get; set; }
    }
}
