
// Copyright (c) Christophe Bertrand
// https://github.com/ChrisBertrandDotNet/CB-Helpers
// https://chrisbertrand.net

/* Requires:
 * (nothing)
ONLY WHEN DOING INTERNAL TESTS:
 * CB_DOTNET\_TESTS\Test.cs
 * cb.data\CB.Data.EnumHelper.cs
 * cb.validation\CB.Validation.ContractConditions.cs
 * CB.Reflection\CB.Reflection.TypeEx.cs
*/

/*
 * Facilitates comparisons by giving operators (== , !=, <, <=, >, >=) and comparers (CompareTo, Equals).
 * Implements IComparer<T>, IEqualityComparer<T>, IComparable<T>.
 */

#if TEST
using CBdotnet.Test;
#endif
using System;
using System.Collections.Generic;

namespace CB.Data
{
	/// <summary>
	/// Implements common comparisons for your class: (== , !=, &lt;, &lt;=, &gt;, &gt;=).
	/// <para>
	/// The inheriting class just need to implement Equals and CompareTo.</para>
	/// <para>
	/// Reminder: this.CompareTo(b) should return
	/// 0 when this==b,
	/// -1 when this &lt; b and
	/// 1 when this &gt; b.
	/// </para>
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public abstract class ComparerAndOperators<T> : IComparer<ComparerAndOperators<T>>, IEqualityComparer<ComparerAndOperators<T>>, IComparable<ComparerAndOperators<T>>
		where T : ComparerAndOperators<T>
	{

		/// <summary>
		/// this.CompareTo(other) should return
		/// 0 when this==other,
		/// -1 when this &lt; other and
		/// 1 when this &gt; other.
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public abstract int CompareTo(T other);

		/// <summary>
		/// Returns true if the given item is equal to this one.
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public abstract bool Equals(T other);

		/// <summary>
		/// Returns 
		/// 0 when x == y,
		/// -1 when x &lt; y and
		/// 1 when x &gt; y.
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		public int Compare(ComparerAndOperators<T> x, ComparerAndOperators<T> y)
		{
			if (object.ReferenceEquals(x, null))
				if (object.ReferenceEquals(y, null))
					return 0;
				else
					return -1;
			return ((T)x).CompareTo((T)y);
		}

		/// <summary>
		/// Returns true if <paramref name="x"/> is equal to <paramref name="y"/>.
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		public bool Equals(ComparerAndOperators<T> x, ComparerAndOperators<T> y)
		{
			if (object.ReferenceEquals(x, null))
				if (object.ReferenceEquals(y, null))
					return true;
				else
					return false;
			return ((T)x).Equals((T)y);
		}

		/// <summary>
		/// Returns the hash code of <paramref name="obj"/>.
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public int GetHashCode(ComparerAndOperators<T> obj)
		{
			if (object.ReferenceEquals(obj, null))
				throw new ArgumentNullException();
			return obj.GetHashCode();
		}

		/// <summary>
		/// Returns true if a == b.
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		public static bool operator ==(ComparerAndOperators<T> a, ComparerAndOperators<T> b)
		{
			if (object.ReferenceEquals(a, b))
				return true;
			if (object.ReferenceEquals(a, null) || object.ReferenceEquals(b, null))
				return false;
			return ((T)a).Equals((T)b);
		}

		/// <summary>
		/// Returns true if a != b.
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		public static bool operator !=(ComparerAndOperators<T> a, ComparerAndOperators<T> b)
		{
			return !(a == b);
		}

		/// <summary>
		/// Returns true if a &gt;= b.
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		public static bool operator >=(ComparerAndOperators<T> a, ComparerAndOperators<T> b)
		{
			return ((T)a).CompareTo((T)b) >= 0;
		}

		/// <summary>
		/// Returns true if a &lt;= b.
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		public static bool operator <=(ComparerAndOperators<T> a, ComparerAndOperators<T> b)
		{
			return ((T)a).CompareTo((T)b) <= 0;
		}

		/// <summary>
		/// Returns true if a &gt; b.
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		public static bool operator >(ComparerAndOperators<T> a, ComparerAndOperators<T> b)
		{
			return ((T)a).CompareTo((T)b) > 0;
		}

		/// <summary>
		/// Returns true if a &lt; b.
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		public static bool operator <(ComparerAndOperators<T> a, ComparerAndOperators<T> b)
		{
			return ((T)a).CompareTo((T)b) < 0;
		}

		/// <summary>
		/// Returns the hash code.
		/// </summary>
		/// <returns></returns>
		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		/// <summary>
		/// Returns true if <paramref name="obj"/> equals this instance.
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public override bool Equals(object obj)
		{
				if (object.ReferenceEquals(obj, null))
				return false;
			var other = obj as T;
			if (object.ReferenceEquals(other, null))
				return false;
			return this == other;
		}

		/// <summary>
		/// Returns 
		/// 0 when this == other,
		/// -1 when this &lt; other and
		/// 1 when this &gt; other.
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public int CompareTo(ComparerAndOperators<T> other)
		{
			return this.CompareTo((T)other);
		}
	}
}

#if TEST
namespace Test
{
	class Int32Comparer : CB.Data.ComparerAndOperators<Int32Comparer>
	{
		public readonly int Value;

		public Int32Comparer(int value)
		{
			this.Value = value;
		}

		public override int CompareTo(Int32Comparer other)
		{
			return this.Value.CompareTo(other.Value);
		}

		public override bool Equals(Int32Comparer other)
		{
			return this.Value == other.Value;
		}

		[Test]
		static void ComparerAndOperators_Test()
		{
			var a5 = new Test.Int32Comparer(5);
			var b5 = new Test.Int32Comparer(5);
			var deux = new Test.Int32Comparer(2);
			var dix = new Test.Int32Comparer(10);
			var zéro = new Test.Int32Comparer(0);
			Testeur.TesteVérité(a5 > zéro);
			Testeur.TesteVérité(a5 >= zéro);
			Testeur.TesteVérité(a5 >= b5);
			Testeur.TesteVérité(a5 <= b5);
			Testeur.TesteFausseté(a5 <= zéro);
			Testeur.TesteFausseté(a5 < zéro);
			Testeur.TesteVérité(a5 == b5);
			Testeur.TesteVérité(a5 != deux);
			Testeur.TesteVérité(a5.Equals(b5));
			Testeur.TesteVérité(a5.CompareTo(deux) == 1);
			Testeur.TesteVérité(a5.CompareTo(b5) == 0);
			Testeur.TesteVérité(a5.CompareTo(dix) == -1);
		}
	}
}
#endif