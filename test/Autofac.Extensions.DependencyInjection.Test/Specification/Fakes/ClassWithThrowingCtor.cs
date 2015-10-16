using System;

namespace Microsoft.Extensions.DependencyInjection.Tests.Fakes
{
    public class ClassWithThrowingCtor
    {
        public ClassWithThrowingCtor(IFakeService service)
        {
            throw new Exception(nameof(ClassWithThrowingCtor));
        }
    }
}