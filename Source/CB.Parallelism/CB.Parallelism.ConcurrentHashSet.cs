
// Copied from https://github.com/dotnet/roslyn/blob/master/src/Compilers/Core/Portable/InternalUtilities/ConcurrentSet.cs
// Original Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.

// modification Copyright Christophe Bertrand
// https://github.com/ChrisBertrandDotNet/CB-Helpers
// https://chrisbertrand.net

/* Requires:
*/

/*
This is a concurrent hash set. It just uses a ConcurrentDictionary<T, byte> internally so it's not very well optimized, but enough for short collections.
 */



/* rappel des symboles de projet:
 * Console : (rien)
 * WPF : (rien)
 * Windows 8.1 pour Windows Store : NETFX_CORE;WINDOWS_APP
 * Windows 8 universel pour Windows (Store) : NETFX_CORE;WINDOWS_APP
 * Windows 8 universel pour Windows Phone (Store) : NETFX_CORE;WINDOWS_PHONE_APP
 * Windows 10 UWP (Store) : NETFX_CORE;WINDOWS_UWP
*/

#if WINDOWS_APP || WINDOWS_PHONE_APP
#define WINDOWS_8_STORE // Windows 8, mais non Windows 10.
#endif

#if TEST
using CBdotnet.Test;
#endif
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;

namespace CB.Parallelism
{

	/// <summary>
	/// A concurrent, simplified HashSet.
	/// </summary>
	[DebuggerDisplay("Count = {Count}")]
	internal sealed class ConcurrentHashSet<T> : ICollection<T>
	{
		/// <summary>
		/// The default concurrency level is 2. That means the collection can cope with up to two
		/// threads making simultaneous modifications without blocking.
		/// Note ConcurrentDictionary's default concurrency level is dynamic, scaling according to
		/// the number of processors.
		/// </summary>
		private const int DefaultConcurrencyLevel = 2;

		/// <summary>
		/// Taken from ConcurrentDictionary.DEFAULT_CAPACITY
		/// </summary>
		private const int DefaultCapacity = 31;

		/// <summary>
		/// The backing dictionary. The values are never used; just the keys.
		/// </summary>
		private readonly ConcurrentDictionary<T, byte> _dictionary;

		/// <summary>
		/// Construct a concurrent set with the default concurrency level.
		/// </summary>
		public ConcurrentHashSet()
		{
			_dictionary = new ConcurrentDictionary<T, byte>(DefaultConcurrencyLevel, DefaultCapacity);
		}

		/// <summary>
		/// Construct a concurrent set using the specified equality comparer.
		/// </summary>
		/// <param name="equalityComparer">The equality comparer for values in the set.</param>
		public ConcurrentHashSet(IEqualityComparer<T> equalityComparer)
		{
			_dictionary = new ConcurrentDictionary<T, byte>(DefaultConcurrencyLevel, DefaultCapacity, equalityComparer);
		}

		/// <summary>
		/// Obtain the number of elements in the set.
		/// </summary>
		/// <returns>The number of elements in the set.</returns>
		public int Count => _dictionary.Count;

		/// <summary>
		/// Determine whether the set is empty.</summary>
		/// <returns>true if the set is empty; otherwise, false.</returns>
		public bool IsEmpty => _dictionary.IsEmpty;

		public bool IsReadOnly => false;

		/// <summary>
		/// Determine whether the given value is in the set.
		/// </summary>
		/// <param name="value">The value to test.</param>
		/// <returns>true if the set contains the specified value; otherwise, false.</returns>
		public bool Contains(T value)
		{
			return _dictionary.ContainsKey(value);
		}

		/// <summary>
		/// Attempts to add a value to the set.
		/// </summary>
		/// <param name="value">The value to add.</param>
		/// <returns>true if the value was added to the set. If the value already exists, this method returns false.</returns>
		public bool Add(T value)
		{
			return _dictionary.TryAdd(value, 0);
		}

		public void AddRange(IEnumerable<T> values)
		{
			if (values != null)
			{
				foreach (var v in values)
				{
					Add(v);
				}
			}
		}

		/// <summary>
		/// Attempts to remove a value from the set.
		/// </summary>
		/// <param name="value">The value to remove.</param>
		/// <returns>true if the value was removed successfully; otherwise false.</returns>
		public bool Remove(T value)
		{
			return _dictionary.TryRemove(value, out var b);
		}

		/// <summary>
		/// Clear the set
		/// </summary>
		public void Clear()
		{
			_dictionary.Clear();
		}

		public struct KeyEnumerator
		{
			private readonly IEnumerator<KeyValuePair<T, byte>> _kvpEnumerator;

			internal KeyEnumerator(IEnumerable<KeyValuePair<T, byte>> data)
			{
				_kvpEnumerator = data.GetEnumerator();
			}

			public T Current => _kvpEnumerator.Current.Key;

			public bool MoveNext()
			{
				return _kvpEnumerator.MoveNext();
			}

			public void Reset()
			{
				_kvpEnumerator.Reset();
			}
		}

		/// <summary>
		/// Obtain an enumerator that iterates through the elements in the set.
		/// </summary>
		/// <returns>An enumerator for the set.</returns>
		public KeyEnumerator GetEnumerator()
		{
			// PERF: Do not use dictionary.Keys here because that creates a snapshot
			// of the collection resulting in a List<T> allocation. Instead, use the
			// KeyValuePair enumerator and pick off the Key part.
			return new KeyEnumerator(_dictionary);
		}

		private IEnumerator<T> GetEnumeratorImpl()
		{
			// PERF: Do not use dictionary.Keys here because that creates a snapshot
			// of the collection resulting in a List<T> allocation. Instead, use the
			// KeyValuePair enumerator and pick off the Key part.
			foreach (var kvp in _dictionary)
			{
				yield return kvp.Key;
			}
		}

		IEnumerator<T> IEnumerable<T>.GetEnumerator()
		{
			return GetEnumeratorImpl();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumeratorImpl();
		}

		void ICollection<T>.Add(T item)
		{
			Add(item);
		}

		public void CopyTo(T[] array, int arrayIndex)
		{
			throw new NotImplementedException();
		}
	}
}