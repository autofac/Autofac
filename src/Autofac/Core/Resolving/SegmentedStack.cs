// This software is part of the Autofac IoC container
// Copyright © 2020 Autofac Contributors
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
using System.Runtime.CompilerServices;

namespace Autofac.Core.Resolving
{
    /// <summary>
    /// Implements a segmented stack of items, which functions like a regular <see cref="Stack{T}"/>, but allows segments
    /// of the stack to be enumerated without including items pushed before the segment.
    /// </summary>
    /// <typeparam name="T">The item type.</typeparam>
    public sealed class SegmentedStack<T> : IEnumerable<T>
        where T : class
    {
        private T[] _array;
        private int _next;
        private int _activeSegmentBase;

        private const int InitialCapacity = 16;

        /// <summary>
        /// Initializes a new instance of the <see cref="SegmentedStack{T}"/> class.
        /// </summary>
        public SegmentedStack()
        {
            _array = new T[InitialCapacity];
        }

        /// <summary>
        /// Push an item onto the stack.
        /// </summary>
        /// <param name="item">The item.</param>
        public void Push(T item)
        {
            // No null check for item here; internally called method only, known to never be null, and is a very hot path.
            var next = _next;
            T[] arr = _array;

            // Array bounds checking cast.
            if ((uint)next < (uint)arr.Length)
            {
                arr[next] = item;
                _next = next + 1;
            }
            else
            {
                PushWithResize(item);
            }
        }

        // Do not inline; makes it easier to profile stack resizing.
        [MethodImpl(MethodImplOptions.NoInlining)]
        private void PushWithResize(T item)
        {
            Array.Resize(ref _array, 2 * _array.Length);
            _array[_next] = item;
            _next++;
        }

        /// <summary>
        /// Pop the item at the top of the stack (and return it).
        /// </summary>
        /// <returns>The item that has just been popped.</returns>
        public T Pop()
        {
            int next = _next - 1;
            var array = _array;

            // Array bounds checking cast.
            if ((uint)next >= (uint)array.Length || next < _activeSegmentBase)
            {
                // Cannot pop below the active segment position.
                throw new InvalidOperationException(SegmentedStackResources.CurrentStackSegmentEmpty);
            }

            _next = next;
            var item = array[next];
            array[next] = null!;
            return item;
        }

        /// <summary>
        /// Gets the count of the items in the active segment.
        /// </summary>
        public int Count => _next - _activeSegmentBase;

        /// <summary>
        /// Enter a new segment. When this method returns <see cref="Count"/> will be zero, and the stack will appear empty.
        /// </summary>
        /// <returns>An <see cref="IDisposable"/> that will return the stack to the previously active segment when disposed.</returns>
        public IDisposable EnterSegment()
        {
            var reset = new StackSegment(this, _activeSegmentBase);

            _activeSegmentBase = _next;

            return reset;
        }

        /// <inheritdoc/>
        public IEnumerator<T> GetEnumerator()
        {
            return new Enumerator(this);
        }

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(this);
        }

        private struct StackSegment : IDisposable
        {
            private readonly SegmentedStack<T> _stack;
            private readonly int _resetPosition;

            public StackSegment(SegmentedStack<T> stack, int resetPosition)
            {
                _stack = stack;
                _resetPosition = resetPosition;
            }

            public void Dispose()
            {
                // If the stack 'next' is not just above the active segment base, then
                // the segment was not fully popped before exiting the segment.
                if (_stack.Count > 0)
                {
                    throw new InvalidOperationException(SegmentedStackResources.CannotExitSegmentWithRemainingItems);
                }

                _stack._activeSegmentBase = _resetPosition;
            }
        }

        private struct Enumerator : IEnumerator<T>, IEnumerator
        {
            private readonly SegmentedStack<T> _stack;
            private readonly int _activeSegmentBase;
            private int _index;
            private T? _currentElement;

            internal Enumerator(SegmentedStack<T> stack)
            {
                _stack = stack;
                _index = -2;
                _activeSegmentBase = _stack._activeSegmentBase;
                _currentElement = null;
            }

            public T Current
            {
                get
                {
                    if (_index < 0)
                    {
                        throw new InvalidOperationException(SegmentedStackResources.EnumeratorNotValid);
                    }

                    return _currentElement!;
                }
            }

            object IEnumerator.Current => Current;

            public void Dispose()
            {
                _index = -1;
            }

            public bool MoveNext()
            {
                var index = _index;

                if (index == -2)
                {
                    // Start the enumerator.
                    _index = index = _stack._next - 1;

                    if (index > _activeSegmentBase)
                    {
                        _currentElement = _stack._array[index];
                        return true;
                    }

                    return false;
                }

                if (index == -1)
                {
                    // Finished.
                    return false;
                }

                if (--index >= _activeSegmentBase)
                {
                    _currentElement = _stack._array[index];
                    _index = index;
                    return true;
                }

                _currentElement = null!;
                return false;
            }

            public void Reset()
            {
                _index = -2;
                _currentElement = null!;
            }
        }
    }
}
