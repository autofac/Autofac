// This software is part of the Autofac IoC container
// Copyright (c) 2007 - 2008 Autofac Contributors
// http://autofac.org
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


namespace Autofac.Component
{
    /// <summary>
    /// Provides a method by which new instances of a
    /// component can be accessed.
    /// </summary>
    public interface IActivator
    {
        /// <summary>
        /// Create a component instance, using container
        /// to resolve the instance's dependencies.
        /// </summary>
        /// <param name="context">The context to use
        /// for dependency resolution.</param>
        /// <param name="parameters">Parameters that can be used in the resolution process.</param>
        /// <returns>A component instance. Note that while the
        /// returned value need not be created on-the-spot, it must
        /// not be returned more than once by consecutive calls. (Throw
        /// an exception if this is attempted. IActivationScope should
        /// manage singleton semantics.)</returns>
        object ActivateInstance(IContext context, IActivationParameters parameters);

        /// <summary>
        /// A 'new context' is a scope that is self-contained
        /// and that can dispose the components it contains before the parent
        /// container is disposed. If the activator is stateless it should return
        /// true, otherwise false.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance can support a new context; otherwise, <c>false</c>.
        /// </value>
		bool CanSupportNewContext { get; }
	}
}
