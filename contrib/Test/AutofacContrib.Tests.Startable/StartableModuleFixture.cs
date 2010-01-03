using Autofac;
using Autofac.Builder;
using AutofacContrib.Startable;
using NUnit.Framework;

namespace AutofacContrib.Tests.Startable
{
    [TestFixture]
    public class StartableModuleFixture
    {
        class SomeStartable
        {
            public int StartCallCount { get; private set; }

            public void Start()
            {
                ++StartCallCount;
            }
        }

        [Test]
        public void NewInstancesStarted()
        {
            var builder = new ContainerBuilder();
            builder.RegisterModule(new StartableModule<SomeStartable>(s => s.Start()));
            builder.RegisterType<SomeStartable>();
            var container = builder.Build();
            var s1 = container.Resolve<SomeStartable>();
            Assert.AreEqual(1, s1.StartCallCount);
            var inner = container.BeginLifetimeScope();
            var s2 = container.Resolve<SomeStartable>();
            Assert.AreEqual(1, s2.StartCallCount);
        }

        [Test]
        public void StarterCreatesInstances()
        {
            int instances = 0;
            var builder = new ContainerBuilder();
            builder.Register(c => { instances++; return new SomeStartable(); });
            builder.RegisterModule(new StartableModule<SomeStartable>(s => s.Start()));
            var container = builder.Build();
            container.Resolve<IStarter>().Start();
            Assert.AreEqual(1, instances);
        }
    }
}
