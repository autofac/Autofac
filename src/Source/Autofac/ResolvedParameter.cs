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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Autofac
{
    /// <summary>
    /// Flexible parameter type allows arbitrary values to be retrieved from the resolution context.
    /// </summary>
    public class ResolvedParameter : Parameter
    {
        Func<ParameterInfo, IComponentContext, bool> _predicate;
        Func<ParameterInfo, IComponentContext, object> _valueAccessor;

        /// <summary>
        /// Create an instance of the ResolvedParameter class.
        /// </summary>
        /// <param name="predicate">A predicate that determines which parameters on a constructor will be supplied by this instance.</param>
        /// <param name="valueAccessor">A function that supplies the parameter value given the context.</param>
        public ResolvedParameter(Func<ParameterInfo, IComponentContext, bool> predicate, Func<ParameterInfo, IComponentContext, object> valueAccessor)
        {
            _predicate = Enforce.ArgumentNotNull(predicate, "predicate");
            _valueAccessor = Enforce.ArgumentNotNull(valueAccessor, "valueAccessor");
        }

        /// <summary>
        /// Returns true if the parameter is able to provide a value to a particular site.
        /// </summary>
        /// <param name="pi">Constructor, method, or property-mutator parameter.</param>
        /// <param name="context">The component context in which the value is being provided.</param>
        /// <param name="valueProvider">If the result is true, the valueProvider parameter will
        /// be set to a function that will lazily retrieve the parameter value. If the result is false,
        /// will be set to null.</param>
        /// <returns>True if a value can be supplied; otherwise, false.</returns>
        public override bool CanSupplyValue(ParameterInfo pi, IComponentContext context, out Func<object> valueProvider)
        {
            Enforce.ArgumentNotNull(pi, "pi");
            Enforce.ArgumentNotNull(context, "context");

            if (_predicate(pi, context))
            {
                valueProvider = () => _valueAccessor(pi, context);
                return true;
            }
            else
            {
                valueProvider = null;
                return false;
            }
        }
    }
}
