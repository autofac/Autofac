// This software is part of the Autofac IoC container
// Copyright © 2011 Autofac Contributors
// https://autofac.org
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
using System.Collections;
using System.Collections.Generic;

namespace Autofac.Core.Resolving.Pipeline
{
    /// <summary>
    /// Enumerator for a pipeline builder.
    /// </summary>
    internal sealed class PipelineBuilderEnumerator : IEnumerator, IEnumerator<IResolveMiddleware>
    {
        private readonly MiddlewareDeclaration? _first;
        private MiddlewareDeclaration? _current;
        private bool _ended;

        /// <summary>
        /// Initializes a new instance of the <see cref="PipelineBuilderEnumerator"/> class.
        /// </summary>
        /// <param name="first">The first middleware declaration.</param>
        public PipelineBuilderEnumerator(MiddlewareDeclaration? first)
        {
            _first = first;
        }

        /// <inheritdoc />
        object IEnumerator.Current => _current?.Middleware ?? throw new InvalidOperationException();

        /// <inheritdoc />
        public IResolveMiddleware Current => _current?.Middleware ?? throw new InvalidOperationException();

        /// <inheritdoc />
        public bool MoveNext()
        {
            if (_ended)
            {
                return false;
            }

            if (_current is object)
            {
                _current = _current.Next;

                _ended = !(_current is object);

                return !_ended;
            }

            if (_first is object)
            {
                _current = _first;

                return true;
            }

            _ended = true;
            return false;
        }

        /// <inheritdoc />
        public void Reset()
        {
            _current = null;
            _ended = false;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            // Nothing to dispose here.
        }
    }
}
