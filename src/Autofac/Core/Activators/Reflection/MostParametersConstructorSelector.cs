// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Autofac.Core.Activators.Reflection
{
    /// <summary>
    /// Selects the constructor with the most parameters.
    /// </summary>
    public class MostParametersConstructorSelector : IConstructorSelector
    {
        /// <summary>
        /// Selects the best constructor from the available constructors.
        /// </summary>
        /// <param name="constructorBindings">Available constructors.</param>
        /// <param name="parameters">Parameters to the instance being resolved.</param>
        /// <returns>The best constructor.</returns>
        /// <exception cref='DependencyResolutionException'>A single unambiguous match could not be chosen.</exception>
        public BoundConstructor SelectConstructorBinding(BoundConstructor[] constructorBindings, IEnumerable<Parameter> parameters)
        {
            if (constructorBindings == null)
            {
                throw new ArgumentNullException(nameof(constructorBindings));
            }

            if (constructorBindings.Length == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(constructorBindings));
            }

            if (constructorBindings.Length == 1)
            {
                return constructorBindings[0];
            }

            var highestArgCount = -1;
            var countAtHighest = 0;
            BoundConstructor? chosen = null;

            for (var idx = 0; idx < constructorBindings.Length; idx++)
            {
                var binding = constructorBindings[idx];
                var count = binding.ArgumentCount;

                if (!binding.CanInstantiate)
                {
                    continue;
                }

                if (count > highestArgCount)
                {
                    highestArgCount = count;
                    countAtHighest = 1;
                    chosen = binding;
                }
                else if (count == highestArgCount)
                {
                    countAtHighest++;
                }
            }

            if (countAtHighest == 1)
            {
                return chosen!;
            }

            throw new DependencyResolutionException(string.Format(
                CultureInfo.CurrentCulture,
                MostParametersConstructorSelectorResources.UnableToChooseFromMultipleConstructors,
                highestArgCount,
                chosen!.TargetConstructor.DeclaringType));
        }
    }
}
