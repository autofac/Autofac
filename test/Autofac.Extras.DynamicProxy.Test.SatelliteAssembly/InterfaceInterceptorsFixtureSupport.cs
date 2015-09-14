using System;

namespace Autofac.Extras.DynamicProxy.Test.SatelliteAssembly
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
