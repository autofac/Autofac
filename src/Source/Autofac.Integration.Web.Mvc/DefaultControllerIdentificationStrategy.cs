// This software is part of the Autofac IoC container
// Copyright (c) 2010 Autofac Contributors
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
using System.Web.Mvc;
using Autofac.Core;

namespace Autofac.Integration.Web.Mvc
{
    /// <summary>
    /// Identifies controllers as TypedService(controllerType). 
    /// The type will include the namespace and therefore supports identical controller names
    /// from different MVC Areas
    /// </summary>
    public class DefaultControllerIdentificationStrategy
        : IControllerIdentificationStrategy
    {
        /// <summary>
        /// Determines which service is registered for the supplied
        /// controller type.
        /// </summary>
        /// <param name="controllerType">Type of the controller.</param>
        /// <returns>
        /// The service to be registered for the controller.
        /// </returns>
        public Service ServiceForControllerType(Type controllerType)
        {
            if (controllerType == null)
                throw new ArgumentNullException("controllerType");


            return new TypedService(controllerType);
        }
    }
}
