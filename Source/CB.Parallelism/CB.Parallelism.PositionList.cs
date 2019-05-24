
// Copyright Christophe Bertrand
// https://github.com/ChrisBertrandDotNet/CB-Helpers
// https://chrisbertrand.net

/* The concept of collection index is incompatible with multi-tasking by nature.
 * Here I propose a list that replaces the concept of index by a concept called "Position".
 * Like an index, a Position points at an item in the list. But unlike an index, when items are inserted or removed before the item, the Position still points at this item.
 * 
 * For example, let's imagine we have two strings in a list: { "a", "c" } and a Position that points to the second item ("c").
 * Then a parallel thread inserts the string "b" before the latter Position. So the list is now { "a", "b", "c" }. After that, the Pointer still points at "c", although "c" is the third string in the list now.
 * 
 * Another difference is Pointers are to be created when needed, where any item in a regular collection automatically has an index.
 * You can get a Pointer at item insertion, for example, and move it from this item to another item.
 */

/* Requires:
 * (nothing)
*/

/* TODO:
 * 
*	Add members to PositionList:
	*	Position InsertItems(Position start, RelativePosition insertionPosition, IEnumerable<T> newItems, bool returnsNewPosition)
	*	Position MoveItems(Position start, Position end, Position destination, RelativePosition insertionPosition, bool returnsNewPosition)
	*	void ReplaceValues(Position start, IEnumerable<T> values)
*	Create structure Range, meaning a range between two positions, including or excluding the positions.
	*	note: the two positions are duplicated while creating an instance of Range.
	*	And add members to PositionList accordingly:
		*	DeleteValues(Range range, T value)
		*	MoveItems(Range range, Position destination, RelativePosition insertionPosition)
		*	RemoveRange(Range range) : removes the two positions at the same time.
		*	ReplaceValues(Range range, T value)
		*	ReplaceValues(Range range, IEnumerable<T> values)
*	Create class PositionString that is similar to a String except chars are pointed by Positions in place of indexes.
	*	Inherit PositionList<char> and add some features:
		*	Converts implicitely from and to String.
		*	Compares with strings.
*	Optimize PositionString by storing 4 Chars in an UInt64. That means writing a complete class instead of just inheriting from PositionList.
 */

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace CB.Parallelism
{

	/// <summary>
	/// The relative position of this item or <see cref="Position"/>, before or after the other one.
	/// </summary>
	public enum RelativePosition
	{
		/// <summary>
		/// This item/position is before the other one.
		/// </summary>
		Before,

		/// <summary>
		/// This position equals the other one.
		/// </summary>
		Equal,

		/// <summary>
		/// This item/position is after the other one.
		/// </summary>
		After
	}

	/// <summary>
	/// A position points at an item in a <see cref="PositionList{T}"/>.
	/// <para>Two positions or more can point at the same item.</para>
	/// <para>A position can be moved, therefore pointing at a different item.</para>
	/// <para>If the item is removed, the position points at the next item.</para>
	/// <para>If the list is empty, the position points at the start and remains valid.</para>
	/// <para>This class is not generic therefore not as efficient as <see cref="PositionList{T}.PositionInList"/> which should be used preferably.</para>
	/// </summary>
	public abstract class Position
	{
		/// <summary>
		/// The position index in the list.
		/// </summary>
		internal enum IndexInList
		{
			/// <summary>
			/// The list being empty, this index is undefined.
			/// </summary>
			Undefined = -1,
			/// <summary>
			/// The first valid position in the list.
			/// </summary>
			Start = 0
		}

		/// <summary>
		/// The current item index, or <see cref="IndexInList.Undefined"/> if the list is empty.
		/// </summary>
		internal IndexInList Index;

		/// <summary>
		/// If true, this position was removed therefore is not maintained by the list.
		/// </summary>
		public abstract bool IsRemoved { get; }

		/// <summary>
		/// The current item index, or null if the list is empty.
		/// <para>It should be read for test purposes only, as this value can be changed at any moment by a parallel thread.</para>
		/// </summary>
		public int? DebugIndex => Index != IndexInList.Undefined ? (int?)Index : null;


		/// <summary>
		/// Returns true if this position is at end of the list.
		/// </summary>
		/// <returns></returns>
		public abstract bool IsAtListEnd { get; }

		/// <summary>
		/// Returns true if this position is at start of the list.
		/// </summary>
		/// <returns></returns>
		public abstract bool IsAtListStart { get; }

		/// <summary>
		/// Gets or sets the item at that position.
		/// <para>If the list is empty, _get_ returns a default value, and _set_ does nothing.</para>
		/// <para><see cref="PositionList{T}.PositionInList.ItemValue"/> should be used preferably.</para>
		/// </summary>
		/// <exception cref="System.InvalidCastException">The value to set is not the right type. See <see cref="PositionList{T}.PositionInList"/>.</exception>
		public abstract object ItemValueAsObject { get; set; }

		/// <summary>
		/// Makes a copy of this position. This copy will be independently maintained by the list.
		/// </summary>
		/// <returns></returns>
		public abstract Position Duplicate();

		/// <summary>
		/// Moves the position to another item.
		/// <para>If the current position is at list end, it is not modified and the function returns false.</para>
		/// </summary>
		/// <param name="offset">Number of items to count. A positive number counts next items. A negative number counts previous items.</param>
		/// <returns>True if the movement is possible or if <paramref name="offset"/> is zero.</returns>
		public abstract bool Move(int offset);

		/// <summary>
		/// Moves the position one item backward.
		/// </summary>
		/// <returns></returns>
		public bool MoveBackward()
		{
			return Move(-1);
		}

		/// <summary>
		/// Moves the position one item forward.
		/// </summary>
		/// <returns></returns>
		public bool MoveForward()
		{
			return Move(1);
		}

		/// <summary>
		/// Removes this position, as an item pointer, not the item itself. Therefore the list will not manage this position anymore.
		/// </summary>
		/// <returns></returns>
		public abstract bool Remove();

		/// <summary>
		/// Return true if this position points at an item.
		/// Or false if the list is empty.
		/// </summary>
		/// <returns></returns>
		public abstract bool PointsAtAnItem { get; }
	}


	/// <summary>
	/// A concurrent list that is "indexed" by mobile Positions.
	/// <para>A <see cref="Position"/> is equivalent to an <see cref="IList"/> index, but remains valid when items are inserted or deleted.</para>
	/// <para>You can get positions when adding an item and when searching for items.</para>
	/// <para>This list is thread-safe. A lock is available, see <see cref="PositionList{T}.SyncRoot"/>.</para>
	/// <para>This list accepts duplicate items.</para>
	/// </summary>
	/// <typeparam name="T"></typeparam>
	[DebuggerDisplay("Count = {Count}")]
	public class PositionList<T> : IProducerConsumerCollection<T>, ICollection<T>
	{
		// Internal note: Position.Index reflects the position of an item of the same index in the list, or -1 if the list is empty.

		/// <summary>
		/// internal list.
		/// </summary>
		protected readonly System.Collections.Generic.IList<T> list;
		/// <summary>
		/// Lock.
		/// </summary>
		protected readonly object accessLock = new object();
		readonly HashSet<PositionInList> positions = new HashSet<PositionInList>();
		readonly Func<T, T, bool> comparer;

		#region Sub types

		/// <summary>
		/// A position points at an item in a <see cref="PositionList{T}"/>.
		/// <para>Two positions or more can point at the same item.</para>
		/// <para>A position can be moved, therefore pointing at a different item.</para>
		/// <para>If the item is removed, the position points at the next item.</para>
		/// <para>If the list is empty, the position points at the start and remains valid.</para>
		/// </summary>
		public sealed class PositionInList : Position, IComparable<PositionInList>, IEquatable<PositionInList>
		{
			/// <summary>
			/// The <see cref="PositionList{T}"/> this position points to. Or null if this position is removed.
			/// </summary>
			public PositionList<T> List { get; internal set; }

			/// <summary>
			/// Gets or sets the item at that position.
			/// <para>If the list is empty, get returns a default value, and set does nothing.</para>
			/// </summary>
			/// <exception cref="System.NullReferenceException">This position is removed.</exception>
			public T ItemValue
			{
				get => this.List[this];
				set => this.List[this] = value;
			}

			/// <summary>
			/// Gets or sets the item at that position.
			/// <para>If the list is empty, get returns a default value, and set does nothing.</para>
			/// </summary>
			/// <exception cref="System.InvalidCastException">The value to set is not a <typeparamref name="T"/>.</exception>
			/// <exception cref="System.NullReferenceException">This position is removed.</exception>
			public override object ItemValueAsObject { get => ItemValue; set => ItemValue = (T)value; }

			/// <summary>
			/// Returns true if this position is at end of the list.
			/// <para>If the list is empty, returns false.</para>
			/// </summary>
			/// <returns></returns>
			/// <exception cref="System.NullReferenceException">This position is removed.</exception>
			public override bool IsAtListEnd => (int)this.Index == (this.List.Count - 1) && List.IsNotEmpty;

			/// <summary>
			/// Returns true if this position is at start of the list.
			/// <para>If the list is empty, returns false.</para>
			/// </summary>
			/// <returns></returns>
			public override bool IsAtListStart => this.Index == 0;

			/// <summary>
			/// Return true if this position points at an item.
			/// Or false if the list is empty.
			/// </summary>
			/// <returns></returns>
			public override bool PointsAtAnItem => this.Index != IndexInList.Undefined;

			/// <summary>
			/// If true, this position was removed therefore is not maintained by the list.
			/// </summary>
			public override bool IsRemoved => this.List == null;

			internal PositionInList(PositionList<T> list)
			{
				Debug.Assert(list != null);
				this.List = list;
			}

			/// <summary>
			/// Checks equality.
			/// </summary>
			/// <param name="a"></param>
			/// <param name="b"></param>
			/// <returns></returns>
			/// <exception cref="System.ArgumentException">Both positions are removed.</exception>
			public static bool operator ==(PositionInList a, PositionInList b)
			{
				if (object.ReferenceEquals(a, null) && object.ReferenceEquals(b, null))
					return true;
				if (object.ReferenceEquals(a, null) || object.ReferenceEquals(b, null))
					return false;
				if (a.List != b.List)
					return false;
				if (a.List == null)
					throw new ArgumentException("Both positions are removed.");
				lock (a.List.accessLock)
					return a.Index == b.Index;
			}

			/// <summary>
			/// Checks difference.
			/// </summary>
			/// <param name="a"></param>
			/// <param name="b"></param>
			/// <returns></returns>
			/// <exception cref="System.ArgumentException">Both positions are removed.</exception>
			public static bool operator !=(PositionInList a, PositionInList b) => !(a == b);

			static void _checkParams(PositionInList a, PositionInList b)
			{
				if (object.ReferenceEquals(a, null))
					throw new ArgumentNullException(nameof(a));
				if (object.ReferenceEquals(b, null))
					throw new ArgumentNullException(nameof(b));
				if (a.IsRemoved)
					throw new ArgumentException(nameof(a) + " is removed.");
				if (b.IsRemoved)
					throw new ArgumentException(nameof(b) + " is removed.");
				if (a.List != b.List)
					throw new ArgumentException("Positions are not on the same list.");
			}

			/// <summary>
			/// Return true if <paramref name="a"/> is after <paramref name="b"/>.
			/// </summary>
			/// <param name="a"></param>
			/// <param name="b"></param>
			/// <returns></returns>
			public static bool operator >(PositionInList a, PositionInList b)
			{
				_checkParams(a, b);
				lock (a.List.accessLock)
					return a.Index > b.Index;
			}

			/// <summary>
			/// Return true if <paramref name="a"/> is before <paramref name="b"/>.
			/// </summary>
			/// <param name="a"></param>
			/// <param name="b"></param>
			/// <returns></returns>
			public static bool operator <(PositionInList a, PositionInList b)
			{
				_checkParams(a, b);
				lock (a.List.accessLock)
					return a.Index < b.Index;
			}

			/// <summary>
			/// Return true if <paramref name="a"/> is after <paramref name="b"/> or is at the same position.
			/// </summary>
			/// <param name="a"></param>
			/// <param name="b"></param>
			/// <returns></returns>
			public static bool operator >=(PositionInList a, PositionInList b)
			{
				_checkParams(a, b);
				lock (a.List.accessLock)
					return a.Index >= b.Index;
			}

			/// <summary>
			/// Return true if <paramref name="a"/> is before <paramref name="b"/> or is at the same position.
			/// </summary>
			/// <param name="a"></param>
			/// <param name="b"></param>
			/// <returns></returns>
			public static bool operator <=(PositionInList a, PositionInList b)
			{
				_checkParams(a, b);
				lock (a.List.accessLock)
					return a.Index <= b.Index;
			}

			/// <summary>
			/// Checks equality.
			/// </summary>
			/// <param name="obj"></param>
			/// <returns></returns>
			public override bool Equals(object obj)
			{
				var obj2 = obj as PositionInList;
				if (object.ReferenceEquals(obj2, null))
					return false;
				return this == obj2;
			}

			/// <summary>
			/// Constant hash code. It does not change when the position moves or is removed.
			/// </summary>
			/// <returns></returns>
			public override int GetHashCode()
			{
				return base.GetHashCode();
			}

			/// <summary>
			/// Description of the current state.
			/// </summary>
			/// <returns></returns>
			public override string ToString()
			{
				if (this.IsRemoved)
					return "It is removed.";
				if (this.Index == IndexInList.Undefined)
					return "Position is undefined for the list is empty.";
				if (IsAtListStart)
					return "At list start";
				if (IsAtListEnd)
					return "At list end";
				return "Current index = " + Index.ToString();
			}

			/// <summary>
			/// Makes a copy of this position. This copy will be maintained by the list.
			/// </summary>
			/// <returns></returns>
			public override Position Duplicate() => List.DuplicatePosition(this);

			/// <summary>
			/// Moves the position to another item.
			/// <para>If the current position is at list end, it is not modified and the function returns false.</para>
			/// </summary>
			/// <param name="offset">Number of items to count. A positive number counts next items. A negative number counts previous items.</param>
			/// <returns>True if the movement is possible or if <paramref name="offset"/> is zero.</returns>
			public override bool Move(int offset) => this.List.MovePosition(this, offset);

			/// <summary>
			/// Removes this position, as an item pointer. The list remains unchanged, the item itself is not removed.
			/// </summary>
			/// <returns></returns>
			public override bool Remove() => this.List.RemovePosition(this);

			/// <summary>
			/// Returns a number that depends on the relative position of <paramref name="other"/> in the list.
			/// <para>
			/// Returns 0 if this position equals the position of <paramref name="other"/>.
			/// Returns -1 if this position is before the position of <paramref name="other"/>.
			/// Returns 1 if this position is after the position of <paramref name="other"/> or if <paramref name="other"/> is invalid (null or removed).
			/// </para>
			/// </summary>
			/// <param name="other"></param>
			/// <returns></returns>
			public int CompareTo(PositionInList other)
			{
				if (other == null || other.IsRemoved)
					return 1;

				lock (this.List.SyncRoot)
				{
					if (this.Index < other.Index)
					{
						return -1;
					}
					if (this.Index > other.Index)
					{
						return 1;
					}
				}
				return 0;
			}

			/// <summary>
			/// Returns a number that depends on the relative position of <paramref name="other"/> in the list.
			/// <para>
			/// Returns <see cref="RelativePosition.Equal"/> if this position equals the position of <paramref name="other"/>.
			/// Returns <see cref="RelativePosition.Before"/> if this position is before the position of <paramref name="other"/>.
			/// Returns <see cref="RelativePosition.After"/> if this position is after the position of <paramref name="other"/> or if <paramref name="other"/> is invalid (null or removed).
			/// </para>
			/// </summary>
			/// <param name="other"></param>
			/// <returns></returns>
			public RelativePosition CompareToPosition(PositionInList other)
			{
				if (other == null || other.IsRemoved)
					return RelativePosition.After;

				lock (this.List.SyncRoot)
				{
					if (this.Index < other.Index)
					{
						return RelativePosition.Before;
					}
					if (this.Index > other.Index)
					{
						return RelativePosition.After;
					}
				}
				return RelativePosition.Equal;
			}

			/// <summary>
			/// Returns true if this position equals the position of <paramref name="other"/>.
			/// </summary>
			/// <param name="other"></param>
			/// <returns></returns>
			/// <exception cref="System.ArgumentException">Both positions are removed.</exception>
			public bool Equals(PositionInList other)
			{
				return this == other;
			}
		}

		#endregion

		/// <summary>
		/// Creates a new list.
		/// </summary>
		/// <param name="comparer">Item comparer, or null for the default comparer.</param>
		public PositionList(Func<T, T, bool> comparer = null)
			: this(0, comparer)
		{ }

		/// <summary>
		/// Creates a new list.
		/// </summary>
		/// <param name="capacity">The number of elements that the new list can initially store.</param>
		/// <param name="comparer">Item comparer, or null for the default comparer.</param>
		public PositionList(int capacity, Func<T, T, bool> comparer = null)
		{
			if (capacity < 0)
				throw new ArgumentOutOfRangeException(nameof(capacity));
			list = new System.Collections.Generic.List<T>(capacity);
			this.comparer = comparer ?? EqualityComparer<T>.Default.Equals;
		}

		/// <summary>
		/// Creates a position list wrapper for the given <paramref name="source"/> list.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="comparer">Item comparer, or null for the default comparer.</param>
		PositionList(System.Collections.Generic.IList<T> source, Func<T, T, bool> comparer)
		{
			list = source;
			this.comparer = comparer ?? EqualityComparer<T>.Default.Equals;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="PositionList{T}"/> class that contains elements copied from the specified collection and has sufficient capacity to accommodate the number of elements copied.
		/// </summary>
		/// <param name="items"></param>
		/// <returns></returns>
		public PositionList(System.Collections.Generic.IEnumerable<T> items)
			: this(items.Count())
		{
			if (items == null)
				throw new ArgumentNullException(nameof(items));
			foreach (T item in items)
				this.list.Add(item);
		}


		/// <summary>
		/// Initializes a new instance of the <see cref="PositionList{T}"/> class that contains elements copied from the specified list and has sufficient capacity to accommodate the number of elements copied.
		/// </summary>
		/// <param name="items"></param>
		/// <returns></returns>
		public PositionList(System.Collections.Generic.IList<T> items)
			: this(items.Count)
		{
			for (int i = 0; i < items.Count; i++)
				this.list.Add(items[i]);
		}

		/// <summary>
		/// Creates a new list that contains <paramref name="count"/> default values.
		/// </summary>
		/// <param name="count"></param>
		/// <returns></returns>
		public static PositionList<T> NewWithDefaultValues(int count)
		{
			var source = new System.Collections.Generic.List<T>(count);
			for (int i = 0; i < count; i++)
				source.Add(default(T));
			return new PositionList<T>(source);
		}

		/// <summary>
		/// Creates a new list that contains <paramref name="count"/> values.
		/// </summary>
		/// <param name="count"></param>
		/// <param name="value">Value of the new items.</param>
		/// <returns></returns>
		public static PositionList<T> NewWithValues(int count, T value = default(T))
		{
			var source = new System.Collections.Generic.List<T>(count);
			for (int i = 0; i < count; i++)
				source.Add(value);
			return new PositionList<T>(source);
		}

		#region Item functions

		/// <summary>
		/// Adds an item to the end of the list.
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public void Add(T item)
		{
			lock (accessLock)
			{
				var empty = IsEmpty;

				list.Add(item);
				if (empty)
				{
					foreach (var pos in positions)
						pos.Index = 0;
					_CheckAllPositionsOnDebug();
				}
			}
		}

		/// <summary>
		/// Converts <paramref name="position"/> and checks it is valid and it points at an item.
		/// Otherwize, returns null.
		/// </summary>
		/// <param name="position"></param>
		/// <returns></returns>
		PositionInList AsExistingPositionInList(Position position)
		{
			var pos = position as PositionInList;
			if (pos.List != this || pos.IsRemoved || !positions.Contains(pos) || pos.Index == Position.IndexInList.Undefined)
				return null;
			return pos;
		}

		/// <summary>
		/// Converts <paramref name="position"/> and checks it is valid.
		/// Otherwize, returns null.
		/// </summary>
		/// <param name="position"></param>
		/// <returns></returns>
		PositionInList AsPositionInList(Position position)
		{
			var pos = position as PositionInList;
			if (pos.List != this || pos.IsRemoved || !positions.Contains(pos))
				return null;
			return pos;
		}

		/// <summary>
		/// Removes all items.
		/// <para>Then all positions are set to undefined.</para>
		/// </summary>
		public void Clear()
		{
			lock (accessLock)
			{
				list.Clear();
				foreach (var position in positions)
					position.Index = Position.IndexInList.Undefined;
			}
		}

		/// <summary>
		/// Returns true if the list contains this item.
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public bool Contains(T item)
		{
			lock (accessLock)
				return list.Contains(item);
		}

		/// <summary>
		/// Gets or sets the item at the position.
		/// <para>If the list is empty, get returns a default value, and set does nothing.</para>
		/// <para>If <paramref name="position"/> is invalid, get and set do nothing.</para>
		/// </summary>
		/// <param name="position"></param>
		/// <returns></returns>
		public T this[Position position]
		{
			get
			{
				lock (accessLock)
				{
					var pos = position as PositionInList;
					if (pos == null || pos.List != this)
						return default(T);
					if (IsNotEmpty)
						return list[(int)pos.Index];
					else
						return default(T);
				}
			}
			set
			{
				lock (accessLock)
				{
					var pos = position as PositionInList;
					if (pos == null || pos.List != this)
						return;
					if (IsNotEmpty)
						list[(int)pos.Index] = value;
				}
			}
		}

		/// <summary>
		/// Returns true if the list is empty.
		/// </summary>
		public bool IsEmpty => list.Count == 0;

		/// <summary>
		/// Returns true if the list is not empty.
		/// </summary>
		public bool IsNotEmpty => list.Count != 0;

		/// <summary>
		/// Sets the item value at the given position.
		/// If there is an item at the position, replaces its value.
		/// If there is no item at the position, adds the item to the list.
		/// </summary>
		/// <param name="position"></param>
		/// <param name="item"></param>
		/// <returns>True if an item was replaced. Or false if the position is invalid.</returns>
		public bool ReplaceValueAt(Position position, T item)
		{
			lock (accessLock)
			{
				var pos = position as PositionInList;
				if (pos == null || pos.List != this)
					return false;
				if (list.Count != 0)
					list[(int)pos.Index] = item;
				else
					list.Add(item);
				return true;
			}
		}

		#endregion Item functions

		#region Position management

		PositionInList _NewPosition(PositionInList.IndexInList index)
		{
			Debug.Assert((int)index < list.Count && ((IsEmpty && index == Position.IndexInList.Undefined) || (IsNotEmpty && index >= 0)));
			var position = new PositionInList(this) { Index = index };
			this.positions.Add(position);
			_CheckPositionOnDebug(position);
			return position;
		}

		/// <summary>
		/// Checks potential internal programming errors, on debug only.
		/// </summary>
		/// <param name="position"></param>
		[Conditional("DEBUG")]
		void _CheckPositionOnDebug(Position position)
		{
			Debug.Assert(IsValidPosition(position));
			Debug.Assert((int)position.Index < list.Count && ((this.IsEmpty && position.Index == Position.IndexInList.Undefined) || (this.IsNotEmpty && position.Index >= 0)));
		}

		/// <summary>
		/// Checks potential internal programming errors, on debug only.
		/// </summary>
		[Conditional("DEBUG")]
		void _CheckAllPositionsOnDebug()
		{
			foreach (var pos in positions)
				_CheckPositionOnDebug(pos);
		}

		/// <summary>
		/// Adds an item to the end of the list, then returns a new position that points at that item.
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public PositionInList AddItemAndGetPosition(T item)
		{
			lock (accessLock)
			{
				var position = _NewPosition((PositionInList.IndexInList)list.Count);
				list.Add(item);
				return position;
			}
		}

		/// <summary>
		/// Deletes all values between <paramref name="start"/> and <paramref name="end"/>, <paramref name="start"/> and <paramref name="end"/> being included.
		/// Item values are replaced by default of <typeparamref name="T"/>.
		/// </summary>
		/// <param name="start"></param>
		/// <param name="end"></param>
		/// <returns></returns>
		public int DeleteValues(Position start, Position end)
		{
			return this.DeleteValues(start, true, end, true);
		}

		/// <summary>
		/// Deletes all values between <paramref name="start"/> and <paramref name="end"/>, <paramref name="start"/> and <paramref name="end"/> being included or not.
		/// Item values are replaced by default of <typeparamref name="T"/>.
		/// </summary>
		/// <param name="start"></param>
		/// <param name="includeStart"></param>
		/// <param name="end"></param>
		/// <param name="includeEnd"></param>
		/// <returns></returns>
		public int DeleteValues(Position start, bool includeStart, Position end, bool includeEnd)
		{
			return this.ReplaceValues(start, includeStart, end, includeEnd, default(T));
		}

		/// <summary>
		/// Replaces all values between <paramref name="start"/> and <paramref name="end"/>, these positions being included or not.
		/// Item values are replaced by <paramref name="value"/>.
		/// </summary>
		/// <param name="start"></param>
		/// <param name="includeStart"></param>
		/// <param name="end"></param>
		/// <param name="includeEnd"></param>
		/// <param name="value"></param>
		/// <returns>Count of replaced values.</returns>
		public int ReplaceValues(Position start, bool includeStart, Position end, bool includeEnd, T value)
		{
			lock (accessLock)
			{
				var startPos = start as PositionInList;
				if (startPos == null || startPos.List != this)
					return 0;
				var endPos = end as PositionInList;
				if (endPos == null || endPos.List != this)
					return 0;

				var startIndex = startPos.Index;
				if (!includeStart) startIndex++;
				if (startIndex == Position.IndexInList.Undefined) startIndex = 0;
				var endIndex = endPos.Index;
				if (!includeEnd) endIndex--;
				int replacementCount = 0;

				for (int i = (int)startIndex; i <= (int)endIndex; i++)
				{
					Debug.Assert(i != (int)PositionInList.IndexInList.Undefined);
					list[i] = value;
					replacementCount++;
				}
				return replacementCount;
			}
		}

		/// <summary>
		/// Makes a copy of <paramref name="position"/>. This copy will be maintained by the list. The copy and the original are independent.
		/// </summary>
		/// <param name="position"></param>
		/// <returns>The duplicated position, or null if source <paramref name="position"/> is not valid.</returns>
		public PositionInList DuplicatePosition(Position position)
		{
			lock (accessLock)
			{
				_CheckPositionOnDebug(position);
				var pos = position as PositionInList;
				if (pos == null || pos.List != this)
					return null;
				return _NewPosition(pos.Index);
			}
		}

		/// <summary>
		/// Creates a position at the last item of the list.
		/// </summary>
		/// <returns>A valid position, even if the list is empty.</returns>
		public PositionInList GetEndPosition()
		{
			lock (accessLock)
				return _NewPosition((PositionInList.IndexInList)(list.Count - 1));
		}

		/// <summary>
		/// Creates a <see cref="Position"/> that points at the first occurence of the given item.
		/// <para>If <paramref name="item"/> is not in the list, returns null.</para>
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public PositionInList GetFirstOccurenceOf(T item)
		{
			lock (accessLock)
				return _GetFirstOccurenceOf(item);
		}

		PositionInList _GetFirstOccurenceOf(T item)
		{
			if (IsNotEmpty)
				for (int i = 0; i < list.Count; i++)
					if (comparer(list[i], item))
						return _NewPosition((PositionInList.IndexInList)i);
			return null;
		}

		PositionInList.IndexInList _GetIndexOfFirstOccurenceOf(T item)
		{
			if (IsNotEmpty)
				for (int i = 0; i < list.Count; i++)
					if (comparer(list[i], item))
						return (PositionInList.IndexInList)i;
			return Position.IndexInList.Undefined;
		}

		/*/// <summary>
		/// Enumerates the list items.
		/// <para>Unlike <see cref="GetEnumerator"/>, the list is locked while enumerating it.</para>
		/// </summary>
		/// <returns></returns>
		public IEnumerator<T> GetLockedEnumerator()
		{
			lock (accessLock)
				return list.GetEnumerator();
		}*/

		/// <summary>
		/// Creates a position at the next occurence of the item, after or from <paramref name="position"/>.
		/// <para>If no item is found, returns null.</para>
		/// <para>The item at <paramref name="position"/> is ignored.</para>
		/// </summary>
		/// <param name="item"></param>
		/// <param name="position"></param>
		/// <param name="afterThisPosition">If true, the function searches from the position after <paramref name="position"/>. If false, the function searches from <paramref name="position"/>.</param>
		/// <returns></returns>
		public PositionInList GetNextOccurenceOf(T item, Position position, bool afterThisPosition)
		{
			lock (accessLock)
			{
				_CheckPositionOnDebug(position);
				var pos = position as PositionInList;
				if (pos == null || pos.List != this)
					return null;
				var startIndex = afterThisPosition ? pos.Index + 1 : pos.Index;
				for (int i = (int)startIndex; i < list.Count; i++)
					if (comparer(list[i], item))
						return _NewPosition((PositionInList.IndexInList)i);
				return null;
			}
		}

		PositionInList.IndexInList _GetNextOccurenceOf(T item, PositionInList.IndexInList index)
		{
			Debug.Assert(index >= 0);
			for (int i = (int)index; i < list.Count; i++)
				if (comparer(list[i], item))
					return (PositionInList.IndexInList)i;
			return Position.IndexInList.Undefined;
		}

		/// <summary>
		/// Creates a position at the previous occurence of the item, before the given position.
		/// <para>If no item is found, returns null.</para>
		/// <para>The item at <paramref name="position"/> is ignored.</para>
		/// </summary>
		/// <param name="item"></param>
		/// <param name="position"></param>
		/// <returns></returns>
		public PositionInList GetPreviousOccurenceOf(T item, Position position)
		{
			lock (accessLock)
			{
				_CheckPositionOnDebug(position);
				var pos = position as PositionInList;
				if (pos == null || pos.List != this)
					return null;
				for (int i = (int)pos.Index - 1; i >= 0; i--) // moves backward.
					if (comparer(list[i], item))
						return _NewPosition((PositionInList.IndexInList)i);
				return null;
			}
		}

		/// <summary>
		/// Creates a <see cref="Position"/> at the first item of the list.
		/// </summary>
		public PositionInList GetStartPosition()
		{
			lock (accessLock)
				return _NewPosition(IsNotEmpty ? PositionInList.IndexInList.Start : PositionInList.IndexInList.Undefined);
		}

		/// <summary>
		/// Inserts an item before or after <paramref name="position"/>. After that, <paramref name="position"/> still points at the old item.
		/// </summary>
		/// <param name="position"></param>
		/// <param name="insertionPosition">The relative position of the new item, before or after (not equal) <paramref name="position"/>.</param>
		/// <param name="item"></param>
		/// <exception cref="System.ArgumentOutOfRangeException"><paramref name="insertionPosition"/> is <see cref="RelativePosition.Equal"/>.</exception>
		public bool Insert(Position position, RelativePosition insertionPosition, T item)
		{
			if (insertionPosition == RelativePosition.Equal)
				throw new ArgumentOutOfRangeException(nameof(insertionPosition));
			lock (accessLock)
			{
				var pos = AsPositionInList(position);
				if (pos == null)
					return false;
				return _Insert(pos, insertionPosition, item) != Position.IndexInList.Undefined;
			}
		}

		/// <summary>
		/// Inserts an item after <paramref name="position"/>. After that, <paramref name="position"/> still points at the old item.
		/// </summary>
		/// <param name="position"></param>
		/// <param name="item"></param>
		/// <returns></returns>
		public bool InsertAfter(Position position, T item)
		{
			return this.Insert(position, RelativePosition.After, item);
		}

		/// <summary>
		/// Inserts an item before <paramref name="position"/>. After that, <paramref name="position"/> still points at the old item.
		/// </summary>
		/// <param name="position"></param>
		/// <param name="item"></param>
		/// <returns></returns>
		public bool InsertBefore(Position position, T item)
		{
			return this.Insert(position, RelativePosition.Before, item);
		}

		/// <summary>
		/// Inserts an item before or after <paramref name="position"/>. After that, <paramref name="position"/> still points at the old item.
		/// </summary>
		/// <param name="position"></param>
		/// <param name="insertionPosition">The relative position of the new item.</param>
		/// <param name="item"></param>
		/// <returns>The new position or null if <paramref name="position"/> is invalid or if <paramref name="insertionPosition"/> is <see cref="RelativePosition.Equal"/>..</returns>
		public PositionInList InsertAndGetNewPosition(Position position, RelativePosition insertionPosition, T item)
		{
			if (insertionPosition == RelativePosition.Equal)
				return null;

			lock (accessLock)
			{
				var pos = AsPositionInList(position);
				if (pos == null)
					return null;
				var newIndex = _Insert(pos, insertionPosition, item);
				if (newIndex == Position.IndexInList.Undefined)
					return null;
				return _NewPosition(newIndex);
			}
		}

		PositionInList.IndexInList _Insert(PositionInList position, RelativePosition insertionPosition, T value)
		{
			Debug.Assert(insertionPosition != RelativePosition.Equal);
			_CheckPositionOnDebug(position);

			if (list.Count == 0)
			{
				this.list.Add(value);
				// updates all positions, including the parameter:
				foreach (var p in positions)
					p.Index = Position.IndexInList.Start;
				_CheckAllPositionsOnDebug();
				return Position.IndexInList.Start;
			}
			int index;
			switch (insertionPosition)
			{
				case RelativePosition.Before:
					index = (int)position.Index; break;
				case RelativePosition.After:
				default:
					index = (int)position.Index + 1;
					Debug.Assert(index <= list.Count);
					if (index == list.Count)
					{
						list.Add(value);
						// no use to update any position.
						return (PositionInList.IndexInList)index;
					}
					break;
			}
			list.Insert(index, value);

			// updates all positions, including the parameter:
			foreach (var p in positions)
			{
				if ((int)p.Index >= index)
					p.Index++;
			}
			_CheckAllPositionsOnDebug();
			Debug.Assert(this.IsEmpty ? this.positions.All(p => p.Index == 0) : true);
			return (PositionInList.IndexInList)index;
		}

		/// <summary>
		/// Returns true if <paramref name="position"/> is at end of the list.
		/// <para>If the list is empty, returns false.</para>
		/// </summary>
		/// <param name="position"></param>
		/// <returns></returns>
		public bool IsPositionAtEnd(Position position)
		{
			lock (accessLock)
			{
				_CheckPositionOnDebug(position);
				var pos = position as PositionInList;
				if (pos == null || pos.List != this || IsEmpty)
					return false;
				return (int)pos.Index == (list.Count - 1);
			}
		}

		/// <summary>
		/// Returns true if <paramref name="position"/> is at start of the list.
		/// <para>If the list is empty, returns false.</para>
		/// </summary>
		/// <param name="position"></param>
		/// <returns></returns>
		public bool IsPositionAtStart(Position position)
		{
			lock (accessLock)
			{
				_CheckPositionOnDebug(position);
				var pos = position as PositionInList;
				if (pos == null || pos.List != this)
					return false;
				return pos.Index == 0;
			}
		}

		/// <summary>
		/// True if <paramref name="position"/> is non-null and is managed by this list.
		/// <para>If the list is empty, returns true.</para>
		/// </summary>
		/// <param name="position"></param>
		/// <returns></returns>
		public bool IsValidPosition(Position position)
		{
			lock (accessLock)
			{
				var pos = position as PositionInList;
				return pos != null && pos.List == this
					&& positions.Contains(pos) // proves this position is managed by the list.
					&& !pos.IsRemoved;
			}
		}

		/// <summary>
		/// Moves an item from <paramref name="position"/> to <paramref name="destination"/>.
		/// </summary>
		/// <param name="position"></param>
		/// <param name="destination"></param>
		/// <param name="insertionPosition">Realtive to <paramref name="destination"/>.</param>
		/// <param name="returnsNewPosition"></param>
		/// <returns>If <paramref name="returnsNewPosition"/> is true and the item is moved successfully, returns the new position. Otherwize, returns null (even if the operation is successfull).</returns>
		public PositionInList MoveItem(Position position, Position destination, RelativePosition insertionPosition, bool returnsNewPosition)
		{
			lock (accessLock)
			{
				var startPos = AsExistingPositionInList(position);
				var destPos = AsExistingPositionInList(destination);
				if (startPos == null || destPos == null || insertionPosition == RelativePosition.Equal)
					return null;
				var value = list[(int)position.Index];

#if true // to be revised
				var oldIndex = position.Index;
				_RemoveItemAt(oldIndex);
				var index = _Insert(destPos, insertionPosition, value);
				Debug.Assert(index != Position.IndexInList.Undefined);
#else // todo: finish and test it
				var index = destination.Index ;
				if (insertionPosition == RelativePosition.After)
					index++;
				if (index != startPos.Index)
				{
					if ((int)index == list.Count)
						list.Add(value);
					else
						list.Insert((int)index, value);
					Mise à jour
				}
#endif
				_CheckAllPositionsOnDebug();
				if (returnsNewPosition)
					return _NewPosition(index);
				return null;
			}
		}

		/// <summary>
		/// Moves the position to another item.
		/// <para>If the current position is at list end, it is not modified and the function returns false.</para>
		/// </summary>
		/// <param name="position"></param>
		/// <param name="offset">Number of items to count. A positive number counts next items. A negative number counts previous items.</param>
		/// <returns>True if the movement is possible or if <paramref name="offset"/> is zero. False if <paramref name="position"/> is invalid.</returns>
		public bool MovePosition(Position position, int offset)
		{
			if (offset == 0)
				return true;
			lock (accessLock)
			{
				_CheckPositionOnDebug(position);
				if (this.IsEmpty)
					return false;
				var pos = position as PositionInList;
				if (pos == null || pos.List != this)
					return false;
				var finalIndex = pos.Index + offset;
				if (finalIndex < 0 || (int)finalIndex >= list.Count)
					return false;
				pos.Index = finalIndex;
				_CheckPositionOnDebug(pos);
				return true;
			}
		}

		/// <summary>
		/// Moves the position one item backward.
		/// </summary>
		/// <param name="position"></param>
		/// <returns></returns>
		public bool MovePositionBackward(Position position)
		{
			return MovePosition(position, -1);
		}

		/// <summary>
		/// Moves the position one item forward.
		/// </summary>
		/// <param name="position"></param>
		/// <returns></returns>
		public bool MovePositionForward(Position position)
		{
			return MovePosition(position, 1);
		}

		/// <summary>
		/// Return true if <paramref name="position"/> is valid and points at an item.
		/// Or false if the list is empty or if the position is invalid (null or not from this list).
		/// </summary>
		/// <param name="position"></param>
		/// <returns></returns>
		public bool PositionPointsAtAnItem(Position position)
		{
			//lock (accessLock)
			{
				var pos = position as PositionInList;
				if (pos == null || pos.List != this)
					return false;
				return pos.Index != Position.IndexInList.Undefined;
			}
		}


		/// <summary>
		/// Removes the item at <paramref name="position"/>.
		/// <para>Then <paramref name="position"/> points at the next item, or the last item of the list.</para>
		/// </summary>
		/// <param name="position"></param>
		/// <returns>True if there is an item at the <paramref name="position"/>.</returns>
		public bool RemoveItemAt(Position position)
		{
			lock (accessLock)
			{
				_CheckPositionOnDebug(position);
				if (IsEmpty)
					return false;
				var pos = position as PositionInList;
				if (pos == null || pos.List != this)
					return false;
				return _RemoveItemAt(pos);
			}
		}

		bool _RemoveItemAt(PositionInList position)
		{
			Debug.Assert(IsNotEmpty);
			_CheckPositionOnDebug(position);
			var index = position.Index;
			return _RemoveItemAt(index);
		}

		bool _RemoveItemAt(PositionInList.IndexInList index)
		{
			Debug.Assert(IsNotEmpty);
			if (index == Position.IndexInList.Undefined)
				return false;
			var isLastItem = (int)index == list.Count - 1;
			list.RemoveAt((int)index);
			if (isLastItem)
			{
				foreach (var pos in positions)
					if (pos.Index == index)
						pos.Index--;
			}
			else
				foreach (var pos in positions)
					if (pos.Index > index)
						pos.Index--;
			_CheckAllPositionsOnDebug();
			return true;
		}

		/// <summary>
		/// Removes this position, as an item pointer. The list remains unchanged, the item itself is not removed.
		/// </summary>
		/// <param name="position"></param>
		/// <returns></returns>
		public bool RemovePosition(Position position)
		{
			lock (accessLock)
			{
				_CheckPositionOnDebug(position);
				var pos = position as PositionInList;
				if (pos == null || pos.List != this)
					return false;
				pos.List = null;
				pos.Index = Position.IndexInList.Undefined;
				return positions.Remove(pos);
			}
		}

#endregion Position management

#region IProducerConsumerCollection<T>

		/// <summary>
		/// returns the number of items in the list.
		/// </summary>
		public int Count => list.Count; // no locking since List.Count is atomic.

		/// <summary>
		/// Lets you do atomic operations.
		/// </summary>
		public object SyncRoot => accessLock;

		/// <summary>
		/// Returns true.
		/// </summary>
		public bool IsSynchronized => true;

		/// <summary>
		/// Returns false.
		/// </summary>
		public bool IsReadOnly => false;

		/// <summary>
		/// Copy the list to the given <paramref name="array"/> at <paramref name="index"/>.
		/// <para>The list is locked while copying.</para>
		/// </summary>
		/// <param name="array">The array where list items are copied to.</param>
		/// <param name="index">Index in the <paramref name="array"/>.</param>
		public void CopyTo(T[] array, int index)
		{
			lock (accessLock)
				list.CopyTo(array, index);
		}

		/// <summary>
		/// Copy the list to the given <paramref name="array"/> at <paramref name="index"/>.
		/// <para>The list is locked while copying.</para>
		/// </summary>
		/// <param name="array">The array where list items are copied to.</param>
		/// <param name="index">Index in the <paramref name="array"/>.</param>
		public void CopyTo(Array array, int index)
		{
			lock (accessLock)
			{
				int iTarget = index;
				for (int i = 0; i < list.Count && iTarget < array.Length; i++)
					array.SetValue(list[i], iTarget++);
			}
		}

		/// <summary>
		/// Enumerates the list items.
		/// <para>Please note this enumeration _does_ lock the list. So parallel accesses are paused.</para>
		/// </summary>
		/// <returns></returns>
		public IEnumerator<T> GetEnumerator()
		{
			lock (accessLock)
			{
				var position = this.GetStartPosition();
				for (; ; )
				{
					bool exists = TryGetItemAt(position, out T item);
					if (exists)
						yield return item;
					else
						break;
					bool nextExists = MovePositionForward(position);
					if (!nextExists)
						break;
				}
				position.Remove();
			}
		}

		/// <summary>
		/// Locks the list then creates an array and copy all items to the array.
		/// </summary>
		/// <returns></returns>
		public T[] ToArray()
		{
			lock (accessLock)
				return (list as System.Collections.Generic.List<T>)?.ToArray()
					?? System.Linq.Enumerable.ToArray(list);
		}

		/// <summary>
		/// Adds the item at the end of the list.
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public bool TryAdd(T item)
		{
			lock (accessLock)
			{
				var empty = IsEmpty;
				list.Add(item);
				if (empty)
					foreach (var pos in positions)
						pos.Index = 0;
				_CheckAllPositionsOnDebug();
				return true;
			}
		}

		/// <summary>
		/// Gets the item value at <paramref name="position"/>.
		/// </summary>
		/// <param name="position"></param>
		/// <param name="item"></param>
		/// <returns>True if <paramref name="position"/> is valid and an item was read.</returns>
		public bool TryGetItemAt(Position position, out T item)
		{
			lock (accessLock)
			{
				var pos = position as PositionInList;
				if (pos != null && pos.List == this && IsNotEmpty)
				{
					item = list[(int)pos.Index];
					return true;
				}
				item = default(T);
				return false;
			}
		}

		/// <summary>
		/// Sets the item value at <paramref name="position"/>.
		/// </summary>
		/// <param name="position"></param>
		/// <param name="value"></param>
		/// <returns>True if <paramref name="position"/> is valid and an item was written.</returns>
		public bool TrySetItemAt(Position position, T value)
		{
			lock (accessLock)
			{
				var pos = position as PositionInList;
				if (pos != null && pos.List == this && IsNotEmpty)
				{
					list[(int)pos.Index] = value;
					return true;
				}
				return false;
			}
		}

		/// <summary>
		/// Extracts the last item of the list and returns it.
		/// <para>The item is removed from the list.</para>
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public bool TryTake(out T item)
		{
			lock (accessLock)
			{
				if (list.Count == 0)
				{
					item = default(T);
					return false;
				}
				// removes the last item.
				var index = list.Count - 1;
				list.RemoveAt(index);
				foreach (var position in positions)
				{
					if ((int)position.Index == index)
						position.Index--;
				}
				_CheckAllPositionsOnDebug();
				item = list[index];
				return true;
			}
		}

		/// <summary>
		/// Enumerates the list items.
		/// <para>Please note this enumeration _does_ not lock the list.</para>
		/// </summary>
		/// <returns></returns>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

#endregion

#region ICollection<T>

		/// <summary>
		/// Removes the first occurence of <paramref name="item"/>.
		/// </summary>
		/// <param name="item"></param>
		/// <returns>True if an occurence was found.</returns>
		public bool Remove(T item)
		{
			lock (accessLock)
			{
				var index = _GetIndexOfFirstOccurenceOf(item);
				return _RemoveItemAt(index);
			}
		}

		/// <summary>
		/// Removes the first occurence of <paramref name="item"/>, searching from <paramref name="position"/>.
		/// </summary>
		/// <param name="position"></param>
		/// <param name="item"></param>
		/// <param name="afterThisPosition">If true, the function searches from the position after <paramref name="position"/>. If false, the function searches from <paramref name="position"/>.</param>
		/// <returns>True if an occurence was found.</returns>
		public bool RemoveFrom(Position position, T item, bool afterThisPosition)
		{
			lock (accessLock)
			{
				var pos = position as PositionInList;
				if (pos == null || pos.List != this)
					return false;
				var startIndex = afterThisPosition ? pos.Index + 1 : pos.Index;
				var index = _GetNextOccurenceOf(item, startIndex);
				return _RemoveItemAt(index);
			}
		}

		/// <summary>
		/// Removes all occurences of <paramref name="item"/>.
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public int RemoveAllOccurencesOf(T item)
		{
			lock (accessLock)
			{
				int n = 0;
				PositionInList.IndexInList index = 0;
				for (; ; )
				{
					index = _GetNextOccurenceOf(item, index);
					if (!_RemoveItemAt(index))
						break;
					n++;
				}
				return n;
			}
		}

#endregion

		/// <summary>
		/// Determines whether the items of <paramref name="a"/> are equal to the items of <paramref name="b"/>.
		/// <para>The comparison is done using the comparer of <paramref name="a"/>.</para>
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		public static bool operator ==(PositionList<T> a, PositionList<T> b)
		{
			if (object.ReferenceEquals(a, b))
				return true;
			if (object.ReferenceEquals(a, null) || object.ReferenceEquals(b, null))
				return false;
			if (a.Count == 0 && b.Count == 0)
				return true;

			lock (a.accessLock)
				lock (b.accessLock)
				{
					if (a.Count != b.Count)
						return false;
					var listA = a.list;
					var listB = b.list;
					for (int i = 0; i < listA.Count; i++)
						if (!a.comparer(listA[i], listB[i]))
							return false;
					return true;
				}
		}

		/// <summary>
		/// Determines whether the items of <paramref name="a"/> are different from the items of <paramref name="b"/>.
		/// <para>The comparison is done using the comparer of <paramref name="b"/>.</para>
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		public static bool operator !=(PositionList<T> a, PositionList<T> b)
		{
			return !(a == b);
		}

		/// <summary>
		/// Determines whether the specified object is equal to the current object.
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public override bool Equals(object obj)
		{
			return this == (obj as PositionList<T>);
		}

		/// <summary>
		/// Returns the default hash code.
		/// <para>Items are not taken into account.</para>
		/// </summary>
		/// <returns></returns>
		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
	}

#if false
	/// <summary>
	/// Extension functions for <see cref="Position.List{T}"/>
	/// </summary>
	public static class PositionListExtender
	{
	}
#endif

	/// <summary>
	/// Similar to <see cref="System.String"/> or <see cref="System.Text.StringBuilder"/>, except chars are pointed by Positions in place of indexes.
	/// </summary>
	public class PositionString : PositionList<char>, IEquatable<string>
	{
		/// <summary>
		/// A new empty instance.
		/// </summary>
		public PositionString()
		{ }

		/// <summary>
		/// A new empty instance with a reserved capacity.
		/// </summary>
		public PositionString(int capacity)
		{ }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="text"></param>
		public PositionString(string text)
			: base(text)
		{ }

		/// <summary>
		/// Converts <paramref name="text"/> to a <see cref="PositionString"/>.
		/// </summary>
		/// <param name="text"></param>
		public static implicit operator PositionString(string text)
		{
			return new PositionString(text);
		}

		/// <summary>
		/// Converts <paramref name="positionString"/> to a System.String.
		/// </summary>
		/// <param name="positionString"></param>
		public static implicit operator string(PositionString positionString)
		{
			return positionString.ToString();
		}

		/// <summary>
		/// Converts the value of this instance to a System.String.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			lock (base.accessLock)
			{
				var sb = new StringBuilder(base.list.Count);
				var charList = base.list;
				for (int i = 0; i < charList.Count; i++)
					sb.Append(charList[i]);
				return sb.ToString();
			}
		}

		/// <summary>
		/// Returns true if this string equals <paramref name="other"/>.
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public bool Equals(string other)
		{
			lock (base.accessLock)
			{
				var charList = base.list;
				if (other == null || charList.Count != other.Length)
					return false;
				for (int i = 0; i < charList.Count; i++)
					if (other[i] != charList[i])
						return false;
				return true;
			}
		}

		/// <summary>
		/// <para>Implementation note: we do not use an equivalent of dotnet's <see cref="System.String.GetHashCode"/> which is patented (and patents are not covered by the MIT license of dotnet Core). See http://www.google.com/patents/US20130262421</para>
		/// So please do not compare hash codes from <see cref="System.String.GetHashCode"/> and this one.
		/// </summary>
		/// <returns></returns>
		public override int GetHashCode()
		{
			lock (base.accessLock)
			{
				int code = System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(this); // Differentiates instances.

				var charList = base.list;
				for (int i = 0; i < charList.Count; i++)
					code = 31 * code + charList[i].GetHashCode();
				return code;
			}
		}
	}

}