# Autofac

Autofac is an [IoC container](http://martinfowler.com/articles/injection.html) for Microsoft .NET. It manages the dependencies between classes so that **applications stay easy to change as they grow** in size and complexity. This is achieved by treating regular .NET classes as *[components](http://autofac.readthedocs.org/en/latest/glossary.html)*.

[![Build status](https://ci.appveyor.com/api/projects/status/s0vgb4m8tv9ar7we?svg=true)](https://ci.appveyor.com/project/Autofac/autofac)

![MyGet publish status](https://www.myget.org/BuildSource/Badge/autofac?identifier=e0f25040-634c-4b7d-aebe-0f62b9c465a8)

## Get Packages

You can get Autofac by [grabbing the latest NuGet packages](https://github.com/autofac/Autofac/wiki/Nu-Get-Packages) or using [our NuGet script builder](http://autofac.org/scriptgen/) to get exactly what you need.

If you're feeling adventurous, [continuous integration builds are on MyGet](https://www.myget.org/gallery/autofac).

[Release notes are available on the wiki](https://github.com/autofac/Autofac/wiki/Release-Notes).

## Get Help

**Need help with Autofac?** We have [a documentation site](http://autofac.readthedocs.org/) as well as [API documentation](http://autofac.org/apidoc/). We're ready to answer your questions on [Stack Overflow](http://stackoverflow.com/questions/tagged/autofac) or check out the [discussion forum](https://groups.google.com/forum/#forum/autofac).

## Get Started

Our [Getting Started](http://autofac.readthedocs.org/en/latest/getting-started/index.html) tutorial walks you through integrating Autofac with a simple application and gives you some starting points for learning more.

Super-duper quick start:

[Register components with a `ContainerBuilder`](http://autofac.readthedocs.org/en/latest/register/registration.html) and then build the component container.

```C#
var builder = new ContainerBuilder();

builder.Register(c => new TaskController(c.Resolve<ITaskRepository>()));
builder.RegisterType<TaskController>();
builder.RegisterInstance(new TaskController());
builder.RegisterAssemblyTypes(controllerAssembly);

var container = builder.Build();
```

[Resolve services from a lifetime scope](http://autofac.readthedocs.org/en/latest/resolve/index.html) - either the container or a nested scope:

```C#
var taskController = container.Resolve<TaskController>();
```

There is a growing number of [application integration libraries](http://autofac.readthedocs.org/en/latest/integration/index.html) that make using Autofac with your application a snap. Support for several popular frameworks is also available through the "Extras" packages.

**[Intrigued? Check out our Getting Started walkthrough!](http://autofac.readthedocs.org/en/latest/getting-started/index.html)**

## Project

Autofac is licensed under the MIT license, so you can comfortably use it in commercial applications (we still love [contributions](http://autofac.readthedocs.org/en/latest/contributors.html) though).

**File issues in the repo with the associated feature/code.**

- [Autofac](https://github.com/autofac/Autofac) - Core dependency resolution and common functions (this repo).
- [Autofac.Configuration](https://github.com/autofac/Autofac.Configuration) - JSON/XML file-based configuration support.
- [Autofac.Extras.AggregateService](https://github.com/autofac/Autofac.Extras.AggregateService) - Dynamic aggregate service implementation generation.
- [Autofac.Extras.AttributeMetadata](https://github.com/autofac/Autofac.Extras.AttributeMetadata) - Metadata scanning/filtering through attributes.
- [Autofac.Extras.CommonServiceLocator](https://github.com/autofac/Autofac.Extras.CommonServiceLocator) - Common Service Locator implementation backed by Autofac.
- [Autofac.Extras.DomainServices](https://github.com/autofac/Autofac.Extras.DomainServices) - RIA/domain services support.
- [Autofac.Extras.DynamicProxy](https://github.com/autofac/Autofac.Extras.DynamicProxy) - Decorators and interceptors.
- [Autofac.Extras.EnterpriseLibraryConfigurator](https://github.com/autofac/Autofac.Extras.EnterpriseLibraryConfigurator) - Enterprise Library 5 configuration support.
- [Autofac.Extras.FakeItEasy](https://github.com/autofac/Autofac.Extras.FakeItEasy) - FakeItEasy mocking framework integration.
- [Autofac.Extras.Moq](https://github.com/autofac/Autofac.Extras.Moq) - Moq mocking framework integration.
- [Autofac.Extras.MvvmCross](https://github.com/autofac/Autofac.Extras.MvvmCross) - MvvmCross integration.
- [Autofac.Extras.NHibernate](https://github.com/autofac/Autofac.Extras.NHibernate) - NHibernate integration.
- [Autofac.Mef](https://github.com/autofac/Autofac.Mef) - MEF catalog integration.
- [Autofac.Multitenant.Wcf](https://github.com/autofac/Autofac.Multitenant.Wcf) - Multitenant WCF service hosting.
- [Autofac.Multitenant](https://github.com/autofac/Autofac.Multitenant) - Multitenant dependency resolution support.
- [Autofac.Mvc.Owin](https://github.com/autofac/Autofac.Mvc.Owin) - OWIN support for ASP.NET MVC.
- [Autofac.Owin](https://github.com/autofac/Autofac.Owin) - Core OWIN support - shared middleware for request lifetime integration.
- [Autofac.SignalR](https://github.com/autofac/Autofac.SignalR) - Application integration for SignalR.
- [Autofac.Wcf](https://github.com/autofac/Autofac.Wcf) - WCF service hosting.
- [Autofac.Web](https://github.com/autofac/Autofac.Web) - ASP.NET web forms integration.
- [Autofac.WebApi.Owin](https://github.com/autofac/Autofac.WebApi.Owin) - OWIN support for Web API.
- [Autofac.WebApi](https://github.com/autofac/Autofac.WebApi) - Application integration for Web API.

## Contributing / Pull Requests

Refer to the [Readme for Autofac Developers](https://github.com/autofac/Autofac/blob/master/developers.md)
for setting up and building Autofac source. We also have a [contributors guide](http://autofac.readthedocs.org/en/latest/contributors.html) to help you get started.


