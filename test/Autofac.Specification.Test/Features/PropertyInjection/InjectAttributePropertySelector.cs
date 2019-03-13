using System;
using System.Linq;
using System.Reflection;
using Autofac.Core;

namespace Autofac.Specification.Test.Features.PropertyInjection
{
    public class InjectAttributePropertySelector : IPropertySelector
    {
        public bool InjectProperty(PropertyInfo propertyInfo, object instance)
        {
            return propertyInfo.GetCustomAttributes<InjectAttribute>().Any();
        }
    }
}
