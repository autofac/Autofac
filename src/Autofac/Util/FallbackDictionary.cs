using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Autofac.Util
{
    /// <summary>
    /// Dictionary used to allow local property get/set and fall back to parent values.
    /// </summary>
    internal class FallbackDictionary<TKey, TValue> : IDictionary<TKey, TValue>
    {
        /// <summary>
        /// Storage for local values set in the dictionary.
        /// </summary>
        private IDictionary<TKey, TValue> _localValues = new Dictionary<TKey, TValue>();

        /// <summary>
        /// Initializes a new instance of the <see cref="FallbackDictionary{TKey, TValue}"/> class
        /// with an empty parent.
        /// </summary>
        public FallbackDictionary()
            : this(new Dictionary<TKey, TValue>())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FallbackDictionary{TKey, TValue}"/> class
        /// with a specified parent.
        /// </summary>
        /// <param name="parent">
        /// The parent dictionary to which values should fall back when not present in the current dictionary.
        /// </param>
        public FallbackDictionary(IDictionary<TKey, TValue> parent)
        {
            if (parent == null)
            {
                throw new ArgumentNullException(nameof(parent));
            }

            this.Parent = parent;
        }

        /// <summary>
        /// Gets the number of elements contained in the dictionary.
        /// </summary>
        /// <value>
        /// The number of elements contained in this collection plus the parent collection, minus overlapping key counts.
        /// </value>
        public int Count
        {
            get
            {
                return this.Keys.Count;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this collection is read-only.
        /// </summary>
        /// <value>
        /// Always returns <see langword="false" />.
        /// </value>
        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets an <see cref="ICollection{TKey}"/> containing the keys of the dictionary.
        /// </summary>
        /// <value>
        /// An <see cref="ICollection{TKey}"/> containing the keys of the dictionary without duplicates.
        /// </value>
        /// <remarks>
        /// The order of the keys in the returned <see cref="ICollection{TKey}"/> is unspecified,
        /// but it is guaranteed to be the same order as the corresponding values in the <see cref="ICollection{TKey}"/>
        /// returned by the <see cref="Values"/> property.
        /// </remarks>
        public ICollection<TKey> Keys
        {
            get
            {
                return this.OrderedKeys().ToArray();
            }
        }

        /// <summary>
        /// Gets the parent dictionary.
        /// </summary>
        /// <value>
        /// The parent dictionary to which values should fall back when not present in the current dictionary.
        /// </value>
        public IDictionary<TKey, TValue> Parent { get; private set; }

        /// <summary>
        /// Gets an <see cref="ICollection{TKey}"/> containing the values of the dictionary.
        /// </summary>
        /// <value>
        /// An <see cref="ICollection{TKey}"/> containing the values of the dictionary with overrides taken into account.
        /// </value>
        /// <remarks>
        /// The order of the keys in the returned <see cref="ICollection{TKey}"/> is unspecified,
        /// but it is guaranteed to be the same order as the corresponding keys in the <see cref="ICollection{TKey}"/>
        /// returned by the <see cref="Keys"/> property.
        /// </remarks>
        public ICollection<TValue> Values
        {
            get
            {
                var keys = this.Keys.ToArray();
                var values = new TValue[keys.Length];
                for (var i = 0; i < keys.Length; i++)
                {
                    values[i] = this[keys[i]];
                }

                return values;
            }
        }

        /// <summary>
        /// Gets or sets the <typeparamref name="TValue"/> with the specified key.
        /// </summary>
        /// <value>
        /// The <typeparamref name="TValue"/>.
        /// </value>
        /// <param name="key">The key.</param>
        /// <remarks>
        /// <para>
        /// Changes made to this dictionary do not affect the parent.
        /// </para>
        /// </remarks>
        public TValue this[TKey key]
        {
            get
            {
                TValue value;
                if (this._localValues.TryGetValue(key, out value))
                {
                    return value;
                }

                return this.Parent[key];
            }

            set
            {
                this._localValues[key] = value;
            }
        }

        /// <summary>
        /// Adds an item to the dictionary.
        /// </summary>
        /// <param name="item">The object to add to the dictionary.</param>
        /// <remarks>
        /// <para>
        /// Changes made to this dictionary do not affect the parent.
        /// </para>
        /// </remarks>
        public void Add(KeyValuePair<TKey, TValue> item)
        {
            this.Add(item.Key, item.Value);
        }

        /// <summary>
        /// Adds an element with the provided key and value to the dictionary.
        /// </summary>
        /// <param name="key">The object to use as the key of the element to add.</param>
        /// <param name="value">The object to use as the value of the element to add.</param>
        /// <remarks>
        /// <para>
        /// Changes made to this dictionary do not affect the parent.
        /// </para>
        /// </remarks>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="key" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown if an element with the same key is already present in the local or parent dictionary.
        /// </exception>
        public void Add(TKey key, TValue value)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (this.Parent.ContainsKey(key))
            {
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, FallbackDictionaryResources.DuplicateItem, key));
            }

            this._localValues.Add(key, value);
        }

        /// <summary>
        /// Removes all items from the dictionary. Does not clear parent entries, only local overrides.
        /// </summary>
        public void Clear()
        {
            this._localValues.Clear();
        }

        /// <summary>
        /// Determines whether the dictionary contains a specific value.
        /// </summary>
        /// <param name="item">The object to locate in the dictionary.</param>
        /// <returns>
        /// <see langword="true" /> if <paramref name="item" /> is found in the dictionary; otherwise, <see langword="false" />.
        /// </returns>
        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            if (this._localValues.ContainsKey(item.Key))
            {
                return this._localValues.Contains(item);
            }

            return this.Parent.Contains(item);
        }

        /// <summary>
        /// Determines whether the dictionary contains an element with the specified key.
        /// </summary>
        /// <param name="key">The key to locate in the dictionary.</param>
        /// <returns>
        /// <see langword="true" /> if the dictionary or its parent contains an element with the key; otherwise, <see langword="false" />.
        /// </returns>
        public bool ContainsKey(TKey key)
        {
            return this.Keys.Contains(key);
        }

        /// <summary>
        /// Copies the elements of the dictionary to an <see cref="System.Array" />, starting at a particular <see cref="System.Array" /> index.
        /// </summary>
        /// <param name="array">
        /// The one-dimensional <see cref="System.Array" /> that is the destination of the elements copied from
        /// the dictionary. The <see cref="System.Array" /> must have zero-based indexing.
        /// </param>
        /// <param name="arrayIndex">
        /// The zero-based index in <paramref name="array" /> at which copying begins.
        /// </param>
        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            var resolved = (ICollection<KeyValuePair<TKey, TValue>>)new Dictionary<TKey, TValue>(this);
            resolved.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// An enumerator that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            foreach (var key in this.OrderedKeys())
            {
                yield return new KeyValuePair<TKey, TValue>(key, this[key]);
            }
        }

        /// <summary>
        /// Removes the first occurrence of a specific object from the dictionary.
        /// </summary>
        /// <param name="item">The object to remove from the dictionary.</param>
        /// <returns>
        /// <see langword="true" /> if <paramref name="item" /> was successfully removed from the dictionary; otherwise, <see langword="false" />.
        /// This method also returns <see langword="false" /> if <paramref name="item" /> is not found in the original dictionary.
        /// </returns>
        /// <remarks>
        /// <para>
        /// Changes made to this dictionary do not affect the parent.
        /// </para>
        /// </remarks>
        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            return this._localValues.Remove(item);
        }

        /// <summary>
        /// Removes the element with the specified key from the dictionary.
        /// </summary>
        /// <param name="key">The key of the element to remove.</param>
        /// <returns>
        /// <see langword="true" /> if the element is successfully removed; otherwise, <see langword="false" />.
        /// This method also returns <see langword="false" /> if <paramref name="key" /> was not found in the original dictionary.
        /// </returns>
        /// <remarks>
        /// <para>
        /// Changes made to this dictionary do not affect the parent.
        /// </para>
        /// </remarks>
        public bool Remove(TKey key)
        {
            return this._localValues.Remove(key);
        }

        /// <summary>
        /// Gets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key whose value to get.</param>
        /// <param name="value">When this method returns, the value associated with the specified key, if the key is found; otherwise, the default value for the type of the <paramref name="value" /> parameter. This parameter is passed uninitialized.</param>
        /// <returns>
        /// <see langword="true" /> if the dictionary or parent contains an element with the specified key; otherwise, <see langword="false" />.
        /// </returns>
        public bool TryGetValue(TKey key, out TValue value)
        {
            if (this._localValues.TryGetValue(key, out value))
            {
                return true;
            }

            return this.Parent.TryGetValue(key, out value);
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        /// <summary>
        /// Gets the list of correctly ordered unique keys from the local and parent dictionaries.
        /// </summary>
        /// <returns>
        /// An <see cref="IEnumerable{T}"/> with the unique set of all keys.
        /// </returns>
        private IEnumerable<TKey> OrderedKeys()
        {
            return this._localValues.Keys.Union(this.Parent.Keys).Distinct().OrderBy(k => k);
        }
    }
}
