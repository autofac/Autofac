// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

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
