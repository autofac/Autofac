# Autofac Contributor Guide

Contributions to Autofac, whether new features or bug fixes, are deeply appreciated and benefit the whole user community.

The following guidelines help ensure the smooth running of the project, and keep a consistent standard across the codebase. They are guidelines only - should you feel a need to deviate from them it is probably for a good reason - but please adhere to them as closely as possible.

If you'd like to contribute code or documentation to Autofac, we welcome pull requests. [Questions and suggestions are welcome on the newsgroup.](https://groups.google.com/forum/#!forum/autofac).

**Your contributions must be your own work and licensed under the same terms as Autofac.**

## Code of Conduct

The Autofac Code of Conduct [is posted on GitHub](CODE_OF_CONDUCT.md). It is expected that all contributors follow the code of conduct.

## Process

**When working through contributions, please file issues and submit pull requests in the repository containing the code in question.** For example, if the issue is with the Autofac MVC integration, file it in that repo rather than the core Autofac repo.

- **File an issue.** Either suggest a feature or note a defect. If it's a feature, explain the challenge you're facing and how you think the feature should work. If it's a defect, include a description and reproduction (ideally one or more failing unit tests).
- **Design discussion.** For new features, some discussion on the issue will take place to determine if it's something that should be included with Autofac or be a user-supplied extension. For defects, discussion may happen around whether the issue is truly a defect or if the behavior is correct.
- **Pull request.** Create [a pull request](https://help.github.com/articles/using-pull-requests/) on the `develop` branch of the repository to submit changes to the code based on the information in the issue. Pull requests need to pass the CI build and follow coding standards. See below for more on coding standards in use with Autofac. Note all pull requests should include accompanying unit tests to verify the work.
- **Code review.** Some iteration may take place requiring updates to the pull request (e.g., to fix a typo or add error handling).
- **Pull request acceptance.** The pull request will be accepted into the `develop` branch and pushed to `master` with the next release.

## License

By contributing to Autofac, you assert that:

1. The contribution is your own original work.
2. You have the right to assign the *copyright* for the work (it is not owned by your employer, or you have been given copyright assignment in writing).
3. You license it under the terms applied to the rest of the Autofac project.

## Coding

### Workflow

Autofac and the associated integration libraries follow the [Gitflow workflow process](https://www.atlassian.com/git/tutorials/comparing-workflows/gitflow-workflow/) for handling releases. This means active development is done on the `develop` branch and we push to `master` when it's release time. **If you're creating a pull request or contribution, please do it on the `develop` branch.** We can then build, push to MyGet for testing, and release to NuGet when everything's verified.

We use [semantic versioning](https://semver.org/) for our package versions.

### Developer Environment

**Windows**:

- Visual Studio 2019 or VS Code
- .NET Core SDK (each repo has a `global.json` with the version required)
- PowerShell 5+ / PowerShell Core

**Mac**:

- VS Code
- .NET Core SDK (each repo has a `global.json` with the version required)
- PowerShell 5+ / PowerShell Core
- Mono - install the latest "Visual Studio channel" version; the standalone version or the one from Homebrew won't work.

### Build / Test

Project codelines with scripted builds generally have a `build.ps1` script. This Powershell script will build, package, and execute tests.

Some project codelines rely on convention-based builds so do not have a specific script. In these cases you will not see a `.ps1` or `.proj` file to execute. In these cases...

- The build is executed by running it in Visual Studio or by executing `dotnet build Autofac.sln` on the solution in the codeline root.
- Unit tests can be run from the Visual Studio test explorer or by manually executing the command-line unit test runner from the `packages` folder against the built unit test assembly.

Unit tests are written in XUnit and Moq. **Code contributions should include tests that exercise/demonstrate the contribution.**

**Everything should build and test with zero errors and zero warnings.**

### Coding Standards

Normal .NET coding guidelines apply. See the [Framework Design Guidelines](https://msdn.microsoft.com/en-us/library/ms229042.aspx) for suggestions. We have Roslyn analyzers running on most of the code. These analyzers are actually correct a majority of the time. Please try to fix warnings rather than suppressing the message. If you do need to suppress a false positive, use the `[SuppressMessage]` attribute.

Autofac source code uses four spaces for indents. We use [EditorConfig](https://editorconfig.org/) to ensure consistent formatting in code docs. Visual Studio has this built in since VS 2017. VS Code requires the EditorConfig extension. Many other editors also support EditorConfig.

### Public API and Breaking Changes

Part of the responsibility of working on a widely used package is that you must strive to avoid breaking changes in the public API. A breaking change can be a lot of things:

- Change a type's namespace.
- Remove or rename a method on a class or interface.
- Move an extension method from one static class to a different static class.
- Add an optional parameter to an existing method.
- Add a new abstract method to an existing abstract class.
- Add a new member on an interface.

You have to be careful if you change the public API. Adding a new method to a class is OK... unless it's an abstract class and someone consuming it is now required to implement it.

You'll notice a lot of Autofac is internal and the unit test fixtures have internals visible. This allows for more opportunity to refactor the inner workings of Autofac and its integrations without incurring breaking changes on consumers.

**Adding to the public API is something to seriously consider.** If you're contributing something that expands on the public API, you need to consider that once it's out there, we can't pull it out without running it through a lifecycle - marking it obsolete, making a major version release, providing support for folks who had taken it and still need that feature. Even if it's just one more overload for an existing method, consider if it's really necessary or if the task at hand can be accomplished by something that's already publicly exposed.

### Dependencies and Upgrades

**All Autofac packages should strive to be as long-term compatible as possible with things and not require downstream consumers to take upgrades.**

Core Autofac attempts to be as compatible as possible to allow as many clients to use it as it can. No third-party assemblies outside the base .NET/.NET Core framework are allowed. No upgrades to base requirements are allowed unless there's a technical reason. Taking an upgrade to Autofac core should generally not require you to take any other upgrades in your application.

Integration packages should do their best to balance the need for upgrades with the need for functionality. Generally speaking:

- Unless there's a technical need to take an upgrade to an integration package dependency, **don't**. That includes Autofac - don't require an upgrade to an integration package to force an upgrade to the core Autofac version.
- Integration packages really only need to be compatible with the latest version of the framework with which they integration. For example, the Autofac ASP.NET MVC integration _may_ require use of the latest ASP.NET MVC bits. There is no requirement to maintain backwards compatibility with every old version of ASP.NET MVC and no requirement to fork and maintain multiple branches of the integration in order to support all the ASP.NET MVC versions.

Again, the goal is to be as compatible with as many things for as long as possible.

If you need to take an update to a dependency for a technical or security reason, do it. It's not a bad thing. Just be aware that if you take an upgrade, anyone taking the latest version of the package you're working on will also be forced to take that upgrade and they may not be ready.

Additional considerations:

- Projects should be able to be built straight out of Git (no additional installation needs to take place on the developer's machine). This means NuGet package references, not installation of required components.
- Any third-party libraries consumed by Autofac integration must have licenses compatible with Autofac's (the GPL and licenses like it are incompatible - please ask on the discussion forum if you're unsure).

### Code Documentation and Examples

It is *strongly* encouraged that you update the Autofac documentation when making changes. If your changes impact existing features, the documentation may be updated regardless of whether a binary distribution has been made that includes the changes. [This can also be done through pull request.](https://github.com/autofac/Documentation)

You should also include XML API comments in the code. These are used to generate API documentation as well as for IntelliSense.

**The Golden Rule of Documentation: Write the documentation you'd want to read.** Every developer has seen self explanatory docs and wondered why there wasn't more information. (Parameter: "index." Documentation: "The index.") Please write the documentation you'd want to read if you were a developer first trying to understand how to make use of a feature.

For new integrations or changes to existing integrations, you may need to add or update [the examples repo](https://github.com/autofac/Examples) to show how the integration works.
