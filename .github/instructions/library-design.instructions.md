---
name: library-design
description: >-
  How Autofac weighs public API changes, configuration options, platform
  reach, performance, and dependencies. Applies to every code change in a
  library that many applications already depend on.
applyTo: "**"
---

# Library Design

Applications we will never see depend on every public member, every default, and every behavior Autofac has shipped, including the accidental ones. Each decision below protects those users and the small group of maintainers who support them.

## The Public API Is Permanent

- Never remove, rename, retype, or move a public member, and never change its meaning. Removal takes a deprecation cycle: mark it `[Obsolete]` with guidance, keep it working, and remove it only in a major release.
- Source compatibility is not enough. The assemblies are strong-named, so a change that still compiles can still break an application at run time that was built against the previous version. Each of these breaks binary compatibility: adding an optional parameter to an existing method, changing a return type from `void`, moving an extension method to another static class, and changing a namespace.
- Adding a member to a public interface, or an abstract member to a public abstract class, breaks every implementer.
- A behavior change on an unchanged signature is also a breaking change. A different exception type, ordering, lifetime, or default breaks people as surely as a signature change does.
- Adding public API is a long-term commitment, not a convenience. Every new type, overload, or parameter needs tests, XML documentation, an entry in the documentation site, and support questions answered for years. Before adding one, show that the goal cannot be met with what is already public.
- Keep implementation details `internal`. Tests reach them through `InternalsVisibleTo`, which leaves room to refactor without breaking anyone.

When a change has to break compatibility, say so plainly in the pull request, and say why the break is worth it. Maintainers decide when a major version ships. Do not bump the version yourself.

## Make It Work, Don't Make It Optional

A new option, flag, or "opt-in" setting is usually a design decision that was put off. It moves the decision to every user, who knows less about the trade-off than the person writing the change, and it doubles the behavior that must be tested and supported.

- Find the design that is right for small and large applications alike. If a change helps large containers but costs small ones, keep looking for one that costs nobody, or show the cost is negligible.
- Do not gate a fix or an improvement behind a switch. If it is correct, it should be on for everyone. If it is not safe for everyone, it is not ready.
- When a setting genuinely cannot be avoided, the pull request must explain why no default works and what happens to people who never touch it.

## Run Everywhere Autofac Runs

Autofac targets old and new frameworks on purpose, so that taking an upgrade never forces anything else on an application.

- Respect each project's target frameworks. Core targets `netstandard2.0` through the newest .NET, and the classic integrations target .NET Framework. Code that compiles for only the newest target, or that behaves differently on an older one without a reason, is a defect.
- Core must stay compatible with trimming and Native AOT, and keep working on Mono and full AOT runtimes. Reflection or dynamic code generation needs an AOT-safe path or the correct trimming annotations.
- Never raise a minimum target framework or SDK requirement without a technical need for it.

## Performance Is a Trade-Off

Container code runs on every resolve in many applications, so allocations and time on the hot path matter. That is not a license for code nobody can maintain.

- Show the gain. A performance change comes with a BenchmarkDotNet run, in the repository's `bench` project if it has one, that compares against the current code and reports time and allocations.
- Weigh the gain against what it costs a maintainer to understand, debug, and safely change the code later. A small win that makes a core path hard to reason about is a loss.
- Optimize where the cost actually is: resolve paths, scope creation, and container build. Code that runs once per application gets clarity first.
- Do not trade correctness, thread safety, or platform reach for speed. Caches need a bound and a lifetime, and lock-free code needs to be correct under contention, not just fast without it.

## Dependencies Stay Minimal

- Core takes no third-party dependencies beyond the .NET base libraries.
- Do not raise a package reference's version without a technical reason: a needed API, a security fix, or a compatibility break. Every upgrade we take is one every consumer is forced to take. That includes integrations raising their minimum Autofac version.
- Any new dependency needs a license compatible with MIT, and the pull request must say why it is worth adding.
