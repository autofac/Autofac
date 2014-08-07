==================
Delegate Factories
==================

[TODO: Include an example of using ``RegisterGeneratedFactory``.]

Factory adapters provide the instantiation features of the container to managed components without exposing the container itself to them.

If type ``T`` is registered with the container, Autofac will :doc:`automatically resolve dependencies <../resolve/relationships>` on ``Func<T>`` as factories  that create ``T`` instances through the container.

Creation through Factories
==========================

Shareholdings
-------------

.. sourcecode:: csharp

    public class Shareholding
    {
      public delegate Shareholding Factory(string symbol, uint holding);

      public Shareholding(string symbol, uint holding)
      {
        Symbol = symbol;
        Holding = holding;
      }

      public string Symbol { get; private set; }

      public uint Holding { get; set; }
    }

The ``Shareholding`` class declares a constructor, but also provides a delegate type that can be used to create ``Shareholdings`` indirectly.

Autofac can make use of this to automatically generate a factory that can be accessed through the container:

.. sourcecode:: csharp

    var builder = new ContainerBuilder();
    builder.RegisterType<Shareholding>();
    var container = builder.Build();
    var shareholdingFactory = container.Resolve<Shareholding.Factory>();
    var shareholding = shareholdingFactory.Invoke("ABC", 1234);

The factory is a standard delegate that can be called with ``Invoke()``, as above, or with the function syntax ``shareholdingFactory("ABC", 123)``.

**By default, Autofac matches the parameters of the delegate to the parameters of the constructor by name.** If you use the generic Func types, Autofac will switch to matching parameters by type.

Portfolio
---------

Other components can use the factory:

.. sourcecode:: csharp

    public class Portfolio
    {
      Shareholding.Factory ShareholdingFactory { get; set; }
      IList<Shareholding> _holdings = new List<Shareholding>();

      public Portfolio(Shareholding.Factory shareholdingFactory)
      {
        ShareholdingFactory = shareholdingFactory;
      }

      public void Add(string symbol, uint holding)
      {
        _holdings.Add(ShareholdingFactory(symbol, holding));
      }
    }

To wire this up, the ``Portfolio`` class would be registered with the container before building using:

.. sourcecode:: csharp

    builder.Register<Portfolio>();

Using the Components
--------------------

The components can be used by requesting an instance of ``Portfolio`` from the container:

.. sourcecode:: csharp

    var portfolio = container.Resolve<Portfolio>();
    portfolio.Add("DEF", 4324);

:doc:`Autofac supports the use <../resolve/relationships>` of ``Func<T>`` delegates in addition to hand-coded delegates. ``Func<T>`` parameters are matched by type rather than by name.

The Payoff
==========

Imagine a remote stock quoting service:

.. sourcecode:: csharp

    public interface IQuoteService
    {
      decimal GetQuote(string symbol);
    }

We can add a ``value`` member to the ``Shareholding`` class that makes use of the service:

.. sourcecode:: csharp

    public class Shareholding
    {
      public delegate Shareholding Factory(string symbol, uint holding);

      IQuoteService QuoteService { get; set; }

      public Shareholding(string symbol, uint holding, IQuoteService quoteService)
      {
        QuoteService = quoteService;
        ...
      }

      public decimal Value
      {
        get
        {
          return QuoteService.GetQuote(Symbol) * Holding;
        }
      }

      // ...
    }

An implementor of ``IQuoteService`` can be registered through the container:

.. sourcecode:: csharp

    builder.Register<WebQuoteService>().As<IQuoteService>();

The ``Shareholding`` instances will now be wired up correctly, but note: the signature of ``Shareholding.Factory`` **doesn't change!** Autofac will transparently add the extra parameter to the ``Shareholding`` constructor when a factory delegate is called.

This means that ``Portfolio`` can take advantage of the ``Shareholding.Value`` property *without knowing that a quote service is involved at all.*

.. sourcecode:: csharp

    public class Portfolio
    {
      public decimal Value
      {
        get
        {
          return _holdings.Aggregate(0m, (a, e) => a + e.Value);
        }
      }

      // ...
    }

Caveat
======
In a desktop (i.e. stateful) application, when using disposable components, make sure to create nested lifetime scopes for units of work, so that the nested scope can dispose the items created by the factories within it.