using System;
using System.Reflection.Emit;
using Autofac.Extras.FakeItEasy;
using FakeItEasy;
using NUnit.Framework;

namespace Autofac.Extras.Tests.FakeItEasy
{
    [TestFixture]
    public sealed class AutoFakeFixture
    {
        [Test]
        public void ByDefaultFakesAreNotStrict()
        {
            using (var fake = new AutoFake())
            {
                var foo = fake.Resolve<Foo>();
                Assert.DoesNotThrow(() => foo.Go());
            }
        }

        [Test]
        public void CanResolveStrictFakes()
        {
            using (var fake = new AutoFake(strict: true))
            {
                var foo = fake.Resolve<Foo>();
                Assert.Throws<ExpectationException>(() => foo.Go());
            }
        }

        [Test]
        public void ByDefaultFakesDoNotCallBaseMethods()
        {
            using (var fake = new AutoFake())
            {
                var bar = fake.Resolve<Bar>();
                bar.Go();
                Assert.False(bar.Gone);
            }
        }

        [Test]
        public void CanResolveFakesWhichCallsBaseMethods()
        {
            using (var fake = new AutoFake(callsBaseMethods: true))
            {
                var bar = fake.Resolve<Bar>();
                bar.Go();
                Assert.True(bar.Gone);
            }
        }

        [Test]
        public void ByDefaultFakesRespondToCalls()
        {
            using (var fake = new AutoFake())
            {
                var bar = fake.Resolve<IBar>();
                var result = bar.Spawn();
                Assert.NotNull(result);
            }
        }

        [Test]
        public void CanResolveFakesWhichDoNotRespondToCalls()
        {
            using (var fake = new AutoFake(callsDoNothing: true))
            {
                var bar = fake.Resolve<IBar>();
                var result = bar.Spawn();
                Assert.Null(result);
            }
        }

        [Test]
        public void CanResolveFakesWhichInvokeActionsWhenResolved()
        {
            object resolvedFake = null;
            using (var fake = new AutoFake(onFakeCreated: obj => resolvedFake = obj))
            {
                var bar = fake.Resolve<IBar>();
                Assert.AreSame(bar, resolvedFake);
            }
        }

        [Test]
        public void ProvidesInstances()
        {
            using (var fake = new AutoFake())
            {
                var bar = A.Fake<IBar>();
                fake.Provide(bar);

                var foo = fake.Resolve<Foo>();
                foo.Go();

                A.CallTo(() => bar.Go()).MustHaveHappened();
            }
        }

        [Test]
        public void ProvidesImplementations()
        {
            using (var fake = new AutoFake())
            {
                var baz = fake.Provide<IBaz, Baz>();

                Assert.IsNotNull(baz);
                Assert.IsTrue(baz is Baz);
            }
        }

        [Test]
        public void ByDefaultAbstractTypesAreResolvedToTheSameSharedInstance()
        {
            using (var fake = new AutoFake())
            {
                var bar1 = fake.Resolve<IBar>();
                var bar2 = fake.Resolve<IBar>();

                Assert.AreSame(bar1, bar2);
            }
        }

        [Test]
        public void ByDefaultConcreteTypesAreResolvedToTheSameSharedInstance()
        {
            using (var fake = new AutoFake())
            {
                var baz1 = fake.Resolve<Baz>();
                var baz2 = fake.Resolve<Baz>();

                Assert.AreSame(baz1, baz2);
            }
        }

        public interface IBar
        {
            bool Gone { get; }

            void Go();

            IBar Spawn();
        }

        public abstract class Bar : IBar
        {
            private bool _gone;

            public bool Gone
            {
                get { return this._gone; }
            }

            public virtual void Go()
            {
                this._gone = true;
            }

            public IBar Spawn()
            {
                throw new NotImplementedException();
            }
        }

        public interface IBaz
        {
            void Go();
        }

        public class Baz : IBaz
        {
            private bool _gone;

            public bool Gone
            {
                get { return this._gone; }
            }

            public virtual void Go()
            {
                this._gone = true;
            }
        }

        public class Foo
        {
            private readonly IBar _bar;
            private readonly IBaz _baz;

            public Foo(IBar bar, IBaz baz)
            {
                this._bar = bar;
                this._baz = baz;
            }

            public virtual void Go()
            {
                this._bar.Go();
                this._baz.Go();
            }
        }

        [AttributeUsage(AttributeTargets.Class)]
        public class ForTestAttribute : Attribute
        {
        }
    }
}
