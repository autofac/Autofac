using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Autofac
{
    public class TypedService : Service
    {
        public TypedService(Type serviceType)
        {
            ServiceType = Enforce.ArgumentNotNull(serviceType, "serviceType");
        }

        public Type ServiceType { get; private set; }

        public override string Description
        {
            get
            {    
                return ServiceType.FullName;
            }
        }

        public override bool Equals(object obj)
        {
            TypedService that = obj as TypedService;

            if (that == null)
                return false;

            return ServiceType == that.ServiceType;
        }

        public override int GetHashCode()
        {
            return ServiceType.GetHashCode();
        }
    }
}
