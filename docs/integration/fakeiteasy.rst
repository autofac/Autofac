==========
FakeItEasy
==========

The `FakeItEasy <http://fakeiteasy.github.io>`_ integration package allows you to automatically create fake dependencies for both concrete and fake abstract instances in unit tests using an Autofac container. You can `get the Autofac.Extras.FakeItEasy package on NuGet <https://nuget.org/packages/Autofac.Extras.FakeItEasy>`_.

Getting Started
===============

Given you have a system under test and a dependency:

.. sourcecode:: csharp

    public class SystemUnderTest
    {
      public SystemUnderTest(IDependency dependency)
      {
      }
    }

    public interface IDependency
    {
    }

When writing your unit test, use the ``Autofac.Extras.FakeItEasy.AutoFake`` class to instantiate the system under test. Doing this will automatically inject a fake dependency into the constructor for you.

.. sourcecode:: csharp

    [Test]
    public void Test()
    {
      using (var fake = new AutoFake())
      {
        // The AutoFake class will inject a fake IDependency
        // into the SystemUnderTest constructor
        var sut = fake.Create<SystemUnderTest>();
      }
    }

Configuring Fakes
=================

You can configure the automatic fakes and/or assert calls on them as you would normally with FakeItEasy.

.. sourcecode:: csharp

    [Test]
    public void Test()
    {
      using (var fake = new AutoFake())
      {
        // Arrange - configure the fake
        A.CallTo(() => fake.Create<IDependency>().GetValue()).Returns("expected value");
        var sut = fake.Create<SystemUnderTest>();

        // Act
        var actual = sut.DoWork();

        // Assert - assert on the fake
        A.CallTo(() => fake.Create<IDependency>().GetValue()).MustHaveHappened();
        Assert.AreEqual("expected value", actual);
      }
    }

    public class SystemUnderTest
    {
      private readonly IDependency dependency;

      public SystemUnderTest(IDependency strings)
      {
        this.dependency = strings;
      }

      public string DoWork()
      {
        return this.dependency.GetValue();
      }
    }

    public interface IDependency
    {
      string GetValue();
    }

Configuring Specific Dependencies
=================================

You can configure the ``AutoFake`` to provide a specific instance for a given service type:

.. sourcecode:: csharp

    [Test]
    public void Test()
    {
      using (var fake = new AutoFake())
      {
        var dependency = new Dependency();
        fake.Provide(dependency);

        // ...and the rest of the test.
      }
    }

You can also configure the ``AutoFake`` to provide a specific implementation type for a given service type:

.. sourcecode:: csharp

    [Test]
    public void Test()
    {
      using (var fake = new AutoFake())
      {
        // Configure a component type that doesn't require
        // constructor parameters.
        fake.Provide<IFoo, ConcreteFoo>();

        // Configure a component type that has some
        // constructor parameters passed in.
        fake.Provide<IBar, ConcreteBar>("baz", 123, false);

        // ...and the rest of the test.
      }
    }

Options for Fakes
=================

You can specify options for fake creation using optional constructor parameters on ``AutoFake``:

.. sourcecode:: csharp

    using(var fake = new AutoFake(
        // Create fakes with strict behavior (unconfigured calls throw exceptions)
        strict: true,

        // Calls to fakes of abstract types will call the base methods on the abstract types
        callsBaseMethods: true,

        // Calls to fake methods will return null rather than generated fakes
        callsDoNothing: true,

        // Provide an action to perform upon the creation of each fake
        onFakeCreated: f => { ... }))
    {
      // Use the fakes/run the test.
    }
