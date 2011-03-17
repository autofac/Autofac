using System;
using Autofac.Builder;
using NUnit.Framework;
using Autofac.Core;

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

        void ResolveTwoCtorsWith(Type[] selected)
        {
            var cb = new ContainerBuilder();
            cb.RegisterType<A1>();
            cb.RegisterType<A2>();

            cb.RegisterType<TwoCtors>()
                .UsingConstructor(selected);

            var c = cb.Build();
            var result = c.Resolve<TwoCtors>();
            Assert.IsNotNull(result);
            Assert.AreEqual(typeof(TwoCtors).GetConstructor(selected),
                typeof(TwoCtors).GetConstructor(result.CalledCtor));
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

        public class WithParam
        {
            public int I { get; private set; }
            public WithParam(int i, int j) { I = i + j; }
        }

        [Test]
        public void ParametersProvided()
        {
            var ival = 10;

            var cb = new ContainerBuilder();
            cb.RegisterType<WithParam>()
                .WithParameter(new NamedParameter("i", ival))
                .WithParameter(new NamedParameter("j", ival));

            var c = cb.Build();
            var result = c.Resolve<WithParam>();
            Assert.IsNotNull(result);
            Assert.AreEqual(ival * 2, result.I);
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
            Assert.IsTrue(container.ComponentRegistry.TryGetRegistration(
                new TypedService(typeof(object)), out cr));
            Assert.AreEqual(typeof(A1), cr.Activator.LimitType);
        }
    }
}
