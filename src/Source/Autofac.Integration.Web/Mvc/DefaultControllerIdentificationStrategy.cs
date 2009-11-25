// This software is part of the Autofac IoC container
// Copyright (c) 2007 - 2009 Autofac Contributors
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
    /// Identifies controllers as 'controller.[name]'. This is
    /// tied to the router's controller names by lowercasing them,
    /// and to the controller type names by stripping off the
    /// 'Controller' and doing the same.
    /// </summary>
    public class DefaultControllerIdentificationStrategy
        : IControllerIdentificationStrategy
    {
        const string Prefix = "controller.";
        const string TypeNameSuffix = "Controller";

        /// <summary>
        /// Determines which service is resolved in order to provide the
        /// controller identified by the MVC framework using the provided name.
        /// E.g.: Home (Route) -&gt; controller.home
        /// </summary>
        /// <param name="controllerName">Name of the controller.</param>
        /// <returns>The service identifier.</returns>
        public Service ServiceForControllerName(string controllerName)
        {
            if (controllerName == null)
                throw new ArgumentNullException("controllerName");

            if (controllerName == "")
                throw new ArgumentOutOfRangeException("controllerName");

            return new NamedService(Prefix + controllerName.ToLowerInvariant());
        }

        /// <summary>
        /// Determines which service is registered for the supplied
        /// controller type. This service will correspond to the service
        /// returned by ServiceForControllerName when the controller is
        /// requested by the framework. E.g.:
        /// HomeController -&gt; controller.home
        /// </summary>
        /// <param name="controllerType">Type of the controller.</param>
        /// <returns>
        /// The service to be registered for the controller.
        /// </returns>
        public Service ServiceForControllerType(Type controllerType)
        {
            if (controllerType == null)
                throw new ArgumentNullException("controllerType");

            return ServiceForControllerName(controllerType.Name.Replace(TypeNameSuffix, ""));
        }
    }
}
