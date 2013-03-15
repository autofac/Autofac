// This software is part of the Autofac IoC container
// Copyright © 2013 Autofac Contributors
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

using System.Collections.Generic;

namespace Autofac.Util
{
    internal class SafeDictionary<TKey, TValue>
    {
        readonly object _syncLock = new object();
        readonly Dictionary<TKey, TValue> _dictionary = new Dictionary<TKey, TValue>();

        public TValue this[TKey key]
        {
            set { lock (_syncLock) _dictionary[key] = value; }
        }

        public IEnumerable<TKey> Keys
        {
            get { return _dictionary.Keys; }
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            lock (_syncLock) return _dictionary.TryGetValue(key, out value);
        }

        public bool Remove(TKey key)
        {
            lock (_syncLock) return _dictionary.Remove(key);
        }

        public void Clear()
        {
            lock (_syncLock) _dictionary.Clear();
        }
    }
}