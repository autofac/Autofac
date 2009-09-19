using System;
using System.Reflection;
using Autofac.Util;

namespace Autofac.Core.Activators.Reflection
{
    class AutowiringPropertyInjector
    {
        public void InjectProperties(IComponentContext context, object instance, bool overrideSetValues)
        {
            Enforce.ArgumentNotNull(context, "context");
            Enforce.ArgumentNotNull(instance, "instance");

            Type instanceType = instance.GetType();

            foreach (PropertyInfo property in instanceType.GetProperties(
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetProperty))
            {
                Type propertyType = property.PropertyType;

                if (propertyType.IsValueType)
                    continue;

                if (property.GetIndexParameters().Length != 0)
                    continue;

                if (!context.IsRegistered(propertyType))
                    continue;

                var accessors = property.GetAccessors(false);
                if (accessors.Length == 1 && accessors[0].ReturnType != typeof(void))
                    continue;

                if (!overrideSetValues &&
                    accessors.Length == 2 &&
                    (property.GetValue(instance, null) != null))
                    continue;

                object propertyValue = context.Resolve(propertyType);
                property.SetValue(instance, propertyValue, null);
            }
        }
    }
}
