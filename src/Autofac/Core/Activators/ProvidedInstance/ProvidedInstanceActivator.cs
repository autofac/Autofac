// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics;
using System.Globalization;
using Autofac.Core.Resolving;
using Autofac.Core.Resolving.Pipeline;

namespace Autofac.Core.Activators.ProvidedInstance;

/// <summary>
/// Provides a pre-constructed instance.
/// </summary>
public class ProvidedInstanceActivator : InstanceActivator, IInstanceActivator
{
    private readonly object _instance;
    private bool _activated;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProvidedInstanceActivator"/> class.
    /// </summary>
    /// <param name="instance">The instance to provide.</param>
    public ProvidedInstanceActivator(object instance)
        : base(GetType(instance))
    {
        _instance = instance;
    }

    /// <summary>
    /// Gets or sets a value indicating whether the activator disposes the instance that it holds.
    /// Necessary because otherwise instances that are never resolved will never be
    /// disposed.
    /// </summary>
    public bool DisposeInstance { get; set; }

    /// <inheritdoc/>
    public void ConfigurePipeline(IComponentRegistryServices componentRegistryServices, IResolvePipelineBuilder pipelineBuilder)
    {
        if (pipelineBuilder is null)
        {
            throw new ArgumentNullException(nameof(pipelineBuilder));
        }

        pipelineBuilder.Use(this.DisplayName(), PipelinePhase.Activation, MiddlewareInsertionMode.EndOfPhase, (context, next) =>
        {
            context.Instance = GetInstance();

            next(context);
        });
    }

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        // Only dispose of the instance here if it wasn't activated. If it was activated,
        // then either the owning lifetime scope will dispose of it automatically
        // (see InstanceLookup.Activate) or an OnRelease handler will take care of it.
        if (disposing && DisposeInstance && !_activated)
        {
            // If we are in synchronous dispose, and an object implements IDisposable,
            // then use it.
            if (_instance is IDisposable disposable)
            {
                disposable.Dispose();
            }
            else if (_instance is IAsyncDisposable asyncDisposable)
            {
                Trace.TraceWarning(DisposerResources.TypeOnlyImplementsIAsyncDisposable, LimitType.FullName);

                // Type only implements IAsyncDisposable. We will need to do sync-over-async.
                // We want to ensure we lose all context here, because if we don't we can deadlock.
                // So we push this disposal onto the thread pool.
                Task.Run(async () => await asyncDisposable.DisposeAsync().ConfigureAwait(false))
                    .ConfigureAwait(false)
                    .GetAwaiter().GetResult();
            }
        }

        base.Dispose(disposing);
    }

    /// <inheritdoc />
    protected override ValueTask DisposeAsync(bool disposing)
    {
        if (disposing && DisposeInstance && !_activated)
        {
            // If the item implements IAsyncDisposable we will call its DisposeAsync Method.
            if (_instance is IAsyncDisposable asyncDisposable)
            {
                var vt = asyncDisposable.DisposeAsync();
                if (vt.IsCompletedSuccessfully)
                {
                    return vt;
                }

                static async ValueTask Awaiter(ValueTask vt) => await vt.ConfigureAwait(false);
                return Awaiter(vt);
            }
            else if (_instance is IDisposable disposable)
            {
                // Call the standard Dispose.
                disposable.Dispose();
            }
        }

        return default;

        // Do not call the base, otherwise the standard Dispose will fire.
    }

    private static Type GetType(object instance)
    {
        if (instance == null)
        {
            throw new ArgumentNullException(nameof(instance));
        }

        return instance.GetType();
    }

    private object GetInstance()
    {
        CheckNotDisposed();

        if (_activated)
        {
            throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, ProvidedInstanceActivatorResources.InstanceAlreadyActivated, _instance.GetType()));
        }

        _activated = true;

        return _instance;
    }
}
