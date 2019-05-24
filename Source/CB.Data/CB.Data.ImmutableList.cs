
// Copyright Christophe Bertrand
// https://github.com/ChrisBertrandDotNet/CB-Helpers
// https://chrisbertrand.net

/*
 * 
 */

/* Requires:
. CB.Data\CB.Data.EmptyArray.cs
. CB.Data\CB.Data.EnumHelper.cs

*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;

namespace CB.Data
{
	/// <summary>
	/// An immutable generic collection.
	/// <para>This class is not a wrapper (unlike ReadOnlyCollection&lt;T&gt;). The source items are copied and cannot be modified.</para>
	/// <para>WARNING: The class is supposed to be thread-safe. Not tested yet.</para>
	/// </summary>
	/// <typeparam name="T"></typeparam>
	[DebuggerDisplay("Count = {Count}")]
	[ImmutableObject(true)]
	public class ImmutableList<T> : IReadOnlyList<T>
	{
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		readonly T[] items;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		readonly int count; // small optimisation.
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		readonly IEqualityComparer<T> compareur;

		/// <summary>
		/// Builds a writable array by copying all items.
		/// </summary>
		public T[] AsArray { get { return this.items.ToArray(); } }

		/// <summary>
		/// Builds a writable list by copying all items.
		/// </summary>
		public List<T> AsReadWriteList { get { return new List<T>(this.items); } }

		/// <summary>
		/// Initializes an empty list.
		/// </summary>
		public ImmutableList()
			: this(EmptyArray<T>.Empty)
		{ }

		/// <summary>
		/// Builds a list by copying a collection.
		/// </summary>
		/// <param name="items"></param>
		public ImmutableList(IEnumerable<T> items)
			: this(items, EqualityComparer<T>.Default)
		{ }

		/// <summary>
		/// Builds a list by copying a collection.
		/// </summary>
		/// <param name="items"></param>
		/// <param name="comparer"></param>
		public ImmutableList(IEnumerable<T> items, IEqualityComparer<T> comparer)
		{
			if (items == null)
				throw new ArgumentNullException();
			this.items = items.ToArray();
			this.count = this.items.Length;
			this.compareur = comparer;
		}

		/// <summary>
		/// Gets the number of items contained in the list.
		/// </summary>
		public int Count
		{
			get { return this.count; }
		}

		/// <summary>
		/// Determines whether the list contains the specified value.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public bool Contains(T value)
		{
			return items.Contains(value);
		}

		/// <summary>
		/// Copies all the elements of the list to the specified one-dimensional array starting at the specified destination array index.
		/// </summary>
		/// <param name="array"></param>
		/// <param name="index"></param>
		public void CopyTo(T[] array, int index)
		{
			items.CopyTo(array, index);
		}

		/// <summary>
		/// Creates a shallow copy of the list.
		/// </summary>
		/// <returns></returns>
		public ImmutableList<T> Clone()
		{
			return new ImmutableList<T>(this);
		}

		/// <summary>
		/// Returns an enumerator that iterates through the list.
		/// </summary>
		/// <returns></returns>
		public IEnumerator<T> GetEnumerator()
		{
			int c = count;
			var éléments = this.items;
			for (int a = 0; a < c; a++)
				yield return éléments[a];
		}

		/// <summary>
		/// Returns an enumerator that iterates through the list.
		/// </summary>
		/// <returns></returns>
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return this.items.GetEnumerator();
		}

		IEnumerator<T> IEnumerable<T>.GetEnumerator()
		{
			int c = count;
			var éléments = this.items;
			for (int i = 0; i < c; i++)
				yield return éléments[i];
		}

		/// <summary>
		/// Searches for the specified value and returns the index of its first occurrence in the list.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public int IndexOf(T value)
		{
			int c = count;
			var éléments = this.items;
			for (int i = 0; i < c; i++)
				if (compareur.Equals(éléments[i], value))
					return i;
			return -1;
		}

		/// <summary>
		/// Gets the element at the specified index.
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public T this[int index]
		{
			get { return items[index]; }
		}

	}
}