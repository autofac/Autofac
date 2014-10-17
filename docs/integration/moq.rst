===
Moq
===

The `Moq <https://github.com/Moq/moq4>`_ integration package allows you to automatically create mock dependencies for both concrete and mock abstract instances in unit tests using an Autofac container. You can `get the Autofac.Extras.Moq package on NuGet <https://www.nuget.org/packages/Autofac.Extras.Moq>`_.

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

When writing your unit test, use the ``Autofac.Extras.Moq.AutoMock`` class to instantiate the system under test. Doing this will automatically inject a mock dependency into the constructor for you. At the time you create the ``AutoMock`` factory, you can specify default mock behavior:

* ``AutoMock.GetLoose()`` - creates automatic mocks using loose mocking behavior.
* ``AutoMock.GetStrict()`` - creates automatic mocks using strict mocking behavior.
* ``AutoMock.GetFromRepository(repo)`` - creates mocks based on an existing configured repository.

.. sourcecode:: csharp

    [Test]
    public void Test()
    {
      using (var mock = AutoMock.GetLoose())
      {
        // The AutoMock class will inject a mock IDependency
        // into the SystemUnderTest constructor
        var sut = mock.Create<SystemUnderTest>();
      }
    }

Configuring Mocks
=================

You can configure the automatic mocks and/or assert calls on them as you would normally with Moq.

.. sourcecode:: csharp

    [Test]
    public void Test()
    {
      using (var mock = AutoMock.GetLoose())
      {
        // Arrange - configure the mock
        mock.Mock<IDependency>().Setup(x => x.GetValue()).Returns("expected value");
        var sut = mock.Create<SystemUnderTest>();

        // Act
        var actual = sut.DoWork();

        // Assert - assert on the mock
        mock.Mock<IDependency>().Verify(x => x.GetValue());
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

You can configure the ``AutoMock`` to provide a specific instance for a given service type:

.. sourcecode:: csharp

    [Test]
    public void Test()
    {
      using (var mock = AutoMock.GetLoose())
      {
        var dependency = new Dependency();
        mock.Provide<IDependency>(dependency);

        // ...and the rest of the test.
      }
    }

You can also configure the ``AutoMock`` to provide a specific implementation type for a given service type:

.. sourcecode:: csharp

    [Test]
    public void Test()
    {
      using (var mock = AutoMock.GetLoose())
      {
        // Configure a component type that doesn't require
        // constructor parameters.
        mock.Provide<IDependency, Dependency>();

        // Configure a component type that has some
        // constructor parameters passed in. Use Autofac
        // parameters in the list.
        mock.Provide<IOtherDependency, OtherDependency>(
                    new NamedParameter("id", "service-identifier"),
                    new TypedParameter(typeof(Guid), Guid.NewGuid()));

        // ...and the rest of the test.
      }
    }
