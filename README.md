# Autofac

Autofac is an [IoC container](http://martinfowler.com/articles/injection.html) for Microsoft .NET. It manages the dependencies between classes so that **applications stay easy to change as they grow** in size and complexity. This is achieved by treating regular .NET classes as *[components](https://github.com/autofac/Autofac/wiki/Domain-Model)*.


## NuGet Packages

The easiest way to get Autofac into your application is to [grab the NuGet packages](https://github.com/autofac/Autofac/wiki/Nu-Get-Packages). If you're not into NuGet, you can [download Autofac from this site](https://code.google.com/p/autofac/downloads/list).

## Getting Help

**Need help with Autofac?** We're ready to answer your questions on [Stack Overflow](http://stackoverflow.com/questions/tagged/autofac Stack Overflow) or check out the [discussion forum](https://groups.google.com/forum/#forum/autofac).

## Getting Started

Our [Getting Started](https://github.com/autofac/Autofac/wiki/Getting-Started) tutorial walks you through integrating Autofac with a simple application and gives you some starting points for learning more.

### Adding Components

_[Components are registered](https://github.com/autofac/Autofac/wiki/Component-Creation)_ with a `ContainerBuilder`:

```C#
var builder = new ContainerBuilder();
```

Autofac can use [a Linq expression, a .NET type, or a pre-built instance](https://github.com/autofac/Autofac/wiki/Component-Creation) as a component:

```C#
builder.Register(c => new TaskController(c.Resolve<ITaskRepository>()));

builder.RegisterType<TaskController>();

builder.RegisterInstance(new TaskController());
```

Or, Autofac can find and register the component types in an assembly:

```C#
builder.RegisterAssemblyTypes(controllerAssembly);
```

Calling `Build()` creates a _container_:

```C#
var container = builder.Build();
```

To retrieve a _component instance_ from a container, a _[service](https://github.com/autofac/Autofac/wiki/Domain-Model)_ is requested. By default, components provide their concrete type as a service:

```C#
var taskController = container.Resolve<TaskController>();
```

To specify that the component’s service is an interface, the `As()` method is used at registration time:

```C#
builder.RegisterType<TaskController>().As<IController>();
// enabling
var taskController = container.Resolve<IController>();
```

### Expressing Dependencies

When Autofac instantiates a component, it satisfies the component's _dependencies_ by finding and instantiating other components.

Components express their dependencies to Autofac as constructor parameters:

```C#
public class TaskController : IController
{
    public TaskController(ITaskRepository tasks) { ... }
}
```

In this case Autofac will look for another component that provides the `ITaskRepository` service and call the constructor of `TaskController` with that component as a parameter.

If there is more than one constructor on a component type, Autofac will use the constructor with the most resolvable parameters.

```C#
public TaskController(ITaskRepository tasks)
public TaskController(ITaskRepository tasks, ILog log)
```

Default parameter values can be used to express optional dependencies (properties can be used instead if you prefer):

```C#
public TaskController(ITaskRepository tasks, ILog log = null)
```

[Circular references](https://github.com/autofac/Autofac/wiki/Circular-Dependencies) can be constructed by declaring one of the parameters to be of type `Lazy<T>`.

```C#
public TaskController(Lazy<ITaskRepository> tasks)
```

Autofac understands an advanced vocabulary of "[relationship types](https://github.com/autofac/Autofac/wiki/Relationship-Types)" like `Lazy<T>`, `Func<T>`, `IEnumerable<T>` and others, to vary the relationship between a component and its dependencies.

## Highlights

Autofac keeps out of your way and places as few constraints on your design as possible.

**Simple Extension Points:** [Activation events](https://github.com/autofac/Autofac/wiki/Lifetime-Events) like `OnActivating(e => e.Instance.Start())` can achieve a lot of customization in very little code.

**Robust Resource Management:** Autofac [takes on the burden](https://github.com/autofac/Autofac/wiki/Deterministic-Disposal) of tracking disposable components to ensure that resources are released when they should be.

**Flexible Module System:** Strike a balance between the deployment-time benefits of [XML configuration](https://github.com/autofac/Autofac/wiki/Xml-Configuration) and the clarity of C# code with [Autofac modules](https://github.com/autofac/Autofac/wiki/Structuring-With-Modules).

## Status

> Autofac moved to GitHub on the 22nd January, 2013. The process of cleaning up the issues list and wiki content has only just started. You may stumble across some invalid links while we sort out problems from the migration. The code and NuGet packages all remain in a consistent state.

You can get the latest releases [from this site](https://code.google.com/p/autofac/downloads/list) or from [NuGet](https://www.nuget.org/packages?q=Author%3A%22Autofac+Contributors%22+Owner%3A%22alexmg%22+Autofac*). [Release notes are available on the wiki](https://github.com/autofac/Autofac/wiki/Release-Notes).

If you're feeling bold, you can get [continuous integration builds from MyGet](https://www.myget.org/gallery/autofac).

There is a growing number of [integrations](https://github.com/autofac/Autofac/wiki/Integration) that make using Autofac with your application a snap. Support for several popular frameworks is also available through the "Extras" packages.

Our friendly and open [community](http://groups.google.com/group/autofac) will help you to get the best from the project. You can also ask questions on [Stack Overflow](http://stackoverflow.com/questions/tagged/autofac).

Autofac is licensed under the MIT license, so you can comfortably use it in commercial applications (we still love [contributions](https://github.com/autofac/Autofac/wiki/Contribution-Guidelines) though).
