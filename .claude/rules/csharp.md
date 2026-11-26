---
paths:
  - "**/*.cs"
---

# C# in Autofac

These rules are the standard even where existing code disagrees. Much of the code predates them, so nearby code is not evidence of a convention. Where these rules and the analyzer configuration are silent, match the surrounding code, and say which you did.

## Source

- Start every file with the copyright header from `build/stylecop.json`. StyleCop reports a missing or altered one.
- Use file-scoped namespaces that follow the folder path.
- Name private and internal fields `_camelCase`, and constants and `static readonly` fields `PascalCase`. Locals and parameters are `camelCase` with no prefix.
- Put user-facing exception and diagnostic messages in the `.resx` file next to the class, such as `ContainerBuilderResources.resx` beside `ContainerBuilder.cs`, and format them with `string.Format(CultureInfo.CurrentCulture, ...)`. Do not inline message strings.
- Validate public arguments with `throw new ArgumentNullException(nameof(x))` and the like. `ArgumentNullException.ThrowIfNull` does not exist on `netstandard2.0`.
- Use the newest language features the project's `LangVersion` allows, but no API missing from one of its target frameworks unless it is behind `#if` with an equivalent fallback. Build every target before submitting.
- Use `ConfigureAwait(false)` on every `await` in library code. Never block on async work with `.Result` or `.Wait()`.
- Assume concurrent use. Containers and lifetime scopes are resolved from many threads at once, so shared state needs a clear synchronization story, and that story belongs in a comment.
- Do not add reflection, `Expression.Compile`, or `MakeGenericType` without an AOT-safe path, or without the `[RequiresUnreferencedCode]` and `[RequiresDynamicCode]` annotations the trimming analyzers ask for.
- Catch only exceptions the code can handle or must wrap. The resolve pipeline catches broadly to wrap activation failures in `DependencyResolutionException`, which is an existing boundary, not a pattern to copy elsewhere.

## Analyzer Warnings

Fix the warning instead of suppressing it. A warning that looks wrong usually keys on something cheap to change. Suppress only after showing that the rule does not apply here and no reasonable change satisfies it, and ask the maintainers first. Use `[SuppressMessage]` with a `Justification` on the narrowest member, never project-wide `NoWarn` or a `GlobalSuppressions.cs` file. Test projects may set `NoWarn` for `CS1591` (missing XML documentation).

## XML Documentation

Every public and protected member has XML documentation, because it becomes IntelliSense and the API reference.

- `<summary>` says what the member is for, in terms the name does not already give.
- Document each `<param>`, `<typeparam>`, and `<returns>`, and add `<exception>` for exceptions a caller can anticipate and handle.
- Use `<remarks>` for usage guidance, lifetime or threading behavior, and anything surprising.
- Use `<see cref="..."/>` to link types and members rather than naming them in plain text.

## Tests

- Use xUnit. Use NSubstitute for new mocks, but do not change an existing test project's mocking library. Prefer a real or hand-written implementation to a mock when it is simple to build.
- Name test projects after the project they test, with a `.Test` suffix.
- Autofac has two kinds of test. Choose by what the test exercises, and name it to match. Older tests mix the two and use other names. Leave them alone unless you are already rewriting them.
  - **Unit tests** check the contract of one type or member: argument validation, edge cases, and internal behavior. A test is a unit test even when it calls public API, such as checking that `Register` rejects a null delegate. Name the class after the type, `ContainerBuilderTests`, and each method `MethodName_StateUnderTest`, such as `Register_NullDelegate`. Constructor tests are `Ctor_StateUnderTest`.
  - **Specification tests** check how a feature behaves when an application uses it: register, build, resolve, and assert on what the application gets, through the public API only. Name the class after the feature, `KeyedServiceTests`, and each method after the behavior it pins down, as a PascalCase phrase with no underscores: `ResolveIEnumerableReturnsEveryRegistration`. Core keeps these in `Autofac.Specification.Test`. Repositories with a single test project keep them in its `Specification` folder.
- A bug reported from application code gets a specification test that reproduces the report. Specification tests are the contract, so changing what one asserts is a behavior change, and the pull request must say so.
- An integration that implements another framework's contract runs that framework's conformance suite, such as `DependencyInjectionSpecificationTests`, by inheriting it in the `Specification` folder.
- Keep tests in each class in alphabetical order.
- Make each test self-contained: build what it needs in the test body or through a static helper, not through shared fields or constructor setup.
- Every test asserts something. To show no exception is thrown, assert that `Record.Exception` returns `null`. To show an exception is thrown, use `Assert.Throws<T>`, never `try`/`catch`.
- Write one test per invalid argument rather than one parameterized test. Assert the exception type only, not its message or parameter name.
- Do not add Arrange, Act, and Assert comments.
- A bug fix includes a regression test that fails without the fix. Mention the issue number in a comment on the test.
