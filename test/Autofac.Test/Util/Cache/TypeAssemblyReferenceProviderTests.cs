using Autofac.Builder;
using Autofac.Core;
using Autofac.Features.Indexed;
using Autofac.Util.Cache;

namespace Autofac.Test.Util.Cache;

public class TypeAssemblyReferenceProviderTests
{
    [Theory]
    [InlineData(typeof(string), new[] { typeof(string) })]
    [InlineData(typeof(IEnumerable<ContainerBuilder>), new[] { typeof(IEnumerable<>), typeof(ContainerBuilder) })]
    [InlineData(typeof(IEnumerable<>), new[] { typeof(IEnumerable<>) })]
    [InlineData(typeof(IEnumerable<ContainerBuilder[]>), new[] { typeof(IEnumerable<>), typeof(ContainerBuilder) })]
    [InlineData(typeof(IEnumerable<IIndex<int, Assert>>), new[] { typeof(IEnumerable<>), typeof(IIndex<,>), typeof(Assert) })]
    [InlineData(typeof(DerivedClass), new[] { typeof(DerivedClass), typeof(RegistrationBuilder<,,>), typeof(Assert) })]
    [InlineData(typeof(GenericDerivedClass<Assert>), new[] { typeof(DerivedClass), typeof(RegistrationBuilder<,,>), typeof(Assert), typeof(Mock) })]
    public void SimpleType(Type inputType, Type[] expandedTypeAssemblies)
    {
        var set = TypeAssemblyReferenceProvider.GetAllReferencedAssemblies(inputType);

        Assert.Distinct(set);
        Assert.Equal(expandedTypeAssemblies.Length, set.Count());

        foreach (var item in expandedTypeAssemblies)
        {
            Assert.Contains(item, expandedTypeAssemblies);
        }
    }

    private class DerivedClass
        : RegistrationBuilder<Assert, SimpleActivatorData, SingleRegistrationStyle>
    {
        public DerivedClass(Service defaultService, SimpleActivatorData activatorData, SingleRegistrationStyle style)
            : base(defaultService, activatorData, style)
        {
        }
    }

    private class GenericDerivedClass<T>
        : RegistrationBuilder<IIndex<T, Mock>, SimpleActivatorData, SingleRegistrationStyle>
    {
        public GenericDerivedClass(Service defaultService, SimpleActivatorData activatorData, SingleRegistrationStyle style)
            : base(defaultService, activatorData, style)
        {
        }
    }
}
