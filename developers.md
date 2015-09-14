# Readme for Autofac Developers

This document explains the developer setup and build execution for Autofac.

## Where to Work / Process

Autofac follows the [Gitflow workflow process](https://www.atlassian.com/git/tutorials/comparing-workflows/gitflow-workflow/) for handling releases. This means active development is done on the `develop` branch and we push to `master` when it's release time. **If you're creating a pull request or contribution, please do it on the `develop` branch.** We can then build, push to MyGet for testing, and release to NuGet when everything's verified.

## Developer Environment

 - Visual Studio 2015 Premium/Ultimate.
 - **All** of the latest .NET, VS, and SQL patches through Microsoft Update.
 - **All** of the latest VS updates (stable/RTM, not RC) through VS Extension
   Manager.

## Building the Project

At a PowerShell prompt run `build.ps1`.

This will build everything in a release configuration and create NuGet packages. It will also run tests and code analysis.

**Note:** If you are working on the Autofac core, there is
also a project in `test/Autofac.Tests.AppCert` that should be built/run
separately to verify changes will pass Windows App Store certification. This
build is not chained into the standard developer build since it takes time to
run. [There is a readme in that folder explaining more about how to run that
build and assess results](https://github.com/autofac/Autofac/blob/master/test/Autofac.Tests.AppCert/readme.html).

Production package versions are controlled through the build and the `project.json` files.

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
