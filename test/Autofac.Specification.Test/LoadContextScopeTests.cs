using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using Autofac.Test.Scenarios.ScannedAssembly;

namespace Autofac.Specification.Test;

#if NET5_0_OR_GREATER

public class LoadContextScopeTests
{
    [Fact]
    public void CanLoadInstanceOfScanAssemblyAndUnloadIt()
    {
        using var rootContainer = new ContainerBuilder().Build();

        var currentAssembly = Assembly.GetExecutingAssembly();
        var thisAssemblyPath = currentAssembly.Location;

        // Replace the project/assembly name in the path; this makes sure we use the same dotnet sdk and configuration
        // as this assembly.
        var newAssemblyPath = thisAssemblyPath.Replace(currentAssembly.GetName().Name, "Autofac.Test.Scenarios.LoadContext");

        LoadedWork(rootContainer, newAssemblyPath, out var libRef);

        var timeoutSource = new CancellationTokenSource(TimeSpan.FromSeconds(5));

        // Monitor the generated reference to the assembly load context to make sure it finishes cleanup.
        while (libRef.IsAlive)
        {
            timeoutSource.Token.ThrowIfCancellationRequested();

            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
    }

    private void LoadedWork(IContainer container, string assemblyPath, out WeakReference alcWeakRef)
    {
        var loadContext = new AssemblyLoadContext("test", isCollectible: true);
        alcWeakRef = new WeakReference(loadContext);

        using (var scope = container.BeginLoadContextLifetimeScope(loadContext, builder =>
        {
            var loadedAssembly = loadContext.LoadFromAssemblyPath(assemblyPath);

            builder.RegisterAssemblyTypes(loadedAssembly);
            builder.RegisterInstance(loadedAssembly);
        }))
        {
            var assembly = scope.Resolve<Assembly>();
            var instance = scope.Resolve(assembly.GetType("A.Service1"));

            Assert.Contains(instance.GetType().Assembly, loadContext.Assemblies);
        }

        loadContext.Unload();
    }

    [Fact]
    public void CannotUseDefaultLoadContextForNewLifetimeScope()
    {
        var container = new ContainerBuilder().Build();

        Assert.Throws<InvalidOperationException>(() => container.BeginLoadContextLifetimeScope(AssemblyLoadContext.Default, _ => { }));
    }
}

#endif
