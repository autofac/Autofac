#Contributor Guide

Contributions to Autofac, whether new features or bug fixes, are deeply appreciated and benefit the whole user community.

The following guidelines help ensure the smooth running of the project, and keep a consistent standard across the codebase. They are guidelines only - should you feel a need to deviate from them it is probably for a good reason - but please adhere to them as closely as possible.

If you'd like to contribute code or documentation to Autofac, we welcome pull requests. [Questions and suggestions are welcome on the newsgroup.](https://groups.google.com/forum/#!forum/autofac>).

**Your contributions must be your own work and licensed under the same terms as Autofac.**

##Process

**When working through contributions, please file issues and submit pull requests in the repository containing the code in question.** For example, if the issue is with the Autofac MVC integration, file it in that repo rather than the core Autofac repo.

- **File an issue.** Either suggest a feature or note a defect. If it's a feature, explain the challenge you're facing and how you think the feature should work. If it's a defect, include a description and reproduction (ideally one or more failing unit tests).
- **Design discussion.** For new features, some discussion on the issue will take place to determine if it's something that should be included with Autofac or be a user-supplied extension. For defects, discussion may happen around whether the issue is truly a defect or if the behavior is correct.
- **Pull request.** Create [a pull request](https://help.github.com/articles/using-pull-requests/) on the `develop` branch of the repository to submit changes to the code based on the information in the issue. Pull requests need to pass the CI build and follow coding standards. See below for more on coding standards in use with Autofac. Note all pull requests should include accompanying unit tests to verify the work.
- **Code review.** Some iteration may take place requiring updates to the pull request (e.g., to fix a typo or add error handling).
- **Pull request acceptance.** The pull request will be accepted into the `develop` branch and pushed to `master` with the next release.

##License

By contributing to Autofac, you assert that:

1. The contribution is your own original work.
2. You have the right to assign the *copyright* for the work (it is not owned by your employer, or you have been given copyright assignment in writing).
3. You license it under the terms applied to the rest of the Autofac project.

##Coding

###Workflow

Autofac and the associated integration libraries follow the [Gitflow workflow process](https://www.atlassian.com/git/tutorials/comparing-workflows/gitflow-workflow/) for handling releases. This means active development is done on the `develop` branch and we push to `master` when it's release time. **If you're creating a pull request or contribution, please do it on the `develop` branch.** We can then build, push to MyGet for testing, and release to NuGet when everything's verified.

###Developer Environment

- Visual Studio 2015 (with latest patches/updates).
- PowerShell 4+

###Dependencies

The core Autofac assemblies depend _only_ on the .NET Base Class Libraries (BCL). `Autofac.dll` proper is a Portable Class Library so only depends on a subset of that BCL functionality. This is a conscious decision to keep the project lightweight and easier to maintain.

For core integration assemblies (`Autofac.Integration.*`) the latest version of Autofac relies on the latest version of the integration target. For example, `Autofac.Integration.Mvc` always relies on the latest ASP.NET MVC libraries. This also helps keep the project easier to maintain.

The `Autofac.Extras` features include assemblies that depend on other Open Source (OSS) libraries. It is important when including new dependencies that:

- The project can be built straight out of Git (no additional installation needs to take place on the developer's machine). This means NuGet package references and/or checking in dependencies.
- Any third-party libraries have licenses compatible with Autofac's (the GPL and licenses like it are incompatible - please ask on the discussion forum if you're unsure).

Unit tests are written in XUnit.

###Build / Test

Project codelines with scripted builds generally have a `build.ps1` script. This Powershell script will build, package, and execute tests.

Some project codelines rely on convention-based builds so do not have a specific script. In these cases you will not see a `.ps1` or `.proj` file to execute. In these cases...

- The build is executed by running it in Visual Studio or by executing `msbuild Solution.sln` on the solution in the codeline root.
- Unit tests can be run from the Visual Studio test explorer or by manually executing the command-line unit test runner from the `packages` folder against the built unit test assembly.

###Code Documentation

It is *strongly* encouraged that you update the Autofac documentation when making changes. If your changes impact existing features, the documentation may be updated regardless of whether a binary distribution has been made that includes the changes. [This can also be done through pull request.](https://github.com/autofac/Documentation)

You should also include XML API comments in the code. These are used to generate API documentation as well as for IntelliSense.

**The Golden Rule of Documentation: Write the documentation you'd want to read.** Every developer has seen self explanatory docs and wondered why there wasn't more information. (Parameter: "index." Documentation: "The index.") Please write the documentation you'd want to read if you were a developer first trying to understand how to make use of a feature.

###Coding Standards

Normal .NET coding guidelines apply. See the [Framework Design Guidelines](http://msdn.microsoft.com/en-us/library/ms229042.aspx>) for suggestions. If you have access to ReSharper, code should be 'green' - that is, have no ReSharper warnings or errors with the default settings.

Autofac source code uses four spaces for indents (rather than tabs).

[If you have the EditorConfig add-in for your editor of choice (Visual Studio, Sublime Text, etc.)](http://editorconfig.org/), there are `.editorconfig` settings in the various repositories to help make your life easier.

##Autofac.Extras

Autofac.Extras are companion libraries that get distributed alongside the main Autofac distribution. The Extras are distinguished by:

- Experimental features
- Integrations with other Open Source projects
- Alternatives to the 'typical' way of doing something in the core (e.g. a different configuration syntax)

In many cases, Autofac.Extras is a way of testing alternatives and getting visibility for new ideas that could eventually end up in the core.

If your contribution is accepted to Autofac.Extras it is unlikely that the rest of the project team will have the knowledge to maintain it, so please expect to have bug reports assigned to you for the area (which you may subsequently reassign if you're unable to action them).