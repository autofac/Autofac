using System;

namespace Autofac.Specification.Test.Features.PropertyInjection
{
    public class HasProtectedSetterWithDefaultValue
    {
        public string Val { get; protected set; } = "Default";
    }
}
