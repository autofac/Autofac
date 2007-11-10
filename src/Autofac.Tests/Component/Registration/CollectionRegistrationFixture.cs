using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Autofac.Component.Registration;
using Autofac.Component.Activation;
using Autofac.Builder;

namespace Autofac.Tests.Component.Registration
{
    [TestFixture]
    public class CollectionRegistrationFixture
    {
        [Test]
        public void ResolvesToCollection()
        {
            var instance1 = new object();
            var instance2 = new object();

            var target = new CollectionRegistration<object>();

            target.Add(new ComponentRegistration(
                          new[] { typeof(object) },
                          new ProvidedInstanceActivator(instance1)));

            target.Add(new ComponentRegistration(
                          new[] { typeof(object) },
                          new ProvidedInstanceActivator(instance2)));

            object result = target.ResolveInstance(new Container(), ActivationParameters.Empty, new Disposer());

            Assert.IsNotNull(result);

            var resultType = result.GetType();
            Assert.IsTrue(typeof(IEnumerable<object>).IsAssignableFrom(resultType));
            Assert.IsTrue(typeof(ICollection<object>).IsAssignableFrom(resultType));
            Assert.IsTrue(typeof(IList<object>).IsAssignableFrom(resultType));

            var resultList = new List<object>((IEnumerable<object>)result);

            Assert.AreEqual(2, resultList.Count);
            Assert.IsTrue(resultList.Contains(instance1));
            Assert.IsTrue(resultList.Contains(instance2));
        }

        [Test]
        public void ItemType()
        {
            var target = new CollectionRegistration<object>();
            Assert.AreEqual(typeof(object), target.ItemType);
        }

        [Test]
        public void ServiceTypes()
        {
            var target = new CollectionRegistration<object>();

            var serviceTypes = new List<Type>(target.Services);

            Assert.AreEqual(3, serviceTypes.Count);
            Assert.IsTrue(serviceTypes.Contains(typeof(IEnumerable<object>)));
            Assert.IsTrue(serviceTypes.Contains(typeof(ICollection<object>)));
            Assert.IsTrue(serviceTypes.Contains(typeof(IList<object>)));
        }

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void ServiceNotSupported()
		{
			var builder = new ContainerBuilder();
			builder.RegisterAsCollection<string>();
			builder.Register<object>().As<string>();
			builder.Build();
		}
	}
}
