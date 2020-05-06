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
using System.Collections.Generic;
using System.Linq;

namespace Autofac.Core.Lifetime
{
    public class SharedInstanceKey : IEquatable<SharedInstanceKey>
    {
        public SharedInstanceKey(Guid id)
            : this(new Guid[] { id })
        {
        }

        public SharedInstanceKey(Guid[] ids)
        {
            IDs = ids;
        }

        public IEnumerable<Guid> IDs { get; }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            else if (ReferenceEquals(this, obj))
            {
                return true;
            }

            return Equals(obj as SharedInstanceKey);
        }

        public bool Equals(SharedInstanceKey? other)
        {
            if (other == null)
            {
                return false;
            }

            return IDs.SequenceEqual(other.IDs);
        }

        public override int GetHashCode()
        {
            var hash = 13;
            foreach (var id in IDs)
            {
                hash ^= id.GetHashCode() * 397;
            }

            return hash;
        }
    }
}
