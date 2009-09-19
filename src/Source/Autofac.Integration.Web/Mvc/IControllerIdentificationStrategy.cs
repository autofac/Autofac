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

using System;
using Autofac.Core;

namespace Autofac.Integration.Web.Mvc
{
    /// <summary>
    /// Determines how components relate to controller and type
    /// names.
    /// </summary>
    public interface IControllerIdentificationStrategy
    {
        /// <summary>
        /// Determines which service is resolved in order to provide the
        /// controller identified by the MVC framework using the provided name.
        /// </summary>
        /// <param name="controllerName">Name of the controller.</param>
        /// <returns>The service identifier.</returns>
        Service ServiceForControllerName(string controllerName);

        /// <summary>
        /// Determines which service is registered for the supplied
        /// controller type. This service will correspond to the service
        /// returned by ServiceForControllerName when the controller is
        /// requested by the framework.
        /// </summary>
        /// <param name="controllerType">Type of the controller.</param>
        /// <returns>The service to be registered for the controller.</returns>
        Service ServiceForControllerType(Type controllerType);
    }
}
