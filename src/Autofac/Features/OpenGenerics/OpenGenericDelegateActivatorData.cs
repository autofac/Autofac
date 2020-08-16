using System;
using System.Collections.Generic;
using System.Text;
using Autofac.Builder;
using Autofac.Core;

namespace Autofac.Features.OpenGenerics
{
    public class OpenGenericDelegateActivatorData
    {
        public OpenGenericDelegateActivatorData(Func<IComponentContext, IEnumerable<Parameter>, Type[], object> factory)
        {
            Factory = factory;
        }

        public Func<IComponentContext, IEnumerable<Parameter>, Type[], object> Factory { get; }
    }
}
