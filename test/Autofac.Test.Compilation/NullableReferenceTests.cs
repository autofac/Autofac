using Xunit;
#nullable enable

namespace Autofac.Test.Compilation
{
    public class SimpleReferenceType
    {
    }

    public class BaseClass
    {

    }

    public class DerivedClass : BaseClass
    {

    }

    public class NullableReferenceTests
    {
        [Fact]
        public void NullableTypeNotAllowedForRegistration()
        {
            new AutofacCompile()
                .Body(
                @"
                   var containerBuilder = new ContainerBuilder();
                
                   containerBuilder.Register<SimpleReferenceType?>(c => new SimpleReferenceType());
                ")
                .AssertWarningEndsWith("Nullability of type argument 'Autofac.Test.Compilation.SimpleReferenceType?' doesn't match 'notnull' constraint.");
        }

        [Fact]
        public void NullableTypeNotAllowedForAsMethod()
        {
            var containerBuilder = new ContainerBuilder();

            new AutofacCompile()
                .Body(
                @"
                   var containerBuilder = new ContainerBuilder();
                
                   containerBuilder.Register(c => new DerivedClass()).As<BaseClass?>();
                ")
                .AssertWarningEndsWith("Nullability of type argument 'Autofac.Test.Compilation.BaseClass?' doesn't match 'notnull' constraint.");
        }

        [Fact]
        public void NullableTypeNotAllowedForNamedService()
        {
            new AutofacCompile()
                .Body(
                @"
                   var containerBuilder = new ContainerBuilder();
                
                   containerBuilder.Register(c => new DerivedClass()).Named<BaseClass?>(""name"");
                ")
                .AssertWarningEndsWith("Nullability of type argument 'Autofac.Test.Compilation.BaseClass?' doesn't match 'notnull' constraint.");
        }

        [Fact]
        public void NullableTypeNotAllowedForKeyedService()
        {
            new AutofacCompile()
                .Body(
                @"
                   var containerBuilder = new ContainerBuilder();
                
                   containerBuilder.Register(c => new DerivedClass()).Keyed<BaseClass?>(""name"");
                ")
                .AssertWarningEndsWith("Nullability of type argument 'Autofac.Test.Compilation.BaseClass?' doesn't match 'notnull' constraint.");
        }

        [Fact]
        public void NullableSystemTypeNotAllowedForRegistration()
        {
            new AutofacCompile()
                .Body(
                @"
                   var containerBuilder = new ContainerBuilder();
                
                   containerBuilder.Register<int?>(c => 1);
                ")
                .AssertWarningEndsWith("Nullability of type argument 'int?' doesn't match 'notnull' constraint.");
        }

        [Fact]
        public void NonNullableTypeAllowedForRegistration()
        {
            new AutofacCompile()
                .Body(
                @"
                   var containerBuilder = new ContainerBuilder();
                
                   containerBuilder.Register(c => new SimpleReferenceType());
                ")
                .AssertNoWarnings();
        }
    }
}
