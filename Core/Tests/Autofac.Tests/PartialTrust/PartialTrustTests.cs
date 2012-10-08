using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Security.Permissions;
using Autofac.Tests.Scenarios.Graph1;
using NUnit.Framework;

namespace Autofac.Tests.PartialTrust
{
    /// <summary>
    /// Fixture containing the set of tests that will execute in partial trust.
    /// </summary>
    /// <remarks>
    /// <para>
    /// These tests are not marked with any NUnit attributes because they actually get executed
    /// through NUnit via the <see cref="Autofac.Tests.PartialTrust.PartialTrustTestExecutor"/>.
    /// Any public void method with no parameters found here will execute as a unit test.
    /// </para>
    /// </remarks>
    public class PartialTrustTests : MarshalByRefObject
    {
        public void AppDomainSetupCorrect()
        {
            // The sandbox should have the expected name and we shouldn't be able to demand unrestricted permissions.
            Assert.AreEqual("Sandbox", AppDomain.CurrentDomain.FriendlyName, "The AppDomain friendly name was not correct.");
            Assert.Throws<SecurityException>(() => new SecurityPermission(PermissionState.Unrestricted).Demand());
        }

        public void Integration_CanCorrectlyBuildGraph1()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<A1>().SingleInstance();
            builder.RegisterType<CD1>().As<IC1, ID1>().SingleInstance();
            builder.RegisterType<E1>().SingleInstance();
            builder.Register(ctr => new B1(ctr.Resolve<A1>()));

            var target = builder.Build();

            E1 e = target.Resolve<E1>();
            A1 a = target.Resolve<A1>();
            B1 b = target.Resolve<B1>();
            IC1 c = target.Resolve<IC1>();
            ID1 d = target.Resolve<ID1>();

            Assert.IsInstanceOf<CD1>(c);
            CD1 cd = (CD1)c;

            Assert.AreSame(a, b.A);
            Assert.AreSame(a, cd.A);
            Assert.AreNotSame(b, cd.B);
            Assert.AreSame(c, e.C);
            Assert.AreNotSame(b, e.B);
            Assert.AreNotSame(e.B, cd.B);
        }

        public interface I1<T> { }
        public interface I2<T> { }
        public class C<T> : I1<T>, I2<T> { }

        public void Integration_MultipleServicesOnAnOpenGenericType_ShareTheSameRegistration()
        {
            var builder = new ContainerBuilder();
            builder.RegisterGeneric(typeof(C<>)).As(typeof(I1<>), typeof(I2<>));
            var container = builder.Build();
            container.Resolve<I1<int>>();
            var count = container.ComponentRegistry.Registrations.Count();
            container.Resolve<I2<int>>();
            Assert.AreEqual(count, container.ComponentRegistry.Registrations.Count());
        }
    }
}
