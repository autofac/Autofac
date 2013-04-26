using System;
using System.Linq.Expressions;
using System.Reflection;
using Autofac.Core;
using Autofac.Core.Activators.Reflection;
using Moq;
using NUnit.Framework;

namespace Autofac.Tests.Builder
{
    [TestFixture]
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

        [Test]
        public void ExplicitCtorCalled()
        {
            var selected = new[] { typeof(A1), typeof(A2) };
            ResolveTwoCtorsWith(selected);
        }

        [Test]
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

            Assert.That(result, Is.Not.Null);
            Assert.That(typeof(TwoCtors).GetConstructor(result.CalledCtor), Is.EqualTo(typeof(TwoCtors).GetConstructor(selected)));
        }

        [Test]
        public void ExplicitCtorFromExpression()
        {
            var cb = new ContainerBuilder();
            cb.RegisterType<A1>();
            cb.RegisterType<A2>();

            cb.RegisterType<TwoCtors>()
                .UsingConstructor(() => new TwoCtors(default(A1), default(A2)));

            var c = cb.Build();
            var result = c.Resolve<TwoCtors>();

            Assert.That(result, Is.Not.Null);
            Assert.That(result.CalledCtor, Is.EqualTo(new[] {typeof(A1), typeof(A2)}));
        }

        [Test]
        public void OtherExplicitCtorFromExpression()
        {
            var cb = new ContainerBuilder();
            cb.RegisterType<A1>();
            cb.RegisterType<A2>();

            cb.RegisterType<TwoCtors>()
                .UsingConstructor(() => new TwoCtors(default(A1)));

            var c = cb.Build();
            var result = c.Resolve<TwoCtors>();

            Assert.That(result, Is.Not.Null);
            Assert.That(result.CalledCtor, Is.EqualTo(new[] {typeof(A1)}));
        }

        [Test]
        public void UsingConstructorThatIsNotPresent_ThrowsArgumentException()
        {
            var cb = new ContainerBuilder();
            var registration = cb.RegisterType<TwoCtors>();
            Assert.Throws<ArgumentException>(() => registration.UsingConstructor(typeof(A2)));
        }

        [Test]
        public void NullIsNotAValidConstructor()
        {
            var cb = new ContainerBuilder();
            var registration = cb.RegisterType<TwoCtors>();
            Assert.Throws<ArgumentNullException>(() => registration.UsingConstructor((Type[])null));
        }

        [Test]
        public void NullIsNotAValidExpressionConstructor()
        {
            var cb = new ContainerBuilder();
            var registration = cb.RegisterType<TwoCtors>();
            Assert.Throws<ArgumentNullException>(() => registration.UsingConstructor((Expression<Func<TwoCtors>>)null));
        }

        [Test]
        public void FindConstructorsWith_CustomFinderProvided_AddedToRegistration()
        {
            var cb = new ContainerBuilder();
            var constructorFinder = new Mock<IConstructorFinder>().Object;
            var registration = cb.RegisterType<TwoCtors>().FindConstructorsWith(constructorFinder);

            Assert.That(registration.ActivatorData.ConstructorFinder, Is.EqualTo(constructorFinder));
        }

        [Test]
        public void FindConstructorsWith_NullCustomFinderProvided_ThrowsException()
        {
            var cb = new ContainerBuilder();

            var exception = Assert.Throws<ArgumentNullException>(
                () => cb.RegisterType<TwoCtors>().FindConstructorsWith((IConstructorFinder)null));

            Assert.That(exception.ParamName, Is.EqualTo("constructorFinder"));
        }

        [Test]
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

            Assert.That(finderCalled, Is.EqualTo(true));
        }

        [Test]
        public void FindConstructorsWith_CanUseFunctionToFindPrivateConstructors()
        {
            var cb = new ContainerBuilder();
            cb.RegisterType<A1>();
            cb.RegisterType<PrivateConstructor>().FindConstructorsWith(type => type.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic));
            var container = cb.Build();

            var instance = container.Resolve<PrivateConstructor>();

            Assert.That(instance.A1, Is.Not.Null);
        }

        [Test]
        public void FindConstructorsWith_NullFinderFunctionProvided_ThrowsException()
        {
            var cb = new ContainerBuilder();

            var exception = Assert.Throws<ArgumentNullException>(
                () => cb.RegisterType<TwoCtors>().FindConstructorsWith((Func<Type, ConstructorInfo[]>)null));

            Assert.That(exception.ParamName, Is.EqualTo("finder"));
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

        [Test]
        public void FindConstructorsWith_CustomFinderProvided_CustomFinderInvoked()
        {
            var builder = new ContainerBuilder();
            var finder = new CustomConstructorFinder();
            builder.RegisterType<object>().FindConstructorsWith(finder);
            var container = builder.Build();

            container.Resolve<object>();

            Assert.That(finder.FindConstructorsCalled, Is.True);
        }

        public class WithParam
        {
            public int I { get; private set; }
            public WithParam(int i, int j) { I = i + j; }
        }

        [Test]
        public void ParametersProvided()
        {
            const int ival = 10;

            var cb = new ContainerBuilder();
            cb.RegisterType<WithParam>()
                .WithParameter(new NamedParameter("i", ival))
                .WithParameter(new NamedParameter("j", ival));

            var c = cb.Build();
            var result = c.Resolve<WithParam>();

            Assert.That(result, Is.Not.Null);
            Assert.That(result.I, Is.EqualTo(ival * 2));
        }

        public class WithProp
        {
            public string Prop { get; set; }
            public int Prop2 { get; set; }
        }

        //[Test]
        //public void PropertyProvided()
        //{
        //    var pval = "Hello";

        //    var cb = new ContainerBuilder();
        //    cb.RegisterType<WithProp>()
        //        .WithProperties(new NamedPropertyParameter("Prop", pval))
        //        .WithProperties(new NamedPropertyParameter("Prop2", 1));

        //    var c = cb.Build();

        //    var result = c.Resolve<WithProp>();
        //    Assert.IsNotNull(result);
        //    Assert.AreEqual(pval, result.Prop);
        //    Assert.AreEqual(1, result.Prop2);
        //}

        [Test]
        public void ExposesImplementationType()
        {
            var cb = new ContainerBuilder();
            cb.RegisterType(typeof(A1)).As<object>();
            var container = cb.Build();
            IComponentRegistration cr;
            Assert.That(container.ComponentRegistry.TryGetRegistration(
                new TypedService(typeof(object)), out cr), Is.True);
            Assert.That(cr.Activator.LimitType, Is.EqualTo(typeof(A1)));
        }
    }
}
