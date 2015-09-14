using System;

namespace Autofac.Extras.DynamicProxy.Test.SatelliteAssembly
{
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