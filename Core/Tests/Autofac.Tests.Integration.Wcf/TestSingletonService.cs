using System.ServiceModel;

namespace Autofac.Tests.Integration.Wcf
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class TestSingletonService : ITestService
    {
    }
}