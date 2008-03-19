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
using System.Reflection;

namespace Autofac.Component.Activation
{
    /// <summary>
    /// Selects the constructor with the most parameters.
    /// </summary>
    public class MostParametersConstructorSelector : IConstructorSelector
    {
        #region IConstructorSelector Members

        /// <summary>
        /// Returns the most suitable constructor from those provided.
        /// </summary>
        /// <param name="possibleConstructors">Required. Must contain at least one item.</param>
        /// <returns>The most suitable constructor.</returns>
        public ConstructorInfo SelectConstructor(ICollection<ConstructorInfo> possibleConstructors)
        {
            Enforce.ArgumentNotNull(possibleConstructors, "possibleConstructors");

            if (possibleConstructors.Count == 0)
                throw new ArgumentOutOfRangeException("possibleConstructors");

            ConstructorInfo result = null;
            foreach (ConstructorInfo ci in possibleConstructors)
            {
                if (result == null ||
                    result.GetParameters().Length < ci.GetParameters().Length)
                {
                    result = ci;
                }
            }
            return result;
        }

        #endregion
    }
}
