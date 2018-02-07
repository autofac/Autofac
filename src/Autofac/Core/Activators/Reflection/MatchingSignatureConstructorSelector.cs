// This software is part of the Autofac IoC container
// Copyright © 2011 Autofac Contributors
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
using System.Globalization;
using System.Linq;

namespace Autofac.Core.Activators.Reflection
{
    /// <summary>
    /// Selects a constructor based on its signature.
    /// </summary>
    public class MatchingSignatureConstructorSelector : IConstructorSelector
    {
        private readonly Type[] _signature;

        /// <summary>
        /// Initializes a new instance of the <see cref="MatchingSignatureConstructorSelector"/> class.
        /// </summary>
        /// <param name="signature">Signature to match.</param>
        public MatchingSignatureConstructorSelector(params Type[] signature)
        {
            if (signature == null) throw new ArgumentNullException(nameof(signature));

            _signature = signature;
        }

        /// <summary>
        /// Selects the best constructor from the available constructors.
        /// </summary>
        /// <param name="constructorBindings">Available constructors.</param>
        /// <param name="parameters">Parameters to the instance being resolved.</param>
        /// <returns>The best constructor.</returns>
        public ConstructorParameterBinding SelectConstructorBinding(ConstructorParameterBinding[] constructorBindings, IEnumerable<Parameter> parameters)
        {
            if (constructorBindings == null) throw new ArgumentNullException(nameof(constructorBindings));

            var result = constructorBindings
                .Where(b => b.TargetConstructor.GetParameters().Select(p => p.ParameterType).SequenceEqual(_signature))
                .ToArray();

            if (result.Length == 1)
                return result[0];

            if (!constructorBindings.Any())
                throw new ArgumentException(MatchingSignatureConstructorSelectorResources.AtLeastOneBindingRequired);

            var targetTypeName = constructorBindings.First().TargetConstructor.DeclaringType.Name;
            var signature = string.Join(", ", _signature.Select(t => t.Name).ToArray());

            if (result.Length == 0)
                throw new DependencyResolutionException(string.Format(CultureInfo.CurrentCulture, MatchingSignatureConstructorSelectorResources.RequiredConstructorNotAvailable, targetTypeName, signature));

            throw new DependencyResolutionException(string.Format(CultureInfo.CurrentCulture, MatchingSignatureConstructorSelectorResources.TooManyConstructorsMatch, signature));
        }
    }
}
