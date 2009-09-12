//using Autofac.Builder;
//using NUnit.Framework;

//namespace Autofac.Tests.Builder
//{
//    [TestFixture]
//    public class AutomaticRegistrationBuilderFixture
//    {
//        [Test]
//        public void AutomaticRegistrationBasedOnPredicate()
//        {
//            var builder = new ContainerBuilder();
//            builder.RegisterTypesMatching(t => t == typeof(object));
//            var container = builder.Build();
//            Assert.IsTrue(container.IsRegistered<object>());
//            Assert.IsNotNull(container.Resolve<object>());
//            Assert.IsFalse(container.IsRegistered<string>());
//        }

//        [Test]
//        public void NoAutomaticRegistrationOnFalsePredicate()
//        {
//            var builder = new ContainerBuilder();
//            builder.RegisterTypesMatching(t => false);
//            var container = builder.Build();
//            Assert.IsFalse(container.IsRegistered<object>());
//        }

//        [Test]
//        public void AutomaticRegistrationFromAssembly()
//        {
//            var builder = new ContainerBuilder();
//            builder.RegisterTypesFromAssembly(GetType().Assembly);
//            var container = builder.Build();
//            Assert.IsTrue(container.IsRegistered(GetType()));
//            Assert.IsFalse(container.IsRegistered<string>());
//        }

//        interface IController { }
//        class AController : IController { }
//        class NotController { }

//        [Test]
//        public void AutomaticRegistrationOfAssignable()
//        {
//            var builder = new ContainerBuilder();
//            builder.RegisterTypesAssignableTo<IController>();
//            var container = builder.Build();
//            Assert.IsTrue(container.IsRegistered<AController>());
//            Assert.IsFalse(container.IsRegistered<NotController>());
//        }

//        abstract class AbstractController : IController { }
        
//        [Test]
//        public void DoesntRegisterAbstractClasses()
//        {
//            var builder = new ContainerBuilder();
//            builder.RegisterTypesAssignableTo<IController>();
//            var container = builder.Build();
//            Assert.IsFalse(container.IsRegistered<AbstractController>());
//        }
        
//        [Test]
//        public void RespectsDefaults()
//        {
//            var builder = new ContainerBuilder();
//            builder.SetDefaultOwnership(InstanceOwnership.External);
//            builder.SetDefaultScope(InstanceSharing.Factory);
//            builder.RegisterTypesAssignableTo<DisposeTracker>();
//            DisposeTracker dt1, dt2;
//            using (var container = builder.Build())
//            {
//                dt1 = container.Resolve<DisposeTracker>();
//                dt2 = container.Resolve<DisposeTracker>();
//            }
        	
//            Assert.IsNotNull(dt1);
//            Assert.AreNotSame(dt1, dt2);
//            Assert.IsFalse(dt1.IsDisposed);
//            Assert.IsFalse(dt2.IsDisposed);
//        }

//        [Test]
//        public void ExposesImplementationType()
//        {
//            var cb = new ContainerBuilder();
//            cb.RegisterTypesAssignableTo<IController>();
//            var container = cb.Build();
//            IComponentRegistration cr;
//            Assert.IsTrue(container.ComponentRegistry.TryGetRegistration(
//                new TypedService(typeof(AController)), out cr));
//            Assert.AreEqual(typeof(AController), cr.Descriptor.BestKnownImplementationType);
//        }
//    }
//}
