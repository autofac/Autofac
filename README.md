<p align="center">
  <img src="https://autofac.org/img/autofac_web-banner_character.svg" width="250">
</p>
<p align="center">
  <img src="https://autofac.org/img/autofac_logo-type.svg" height="100">
</p>

Autofac is an [IoC container](http://martinfowler.com/articles/injection.html) for Microsoft .NET. It manages the dependencies between classes so that **applications stay easy to change as they grow** in size and complexity. This is achieved by treating regular .NET classes as *[components](https://autofac.readthedocs.io/en/latest/glossary.html)*.

[![Build status](https://ci.appveyor.com/api/projects/status/s0vgb4m8tv9ar7we?svg=true)](https://ci.appveyor.com/project/Autofac/autofac) ![MyGet publish status](https://www.myget.org/BuildSource/Badge/autofac?identifier=e0f25040-634c-4b7d-aebe-0f62b9c465a8) [![Autofac on Stack Overflow](https://img.shields.io/badge/stack%20overflow-autofac-orange.svg)](https://stackoverflow.com/questions/tagged/autofac) [![Join the chat at https://gitter.im/autofac/autofac](https://img.shields.io/gitter/room/autofac/autofac.svg)](https://gitter.im/autofac/autofac) [![NuGet](https://img.shields.io/nuget/v/Autofac.svg)](https://nuget.org/packages/Autofac)

## Get Packages

You can get Autofac by [grabbing the latest NuGet packages](https://github.com/autofac/Autofac/wiki/Nu-Get-Packages). If you're feeling adventurous, [continuous integration builds are on MyGet](https://www.myget.org/gallery/autofac).

[Release notes are available on the wiki](https://github.com/autofac/Autofac/wiki#release-notes).

## Get Help

**Need help with Autofac?** We have [a documentation site](https://autofac.readthedocs.io/) as well as [API documentation](https://autofac.org/apidoc/). We're ready to answer your questions on [Stack Overflow](https://stackoverflow.com/questions/tagged/autofac) or check out the [discussion forum](https://groups.google.com/forum/#forum/autofac).

## Get Started

Our [Getting Started](https://autofac.readthedocs.io/en/latest/getting-started/index.html) tutorial walks you through integrating Autofac with a simple application and gives you some starting points for learning more.

Super-duper quick start:

[Register components with a `ContainerBuilder`](https://autofac.readthedocs.io/en/latest/register/registration.html) and then build the component container.

```C#
var builder = new ContainerBuilder();

builder.Register(c => new TaskController(c.Resolve<ITaskRepository>()));
builder.RegisterType<TaskController>();
builder.RegisterInstance(new TaskController());
builder.RegisterAssemblyTypes(controllerAssembly);

var container = builder.Build();
```

[Resolve services from a lifetime scope](https://autofac.readthedocs.io/en/latest/resolve/index.html) - either the container or a nested scope:

```C#
var taskController = container.Resolve<TaskController>();
```

There is a growing number of [application integration libraries](https://autofac.readthedocs.io/en/latest/integration/index.html) that make using Autofac with your application a snap. Support for several popular frameworks is also available through the "Extras" packages.

**[Intrigued? Check out our Getting Started walkthrough!](https://autofac.readthedocs.io/en/latest/getting-started/index.html)**

## Project

Autofac is licensed under the MIT license, so you can comfortably use it in commercial applications (we still love [contributions](https://autofac.readthedocs.io/en/latest/contributors.html) though).

**File issues in the repo with the associated feature/code.**

- [Autofac](https://github.com/autofac/Autofac) - Core dependency resolution and common functions (this repo).
- [Autofac.AspNetCore.Multitenant](https://github.com/autofac/Autofac.AspNetCore.Multitenant) - Multitenant DI support for ASP.NET Core applications.
- [Autofac.Configuration](https://github.com/autofac/Autofac.Configuration) - JSON/XML file-based configuration support.
- [Autofac.Extensions.DependencyInjection](https://github.com/autofac/Autofac.Extensions.DependencyInjection) - .NET Core integration for Autofac.
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
- [Autofac.Multitenant](https://github.com/autofac/Autofac.Multitenant) - Multitenant dependency resolution support.
- [Autofac.Multitenant.Wcf](https://github.com/autofac/Autofac.Multitenant.Wcf) - Multitenant WCF service hosting.
- [Autofac.Mvc](https://github.com/autofac/Autofac.Mvc) - ASP.NET MVC integration.
- [Autofac.Mvc.Owin](https://github.com/autofac/Autofac.Mvc.Owin) - OWIN support for ASP.NET MVC.
- [Autofac.Owin](https://github.com/autofac/Autofac.Owin) - Core OWIN support - shared middleware for request lifetime integration.
- [Autofac.ServiceFabric](https://github.com/autofac/Autofac.ServiceFabric) - Application integration for Service Fabric services.
- [Autofac.SignalR](https://github.com/autofac/Autofac.SignalR) - Application integration for SignalR.
- [Autofac.Wcf](https://github.com/autofac/Autofac.Wcf) - WCF service hosting.
- [Autofac.Web](https://github.com/autofac/Autofac.Web) - ASP.NET web forms integration.
- [Autofac.WebApi](https://github.com/autofac/Autofac.WebApi) - Application integration for Web API.
- [Autofac.WebApi.Owin](https://github.com/autofac/Autofac.WebApi.Owin) - OWIN support for Web API.

## Contributing / Pull Requests

Refer to the [Contributor Guide](https://github.com/autofac/Autofac/blob/master/CONTRIBUTING.md)
for setting up and building Autofac source.


