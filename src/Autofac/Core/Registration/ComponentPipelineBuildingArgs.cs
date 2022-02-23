// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac.Core.Resolving.Pipeline;

namespace Autofac.Core.Registration;

/// <summary>
/// Event arguments for the <see cref="IComponentRegistration.PipelineBuilding"/> event.
/// </summary>
public class ComponentPipelineBuildingArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ComponentPipelineBuildingArgs"/> class.
    /// </summary>
    /// <param name="registration">The registration.</param>
    /// <param name="pipelineBuilder">The pipeline builder.</param>
    public ComponentPipelineBuildingArgs(IComponentRegistration registration, IResolvePipelineBuilder pipelineBuilder)
    {
        Registration = registration;
        PipelineBuilder = pipelineBuilder;
    }

    /// <summary>
    /// Gets the component registration whose pipeline is being built.
    /// </summary>
    public IComponentRegistration Registration { get; }

    /// <summary>
    /// Gets the pipeline builder for the registration. Add middleware to the builder to add to the component behaviour.
    /// </summary>
    public IResolvePipelineBuilder PipelineBuilder { get; }
}
