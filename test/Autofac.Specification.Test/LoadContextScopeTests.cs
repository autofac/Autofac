// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections;
using System.Diagnostics.SymbolStore;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using Autofac.Core;
using Autofac.Features.ResolveAnything;

namespace Autofac.Specification.Test;

#if NET5_0_OR_GREATER

public class LoadContextScopeTests
{
    [Fact]
    public void CanLoadInstanceOfScanAssemblyAndUnloadIt()
    {
        using var rootContainer = new ContainerBuilder().Build();

        LoadAssemblyAndTest(
            rootContainer,
            out var loadContextRef,
            (builder, assembly) => builder.RegisterAssemblyTypes(assembly),
            (scope, loadContext, assembly) =>
            {
                var instance = scope.Resolve(assembly.GetType("A.Service1"));

                Assert.Contains(instance.GetType().Assembly, loadContext.Assemblies);
            });

        WaitForUnload(loadContextRef);
    }

    [Fact]
    public void CanLoadInstanceOfAssemblyAndUnloadItAfterActnars()
    {
        var builder = new ContainerBuilder();

        builder.RegisterSource(new AnyConcreteTypeNotAlreadyRegisteredSource());

        using var rootContainer = builder.Build();

        LoadAssemblyAndTest(
            rootContainer,
            out var loadContextRef,
            (builder, assembly) => { },
            (scope, loadContext, assembly) =>
            {
                var instance = scope.Resolve(assembly.GetType("A.Service1"));

                Assert.Contains(instance.GetType().Assembly, loadContext.Assemblies);
            });

        WaitForUnload(loadContextRef);
    }

    [Fact]
    public void CanLoadInstanceOfAssemblyAndUnloadItAfterOnActivatedInModule()
    {
        var builder = new ContainerBuilder();

        using var rootContainer = builder.Build();

        LoadAssemblyAndTest(
            rootContainer,
            out var loadContextRef,
            (builder, assembly) =>
            {
                var module = (IModule)Activator.CreateInstance(assembly.GetType("A.OnActivatedModule"), 100);

                builder.RegisterModule(module);
            },
            (scope, loadContext, assembly) =>
            {
                var serviceType = assembly.GetType("A.Service1");

                var instance = scope.Resolve(serviceType);

                Assert.Contains(instance.GetType().Assembly, loadContext.Assemblies);

                var valueProp = serviceType.GetProperty("Value");

                Assert.Equal(100, valueProp.GetValue(instance));
            });

        WaitForUnload(loadContextRef);
    }

    [Fact]
    public void CanLoadInstanceOfAssemblyAndUnloadItAfterLifetimeScopeEndingInModule()
    {
        var builder = new ContainerBuilder();

        using var rootContainer = builder.Build();

        bool callbackInvoked = false;

        LoadAssemblyAndTest(
            rootContainer,
            out var loadContextRef,
            (builder, assembly) =>
            {
                Action invoke = () => { callbackInvoked = true; };

                var module = (IModule)Activator.CreateInstance(assembly.GetType("A.LifetimeScopeEndingModule"), invoke);

                builder.RegisterModule(module);
            },
            (scope, loadContext, assembly) =>
            {
                var serviceType = assembly.GetType("A.Service1");

                var instance = scope.Resolve(serviceType);

                Assert.Contains(instance.GetType().Assembly, loadContext.Assemblies);
            });

        WaitForUnload(loadContextRef);

        Assert.True(callbackInvoked);
    }

    [Fact]
    public void CanLoadInstanceOfScanAssemblyAndUnloadItAfterEnumerable()
    {
        var builder = new ContainerBuilder();

        using var rootContainer = builder.Build();

        LoadAssemblyAndTest(
            rootContainer,
            out var loadContextRef,
            (builder, assembly) => builder.RegisterType(assembly.GetType("A.Service1")),
            (scope, loadContext, assembly) =>
            {
                var genericEnumerable = typeof(IEnumerable<>).MakeGenericType(assembly.GetType("A.Service1"));

                var resolved = (IEnumerable<object>)scope.Resolve(genericEnumerable);

                Assert.Collection(
                    resolved,
                    item =>
                    {
                        Assert.Equal(item.GetType(), assembly.GetType("A.Service1"));
                        Assert.Contains(item.GetType().Assembly, loadContext.Assemblies);
                    });
            });

        WaitForUnload(loadContextRef);
    }

    [Fact]
    public void EnumerableCanContainItemsFromParentAndLoadedScope()
    {
        var builder = new ContainerBuilder();

        builder.RegisterInstance("string").As<object>();

        using var rootContainer = builder.Build();

        LoadAssemblyAndTest(
            rootContainer,
            out var loadContextRef,
            (builder, assembly) => builder.RegisterType(assembly.GetType("A.Service1")).As<object>(),
            (scope, loadContext, assembly) =>
            {
                var resolved = scope.Resolve<IEnumerable<object>>();

                Assert.Collection(
                    resolved,
                    firstItem =>
                    {
                        Assert.Equal("string", firstItem);
                    },
                    secondItem =>
                    {
                        Assert.IsType(assembly.GetType("A.Service1"), secondItem);
                    });
            });

        WaitForUnload(loadContextRef);
    }

    [Fact]
    public void CannotUseDefaultLoadContextForNewLifetimeScope()
    {
        var container = new ContainerBuilder().Build();

        Assert.Throws<InvalidOperationException>(() => container.BeginLoadContextLifetimeScope(AssemblyLoadContext.Default, _ => { }));
    }

    private void WaitForUnload(WeakReference loadContextRef)
    {
        var timeoutSource = new CancellationTokenSource(TimeSpan.FromSeconds(5));

        // Monitor the generated reference to the assembly load context to make sure it finishes cleanup.
        while (loadContextRef.IsAlive)
        {
            timeoutSource.Token.ThrowIfCancellationRequested();

            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
    }

    private void LoadAssemblyAndTest(
        IContainer container,
        out WeakReference loadContextRef,
        Action<ContainerBuilder, Assembly> scopeBuilder,
        Action<ILifetimeScope, AssemblyLoadContext, Assembly> execution)
    {
        var currentAssembly = Assembly.GetExecutingAssembly();
        var thisAssemblyPath = currentAssembly.Location;

        // Replace the project/assembly name in the path; this makes sure we use the same dotnet sdk and configuration
        // as this assembly.
        var newAssemblyPath = thisAssemblyPath.Replace(currentAssembly.GetName().Name, "Autofac.Test.Scenarios.LoadContext", StringComparison.Ordinal);

        var loadContext = new AssemblyLoadContext("test", isCollectible: true);
        loadContextRef = new WeakReference(loadContext);

        using (var scope = container.BeginLoadContextLifetimeScope(loadContext, builder =>
        {
            var loadedAssembly = loadContext.LoadFromAssemblyPath(newAssemblyPath);

            scopeBuilder(builder, loadedAssembly);

            builder.RegisterInstance(loadedAssembly);
        }))
        {
            execution(scope, loadContext, scope.Resolve<Assembly>());
        }

        loadContext.Unload();
    }
}

#endif
