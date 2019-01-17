
// Copyright Christophe Bertrand
// https://github.com/ChrisBertrandDotNet/CB-Helpers
// https://chrisbertrand.net

/*
 * 
 */

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;

namespace CB.Data
{
	/// <summary>
	/// An immutable generic dictionary.
	/// <para>This class is not a wrapper (unlike ReadOnlyDictionary&lt;K,V&gt;). The source items are copied and cannot be modified.</para>
	/// <para>The class is thread-safe and concurrent.</para>
	/// </summary>
	/// <typeparam name="K">The key type.</typeparam>
	/// <typeparam name="V">The value type.</typeparam>
	[DebuggerDisplay("Count = {Count}")]
	[ImmutableObject(true)]
	public class ImmutableDictionary<K, V> : IReadOnlyDictionary<K, V>
	{
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		readonly ConcurrentDictionary<K, V> dico;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		readonly int count; // small optimization.
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		readonly IEqualityComparer<K> compareurDeClé;

		/// <summary>
		/// Copies the data to a new (writable) dictionary.
		/// </summary>
		public ConcurrentDictionary<K, V> AsReadWriteDictionary { get { return new ConcurrentDictionary<K, V>(this.dico, this.compareurDeClé); } }

		/// <summary>
		/// Initializes an empty dictionary.
		/// </summary>
		public ImmutableDictionary()
		{
			this.compareurDeClé = EqualityComparer<K>.Default;
			dico = new ConcurrentDictionary<K, V>(this.compareurDeClé);
			this.count = this.dico.Count;
		}

		/// <summary>
		/// Builds a new dictionary, by copying the items of a collection.
		/// </summary>
		/// <param name="items">Original collection.</param>
		public ImmutableDictionary(IEnumerable<KeyValuePair<K, V>> items)
			: this(items, EqualityComparer<K>.Default)
		{ }

		/// <summary>
		/// Builds a new dictionary, by copying the items of a collection.
		/// </summary>
		/// <param name="items">Original collection.</param>
		/// <param name="comparer">The equality comparison implementation to use when comparing keys.</param>
		public ImmutableDictionary(IEnumerable<KeyValuePair<K, V>> items, IEqualityComparer<K> comparer)
		{
			if (items == null || comparer == null)
				throw new ArgumentNullException();
			this.dico = new ConcurrentDictionary<K, V>(items, comparer);
			this.count = this.dico.Count;
			this.compareurDeClé = comparer;
		}

		/// <summary>
		/// Determines whether the dictionary contains the specified key.
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public bool ContainsKey(K key)
		{
			return this.dico.ContainsKey(key);
		}

		/// <summary>
		/// Gets a collection containing the keys in the dictionary.
		/// </summary>
		public IEnumerable<K> Keys
		{
			get { return this.dico.Keys; }
		}

		/// <summary>
		/// Attempts to get the value associated with the specified key from the dictionary.
		/// </summary>
		/// <param name="key"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public bool TryGetValue(K key, out V value)
		{
			return this.dico.TryGetValue(key, out value);
		}

		/// <summary>
		/// Gets a collection that contains the values in the dictionary.
		/// </summary>
		public IEnumerable<V> Values
		{
			get { return this.dico.Values; }
		}

		/// <summary>
		/// Gets the value associated with the specified key.
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public V this[K key]
		{
			get { return this.dico[key]; }
		}

		/// <summary>
		/// Gets the number of key/value pairs contained in the dictionary.
		/// </summary>
		public int Count
		{
			get { return this.count; }
		}

		/// <summary>
		/// Returns an enumerator that iterates through the dictionary.
		/// </summary>
		/// <returns></returns>
		public IEnumerator<KeyValuePair<K, V>> GetEnumerator()
		{
			return this.dico.GetEnumerator();
		}

		/// <summary>
		/// Returns an enumerator that iterates through the dictionary.
		/// </summary>
		/// <returns></returns>
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return this.dico.GetEnumerator();
		}
	}
}