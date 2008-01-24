using System.Collections.Generic;
using Autofac.Builder;
using NUnit.Framework;

namespace Autofac.Tests.Builder
{
    [TestFixture]
    public class GenericRegistrationBuilderFixture
    {
        [Test]
        public void BuildGenericRegistration()
        {
            var cb = new ContainerBuilder();
            cb.RegisterGeneric(typeof(List<>)).As(typeof(ICollection<>)).WithScope(InstanceScope.Factory);
            var c = cb.Build();

            ICollection<int> coll = c.Resolve<ICollection<int>>();
            ICollection<int> coll2 = c.Resolve<ICollection<int>>();

            Assert.IsNotNull(coll);
            Assert.IsNotNull(coll2);
            Assert.AreNotSame(coll, coll2);
            Assert.IsTrue(coll.GetType().GetGenericTypeDefinition() == typeof(List<>));
        }
    }
}
