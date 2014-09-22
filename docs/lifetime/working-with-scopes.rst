============================
Working with Lifetime Scopes
============================

Creating a New Lifetime Scope
=============================

You can create a lifetime scope by calling the ``BeginLifetimeScope()`` method on any existing lifetime scope, starting with the root container. **Lifetime scopes are disposable and they track component disposal, so make sure you always call "Dispose()"" or wrap them in "using" statements.**

.. sourcecode:: csharp

  using(var scope = container.BeginLifetimeScope())
  {
    // Resolve services from a scope that is a child
    // of the root container.
    var service = scope.Resolve<IService>();

    // You can also create nested scopes...
    using(var unitOfWorkScope = scope.BeginLifetimeScope())
    {
      var anotherService = unitOfWorkScope.Resolve<IOther>();
    }
  }

Tagging a Lifetime Scope
========================

There are some cases where you want to share services across units of work but you don't want those services to be shared globally like singletons. A common example is "per-request" lifetimes in web applications. (:doc:`You can read more about per-request scoping in the "Instance Scope" topic. <instance-scope>`) In this case, you'd want to tag your lifetime scope and register services as ``InstancePerMatchingLifetimeScope()``.

For example, say you have a component that sends emails. A logical transaction in your system may need to send more than one email, so you can share that component across individual pieces of the logical transaction. However, you don't want the email component to be a global singleton. Your setup might look something like this:

.. sourcecode:: csharp

  // Register your transaction-level shared component
  // as InstancePerMatchingLifetimeScope and give it
  // a "known tag" that you'll use when starting new
  // transactions.
  var builder = new ContainerBuilder();
  builder.RegisterType<EmailSender>()
         .As<IEmailSender>()
         .InstancePerMatchingLifetimeScope("transaction");

  // Both the order processor and the receipt manager
  // need to send email notifications.
  builder.RegisterType<OrderProcessor>()
         .As<IOrderProcessor>();
  builder.RegisterType<ReceiptManager>()
         .As<IReceiptManager>();

  var container = builder.Build();


  // Create transaction scopes with a tag.
  using(var transactionScope = container.BeginLifetimeScope("transaction"))
  {
    using(var orderScope = transactionScope.BeginLifetimeScope())
    {
      // This would resolve an IEmailSender to use, but the
      // IEmailSender would "live" in the parent transaction
      // scope and be shared across any children of the
      // transaction scope because of that tag.
      var op = orderScope.Resolve<IOrderProcessor>();
      op.ProcessOrder();
    }

    using(var receiptScope = transactionScope.BeginLifetimeScope())
    {
      // This would also resolve an IEmailSender to use, but it
      // would find the existing IEmailSender in the parent
      // scope and use that. It'd be the same instance used
      // by the order processor.
      var rm = receiptScope.Resolve<IReceiptManager>();
      rm.SendReceipt();
    }
  }

Again, :doc:`you can read more about tagged scopes and per-request scoping in the "Instance Scope" topic. <instance-scope>`

Adding Registrations to a Lifetime Scope
========================================

Autofac allows you to add registrations "on the fly" as you create lifetime scopes. This can help you when you need to do a sort of "spot weld" limited registration override or if you generally just need some additional stuff in a scope that you don't want to register globally. You do this by passing a lambda to ``BeginLifetimeScope()`` that takes a ``ContainerBuilder`` and adds registrations.

.. sourcecode:: csharp

  using(var scope = container.BeginLifetimeScope(
    builder =>
    {
      builder.RegisterType<Override>().As<IService>();
      builder.RegisterModule<MyModule>();
    }))
  {
    // The additional registrations will be available
    // only in this lifetime scope.
  }
