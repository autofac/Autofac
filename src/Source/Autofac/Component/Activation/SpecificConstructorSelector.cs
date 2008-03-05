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
using System.Collections.Generic;
using System.Reflection;

namespace Autofac.Component.Activation
{
    /// <summary>
    /// Chooses a constructor based on an exact signature.
    /// </summary>
    public class SpecificConstructorSelector : IConstructorSelector
    {
        IList<Type> _signature;

        /// <summary>
        /// Initializes a new instance of the <see cref="SpecificConstructorSelector"/> class.
        /// </summary>
        /// <param name="constructorSignature">The constructor signature.</param>
        public SpecificConstructorSelector(params Type[] constructorSignature)
        {
            Enforce.ArgumentNotNull(constructorSignature, "constructorSignature");

            foreach (Type t in constructorSignature)
                if (t == null)
                    throw new ArgumentException(SpecificConstructorSelectorResources.TypeCannotBeNull);

            _signature = new List<Type>(constructorSignature);
        }

        #region IConstructorSelector Members

        /// <summary>
        /// Returns the most suitable constructor from those provided.
        /// </summary>
        /// <param name="possibleConstructors">Required. Must contain at least one item.</param>
        /// <returns>The most suitable constructor.</returns>
        public ConstructorInfo SelectConstructor(ICollection<ConstructorInfo> possibleConstructors)
        {
            Enforce.ArgumentNotNull(possibleConstructors, "possibleConstructors");

            foreach (ConstructorInfo ci in possibleConstructors)
                if (MatchesSignature(ci.GetParameters()))
                    return ci;

            throw new DependencyResolutionException(SpecificConstructorSelectorResources.RequiredConstructorNotAvailable);
        }

        private bool MatchesSignature(ParameterInfo[] parameters)
        {
            Enforce.ArgumentNotNull(parameters, "parameters");

            if (parameters.Length != _signature.Count)
                return false;

            for (int i = 0; i < _signature.Count; ++i)
                if (parameters[i].ParameterType != _signature[i])
                    return false;

            return true;
        }

        #endregion
    }
}
