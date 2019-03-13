using System;

namespace Autofac.Specification.Test.Features.PropertyInjection
{
    [AttributeUsage(AttributeTargets.Property, Inherited = true)]
    public class InjectAttribute : Attribute
    {
    }
}
