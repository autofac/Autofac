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

using System.Collections.Generic;

namespace Autofac.Core.Resolving.Pipeline
{
    /// <summary>
    /// Defines the possibles phases of the resolve pipeline.
    /// </summary>
    /// <remarks>
    /// A resolve pipeline is split into these distinct phases, that control general ordering
    /// of middlewares and allow integrations and consuming code to specify what point in the pipeline to run their middleware.
    ///
    /// As a general principle, order between phases is strict, and always executes in the same order, but order within a phase should
    /// not be important for most cases, and handlers should be able to run in any order.
    /// </remarks>
    public enum PipelinePhase : int
    {
        /// <summary>
        /// The start of a resolve request. Custom middleware added to this phase executes before circular dependency detection.
        /// </summary>
        ResolveRequestStart = 0,

        /// <summary>
        /// In this phase, the lifetime scope selection takes place. If some middleware needs to change the lifetime scope for resolving against,
        /// it happens here (but bear in mind that the configured Autofac lifetime for the registration will take effect).
        /// </summary>
        ScopeSelection = 10,

        /// <summary>
        /// In this phase, instance decoration will take place (on the way 'out' of the pipeline).
        /// </summary>
        Decoration = 75,

        /// <summary>
        /// At the end of this phase, if a shared instance satisfies the request, the pipeline will stop executing and exit. Add custom
        /// middleware to this phase to choose your own shared instance.
        /// </summary>
        Sharing = 100,

        /// <summary>
        /// This phase occurs just before the service pipeline ends (and the registration pipeline is about to start).
        /// </summary>
        ServicePipelineEnd = 150,

        /// <summary>
        /// This phase occurs at the start of the registration pipeline.
        /// </summary>
        RegistrationPipelineStart = 200,

        /// <summary>
        /// This phase runs just before Activation, is the recommended point at which the resolve parameters should be replaced
        /// (using <see cref="ResolveRequestContext.ChangeParameters(IEnumerable{Parameter})"/>).
        /// </summary>
        ParameterSelection = 250,

        /// <summary>
        /// The Activation phase is the last phase of a pipeline, where a new instance of a component is created.
        /// </summary>
        Activation = 300,
    }
}
