namespace Microsoft.Framework.DependencyInjection.Tests.Fakes
{
    public class ClassWithAmbiguousCtors
    {
        public ClassWithAmbiguousCtors(string data)
        {
        }

        public ClassWithAmbiguousCtors(IFakeService service, string data)
        {
        }

        public ClassWithAmbiguousCtors(IFakeService service, int data)
        {
        }

        public ClassWithAmbiguousCtors(IFakeService service, string data1, int data2)
        {
        }
    }
}