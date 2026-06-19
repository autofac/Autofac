// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac;

// Native AOT smoke test.
//
// Exercises the surface of Autofac that is documented as AOT/trim-safe: registering
// concrete types, resolving by generic type and by System.Type, constructor injection,
// property injection, parameterized resolution with TypedParameter, and child scopes.
//
// Every check throws on failure; the process returns a non-zero exit code so a CI step
// can assert success simply by checking the exit code of the published native binary.
//
// This deliberately does NOT touch the dynamic-code surface (open generics, collections,
// Lazy/Meta, generated factories, assembly scanning). Those are annotated
// [RequiresDynamicCode]/[RequiresUnreferencedCode] and are expected to warn at the call
// site under AOT; exercising them here would defeat the warning-free goal of this test.
var failures = new List<string>();

void Check(string name, Func<bool> assertion)
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

var builder = new ContainerBuilder();
builder.RegisterType<Dependency>().As<IDependency>();
builder.RegisterType<ConstructorConsumer>();
builder.RegisterType<PropertyConsumer>().PropertiesAutowired();
builder.RegisterType<ParameterizedConsumer>();
builder.RegisterType<KeyedDependency>().Keyed<IDependency>("special");

using var container = builder.Build();

// Resolve<T>() of an interface backed by a concrete implementation.
Check("Resolve<IDependency>", () => container.Resolve<IDependency>() is Dependency);

// Resolve(Type) - the closed-Type path the source generator depends on.
Check("Resolve(typeof(IDependency))", () => container.Resolve(typeof(IDependency)) is Dependency);

// Constructor injection of a dependency.
Check("Constructor injection", () =>
{
    var consumer = container.Resolve<ConstructorConsumer>();
    return consumer.Dependency is Dependency;
});

// Property injection (a key Autofac feature beyond MEDI).
Check("Property injection", () =>
{
    var consumer = container.Resolve<PropertyConsumer>();
    return consumer.Dependency is Dependency;
});

// Parameterized resolution with TypedParameter (used by the generated aggregate code).
Check("Resolve with TypedParameter", () =>
{
    var resolved = (ParameterizedConsumer)container.Resolve(
        typeof(ParameterizedConsumer),
        new[] { new TypedParameter(typeof(string), "configured") });
    return resolved.Value == "configured";
});

// Keyed resolution by Type.
Check("ResolveKeyed(Type)", () => container.ResolveKeyed("special", typeof(IDependency)) is KeyedDependency);

// Child lifetime scope resolution.
Check("Child scope resolution", () =>
{
    using var scope = container.BeginLifetimeScope();
    return scope.Resolve<IDependency>() is Dependency;
});

// Child scope with additional registrations.
Check("Child scope with registrations", () =>
{
    using var scope = container.BeginLifetimeScope(b => b.RegisterType<ScopedService>().AsSelf());
    return scope.Resolve<ScopedService>() is not null;
});

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
