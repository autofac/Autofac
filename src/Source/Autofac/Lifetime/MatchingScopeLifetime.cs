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
using System.Globalization;
using System.Linq.Expressions;
using Autofac.Util;

namespace Autofac.Lifetime
{
    /// <summary>
    /// Attaches the component's lifetime to scopes matching a supplied expression.
    /// </summary>
    public class MatchingScopeLifetime : IComponentLifetime
    {
        Func<ILifetimeScope, bool> _matcher;
        string _matchExpressionCode;

        /// <summary>
        /// Match scopes based on the provided expression.
        /// </summary>
        /// <param name="matchExpression">Expression describing scopes that will match.</param>
        public MatchingScopeLifetime(Expression<Func<ILifetimeScope, bool>> matchExpression)
        {
            Enforce.ArgumentNotNull(matchExpression, "matchExpression");
            _matcher = matchExpression.Compile();
            _matchExpressionCode = matchExpression.Body.ToString();
        }

        /// <summary>
        /// Given the most nested scope visible within the resolve operation, find
        /// the scope for the component.
        /// </summary>
        /// <param name="mostNestedVisibleScope">The most nested visible scope.</param>
        /// <returns>The scope for the component.</returns>
        public ISharingLifetimeScope FindScope(ISharingLifetimeScope mostNestedVisibleScope)
        {
            Enforce.ArgumentNotNull(mostNestedVisibleScope, "mostNestedVisibleScope");

            var next = mostNestedVisibleScope;
            while (next != null)
            {
                if (_matcher(next))
                    return next;

                next = next.ParentLifetimeScope;
            }

            throw new DependencyResolutionException(string.Format(
                CultureInfo.CurrentCulture, MatchingScopeLifetimeResources.MatchingScopeNotFound, _matchExpressionCode));
        }
    }
}
