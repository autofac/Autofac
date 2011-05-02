using System;
using System.Reflection;

namespace Autofac.Core.Activators.Reflection
{
    class AutowiringPropertyInjector
    {
        public void InjectProperties(IComponentContext context, object instance, bool overrideSetValues)
        {
            if (context == null) throw new ArgumentNullException("context");
            if (instance == null) throw new ArgumentNullException("instance");

            Type instanceType = instance.GetType();

            foreach (var property in instanceType.GetProperties(
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

                var propertyValue = context.Resolve(propertyType);
                property.SetValue(instance, propertyValue, null);
            }
        }
    }
}
