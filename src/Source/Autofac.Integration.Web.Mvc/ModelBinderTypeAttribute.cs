using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Autofac.Integration.Web.Mvc
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public sealed class ModelBinderTypeAttribute : Attribute
    {
        private readonly Type _targetType;

        public Type TargetType
        {
            get
            {
                return _targetType;
            }
        }

        public ModelBinderTypeAttribute(Type targetType)
        {
            if (targetType == null) throw new ArgumentNullException("targetType");
            _targetType = targetType;
        }


    }
}
