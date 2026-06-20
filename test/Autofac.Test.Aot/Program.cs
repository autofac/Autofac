// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Autofac;
using Autofac.Features.Metadata;
using Autofac.Features.OwnedInstances;

/// <summary>
/// Native AOT smoke test.
/// </summary>
/// <remarks>
/// <para>
/// Validates two things that the static <c>IsAotCompatible</c> build gate cannot:
/// </para>
/// <list type="number">
/// <item>
/// The AOT-safe surface actually WORKS at runtime under Native AOT - i.e. we did
/// not over-suppress and break a path that should function. These are the
/// <c>Check</c> assertions.
/// </item>
/// <item>
/// The features that genuinely require dynamic code FAIL at runtime in the expected
/// way (a <c>DependencyResolutionException</c>), proving the boundary is real and
/// that we did not accidentally make an unsupported scenario "work" by luck. These
/// are the <c>CheckThrows</c> assertions.
/// </item>
/// </list>
/// <para>
/// Empirically (net10.0 / osx-arm64) the dividing line is runtime code generation,
/// NOT value types in general: closing an OPEN GENERIC over a value type needs
/// MakeGenericType and fails, but resolving <c>IEnumerable&lt;valueType&gt;</c>
/// succeeds because no new type is constructed. Reference-typed relationships all work.
/// </para>
/// </remarks>
internal static class Program
{
    [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "A smoke test that exercises the breadth of the resolve surface is necessarily coupled to many types.")]
    public static int Main()
    {
        var failures = new List<string>();

        var builder = new ContainerBuilder();
        builder.RegisterType<Dependency>().As<IDependency>().SingleInstance();
        builder.RegisterType<ConstructorConsumer>();
        builder.RegisterType<PropertyConsumer>().PropertiesAutowired();
        builder.RegisterType<ParameterizedConsumer>();
        builder.RegisterType<KeyedDependency>().Keyed<IDependency>("special");
        builder.RegisterInstance(new ProvidedService()).AsSelf();
        builder.Register(c => new LambdaService(c.Resolve<IDependency>())).AsSelf();
        builder.RegisterType<DisposableService>().AsSelf().InstancePerDependency();
        builder.RegisterModule<SmokeTestModule>();
        RegisterOpenGeneric(builder);

        using var container = builder.Build();

        // --- Scenarios that MUST work under Native AOT ---

        // Resolve<T>() of an interface backed by a concrete implementation.
        Check(failures, "Resolve<IDependency>", () => container.Resolve<IDependency>() is Dependency);

        // Resolve(Type) - the closed-Type path the source generator depends on.
        Check(failures, "Resolve(typeof(IDependency))", () => container.Resolve(typeof(IDependency)) is Dependency);

        // Constructor injection of a dependency.
        Check(failures, "Constructor injection", () => container.Resolve<ConstructorConsumer>().Dependency is Dependency);

        // Public property injection (a key Autofac feature beyond MEDI).
        Check(failures, "Property injection", () => container.Resolve<PropertyConsumer>().Dependency is Dependency);

        // Parameterized resolution with TypedParameter (used by the generated aggregate code).
        Check(failures, "Resolve with TypedParameter", () =>
        {
            var resolved = (ParameterizedConsumer)container.Resolve(
                typeof(ParameterizedConsumer),
                new[] { new TypedParameter(typeof(string), "configured") });
            return resolved.Value == "configured";
        });

        // Keyed resolution by Type.
        Check(failures, "ResolveKeyed(Type)", () => container.ResolveKeyed("special", typeof(IDependency)) is KeyedDependency);

        // Pre-built instance registration (no reflection activation at all).
        Check(failures, "RegisterInstance", () => container.Resolve<ProvidedService>() is not null);

        // Lambda registration (the recommended AOT-friendly registration pattern).
        Check(failures, "Register(lambda)", () => container.Resolve<LambdaService>().Dependency is Dependency);

        // Module registration.
        Check(failures, "RegisterModule", () => container.Resolve<ModuleService>() is not null);

        // SingleInstance sharing returns the same instance.
        Check(failures, "SingleInstance sharing", () => ReferenceEquals(container.Resolve<IDependency>(), container.Resolve<IDependency>()));

        // Child lifetime scope resolution.
        Check(failures, "Child scope resolution", () =>
        {
            using var scope = container.BeginLifetimeScope();
            return scope.Resolve<IDependency>() is Dependency;
        });

        // Child scope with additional registrations.
        Check(failures, "Child scope with registrations", () =>
        {
            using var scope = container.BeginLifetimeScope(b => b.RegisterType<ScopedService>().AsSelf());
            return scope.Resolve<ScopedService>() is not null;
        });

        // IDisposable instances are disposed when their owning scope is disposed.
        Check(failures, "Disposal on scope dispose", () =>
        {
            DisposableService captured;
            using (var scope = container.BeginLifetimeScope())
            {
                captured = scope.Resolve<DisposableService>();
            }

            return captured.IsDisposed;
        });

        // Reference-typed relationships resolve under AOT (no runtime type construction).
        Check(failures, "Resolve IEnumerable<IDependency>", () => container.Resolve<IEnumerable<IDependency>>().Any());
        Check(failures, "Resolve Lazy<IDependency>", () => container.Resolve<Lazy<IDependency>>().Value is Dependency);
        Check(failures, "Resolve Func<IDependency>", () => container.Resolve<Func<IDependency>>()() is Dependency);
        Check(failures, "Resolve Meta<IDependency>", () => container.Resolve<Meta<IDependency>>().Value is Dependency);
        Check(failures, "Resolve Owned<IDependency>", () =>
        {
            using var owned = container.Resolve<Owned<IDependency>>();
            return owned.Value is Dependency;
        });

        // An open generic closed over a REFERENCE type works (no value-type code generation).
        Check(failures, "Resolve IGenericHolder<IDependency> (ref close)", () => container.Resolve<IGenericHolder<IDependency>>() is GenericHolder<IDependency>);

        // --- Scenarios that MUST throw under Native AOT (dynamic-code boundary) ---

        // Closing an open generic over a VALUE type requires MakeGenericType over a
        // value type, which Native AOT cannot generate. This assertion is deliberately
        // runtime/environment sensitive: if a future runtime adds universal shared
        // generics for value types, this could start SUCCEEDING and fail the check.
        // That is by design - a failure here likely means "the AOT boundary moved"
        // (update this test and the AOT docs), not "Autofac is broken".
        CheckThrows(failures, "Resolve IGenericHolder<int> (value-type close)", () => container.Resolve<IGenericHolder<int>>());

        if (failures.Count > 0)
        {
            Console.Error.WriteLine($"\n{failures.Count} AOT smoke check(s) failed:");
            foreach (var failure in failures)
            {
                Console.Error.WriteLine($"  - {failure}");
            }

            return 1;
        }

        Console.WriteLine("\nAll AOT smoke checks passed.");
        return 0;
    }

    // RegisterGeneric is an opt-in dynamic-code API and intentionally raises IL3050 -
    // that warning is itself part of what this test validates (see the value-type-close
    // CheckThrows case). Suppress it here so the rest of the project keeps treating
    // trim/AOT warnings as build-breaking errors.
    [UnconditionalSuppressMessage("AOT", "IL3050:RequiresDynamicCode", Justification = "Deliberately exercises the open-generic dynamic-code boundary; the matching value-type-close resolution is asserted to throw at runtime.")]
    private static void RegisterOpenGeneric(ContainerBuilder builder)
    {
        builder.RegisterGeneric(typeof(GenericHolder<>)).As(typeof(IGenericHolder<>));
    }

    private static void Check(List<string> failures, string name, Func<bool> assertion)
    {
        try
        {
            if (!assertion())
            {
                failures.Add(name);
                Console.Error.WriteLine($"FAIL: {name}");
            }
            else
            {
                Console.WriteLine($"PASS: {name}");
            }
        }
        catch (Exception ex)
        {
            failures.Add($"{name} ({ex.GetType().Name}: {ex.Message})");
            Console.Error.WriteLine($"FAIL: {name} -> {ex}");
        }
    }

    // Asserts that an operation throws (the expected outcome for a dynamic-code-only
    // feature under AOT). A scenario that unexpectedly succeeds is itself a failure -
    // it means the AOT boundary moved and the docs/annotations may be stale.
    private static void CheckThrows(List<string> failures, string name, Action action)
    {
        try
        {
            action();
            failures.Add($"{name} (expected an exception under AOT but none was thrown)");
            Console.Error.WriteLine($"FAIL: {name} -> expected throw, but succeeded");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"PASS: {name} (threw {ex.GetType().Name} as expected)");
        }
    }
}
