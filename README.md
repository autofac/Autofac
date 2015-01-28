# Autofac

Autofac is an [IoC container](http://martinfowler.com/articles/injection.html) for Microsoft .NET. It manages the dependencies between classes so that **applications stay easy to change as they grow** in size and complexity. This is achieved by treating regular .NET classes as *[components](http://autofac.readthedocs.org/en/latest/glossary.html)*.


## NuGet Packages

You can get Autofac by [grabbing the latest NuGet packages](https://github.com/autofac/Autofac/wiki/Nu-Get-Packages) or using [our NuGet script builder](http://autofac.org/scriptgen/) to get exactly what you need. A few older versions remain for download [here](https://code.google.com/p/autofac/downloads/list).

## Getting Help

**Need help with Autofac?** We have [a documentation site](http://autofac.readthedocs.org/) as well as [API documentation](http://autofac.org/apidoc/). We're ready to answer your questions on [Stack Overflow](http://stackoverflow.com/questions/tagged/autofac) or check out the [discussion forum](https://groups.google.com/forum/#forum/autofac).

## Getting Started

Our [Getting Started](http://autofac.readthedocs.org/en/latest/getting-started/index.html) tutorial walks you through integrating Autofac with a simple application and gives you some starting points for learning more.

### Adding Components

_[Components are registered](http://autofac.readthedocs.org/en/latest/register/registration.html)_ with a `ContainerBuilder`:

```C#
var builder = new ContainerBuilder();
```

Autofac can use [a Linq expression, a .NET type, or a pre-built instance](http://autofac.readthedocs.org/en/latest/register/registration.html) as a component:

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

To retrieve a _component instance_ from a container, a _[service](http://autofac.readthedocs.org/en/latest/glossary.html)_ is requested. By default, components provide their concrete type as a service:

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

[Circular references](http://autofac.readthedocs.org/en/latest/advanced/circular-dependencies.html) can be constructed by declaring one of the parameters to be of type `Lazy<T>`.

```C#
public TaskController(Lazy<ITaskRepository> tasks)
```

Autofac understands an advanced vocabulary of "[relationship types](http://autofac.readthedocs.org/en/latest/resolve/relationships.html)" like `Lazy<T>`, `Func<T>`, `IEnumerable<T>` and others, to vary the relationship between a component and its dependencies.

## Highlights

Autofac keeps out of your way and places as few constraints on your design as possible.

**Simple Extension Points:** [Activation events](http://autofac.readthedocs.org/en/latest/lifetime/events.html) like `OnActivating(e => e.Instance.Start())` can achieve a lot of customization in very little code.

**Robust Resource Management:** Autofac [takes on the burden](http://autofac.readthedocs.org/en/latest/lifetime/disposal.html) of tracking disposable components to ensure that resources are released when they should be.

**Flexible Module System:** Strike a balance between the deployment-time benefits of [XML configuration](http://autofac.readthedocs.org/en/latest/configuration/xml.html) and the clarity of C# code with [Autofac modules](http://autofac.readthedocs.org/en/latest/configuration/modules.html).

## Status

> Autofac moved to GitHub on the 22nd January, 2013. The process of cleaning up the issues list and wiki content is ongoing. You may stumble across some invalid links while we sort out problems from the migration. The code and NuGet packages all remain in a consistent state.

You can get the latest releases from [NuGet](https://www.nuget.org/packages?q=Author%3A%22Autofac+Contributors%22+Owner%3A%22alexmg%22+Autofac*). [Release notes are available on the wiki](https://github.com/autofac/Autofac/wiki/Release-Notes).

If you're feeling bold, you can get [continuous integration builds from MyGet](https://www.myget.org/gallery/autofac).

![](https://www.myget.org/BuildSource/Badge/autofac?identifier=e0f25040-634c-4b7d-aebe-0f62b9c465a8)

There is a growing number of [integrations](http://autofac.readthedocs.org/en/latest/integration/index.html) that make using Autofac with your application a snap. Support for several popular frameworks is also available through the "Extras" packages.

Autofac is licensed under the MIT license, so you can comfortably use it in commercial applications (we still love [contributions](https://github.com/autofac/Autofac/wiki/Contribution-Guidelines) though).

## Contributing

Refer to the [Readme for Autofac Developers](https://github.com/autofac/Autofac/blob/master/developers.md)
for setting up, building Autofac and generating the related documentation. We also have a [contributors guide](http://autofac.readthedocs.org/en/latest/contributors.html) to help you get started.


