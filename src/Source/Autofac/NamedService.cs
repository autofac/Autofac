using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Autofac
{
    public class NamedService : Service
    {
        string ServiceName { get; set; }

        public NamedService(string serviceName)
        {
            ServiceName = Enforce.ArgumentNotNullOrEmpty(serviceName, "serviceName");
        }

        public override string Description
        {
            get
            {
                return ServiceName;
            }
        }

        public override bool Equals(object obj)
        {
            NamedService that = obj as NamedService;

            if (that == null)
                return false;

            return ServiceName == that.ServiceName;
        }

        public override int GetHashCode()
        {
            return ServiceName.GetHashCode();
        }
    }
}
