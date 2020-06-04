// This software is part of the Autofac IoC container
// Copyright © 2011 Autofac Contributors
// https://autofac.org
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using Autofac.Core.Pipeline;
using Autofac.Core.Resolving;
using Autofac.Core.Resolving.Pipeline;
using Autofac.Util;

namespace Autofac.Core.Registration
{
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
}
