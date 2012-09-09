using System.Collections;
using System.Collections.Generic;
using Autofac.Extras.Tests.CommonServiceLocator.Components;
using Microsoft.Practices.ServiceLocation;
using NUnit.Framework;

namespace CommonServiceLocator.AutofacAdapter
{
    public abstract class ServiceLocatorTestCase
    {
        IServiceLocator _locator;

        [SetUp]
        public void SetUp()
        {
            _locator = CreateServiceLocator();
        }

        protected abstract IServiceLocator CreateServiceLocator();

        [Test]
        public void GetInstance()
        {
            ILogger instance = _locator.GetInstance<ILogger>();
            Assert.IsNotNull(instance, "instance should not be null");
        }

        [Test]
        [ExpectedException(typeof(ActivationException))]
        public void AskingForInvalidComponentShouldRaiseActivationException()
        {
            _locator.GetInstance<IDictionary>();
        }

        [Test]
        public void GetNamedInstance()
        {
            ILogger instance = _locator.GetInstance<ILogger>(typeof(AdvancedLogger).FullName);
            Assert.IsInstanceOf<AdvancedLogger>(instance, "Should be an advanced logger");
        }

        [Test]
        public void GetNamedInstance2()
        {
            ILogger instance = _locator.GetInstance<ILogger>(typeof(SimpleLogger).FullName);
            Assert.IsInstanceOf<SimpleLogger>(instance, "Should be a simple logger");
        }

        [Test]
        [ExpectedException(typeof(ActivationException))]
        public void GetNamedInstance_WithZeroLenName()
        {
            _locator.GetInstance<ILogger>("");
        }

        [Test]
        [ExpectedException(typeof(ActivationException))]
        public void GetUnknownInstance2()
        {
            _locator.GetInstance<ILogger>("test");
        }

        [Test]
        public void GetAllInstances()
        {
            IEnumerable<ILogger> instances = _locator.GetAllInstances<ILogger>();
            IList<ILogger> list = new List<ILogger>(instances);
            Assert.AreEqual(2, list.Count);
        }

        [Test]
        public void GetlAllInstance_ForUnknownType_ReturnEmptyEnumerable()
        {
            IEnumerable<IDictionary> instances = _locator.GetAllInstances<IDictionary>();
            IList<IDictionary> list = new List<IDictionary>(instances);
            Assert.AreEqual(0, list.Count);
        }

        [Test]
        public void GenericOverload_GetInstance()
        {
            Assert.AreEqual(
                _locator.GetInstance<ILogger>().GetType(),
                _locator.GetInstance(typeof(ILogger), null).GetType(),
                "should get the same type"
                );
        }

        [Test]
        public void GenericOverload_GetInstance_WithName()
        {
            Assert.AreEqual(
                _locator.GetInstance<ILogger>(typeof(AdvancedLogger).FullName).GetType(),
                _locator.GetInstance(typeof(ILogger), typeof(AdvancedLogger).FullName).GetType(),
                "should get the same type"
                );
        }

        [Test]
        public void Overload_GetInstance_NoName_And_NullName()
        {
            Assert.AreEqual(
                _locator.GetInstance<ILogger>().GetType(),
                _locator.GetInstance<ILogger>(null).GetType(),
                "should get the same type"
                );
        }

        [Test]
        public void GenericOverload_GetAllInstances()
        {
            List<ILogger> genericLoggers = new List<ILogger>(_locator.GetAllInstances<ILogger>());
            List<object> plainLoggers = new List<object>(_locator.GetAllInstances(typeof(ILogger)));

            CollectionAssert.AreEqual(genericLoggers, plainLoggers, "instance collections should be equal");
        }
    }
}