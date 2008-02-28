// This software is part of the Autofac IoC container
// Copyright (c) 2007 Nicholas Blumhardt
// nicholas.blumhardt@gmail.com
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
using Autofac.Registrars;
using Autofac.Registrars.Generic;

namespace Autofac.Builder
{
    /// <summary>
    /// Extends ContainerBuilder to register generic types.
    /// </summary>
    public static class GenericRegistrationBuilder
    {
        /// <summary>
        /// Register an un-parameterised generic type, e.g. <code>Repository&lt;&gt;</code>.
        /// Concrete types will be made as they are requested, e.g. with <code>Resolve&lt;Repository&lt;int&gt;&gt;()</code>.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="implementor">The implementor.</param>
        /// <returns></returns>
        public static IGenericRegistrar RegisterGeneric(this ContainerBuilder builder, Type implementor)
        {
            Enforce.ArgumentNotNull(builder, "builder");
            Enforce.ArgumentNotNull(implementor, "implementor");
            return builder.AttachRegistrar<IGenericRegistrar>(
            	new GenericRegistrar(implementor));
        }
    }
}
