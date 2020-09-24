// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Autofac.Core.Lifetime
{
    /// <summary>
    /// Attaches the component's lifetime to scopes matching a supplied expression.
    /// </summary>
    public class MatchingScopeLifetime : IComponentLifetime
    {
        private readonly object[] _tagsToMatch;

        /// <summary>
        /// Initializes a new instance of the <see cref="MatchingScopeLifetime"/> class.
        /// </summary>
        /// <param name="lifetimeScopeTagsToMatch">The tags applied to matching scopes.</param>
        public MatchingScopeLifetime(params object[] lifetimeScopeTagsToMatch)
        {
            _tagsToMatch = lifetimeScopeTagsToMatch ?? throw new ArgumentNullException(nameof(lifetimeScopeTagsToMatch));
        }

        /// <summary>
        /// Gets the list of lifetime scope tags to match.
        /// </summary>
        /// <value>
        /// An <see cref="IEnumerable{T}"/> of object tags to match
        /// when searching for the lifetime scope for the component.
        /// </value>
        public IEnumerable<object> TagsToMatch
        {
            get
            {
                return _tagsToMatch;
            }
        }

        /// <summary>
        /// Given the most nested scope visible within the resolve operation, find
        /// the scope for the component.
        /// </summary>
        /// <param name="mostNestedVisibleScope">The most nested visible scope.</param>
        /// <returns>The scope for the component.</returns>
        public ISharingLifetimeScope FindScope(ISharingLifetimeScope mostNestedVisibleScope)
        {
            if (mostNestedVisibleScope == null)
            {
                throw new ArgumentNullException(nameof(mostNestedVisibleScope));
            }

            ISharingLifetimeScope? next = mostNestedVisibleScope;
            while (next != null)
            {
                if (_tagsToMatch.Contains(next.Tag))
                {
                    return next;
                }

                next = next.ParentLifetimeScope;
            }

            throw new DependencyResolutionException(string.Format(
                CultureInfo.CurrentCulture, MatchingScopeLifetimeResources.MatchingScopeNotFound, string.Join(", ", _tagsToMatch)));
        }
    }
}
