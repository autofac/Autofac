using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Autofac.Extras.Tests.AggregateService
{
    public interface IOpenGenericAggregate
    {
        IOpenGeneric<T> GetOpenGeneric<T>();

        IOpenGeneric<string> GetResolvedGeneric();
    }
}
