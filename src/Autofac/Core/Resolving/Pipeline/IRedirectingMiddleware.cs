// This software is part of the Autofac IoC container
// Copyright © 2020 Autofac Contributors
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

namespace Autofac.Core.Resolving.Pipeline
{
    /// <summary>
    /// Defines a category of service resolve middleware that can 'redirect' a service to a custom registration.
    /// </summary>
    /// <remarks>
    /// Middleware implementing this interface can 'override' the registration reported for a service when the
    /// default registration is requested, but not have that registration reported in the set of concrete registrations for the service.
    ///
    /// The best use-case for this type of middleware is when you want to add some custom behaviour to a service, and resolve
    /// a registration that does not implement that service (but might consume it), e.g. composites.
    ///
    /// You can also use this mechanism to provide a service that has no 'real' backing registrations, but performs some other behaviour. In this
    /// case you still need to supply a target registration, but only for metadata and consistency.
    ///
    /// A service pipeline containing a <see cref="IRedirectingMiddleware"/> will not invoke the registration's pipeline. It is expected that the implementation
    /// invokes the required registration pipeline as needed.
    /// </remarks>
    public interface IRedirectingMiddleware : IResolveMiddleware
    {
        /// <summary>
        /// Gets the target registration of the redirect.
        /// </summary>
        IComponentRegistration TargetRegistration { get; }
    }
}
