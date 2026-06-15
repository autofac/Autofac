// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Autofac.Benchmarks;

/// <summary>
/// Measures the cost of building a container that registers many modules which do
/// not override <see cref="Module.AttachToComponentRegistration"/> or
/// <see cref="Module.AttachToRegistrationSource"/>.
/// </summary>
/// <remarks>
/// See https://github.com/autofac/Autofac/issues/1446. Prior to the fix, every module
/// unconditionally subscribed to the registry's <c>Registered</c> and
/// <c>RegistrationSourceAdded</c> events. Each subscription replays all existing
/// registrations, so build time and allocations grew quadratically with the number of
/// modules. Modules that do not override the hooks no longer subscribe, removing the
/// quadratic behavior. Compare against a baseline package version (for example
/// <c>--baseline-version 9.1.0</c>) to see the difference.
/// </remarks>
public class ModuleRegistrationBenchmark
{
    [Params(100, 1000)]
    public int ModuleCount
    {
        get; set;
    }

    [Benchmark]
    public void BuildContainerWithModules()
    {
        var builder = new ContainerBuilder();

        for (var i = 0; i < ModuleCount; i++)
        {
            builder.RegisterModule<NoHookModule>();
        }

        using var container = builder.Build();
        GC.KeepAlive(container);
    }

    // A module that overrides neither hook - the common case from issue #1446.
    private sealed class NoHookModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<Service>().As<IService>();
        }

        private interface IService
        {
        }

        private sealed class Service : IService
        {
        }
    }
}
