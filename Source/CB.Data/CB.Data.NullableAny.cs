
// Copyright Christophe Bertrand
// https://github.com/ChrisBertrandDotNet/CB-Helpers
// https://chrisbertrand.net

/*
Similar to System.Nullable<T>, but applicable to both class and structure types.
Intended to let generic functions return nullable values that apply equally to both classes and structures.
 */

#if TEST
using CBdotnet.Test;
#endif
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

namespace CB.Data
{
	/// <summary>
	/// A nullable value of type T where T can be a structure or a class, including primitive types.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public struct NullableAny<T> : IEquatable<NullableAny<T>>, IEquatable<T>, IComparable<NullableAny<T>>, IComparable<T>, IComparer<NullableAny<T>>, IComparer<T>
	{
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		static readonly IEqualityComparer<T> equalityComparer = EqualityComparer<T>.Default;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		static readonly Comparer<T> rankComparer = System.Collections.Generic.Comparer<T>.Default;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		static readonly string erreur = string.Format(CultureInfo.InvariantCulture, "{0}<{1}> has not been built with a value.", typeof(NullableAny<T>).Name, typeof(T).Name);

		/// <summary>
		/// A NullableAny where no value has been set.
		/// </summary>
		public static NullableAny<T> Undefined { get { return new NullableAny<T>(); } }

#if !DEBUG
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
#endif
		T _Value;
#if !DEBUG
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
#endif
		bool _HasValue;

		/// <summary>
		/// Gets the value. If the value is not defined, returns default(T).
		/// </summary>
		/// <returns></returns>
		public T GetValueOrDefault()
		{
			return this._HasValue ? this._Value : default(T);
		}

		/// <summary>
		/// Gets the value. If the value is not defined, returns <paramref name="defaultValue"/>.
		/// </summary>
		/// <param name="defaultValue"></param>
		/// <returns></returns>
		public T GetValueOrDefault(T defaultValue)
		{
			return this._HasValue ? this._Value : defaultValue;
		}

		/// <summary>
		/// Returns true if there is no value, or if the value is null.
		/// </summary>
		public bool HasNoValue
		{ get { return !this._HasValue; } }

		/// <summary>
		/// Returns true if there is value, and if the value is not null.
		/// </summary>
		public bool HasValue
		{ get { return this._HasValue; } }

		/// <summary>
		/// Builds a structure with the given value.
		/// </summary>
		/// <param name="value"></param>
		public NullableAny(T value)
		{
			this._Value = value;
			this._HasValue = value != null;
		}

		/// <summary>
		/// Gets or sets the value.
		/// <para>If a null value is set, HasValue will be set to false.</para>
		/// <para>Please note 'set' is not thread-safe.</para>
		/// </summary>
		/// <exception cref="InvalidOperationException">(get) No value has been set, or the value has been set to null.</exception>
		public T Value
		{
			get
			{
				if (!this._HasValue)
					throw new InvalidOperationException(erreur);
				return this._Value;
			}
			set
			{
				this._HasValue = value != null;
				this._Value = value;
			}
		}

		/// <summary>
		/// True if a == b.
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		public static bool operator ==(NullableAny<T> a, NullableAny<T> b)
		{
			return a.Equals(b);
		}

		/// <summary>
		/// True if a != b.
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		public static bool operator !=(NullableAny<T> a, NullableAny<T> b)
		{
			return !a.Equals(b);
		}

		/// <summary>
		/// True if a &lt; b.
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		public static bool operator <(NullableAny<T> a, NullableAny<T> b)
		{
			return a.CompareTo(b) < 0;
		}

		/// <summary>
		/// True if a &lt;= b.
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		public static bool operator <=(NullableAny<T> a, NullableAny<T> b)
		{
			return a.CompareTo(b) <= 0;
		}

		/// <summary>
		/// True if a &gt; b.
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		public static bool operator >(NullableAny<T> a, NullableAny<T> b)
		{
			return a.CompareTo(b) > 0;
		}

		/// <summary>
		/// True if a &gt;= b.
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		public static bool operator >=(NullableAny<T> a, NullableAny<T> b)
		{
			return a.CompareTo(b) >= 0;
		}

		/// <summary>
		/// Compare this value to the other.
		/// Then returns 0 is they are equal, -1 if this value is lower than the other, or +1 if this value is greater than the other.
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		/// <exception cref="InvalidOperationException">No value has been set, or the value has been set to null.</exception>
		public int CompareTo(T other)
		{
			if (rankComparer == null)
				throw new NotSupportedException();
			return rankComparer.Compare(this.Value, other);
		}

		/// <summary>
		/// Compare this value to the other.
		/// Then returns 0 is they are equal, -1 if this value is lower than the other, or +1 if this value is greater than the other.
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public int CompareTo(NullableAny<T> other)
		{
			if (!this._HasValue && !other._HasValue)
				return 0;
			if (rankComparer == null)
				throw new NotSupportedException();
			return rankComparer.Compare(this.Value, other.Value);
		}

		/// <summary>
		/// Compare x to y.
		/// Then returns 0 is they are equal, -1 if x is lower than y, or +1 if x is greater than y.
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		public int Compare(NullableAny<T> x, NullableAny<T> y)
		{
			return x.CompareTo(y);
		}

		/// <summary>
		/// Compare x to y.
		/// Then returns 0 is they are equal, -1 if x is lower than y, or +1 if x is greater than y.
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		public int Compare(T x, T y)
		{
			if (rankComparer == null)
				throw new NotSupportedException();
			return rankComparer.Compare(x, y);
		}

		/// <summary>
		/// True if this value equals the other value, or if both structures has no defined value.
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public bool Equals(NullableAny<T> other)
		{
			if (this._HasValue && other._HasValue)
				return equalityComparer.Equals(this._Value, other._Value);
			if (!this._HasValue && !other._HasValue)
				return true;
			return false;
		}

		/// <summary>
		/// True if this value equals the other value.
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public bool Equals(T other)
		{
			if (this._HasValue)
				return equalityComparer.Equals(this._Value, other);
			return false;
		}

		/// <summary>
		/// True if this value equals the other value, or if both structures has no defined value.
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public override bool Equals(object other)
		{
			if (other is T)
				return equalityComparer.Equals(this._Value, (T)other);
			if (other is NullableAny<T>)
				return this.Equals((NullableAny<T>)other);
			return false;
		}

		/// <summary>
		/// Returns the value's hash code, or zero if no value is defined.
		/// </summary>
		/// <returns></returns>
		public override int GetHashCode()
		{
			if (this._HasValue)
				return this._Value.GetHashCode();
			return 0;
		}

		/// <summary>
		/// Returns the value's ToString(), or string.Empty if no value is defined.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			if (this._HasValue)
				return this._Value.ToString();
			return string.Empty;
		}

		/// <summary>
		/// Transcodes T to NullableAny&lt;T&gt;.
		/// </summary>
		/// <param name="v"></param>
		/// <returns></returns>
		public static implicit operator NullableAny<T>(T v)
		{
			return new NullableAny<T>(v);
		}

		/// <summary>
		/// Transcodes T? to NullableAny&lt;T&gt;.
		/// </summary>
		/// <param name="v"></param>
		/// <returns></returns>
		public static NullableAny<T> FromNullable<S>(S? v) // I can't make an implicit operator since T can be a class.
			where S:struct
		{
			if (v.HasValue)
			{
				var o = (object)v.Value;
				var t = (T)o;
				return new NullableAny<T>(t);
			}
			return new NullableAny<T>();
		}

		/// <summary>
		/// Transcodes NullableAny&lt;T&gt; to T?.
		/// </summary>
		/// <param name="v"></param>
		/// <returns></returns>
		public static S? ToNullable<S>(NullableAny<T> v) // I can't make an implicit operator since T can be a class.
			where S : struct
		{
			if (v.HasValue)
			{
				var o = (object)v.Value;
				var t = (S)o;
				return new S?(t);
			}
			return new S?();
		}

		/// <summary>
		/// Transcodes NullableAny&lt;T&gt; to T.
		/// <para>The transcoder is explicit because if the source data has no value, an exception is thrown.</para>
		/// </summary>
		/// <param name="t"></param>
		/// <returns></returns>
		/// <exception cref="InvalidOperationException">No value has been set in the source, or the value has been set to null.</exception>
		public static explicit operator T(NullableAny<T> t)
		{
			return t.Value;
		}
	}

	/// <summary>
	/// Type extensions for <see cref="CB.Data.NullableAny{T}"/>
	/// </summary>
	public static class NullableAnyHelper
	{
		/// <summary>
		/// Transcodes T? to NullableAny&lt;T&gt;.
		/// </summary>
		/// <param name="v"></param>
		/// <returns></returns>
		public static NullableAny<T> ToNullableAny<T>(this T? v)
			where T : struct
		{
			if (v.HasValue)
				return new NullableAny<T>(v.Value);
			return new NullableAny<T>();
		}

		/// <summary>
		/// Transtypes NullableAny&lt;T&gt; to T?.
		/// </summary>
		/// <param name="v"></param>
		/// <returns></returns>
		public static T? ToNullable<T>(this NullableAny<T> v)
			where T : struct
		{
			if (v.HasValue)
				return new T?(v.Value);
			return new T?();
		}

#if TEST
		[Test]
		static void TesteToNullableAny()
		{
			int? défini = 100;
			var défini2 = défini.ToNullableAny();

			int? nul = default(int?);
			var nul2 = nul.ToNullableAny();

			var défini3 = NullableAny<int>.FromNullable((int?)1000);
			var nul3 = NullableAny<int>.FromNullable(default(int?));
		}
#endif
	}

}