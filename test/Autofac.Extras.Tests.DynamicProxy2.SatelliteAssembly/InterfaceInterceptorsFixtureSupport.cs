using System;

namespace Autofac.Extras.Tests.DynamicProxy2.SatelliteAssembly
{
    public interface IPublicInterfaceSatellite
    {
        string PublicMethod();
    }

    public class InterceptablePublicSatellite : IPublicInterfaceSatellite
    {
        public string PublicMethod()
        {
            throw new NotImplementedException();
        }

        public string InternalMethod()
        {
            throw new NotImplementedException();
        }
    }
}
