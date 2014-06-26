==================
Aggregate Services
==================

Introduction
------------

An aggregate service is useful when you need to treat a set of dependencies as one dependency. When a class depends on several constructor-injected services, or have several property-injected services, moving those services into a separate class yields a simpler API.

An example is super- and subclasses where the superclass have one or more constructor-injected dependencies. The subclasses must usually inherit these dependencies, even though they might only be useful to the superclass. With an aggregate service, the superclass constructor parameters can be collapsed into one parameter, reducing the repetitiveness in subclasses. Another important side effect is that subclasses are now insulated against changes in the superclass dependencies, introducing a new dependency in the superclass means only changing the aggregate service definition.

The pattern and this example `are both further elaborated here <http://peterspattern.com/dependency-injection-and-class-inheritance>`_.

Aggregate services can be implemented by hand, e.g. by building a class with constructor-injected dependencies and exposing those as properties. Writing and maintaining aggregate service classes and accompanying tests can quickly get tedious though. The AggregateService extension to Autofac lets you generate aggregate services directly from interface definitions without having to write any implementation.

Required References
-------------------

You can add aggregate service support to your project using `the Autofac.Extras.AggregateService NuGet package <https://nuget.org/packages/Autofac.Extras.AggregateService>`_ or by manually adding references to these assemblies:

 * Autofac.dll
 * Autofac.Extras.AggregateService.dll
 * Castle.Core.dll (`from the Castle project <http://www.castleproject.org/download/>`_)

Getting Started
---------------

Lets say we have a class with a number of constructor-injected dependencies that we store privately for later use:

.. sourcecode:: csharp

    public class SomeController
    {
      private readonly IFirstService _firstService;
      private readonly ISecondService _secondService;
      private readonly IThirdService _thirdService;
      private readonly IFourthService _fourthService;

      public SomeController(
        IFirstService firstService,
        ISecondService secondService,
        IThirdService thirdService,
        IFourthService fourthService)
      {
        _firstService = firstService;
        _secondService = secondService;
        _thirdService = thirdService;
        _fourthService = fourthService;
      }
    }

To aggregate the dependencies we move those into a separate interface definition and take a dependency on that interface instead.

.. sourcecode:: csharp

    public interface IMyAggregateService
    {
      IFirstService FirstService { get; }
      ISecondService SecondService { get; }
      IThirdService ThirdService { get; }
      IFourthService FourthService { get; }
    }

    public class SomeController
    {
      private readonly IMyAggregateService _aggregateService;

      public SomeController(IMyAggregateService aggregateService)
      {
        _aggregateService = aggregateService;
      }
    }

Finally, we register the aggregate service interface.

.. sourcecode:: csharp

    using Autofac;
    using Autofac.Contrib.AggregateService;
    //...

    var builder = new ContainerBuilder();
    builder.RegisterAggregateService<IMyAggregateService>();
    builder.Register(/*...*/).As<IFirstService>();
    builder.Register(/*...*/).As<ISecondService>();
    builder.Register(/*...*/).As<IThirdService>();
    builder.Register(/*...*/).As<IFourthService>();
    builder.RegisterType<SomeController>();
    var container = builder.Build();

The interface for the aggregate service will automatically have an implementation generated for you and the dependencies will be filled in as expected.

How Aggregate Services are Resolved
-----------------------------------

Properties
----------

Read-only properties mirror the behavior of regular constructor-injected dependencies. The type of each property will be resolved and cached in the aggregate service when the aggregate service instance is constructed. 

Here is a functionally equivalent sample:

.. sourcecode:: csharp

    class MyAggregateServiceImpl: IMyAggregateService
    {
      private IMyService _myService;

      public MyAggregateServiceImpl(IComponentContext context)
      {
        _myService = context.Resolve<IMyService>();
      }
 
      public IMyService MyService 
      { 
        get { return _myService; }      
      }
    }

Methods
-------

Methods will behave like factory delegates and will translate into a resolve call on each invocation. The method return type will be resolved, passing on any parameters to the resolve call. 

A functionally equivalent sample of the method call:

.. sourcecode:: csharp

    class MyAggregateServiceImpl: IMyAggregateService
    {
      public ISomeThirdService GetThirdService(string data)
      {
        var dataParam = new TypedParameter(typeof(string), data);
        return _context.Resolve<ISomeThirdService>(dataParam);
      }
    }

Property Setters and Void Methods
---------------------------------

Property setters and methods without return types does not make sense in the aggregate service. Their presence in the aggregate service interface does not prevent proxy generation. Calling such methods though will throw an exception.

How It Works
------------

Under the covers, the AggregateService uses DynamicProxy2 from `the Castle Project <http://castleproject.org>`_. Given an interface (the aggregate of services into one), a proxy is generated implementing the interface. The proxy will translate calls to properties and methods into ``Resolve`` calls to an Autofac context.

Performance Considerations
--------------------------

Due to the fact that method calls in the aggregate service pass through a dynamic proxy there is a small but non-zero amount of overhead on each method call. A performance study on Castle DynamicProxy2 vs other frameworks can be found `here <http://kozmic.pl/2009/03/31/dynamic-proxy-frameworks-comparison-update/>`_.
