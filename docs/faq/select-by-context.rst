==================================================
How do I pick a service implementation by context?
==================================================

There are times when you may want to register multiple :doc:`components <../glossary>` that all expose the same :doc:`service <../glossary>` but you want to pick which component is used in different instances.

For this question, let's imagine a simple order processing system. In this system, we have...

- A shipping processor that orchestrates the physical mailing of the order contents.
- A notification processor that sends an alert to a user when their order status changes.

In this simple system, the shipping processor might need to take different "plugins" to allow order delivery by different means - postal service, UPS, FedEx, and so on. The notification processor might also need different "plugins" to allow notifications by different means, like email or SMS text.

Your initial class design might look like this:

.. sourcecode:: csharp

    // This interface lets you send some content
    // to a specified destination.
    public interface ISender
    {
      void Send(Destination dest, Content content);
    }

    // We can implement the interface for different
    // "sending strategies":
    public class PostalServiceSender : ISender { ... }
    public class EmailNotifier : ISender { ... }

    // The shipping processor sends the physical order
    // to a customer given a shipping strategy (postal service,
    // UPS, FedEx, etc.).
    public class ShippingProcessor
    {
      public ShippingProcessor(ISender shippingStrategy) { ... }
    }

    // The customer notifier sends the customer an alert when their
    // order status changes using the channel specified by the notification
    // strategy (email, SMS, etc.).
    public class CustomerNotifier
    {
      public CustomerNotifier(ISender notificationStrategy) { ... }
    }

When you register things in Autofac, you might have registrations that look like this:

.. sourcecode:: csharp

    var builder = new ContainerBuilder();
    builder.RegisterType<PostalServiceSender>().As<ISender>();
    builder.RegisterType<EmailNotifier>().As<ISender>();
    builder.RegisterType<ShippingProcessor>();
    builder.RegisterType<CustomerNotifier>();
    var container = builder.Build();



.. contents:: **How do you make sure the shipping processor gets the postal service strategy and the customer notifier gets the email strategy?**
  :local:
  :depth: 1

Option 1: Redesign Your Interfaces
==================================

When you run into a situation where you have a bunch of components that implement identical services but *they can't be treated identically*, **this is generally an interface design problem**.

From an object oriented development perspective, you'd want your objects to adhere to the `Liskov substitution principle <http://en.wikipedia.org/wiki/Liskov_substitution_principle>`_ and this sort of breaks that.

Think about it from another angle: the standard "animal" example in object orientation. Say you have some animal objects and you are creating a special class that represents a bird cage that can hold small birds:

.. sourcecode:: csharp

    public abstract class Animal
    {
      public abstract string MakeNoise();
      public abstract AnimalSize Size { get; }
    }

    public enum AnimalSize
    {
      Small, Medium, Large
    }

    public class HouseCat : Animal
    {
      public override string MakeNoise() { return "Meow!"; }
      public override AnimalSize { get { return AnimalSize.Small; } }
    }

    public abstract class Bird : Animal
    {
      public override string MakeNoise() { return "Chirp!"; }
    }

    public class Parakeet : Bird
    {
      public override AnimalSize { get { return AnimalSize.Small; } }
    }

    public class BaldEagle : Bird
    {
      public override string MakeNoise() { return "Screech!"; }
      public override AnimalSize { get { return AnimalSize.Large; } }
    }

OK, there are our animals. Obviously we can't treat them all equally, so if we made a bird cage class, we *probably wouldn't do it like this*:

.. sourcecode:: csharp

    public class BirdCage
    {
      public BirdCage(Animal animal)
      {
        if(!(animal is Bird) || animal.Size != AnimalSize.Small)
        {
          // We only support small birds.
          throw new NotSupportedException();
        }
      }
    }

**Designing your bird cage to take just any animal doesn't make sense.** You'd at least want to make it take *only birds*:

.. sourcecode:: csharp

    public class BirdCage
    {
      public BirdCage(Bird bird)
      {
        if(bird.Size != AnimalSize.Small)
        {
          // We know it's a bird, but it needs to be a small bird.
          throw new NotSupportedException();
        }
      }
    }

**But if we change the class design just a little bit**, we can make it even easier and force only the right kind of birds to even be allowed to be used:

.. sourcecode:: csharp

    // We still keep the base Bird class...
    public abstract class Bird : Animal
    {
      public override string MakeNoise() { return "Chirp!"; }
    }

    // But we also add a "PetBird" class - for birds that
    // are small and kept as pets.
    public abstract class PetBird : Bird
    {
      // We "seal" the override to ensure all pet birds are small.
      public sealed override AnimalSize { get { return AnimalSize.Small; } }
    }

    // A parakeet is a pet bird, so we change the base class.
    public class Parakeet : PetBird { }
    {
      public override AnimalSize { get { return AnimalSize.Small; } }
    }

    // Bald eagles aren't generally pets, so we don't change the base class.
    public class BaldEagle : Bird
    {
      public override string MakeNoise() { return "Screech!"; }
      public override AnimalSize { get { return AnimalSize.Large; } }
    }

**Now it's easy to design our bird cage to only support small pet birds.** We just use the correct base class in the constructor:

.. sourcecode:: csharp

    public class BirdCage
    {
      public BirdCage(PetBird bird) { }
    }

Obviously this is a fairly contrived example with flaws if you dive too far into the analogy, but the principal holds - redesigning the interfaces helps us ensure the bird cage only gets what it expects and nothing else.

Bringing this back to the ordering system, *it might seem like every delivery mechanism is just "sending something"* but the truth is, they send *very different types of things*. Maybe there's a base interface that is for general "sending of things," but you probably need an intermediate level to differentiate between the types of things being sent:

.. sourcecode:: csharp

    // We can keep the ISender interface if we want...
    public interface ISender
    {
      void Send(Destination dest, Content content);
    }

    // But we'll introduce intermediate interfaces, even
    // if they're just "markers," so we can differentiate between
    // the sort of sending the strategies can perform.
    public interface IOrderSender : ISender { }
    public interface INotificationSender : ISender { }

    // We change the strategies so they implement the appropriate
    // interfaces based on what they are allowed to send.
    public class PostalServiceSender : IOrderSender { ... }
    public class EmailNotifier : INotificationSender { ... }

    // Finally, we update the classes consuming the sending
    // strategies so they only allow the right kind of strategy
    // to be used.
    public class ShippingProcessor
    {
      public ShippingProcessor(IOrderSender shippingStrategy) { ... }
    }

    public class CustomerNotifier
    {
      public CustomerNotifier(INotificationSender notificationStrategy) { ... }
    }

**By doing some interface redesign, you don't have to "choose a dependency by context"** - you use the types to differentiate and let auto-wireup magic happen during :doc:`resolution <../resolve/index>`.

**If you have the ability to affect change on your solution, this is the recommended option.**


Option 2: Change the Registrations
==================================

One of the things Autofac lets you do when you :doc:`register components <../register/index>` is to register lambda expressions rather than just types. You can manually associate the appropriate type with the consuming component in that way:

.. sourcecode:: csharp

    var builder = new ContainerBuilder();
    builder.Register(ctx => new ShippingProcessor(new PostalServiceSender()));
    builder.Register(ctx => new CustomerNotifier(new EmailNotifier()));
    var container = builder.Build();

If you want to keep the senders being resolved from Autofac, you can expose them both as their interface types and as themselves, then resolve them in the lambda:

.. sourcecode:: csharp

    var builder = new ContainerBuilder();

    // Add the "AsSelf" clause to expose these components
    // both as the ISender interface and as their natural type.
    builder.RegisterType<PostalServiceSender>()
           .As<ISender>()
           .AsSelf();
    builder.RegisterType<EmailNotifier>()
           .As<ISender>()
           .AsSelf();

    // Lambda registrations resolve based on the specific type, not the
    // ISender interface.
    builder.Register(ctx => new ShippingProcessor(ctx.Resolve<PostalServiceSender>()));
    builder.Register(ctx => new CustomerNotifier(ctx.Resolve<EmailNotifier>()));
    var container = builder.Build();

If using the lambda mechanism feels too "manual" or if the processor objects take lots of parameters, you can :doc:`manually attach parameters to the registrations <../register/parameters>`:

.. sourcecode:: csharp

    var builder = new ContainerBuilder();

    // Keep the "AsSelf" clause.
    builder.RegisterType<PostalServiceSender>()
           .As<ISender>()
           .AsSelf();
    builder.RegisterType<EmailNotifier>()
           .As<ISender>()
           .AsSelf();

    // Attach resolved parameters to override Autofac's
    // lookup just on the ISender parameters.
    builder.RegisterType<ShippingProcessor>()
           .WithParameter(
             new ResolvedParameter(
               (pi, ctx) => pi.ParameterType == typeof(ISender),
               (pi, ctx) => ctx.Resolve<PostalServiceSender>()));
    builder.RegisterType<CustomerNotifier>();
           .WithParameter(
             new ResolvedParameter(
               (pi, ctx) => pi.ParameterType == typeof(ISender),
               (pi, ctx) => ctx.Resolve<EmailNotifier>()));
    var container = builder.Build();

Using the parameter method, you still get the "auto-wireup" benefit when creating both the senders and the processors, but you can specify a very specific override in those cases.

**If you can't change your interfaces and you want to keep things simple, this is the recommended option.**


Option 3: Use Keyed Services
============================
Perhaps you are able to change your registrations but you are also using :doc:`modules <../configuration/modules>` to register lots of different components and can't really tie things together by type. A simple way to get around this is to use :doc:`keyed services <../advanced/keyed-services>`.

In this case, Autofac lets you assign a "key" or "name" to a service registration and resolve based on that key from another registration. In the module where you register your senders, you would associate the appropriate key with each sender; in the module where you register you processors, you'd apply parameters to the registrations to get the appropriate keyed service dependency.

In the module that registers your senders, add key names:

.. sourcecode:: csharp

    public class SenderModule : Module
    {
      protected override void Load(ContainerBuilder builder)
      {
        builder.RegisterType<PostalServiceSender>()
               .As<ISender>()
               .Keyed<ISender>("order");
        builder.RegisterType<EmailNotifier>()
               .As<ISender>()
               .Keyed<ISender>("notification");
      }
    }

In the module that registers the processors, add parameters that use the known keys:

.. sourcecode:: csharp

    public class ProcessorModule : Module
    {
      protected override void Load(ContainerBuilder builder)
      {
        builder.RegisterType<ShippingProcessor>()
               .WithParameter(
                 new ResolvedParameter(
                   (pi, ctx) => pi.ParameterType == typeof(ISender),
                   (pi, ctx) => ctx.ResolveKeyed<ISender>("order")));
        builder.RegisterType<CustomerNotifier>();
               .WithParameter(
                 new ResolvedParameter(
                   (pi, ctx) => pi.ParameterType == typeof(ISender),
                   (pi, ctx) => ctx.ResolveKeyed<ISender>("notification")));
      }
    }

Now when the processors are resolved, they'll search for the keyed service registrations and you'll get the right one injected.

*You can have more than one service with the same key* so this will work if you have a situation where your sender takes in an ``IEnumerable<ISender>`` as well via :doc:`implicitly supported relationships <../resolve/relationships>`. Just set the parameter to ``ctx.ResolveKeyed<IEnumerable<ISender>>("order")`` with the appropriate key in the processor registration; and register each sender with the appropriate key.

**If you have the ability to change the registrations and you're not locked into doing assembly scanning for all your registrations, this is the recommended option.**


Option 4: Use Metadata
======================
If you need something more flexible than :doc:`keyed services <../advanced/keyed-services>` or if you don't have the ability to directly affect registrations, you may want to consider using the :doc:`registration metadata <../advanced/metadata>` facility to tie the right services together.

You can associate metadata with registrations directly:

.. sourcecode:: csharp

    public class SenderModule : Module
    {
      protected override void Load(ContainerBuilder builder)
      {
        builder.RegisterType<PostalServiceSender>()
               .As<ISender>()
               .WithMetadata("send-allowed", "order");
        builder.RegisterType<EmailNotifier>()
               .As<ISender>()
               .WithMetadata("send-allowed", "notification");
      }
    }

You can then make use of the metadata as parameters on consumer registrations:

.. sourcecode:: csharp

    public class ProcessorModule : Module
    {
      protected override void Load(ContainerBuilder builder)
      {
        builder.RegisterType<ShippingProcessor>()
               .WithParameter(
                 new ResolvedParameter(
                   (pi, ctx) => pi.ParameterType == typeof(ISender),
                   (pi, ctx) => ctx.Resolve<IEnumerable<Meta<ISender>>>()
                                   .First(a => a.Metadata["send-allowed"].Equals("order"))));
        builder.RegisterType<CustomerNotifier>();
               .WithParameter(
                 new ResolvedParameter(
                   (pi, ctx) => pi.ParameterType == typeof(ISender),
                   (pi, ctx) => ctx.Resolve<IEnumerable<Meta<ISender>>>()
                                   .First(a => a.Metadata["send-allowed"].Equals("notification"))));
      }
    }

(Yes, this is *just slightly* more complex than using keyed services, but you may desire the :doc:`flexibility the metadata facility offers <../advanced/metadata>`.)

**If you can't change the registrations of the sender components, but you're allowed to change the object definitions**, you can add metadata to components using the "attribute metadata" mechanism. First you'd create your custom metadata attribute:

.. sourcecode:: csharp

    [System.ComponentModel.Composition.MetadataAttribute]
    public class SendAllowedAttribute : Attribute
    {
        public string SendAllowed { get; set; }

        public SendAllowedAttribute(string sendAllowed)
        {
          this.SendAllowed = sendAllowed;
        }
    }

Then you can apply your custom metadata attribute to the sender components:

.. sourcecode:: csharp

    [SendAllowed("order")]
    public class PostalServiceSender : IOrderSender { ... }

    [SendAllowed("notification")]
    public class EmailNotifier : INotificationSender { ... }

When you register your senders, make sure to register the ``AttributedMetadataModule``:

.. sourcecode:: csharp

    public class SenderModule : Module
    {
      protected override void Load(ContainerBuilder builder)
      {
        builder.RegisterType<PostalServiceSender>().As<ISender>();
        builder.RegisterType<EmailNotifier>().As<ISender>();
        builder.RegisterModule<AttributedMetadataModule>();
      }
    }

The consuming components can then use the metadata just like normal - the names of the attribute properties become the names in the metadata:

.. sourcecode:: csharp

    public class ProcessorModule : Module
    {
      protected override void Load(ContainerBuilder builder)
      {
        builder.RegisterType<ShippingProcessor>()
               .WithParameter(
                 new ResolvedParameter(
                   (pi, ctx) => pi.ParameterType == typeof(ISender),
                   (pi, ctx) => ctx.Resolve<IEnumerable<Meta<ISender>>>()
                                   .First(a => a.Metadata["SendAllowed"].Equals("order"))));
        builder.RegisterType<CustomerNotifier>();
               .WithParameter(
                 new ResolvedParameter(
                   (pi, ctx) => pi.ParameterType == typeof(ISender),
                   (pi, ctx) => ctx.Resolve<IEnumerable<Meta<ISender>>>()
                                   .First(a => a.Metadata["SendAllowed"].Equals("notification"))));
      }
    }

For your consuming components, you can also use attributed metadata if you don't mind adding a custom Autofac attribute to your parameter definition:

.. sourcecode:: csharp

    public class ShippingProcessor
    {
      public ShippingProcessor([WithMetadata("SendAllowed", "order")] ISender shippingStrategy) { ... }
    }

    public class CustomerNotifier
    {
      public CustomerNotifier([WithMetadata("SendAllowed", "notification")] ISender notificationStrategy) { ... }
    }

If your consuming components use the attribute, you need to register them ``WithAttributeFilter``:

.. sourcecode:: csharp

    public class ProcessorModule : Module
    {
      protected override void Load(ContainerBuilder builder)
      {
        builder.RegisterType<ShippingProcessor>().WithAttributeFilter();
        builder.RegisterType<CustomerNotifier>().WithAttributeFilter();
      }
    }

Again, the metadata mechanism is very flexible. You can mix and match the way you associate metadata with components and service consumers - attributes, parameters, and so on. You can read more about :doc:`registration metadata <../advanced/metadata>`, :doc:`registration parameters <../register/parameters>`, and :doc:`registration parameters <../resolve/parameters>` on their respective pages.

**If you are already using metadata or need the flexibility metadata offers, this is the recommended option.**