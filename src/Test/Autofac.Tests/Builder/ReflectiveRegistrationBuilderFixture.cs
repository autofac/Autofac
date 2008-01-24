using Autofac.Builder;
using NUnit.Framework;
using System;

namespace Autofac.Tests.Builder
{
    [TestFixture]
    public class ReflectiveRegistrationBuilderFixture
    {
        class A1 { }
        class A2 { }

        class TwoCtors
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
            var cb = new ContainerBuilder();
            cb.Register<A1>();
            cb.Register<A2>();

            var selected = new[] { typeof(A1), typeof(A2) };

            cb.Register<TwoCtors>()
                .UsingConstructor(selected);

            var c = cb.Build();
            var result = c.Resolve<TwoCtors>();
            Assert.IsNotNull(result);
            Assert.AreEqual(typeof(TwoCtors).GetConstructor(selected),
                typeof(TwoCtors).GetConstructor(result.CalledCtor));
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void ExplicitCtorNotPresent()
        {
            var cb = new ContainerBuilder();
            cb.Register<TwoCtors>()
                .UsingConstructor(typeof(A2));
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ExplicitCtorNull()
        {
            var cb = new ContainerBuilder();
            cb.Register<TwoCtors>()
                .UsingConstructor(null);
        }

        class WithParam
        {
            public int I { get; private set; }
            public WithParam(int i) { I = i; }
        }

        [Test]
        public void ParametersProvided()
        {
            var ival = 10;

            var cb = new ContainerBuilder();
            cb.Register<WithParam>().
                WithArguments(new Parameter("i", ival));

            var c = cb.Build();
            var result = c.Resolve<WithParam>();
            Assert.IsNotNull(result);
            Assert.AreEqual(ival, result.I);
        }

        class WithProp
        {
            public string Prop { get; set; }
        }

        [Test]
        public void PropertyProvided()
        {
            var pval = "Hello";

            var cb = new ContainerBuilder();
            cb.Register<WithProp>()
                .WithProperties(new Parameter("Prop", pval));

            var c = cb.Build();

            var result = c.Resolve<WithProp>();
            Assert.IsNotNull(result);
            Assert.AreEqual(pval, result.Prop);
        }
    }
}
