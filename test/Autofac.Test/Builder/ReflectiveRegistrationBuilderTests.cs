﻿using System;
using System.Linq.Expressions;
using System.Reflection;
using Autofac.Core;
using Autofac.Core.Activators.Reflection;
using Xunit;
using System.Linq;

namespace Autofac.Test.Builder
{
    public class ReflectiveRegistrationBuilderTests
    {
        public class A1 { }
        public class A2 { }

        public class TwoCtors
        {
            public Type[] CalledCtor { get; private set; }

            public TwoCtors(A1 a1)
            {
                CalledCtor = new[] { typeof(A1) };
            }

            public TwoCtors(A1 a1, A2 a2)
            {
                CalledCtor = new[] { typeof(A1), typeof(A2) };
            }
        }

        public class PrivateConstructor
        {
            public A1 A1 { get; set; }

            private PrivateConstructor(A1 a1)
            {
                A1 = a1;
            }
        }

        [Fact]
        public void ExplicitCtorCalled()
        {
            var selected = new[] { typeof(A1), typeof(A2) };
            ResolveTwoCtorsWith(selected);
        }

        [Fact]
        public void OtherExplicitCtorCalled()
        {
            var selected = new[] { typeof(A1) };
            ResolveTwoCtorsWith(selected);
        }

        static void ResolveTwoCtorsWith(Type[] selected)
        {
            var cb = new ContainerBuilder();
            cb.RegisterType<A1>();
            cb.RegisterType<A2>();

            cb.RegisterType<TwoCtors>()
                .UsingConstructor(selected);

            var c = cb.Build();
            var result = c.Resolve<TwoCtors>();

            Assert.NotNull(result);
            Assert.Equal(typeof(TwoCtors).GetConstructor(selected), typeof(TwoCtors).GetConstructor(result.CalledCtor));
        }

        [Fact]
        public void ExplicitCtorFromExpression()
        {
            var cb = new ContainerBuilder();
            cb.RegisterType<A1>();
            cb.RegisterType<A2>();

            cb.RegisterType<TwoCtors>()
                .UsingConstructor(() => new TwoCtors(default(A1), default(A2)));

            var c = cb.Build();
            var result = c.Resolve<TwoCtors>();

            Assert.NotNull(result);
            Assert.Equal(new[] {typeof(A1), typeof(A2)}, result.CalledCtor);
        }

        [Fact]
        public void OtherExplicitCtorFromExpression()
        {
            var cb = new ContainerBuilder();
            cb.RegisterType<A1>();
            cb.RegisterType<A2>();

            cb.RegisterType<TwoCtors>()
                .UsingConstructor(() => new TwoCtors(default(A1)));

            var c = cb.Build();
            var result = c.Resolve<TwoCtors>();

            Assert.NotNull(result);
            Assert.Equal(new[] {typeof(A1)}, result.CalledCtor);
        }

        [Fact]
        public void UsingConstructorThatIsNotPresent_ThrowsArgumentException()
        {
            var cb = new ContainerBuilder();
            var registration = cb.RegisterType<TwoCtors>();
            Assert.Throws<ArgumentException>(() => registration.UsingConstructor(typeof(A2)));
        }

        [Fact]
        public void NullIsNotAValidConstructor()
        {
            var cb = new ContainerBuilder();
            var registration = cb.RegisterType<TwoCtors>();
            Assert.Throws<ArgumentNullException>(() => registration.UsingConstructor((Type[])null));
        }

        [Fact]
        public void NullIsNotAValidExpressionConstructor()
        {
            var cb = new ContainerBuilder();
            var registration = cb.RegisterType<TwoCtors>();
            Assert.Throws<ArgumentNullException>(() => registration.UsingConstructor((Expression<Func<TwoCtors>>)null));
        }

        [Fact]
        public void FindConstructorsWith_CustomFinderProvided_AddedToRegistration()
        {
            var cb = new ContainerBuilder();
            var constructorFinder = Mocks.GetConstructorFinder();
            var registration = cb.RegisterType<TwoCtors>().FindConstructorsWith(constructorFinder);

            Assert.Equal(constructorFinder, registration.ActivatorData.ConstructorFinder);
        }

        [Fact]
        public void FindConstructorsWith_NullCustomFinderProvided_ThrowsException()
        {
            var cb = new ContainerBuilder();

            var exception = Assert.Throws<ArgumentNullException>(
                () => cb.RegisterType<TwoCtors>().FindConstructorsWith((IConstructorFinder)null));

            Assert.Equal("constructorFinder", exception.ParamName);
        }

        [Fact]
        public void FindConstructorsWith_FinderFunctionProvided_PassedToConstructorFinder()
        {
            var cb = new ContainerBuilder();
            var finderCalled = false;
            Func<Type, ConstructorInfo[]> finder = type =>
            {
                finderCalled = true;
                return type.GetConstructors();
            };
            cb.RegisterType<A1>();
            cb.RegisterType<TwoCtors>().FindConstructorsWith(finder);
            var container = cb.Build();

            container.Resolve<TwoCtors>();

            Assert.True(finderCalled);
        }

        [Fact]
        public void FindConstructorsWith_CanUseFunctionToFindPrivateConstructors()
        {
            var cb = new ContainerBuilder();
            cb.RegisterType<A1>();
            cb.RegisterType<PrivateConstructor>().FindConstructorsWith(type => type.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic));
            var container = cb.Build();

            var instance = container.Resolve<PrivateConstructor>();

            Assert.NotNull(instance.A1);
        }

        [Fact]
        public void FindConstructorsWith_NullFinderFunctionProvided_ThrowsException()
        {
            var cb = new ContainerBuilder();

            var exception = Assert.Throws<ArgumentNullException>(
                () => cb.RegisterType<TwoCtors>().FindConstructorsWith((Func<Type, ConstructorInfo[]>)null));

            Assert.Equal("finder", exception.ParamName);
        }

        public class CustomConstructorFinder : IConstructorFinder
        {
            public bool FindConstructorsCalled { get; private set; }

            public ConstructorInfo[] FindConstructors(Type targetType)
            {
                FindConstructorsCalled = true;
                return new DefaultConstructorFinder().FindConstructors(targetType);
            }
        }

        [Fact]
        public void FindConstructorsWith_CustomFinderProvided_CustomFinderInvoked()
        {
            var builder = new ContainerBuilder();
            var finder = new CustomConstructorFinder();
            builder.RegisterType<object>().FindConstructorsWith(finder);
            var container = builder.Build();

            container.Resolve<object>();

            Assert.True(finder.FindConstructorsCalled);
        }

        public class WithParam
        {
            public int I { get; private set; }
            public WithParam(int i, int j) { I = i + j; }
        }

        [Fact]
        public void ParametersProvided()
        {
            const int ival = 10;

            var cb = new ContainerBuilder();
            cb.RegisterType<WithParam>()
                .WithParameter(new NamedParameter("i", ival))
                .WithParameter(new NamedParameter("j", ival));

            var c = cb.Build();
            var result = c.Resolve<WithParam>();

            Assert.NotNull(result);
            Assert.Equal(ival * 2, result.I);
        }

        public class WithProp
        {
            public string Prop { get; set; }
            public int Prop2 { get; set; }
        }

        //[Fact]
        //public void PropertyProvided()
        //{
        //    var pval = "Hello";

        //    var cb = new ContainerBuilder();
        //    cb.RegisterType<WithProp>()
        //        .WithProperties(new NamedPropertyParameter("Prop", pval))
        //        .WithProperties(new NamedPropertyParameter("Prop2", 1));

        //    var c = cb.Build();

        //    var result = c.Resolve<WithProp>();
        //    Assert.NotNull(result);
        //    Assert.Equal(pval, result.Prop);
        //    Assert.Equal(1, result.Prop2);
        //}

        [Fact]
        public void ExposesImplementationType()
        {
            var cb = new ContainerBuilder();
            cb.RegisterType(typeof(A1)).As<object>();
            var container = cb.Build();
            IComponentRegistration cr;
            Assert.True(container.ComponentRegistry.TryGetRegistration(new TypedService(typeof(object)), out cr));
            Assert.Equal(typeof(A1), cr.Activator.LimitType);
        }

        [Fact]
        public void DoesNotResolvePropertiesByDefault()
        {
            const string str = "test";

            var cb = new ContainerBuilder();
            cb.RegisterType<WithPropInjection>();
            cb.RegisterInstance(str);

            var c = cb.Build();
            var result = c.Resolve<WithPropInjection>();

            Assert.NotNull(result);
            Assert.Null(result.Prop);
            Assert.Null(result.GetProp2());
        }

        [Fact]
        public void ResolvePropertiesWithDefaultFinder()
        {
            const string str = "test";

            var cb = new ContainerBuilder();
            cb.RegisterType<WithPropInjection>()
                .FindProperties();
            cb.RegisterInstance(str);

            var c = cb.Build();
            var result = c.Resolve<WithPropInjection>() ;

            Assert.NotNull(result);
            Assert.Equal(str, result.Prop);
            Assert.Null(result.GetProp2());
        }

        [Fact]
        public void ResolvePropertiesWithCustomDelegate()
        {
            var finderCalled = false;

            var cb = new ContainerBuilder();
            cb.RegisterType<WithPropInjection>()
                .FindPropertiesWith(type =>
                {
                    finderCalled = true;
                    return new PropertyInfo[0];
                });

            var c = cb.Build();
            var result = c.Resolve<WithPropInjection>() ;

            Assert.True(finderCalled);
            Assert.NotNull(result);
            Assert.Null(result.Prop);
            Assert.Null(result.GetProp2());
        }

        [Fact]
        public void ResolvePropertiesWithCustomImplementation()
        {
            const string str = "test";

            var cb = new ContainerBuilder();
            cb.RegisterType<WithPropInjection>()
                .FindPropertiesWith(new InjectAttributePropertyFinder());
            cb.RegisterInstance(str);

            var c = cb.Build();
            var result = c.Resolve<WithPropInjection>() ;

            Assert.NotNull(result);
            Assert.Null(result.Prop);
            Assert.Equal(str, result.GetProp2());
        }

        private class InjectAttributePropertyFinder : IPropertyFinder
        {
            public PropertyInfo[] FindProperties(Type type)
            {
                return type.GetTypeInfo().DeclaredProperties
                    .Where(prop => prop.GetCustomAttributes<InjectAttribute>().Any())
                    .ToArray();
            }
        }

        private class InjectAttribute : Attribute { }

        private class WithPropInjection
        {
            public string Prop { get; set; }

            [Inject]
            private string Prop2 { get; set; }

            public string GetProp2() => Prop2;
        }
    }
}
