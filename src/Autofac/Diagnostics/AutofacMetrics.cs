// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Autofac.Diagnostics;

/// <summary>
/// Centralized metrics wiring for Autofac diagnostics.
/// </summary>
[ExcludeFromCodeCoverage]
internal static class AutofacMetrics
{
    private const string DiagnosticsEnvironmentVariable = "AUTOFAC_METRICS";

    static AutofacMetrics()
    {
        MetricsEnabled = IsEnabled(DiagnosticsEnvironmentVariable);
        if (!MetricsEnabled)
        {
            return;
        }

        DiagnosticsMeter = new Meter("autofac", "1.0.0");

        CollectionBuildDuration = DiagnosticsMeter.CreateHistogram<double>(
            name: "autofac.collection.build.duration",
            unit: "s",
            description: "Time spent materializing implicit collection services.");
        CollectionBuildCount = DiagnosticsMeter.CreateCounter<long>(
            name: "autofac.collection.build.count",
            description: "Number of implicit collection builds performed.");
        CollectionItemCount = DiagnosticsMeter.CreateCounter<long>(
            name: "autofac.collection.build.items",
            description: "Total number of elements added to implicit collections.");

        LockContentionDuration = DiagnosticsMeter.CreateHistogram<double>(
            name: "autofac.lock.contention.duration",
            unit: "s",
            description: "Time threads waited to enter Autofac locks, broken down by category/detail.");
        LockContentionCount = DiagnosticsMeter.CreateCounter<long>(
            name: "autofac.lock.contention.count",
            description: "Number of lock contention events observed in Autofac.");
        LockContentionTotalTime = DiagnosticsMeter.CreateCounter<double>(
            name: "autofac.lock.contention.total_time",
            unit: "s",
            description: "Total time spent waiting on Autofac locks.");

        PropertyInjectionDuration = DiagnosticsMeter.CreateHistogram<double>(
            name: "autofac.property.injection.duration",
            unit: "s",
            description: "Time spent performing property injection.");
        PropertyInjectionCount = DiagnosticsMeter.CreateCounter<long>(
            name: "autofac.property.injection.count",
            description: "Number of instances that had property injection applied.");
        PropertyInjectionAssignments = DiagnosticsMeter.CreateCounter<long>(
            name: "autofac.property.injection.assignments",
            description: "Number of individual property assignments performed.");

        ReflectionActivationDuration = DiagnosticsMeter.CreateHistogram<double>(
            name: "autofac.reflection.activation.duration",
            unit: "s",
            description: "Time spent activating components via ReflectionActivator.");

        MiddlewareExecutionDuration = DiagnosticsMeter.CreateHistogram<double>(
            name: "autofac.middleware.duration",
            unit: "s",
            description: "Time spent executing Autofac resolve pipeline middleware.");
        MiddlewareExecutionCount = DiagnosticsMeter.CreateCounter<long>(
            name: "autofac.middleware.count",
            description: "Number of resolve pipeline middleware executions.");
    }

    /// <summary>
    /// Gets a value indicating whether diagnostics metrics are enabled.
    /// </summary>
    public static bool MetricsEnabled
    {
        get;
    }

    /// <summary>
    /// Gets the underlying diagnostics meter.
    /// </summary>
    public static Meter? DiagnosticsMeter
    {
        get;
    }

    /// <summary>
    /// Gets the histogram tracking lock contention duration.
    /// </summary>
    public static Histogram<double>? LockContentionDuration
    {
        get;
    }

    /// <summary>
    /// Gets the counter tracking the number of lock contention events.
    /// </summary>
    public static Counter<long>? LockContentionCount
    {
        get;
    }

    /// <summary>
    /// Gets the counter accumulating total lock wait time.
    /// </summary>
    public static Counter<double>? LockContentionTotalTime
    {
        get;
    }

    /// <summary>
    /// Gets the histogram tracking implicit collection build durations.
    /// </summary>
    public static Histogram<double>? CollectionBuildDuration
    {
        get;
    }

    /// <summary>
    /// Gets the counter measuring how many collections were materialized.
    /// </summary>
    public static Counter<long>? CollectionBuildCount
    {
        get;
    }

    /// <summary>
    /// Gets the counter measuring how many items were added across all collections.
    /// </summary>
    public static Counter<long>? CollectionItemCount
    {
        get;
    }

    /// <summary>
    /// Gets the histogram tracking property injection durations.
    /// </summary>
    public static Histogram<double>? PropertyInjectionDuration
    {
        get;
    }

    /// <summary>
    /// Gets the counter measuring how many instances had property injection.
    /// </summary>
    public static Counter<long>? PropertyInjectionCount
    {
        get;
    }

    /// <summary>
    /// Gets the counter tracking the number of property assignments performed.
    /// </summary>
    public static Counter<long>? PropertyInjectionAssignments
    {
        get;
    }

    /// <summary>
    /// Gets the histogram tracking reflection activator durations.
    /// </summary>
    public static Histogram<double>? ReflectionActivationDuration
    {
        get;
    }

    /// <summary>
    /// Gets the histogram tracking middleware execution duration.
    /// </summary>
    public static Histogram<double>? MiddlewareExecutionDuration
    {
        get;
    }

    /// <summary>
    /// Gets the counter tracking how many middleware executions occurred.
    /// </summary>
    public static Counter<long>? MiddlewareExecutionCount
    {
        get;
    }

    /// <summary>
    /// Records a collection build event.
    /// </summary>
    /// <param name="kind">The kind of collection (e.g., standard, any-keyed).</param>
    /// <param name="detail">Additional detail such as the service description.</param>
    /// <param name="itemCount">The number of elements added to the collection.</param>
    /// <param name="elapsed">The elapsed time for the build.</param>
    public static void RecordCollectionBuild(string kind, string? detail, int itemCount, TimeSpan elapsed)
    {
        if (!MetricsEnabled || CollectionBuildDuration is null || elapsed <= TimeSpan.Zero)
        {
            return;
        }

        var tags = new TagList
        {
            { "autofac.collection.kind", kind },
            { "autofac.collection.detail", detail ?? "<null>" },
        };

        CollectionBuildDuration.Record(elapsed.TotalSeconds, tags);
        CollectionBuildCount?.Add(1, tags);
        CollectionItemCount?.Add(itemCount, tags);
    }

    /// <summary>
    /// Records a property injection event.
    /// </summary>
    /// <param name="instanceType">The concrete instance type.</param>
    /// <param name="inspectedProperties">The number of properties evaluated for injection.</param>
    /// <param name="assignedProperties">The number of properties that were assigned.</param>
    /// <param name="elapsed">The elapsed time for the injection.</param>
    public static void RecordPropertyInjection(Type instanceType, int inspectedProperties, int assignedProperties, TimeSpan elapsed)
    {
        if (!MetricsEnabled || PropertyInjectionDuration is null || elapsed <= TimeSpan.Zero)
        {
            return;
        }

        var tags = new TagList
        {
            { "autofac.property.type", instanceType.FullName ?? instanceType.Name },
            { "autofac.property.inspected", inspectedProperties },
        };

        PropertyInjectionDuration.Record(elapsed.TotalSeconds, tags);
        PropertyInjectionCount?.Add(1, tags);
        PropertyInjectionAssignments?.Add(assignedProperties, tags);
    }

    /// <summary>
    /// Records a reflection-based activation event.
    /// </summary>
    /// <param name="implementationType">The activated implementation type.</param>
    /// <param name="elapsed">The elapsed time for activation.</param>
    public static void RecordReflectionActivation(Type implementationType, TimeSpan elapsed)
    {
        if (!MetricsEnabled || ReflectionActivationDuration is null || elapsed <= TimeSpan.Zero)
        {
            return;
        }

        var tags = new TagList
        {
            { "autofac.reflection.type", implementationType.FullName ?? implementationType.Name },
        };

        ReflectionActivationDuration.Record(elapsed.TotalSeconds, tags);
    }

    /// <summary>
    /// Records the wait duration for a lock contention event. This may include
    /// time spent waiting as well as acquiring the lock, but will be closely
    /// correlated with contention time.
    /// </summary>
    /// <param name="category">The lock category (e.g., service or lifetime scope).</param>
    /// <param name="detail">
    /// Additional details about the lock, if any. Note this will likely
    /// generate high-cardinality metrics in a production environment since it
    /// will track information about services and lifetime scopes acquiring
    /// locks and include identities for each.
    /// </param>
    /// <param name="elapsed">The time spent waiting.</param>
    public static void RecordLockContention(string category, string? detail, TimeSpan elapsed)
    {
        if (!MetricsEnabled || LockContentionDuration is null || elapsed <= TimeSpan.Zero)
        {
            return;
        }

        var tags = new TagList
        {
            { "autofac.lock.category", category },
            { "autofac.lock.detail", detail ?? "<null>" },
        };

        LockContentionDuration.Record(elapsed.TotalSeconds, tags);
        LockContentionCount?.Add(1, tags);
        LockContentionTotalTime?.Add(elapsed.TotalSeconds, tags);
    }

    /// <summary>
    /// Records a resolve pipeline middleware execution event.
    /// </summary>
    /// <param name="middlewareName">The middleware name.</param>
    /// <param name="elapsed">The elapsed time for the execution.</param>
    public static void RecordMiddlewareExecution(string middlewareName, TimeSpan elapsed)
    {
        if (!MetricsEnabled || MiddlewareExecutionDuration is null || elapsed <= TimeSpan.Zero)
        {
            return;
        }

        var tags = new TagList
        {
            { "autofac.middleware.name", middlewareName },
        };

        MiddlewareExecutionDuration.Record(elapsed.TotalSeconds, tags);
        MiddlewareExecutionCount?.Add(1, tags);
    }

    private static bool IsEnabled(string variableName)
    {
        var envValue = Environment.GetEnvironmentVariable(variableName);
        return string.Equals(envValue, "1", StringComparison.OrdinalIgnoreCase)
            || string.Equals(envValue, "true", StringComparison.OrdinalIgnoreCase);
    }
}
