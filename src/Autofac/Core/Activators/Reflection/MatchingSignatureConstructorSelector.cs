// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

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
            _signature = signature ?? throw new ArgumentNullException(nameof(signature));
        }

        /// <summary>
        /// Selects the best constructor from the available constructors.
        /// </summary>
        /// <param name="constructorBindings">Available constructors.</param>
        /// <param name="parameters">Parameters to the instance being resolved.</param>
        /// <returns>The best constructor.</returns>
        public BoundConstructor SelectConstructorBinding(BoundConstructor[] constructorBindings, IEnumerable<Parameter> parameters)
        {
            if (constructorBindings == null)
            {
                throw new ArgumentNullException(nameof(constructorBindings));
            }

            var matchingCount = 0;
            var validCount = 0;
            BoundConstructor? chosen = null;

            for (var idx = 0; idx < constructorBindings.Length; idx++)
            {
                var binding = constructorBindings[idx];

                if (binding.CanInstantiate)
                {
                    validCount++;

                    // Concievably could store the set of parameter types in the binder as well, but
                    // that's yet more memory up-front, for a less used constructor selector.
                    if (binding.Binder.Parameters.Select(p => p.ParameterType).SequenceEqual(_signature))
                    {
                        chosen = binding;
                        matchingCount++;
                    }
                }
            }

            if (matchingCount == 1)
            {
                return chosen!;
            }

            if (validCount == 0)
            {
                throw new ArgumentException(MatchingSignatureConstructorSelectorResources.AtLeastOneBindingRequired);
            }

            var targetTypeName = constructorBindings[0].TargetConstructor.DeclaringType.Name;
            var signature = string.Join(", ", _signature.Select(t => t.Name).ToArray());

            if (matchingCount == 0)
            {
                throw new DependencyResolutionException(string.Format(CultureInfo.CurrentCulture, MatchingSignatureConstructorSelectorResources.RequiredConstructorNotAvailable, targetTypeName, signature));
            }

            throw new DependencyResolutionException(string.Format(CultureInfo.CurrentCulture, MatchingSignatureConstructorSelectorResources.TooManyConstructorsMatch, signature));
        }
    }
}
