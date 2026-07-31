// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac.Core;

namespace Autofac.Benchmarks;

/// <summary>
/// Measures container build (registration) time for many <em>distinct</em> implementation
/// types that <em>share a base class</em> exposing many inherited writable properties.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="Autofac.Core.Activators.Reflection.ReflectionActivator"/> scans each
/// implementation type for <c>[ServiceKey]</c> on constructor parameters and properties.
/// Scanning <em>inherited</em> members from the derived type yields
/// <see cref="System.Reflection.PropertyInfo"/> objects whose <c>ReflectedType</c> is the
/// derived type, so a per-member attribute cache misses on every (derived type x inherited
/// member) pair - O(types x inherited members) of attribute reflection during registration.
/// Scanning declared-only members per type and recursing into the base type collapses that
/// to O(distinct declared members).
/// </para>
/// <para>
/// This benchmark exists because <see cref="ContainerBuildBenchmark"/> cannot detect that
/// cost: it registers a single sealed type with no properties and no base class, so there
/// are no inherited members to rescan, and the per-type memoization makes the scan a one-off
/// regardless of registration count. The two <c>TypeCount</c> values are the point of this
/// benchmark - a regression shows up as super-linear growth between them.
/// </para>
/// <para>
/// No <c>[ServiceKey]</c> attribute appears anywhere below, deliberately: the scan cost is
/// paid by every reflection-based registration whether or not the feature is used.
/// </para>
/// </remarks>
public class ContainerBuildInheritedMembersBenchmark
{
    private const int InheritedPropertyCount = 20;

    private List<Type> _implementationTypes = new();

    [Params(250, 1000)]
    public int TypeCount
    {
        get; set;
    }

    [GlobalSetup]
    public void Setup()
    {
        _implementationTypes = DistinctDerivedTypes(TypeCount);

        // Guard the fixture itself: the benchmark is only meaningful while the registered
        // types really are distinct and really do inherit a wide set of writable properties.
        if (_implementationTypes.Distinct().Count() != TypeCount)
        {
            throw new InvalidOperationException("Implementation types are not distinct.");
        }

        var writableInheritedProperties = typeof(WideBase).GetProperties().Count(property => property.CanWrite);
        if (writableInheritedProperties != InheritedPropertyCount)
        {
            throw new InvalidOperationException(
                $"Expected {InheritedPropertyCount} writable properties on {nameof(WideBase)} but found {writableInheritedProperties}.");
        }
    }

    [Benchmark]
    public void Build()
    {
        // The [ServiceKey] scan caches live in the process-wide ReflectionCacheSet.Shared and
        // are not cleared on container build (their usage includes resolution), so without
        // this reset only the first invocation would scan anything and BenchmarkDotNet's
        // steady-state measurement would report a warm no-op. Clearing here - rather than in
        // an [IterationSetup] - keeps the reset inside the measured region whatever
        // invocation count BenchmarkDotNet picks, and avoids attaching a job attribute that
        // would compose badly with the jobs Program.cs configures. The clear itself is
        // proportional to the entries this benchmark created, and is negligible next to the
        // scan it forces.
        ReflectionCacheSet.Shared.Clear();

        var builder = new ContainerBuilder();

        foreach (var implementationType in _implementationTypes)
        {
            builder.RegisterType(implementationType);
        }

        using var container = builder.Build();
        GC.KeepAlive(container);
    }

    /// <summary>
    /// Produces <paramref name="count"/> distinct closed <see cref="Derived{T}"/> types, all
    /// inheriting <see cref="WideBase"/>. The index is encoded as a fixed-depth nesting of
    /// <see cref="Wrap{TA, TB}"/> over two markers, which yields distinct type arguments
    /// without a hand-written type declaration per registration. Nesting depth stays at
    /// log2(count), so type names never get deep enough for name construction to skew the
    /// measurement.
    /// </summary>
    /// <param name="count">The number of distinct types to produce.</param>
    /// <returns>The distinct closed implementation types.</returns>
    private static List<Type> DistinctDerivedTypes(int count)
    {
        var bits = 1;
        while (1 << bits < count)
        {
            bits++;
        }

        var types = new List<Type>(count);

        for (var i = 0; i < count; i++)
        {
            var argument = typeof(Zero);

            for (var bit = 0; bit < bits; bit++)
            {
                var next = ((i >> bit) & 1) == 1 ? typeof(One) : typeof(Zero);
                argument = typeof(Wrap<,>).MakeGenericType(argument, next);
            }

            types.Add(typeof(Derived<>).MakeGenericType(argument));
        }

        return types;
    }

    /// <summary>
    /// Stands in for a framework or domain base class with many injectable properties; every
    /// registered type inherits these writable members.
    /// </summary>
    private class WideBase
    {
        public string? Property01
        {
            get; set;
        }

        public string? Property02
        {
            get; set;
        }

        public string? Property03
        {
            get; set;
        }

        public string? Property04
        {
            get; set;
        }

        public string? Property05
        {
            get; set;
        }

        public string? Property06
        {
            get; set;
        }

        public string? Property07
        {
            get; set;
        }

        public string? Property08
        {
            get; set;
        }

        public string? Property09
        {
            get; set;
        }

        public string? Property10
        {
            get; set;
        }

        public string? Property11
        {
            get; set;
        }

        public string? Property12
        {
            get; set;
        }

        public string? Property13
        {
            get; set;
        }

        public string? Property14
        {
            get; set;
        }

        public string? Property15
        {
            get; set;
        }

        public string? Property16
        {
            get; set;
        }

        public string? Property17
        {
            get; set;
        }

        public string? Property18
        {
            get; set;
        }

        public string? Property19
        {
            get; set;
        }

        public string? Property20
        {
            get; set;
        }
    }

    /// <summary>
    /// The registered implementation type. Declares no members of its own, so all of
    /// <see cref="WideBase"/>'s properties reach it by inheritance.
    /// </summary>
    /// <typeparam name="T">Marker type argument that makes each closed type distinct.</typeparam>
    private sealed class Derived<T> : WideBase
    {
    }

    private sealed class Zero
    {
    }

    private sealed class One
    {
    }

    /// <summary>
    /// Combines two marker types so an index can be encoded as a nesting of them.
    /// </summary>
    /// <typeparam name="TA">First marker.</typeparam>
    /// <typeparam name="TB">Second marker.</typeparam>
    private sealed class Wrap<TA, TB>
    {
    }
}
