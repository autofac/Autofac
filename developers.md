# Readme for Autofac Developers

This document explains the developer setup and build execution for Autofac.

## Developer Environment

 - Visual Studio 2013 Premium/Ultimate. (Include the *Windows Phone 8.0 SDK*
   feature when installing.) This will give you:

   - .NET 4.5
   - WCF RIA Services
   - Portable Class Library tooling
   - FxCop
   - SQL Server Express

 - **All** of the latest .NET, VS, and SQL patches through Microsoft Update.
 - **All** of the latest VS updates (stable/RTM, not RC) through VS Extension
   Manager.
 - [NUnit Test Adapter for VS11](http://nunit.org/index.php?p=vsTestAdapter&amp;r=2.6.3)
   (optional - to run unit tests inside Visual Studio)

## Building the Project

Developer build:

`msbuild default.proj`

Production/Release build:

`msbuild default.proj /p:Production=true`

The **developer build** will...

 - Clean all build artifacts.
 - Build the solution.
 - Execute the unit tests.
 - Run code analysis.

The **production/release build** will do everything in the developer build
*plus*...

 - Create zip packages for distribution.
 - Create NuGet packages for distribution.
 - Build the compiled API help documentation.

**Note for developers:** If you are working on the Autofac core, there is
also a project in `Core/Tests/Autofac.Tests.AppCert` that should be built/run
separately to verify changes will pass Windows App Store certification. This
build is not chained into the standard developer build since it takes time to
run. [There is a readme in that folder explaining more about how to run that
build and assess results](https://github.com/autofac/Autofac/blob/master/Core/Tests/Autofac.Tests.AppCert/readme.html).
Production package versions are centrally controlled through the
`PackageVersions.proj`. Documentation in that file explains how to use it.
Before releasing new versions for consumption, be sure to update the
appropriate version(s).

## Updating the API Documentation Site

The API docs are viewable at [http://api.autofac.org](http://api.autofac.org).
This is hosted on GitHub pages in the
[https://github.com/autofac/autofac.github.com](https://github.com/autofac/autofac.github.com)
repository.

 1. Build the API documentation.
 2. Update the contents in the `/apidoc` folder with the new docs (add/remove/update).
 3. Make sure the index page in the `/apidoc` is `index.html` - lower case,
    full `html` extension. (By default, Sandcastle makes it `Index.htm` which
    doesn't work.)
