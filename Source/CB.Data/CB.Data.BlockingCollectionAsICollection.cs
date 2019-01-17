
// Copyright (c) Christophe Bertrand
// https://github.com/ChrisBertrandDotNet/CB-Helpers
// https://chrisbertrand.net

/*
 * 
 Presents System.Collections.Concurrent.BlockingCollection as a ICollection<T>.
 */

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace CB.Data
{
	/// <summary>
	/// Makes System.Collections.Concurrent.BlockingCollection compatible with ICollection&lt;T&gt;.
	/// <para>
	/// Except:
	/// Remove(T item) throws NotSupportedException.
	/// </para>
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class BlockingCollectionAsICollection<T> : System.Collections.Concurrent.BlockingCollection<T>, ICollection<T>
	{
		/// <summary>
		/// Builds a new collection.
		/// </summary>
		public BlockingCollectionAsICollection()
			: base()
		{ }

		/// <summary>
		/// Builds a new collection.
		/// </summary>
		/// <param name="collection"></param>
		public BlockingCollectionAsICollection(IProducerConsumerCollection<T> collection)
			: base(collection)
		{ }

		/// <summary>
		/// Builds a new collection.
		/// </summary>
		/// <param name="boundedCapacity"></param>
		public BlockingCollectionAsICollection(int boundedCapacity)
			: base(boundedCapacity)
		{ }

		/// <summary>
		/// Builds a new collection.
		/// </summary>
		/// <param name="collection"></param>
		/// <param name="boundedCapacity"></param>
		public BlockingCollectionAsICollection(IProducerConsumerCollection<T> collection, int boundedCapacity)
			: base(collection, boundedCapacity)
		{ }

		/// <summary>
		/// Items will be removed one by one.
		/// That may be slow for a big collection, as the items are removed one-by-one.
		/// </summary>
		public void Clear()
		{
			T e;
			while (base.TryTake(out e)) ;
		}

		/// <summary>
		/// Returns true if the collection contains this item.
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public bool Contains(T item)
		{
			return System.Linq.Enumerable.Contains(this, item);
		}

		/// <summary>
		/// Returns true if the collection is read-only.
		/// </summary>
		public bool IsReadOnly
		{
			get { return false; }
		}

		/// <summary>
		/// Not supported.
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		/// <exception cref="System.NotSupportedException">Always throw this exception.</exception>
		public bool Remove(T item)
		{
			throw new NotSupportedException("BlockingCollection");
		}
	}
}