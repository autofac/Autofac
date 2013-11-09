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
                var foo = fake.Create<Foo>();
                Assert.DoesNotThrow(() => foo.Go());
            }
        }

        [Test]
        public void CanCreateStrictFakes()
        {
            using (var fake = new AutoFake(strict: true))
            {
                var foo = fake.Create<Foo>();
                Assert.Throws<ExpectationException>(() => foo.Go());
            }
        }

        [Test]
        public void ByDefaultFakesDoNotCallBaseMethods()
        {
            using (var fake = new AutoFake())
            {
                var bar = fake.Create<Bar>();
                bar.Go();
                Assert.False(bar.Gone);
            }
        }

        [Test]
        public void CanCreateFakesWhichCallsBaseMethods()
        {
            using (var fake = new AutoFake(callsBaseMethods: true))
            {
                var bar = fake.Create<Bar>();
                bar.Go();
                Assert.True(bar.Gone);
            }
        }

        [Test]
        public void ByDefaultFakesRespondToCalls()
        {
            using (var fake = new AutoFake())
            {
                var bar = fake.Create<IBar>();
                var result = bar.Spawn();
                Assert.NotNull(result);
            }
        }

        [Test]
        public void CanCreateFakesWhichDoNotRespondToCalls()
        {
            using (var fake = new AutoFake(callsDoNothing: true))
            {
                var bar = fake.Create<IBar>();
                var result = bar.Spawn();
                Assert.Null(result);
            }
        }

        [Test]
        public void CanCreateFakesWhichInvokeActionsWhenCreated()
        {
            object createdFake = null;
            using (var fake = new AutoFake(onFakeCreated: obj => createdFake = obj))
            {
                var bar = fake.Create<IBar>();
                Assert.AreSame(bar, createdFake);
            }
        }

        [Test]
        public void ProvidesInstances()
        {
            using (var fake = new AutoFake())
            {
                var bar = A.Fake<IBar>();
                fake.Provide(bar);

                var foo = fake.Create<Foo>();
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
