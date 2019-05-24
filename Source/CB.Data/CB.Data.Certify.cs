
// Copyright Christophe Bertrand
// https://github.com/ChrisBertrandDotNet/CB-Helpers
// https://chrisbertrand.net

/* Requires:
. CB.Data\CB.Data.EmptyArray.cs
. CB.Data\CB.Data.EnumHelper.cs
. CB.Data\CB.Data.ImmutableList.cs
. CB.Reflection\CB.Reflection.TypeEx.cs
*/

/*
 * 
 * Gives (non-null) certified references.
 * It's a kind of contrary to Nullable<T>.
 * 
 * IMPORTANT: it may be easy to make a mistake: by defining a field NonNull<> without building an instance,
 * or a local variable initialized by default(NonNull<>),
 * the compiler will not warn. The error will appear on the next value reading only.
 * 
 * For the String, use NonEmpty, that certifies the reference is not a String.Empty as well.
 * 
 * NEW: C# 8.0 now has its own non-null referencing system, making this NonNull class useless.
 * Anyway, DefinedString, DefinedEnum and Bounded are still useful.
 */

#if TEST
using CBdotnet.Test;
#endif
using CB.Reflection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;

namespace CB.Data
{

	/// <summary>
	/// A type that has a main value.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public interface IValue<T> : IEquatable<T>
	{
		/// <summary>
		/// True if the value is defined.
		/// </summary>
		bool HasValue { get; }
		/// <summary>
		/// Gets the value.
		/// </summary>
		T Value { get; set; }
	}

	/// <summary>
	/// Certifies a reference is always defined (non-null).
	/// <para>
	/// IMPORTANT:
	/// Never initialize a blank structure, using default(NonNull&lt;&gt;).
	/// And never let a NonNull&lt;&gt; in a field without initializing it with a reference.
	/// </para>
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public struct NonNull<T> : IValue<T>, IEquatable<NonNull<T>>
		where T : class
	{
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		static readonly string erreur = string.Format(CultureInfo.InvariantCulture, "{0}<{1}> has not been built with a value.", typeof(NonNull<T>).Name, typeof(T).Name);
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		static IEqualityComparer<T> comparer = EqualityComparer<T>.Default;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		T _Value; // can not be null.

		/// <summary>
		/// Gets or sets the value.
		/// </summary>
		/// <exception cref="System.InvalidOperationException">When trying to get the value of an uninitialized NonNull.</exception>
		/// <exception cref="System.ArgumentNullException">When trying to set the value to null.</exception>
		public T Value
		{
			get
			{
				if (this._Value == null)
					throw new InvalidOperationException(erreur);
				return this._Value;
			}
			set
			{
				this._Value = value ?? throw new ArgumentNullException();
			}
		}

		/// <summary>
		/// Creates a non-null reference.
		/// </summary>
		/// <param name="value"></param>
		/// <exception cref="System.ArgumentNullException">The value is a null reference.</exception>
		public NonNull(T value)
		{
			this._Value = value ?? throw new ArgumentNullException();
		}

		/// <summary>
		/// True is a and b are equal.
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		public static bool operator ==(NonNull<T> a, NonNull<T> b)
		{
			if (object.ReferenceEquals(a._Value, null) || object.ReferenceEquals(b._Value, null))
				return false;
			if (object.ReferenceEquals(a._Value, b._Value))
				return true;

#if true
			return comparer.Equals(a._Value, b._Value);
#else
			if (a._Value is IEquatable<T>)
				return ((IEquatable<T>)a._Value).Equals(b._Value);
			else
				if (a._Value is IEqualityComparer<T>)
					return ((IEqualityComparer<T>)a._Value).Equals(a._Value, b._Value);
			return object.Equals(a._Value, b._Value);
#endif
		}

		/// <summary>
		/// True if a and b are different.
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		public static bool operator !=(NonNull<T> a, NonNull<T> b)
		{
			return !(a == b);
		}

		/// <summary>
		/// True if a and b are equal.
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		public static bool operator ==(NonNull<T> a, T b)
		{
			return a.Value == b;
		}

		/// <summary>
		/// True if a and b are different.
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		public static bool operator !=(NonNull<T> a, T b)
		{
			return !(a == b);
		}

		/// <summary>
		/// True if this is equal to the other.
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public override bool Equals(object other)
		{
			if (object.Equals(this._Value, null))
				return false;
			if (!(other is NonNull<T>))
				return false;
			return this == (NonNull<T>)other;
		}

		/// <summary>
		/// Gets the hash code of the value.
		/// </summary>
		/// <returns></returns>
		public override int GetHashCode()
		{
			return this._Value.GetHashCode();
		}

		/// <summary>
		/// Returns a string that represents the value.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return this._Value.ToString();
		}

		/// <summary>
		/// Transtype an original reference to a non-null reference.
		/// </summary>
		/// <param name="value"></param>
		/// <exception cref="System.ArgumentNullException">The value is a null reference.</exception>
		public static implicit operator NonNull<T>(T value)
		{
			return new NonNull<T>(value);
		}

		/// <summary>
		/// Transtype the non-null reference to an original type reference.
		/// </summary>
		/// <param name="value"></param>
		public static implicit operator T(NonNull<T> value)
		{
			return value.Value;
		}

		/// <summary>
		/// True if this reference/value equals the other one.
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public bool Equals(NonNull<T> other)
		{
			return this == other;
		}

		/// <summary>
		/// True if a value is defined.
		/// </summary>
		public bool HasValue
		{
			get { return _Value != null; }
		}

		/// <summary>
		/// True if this value equals the other one.
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public bool Equals(T other)
		{
			if (object.Equals(this._Value, null))
				return false;
			return comparer.Equals(this._Value, other);
		}
	}

#if TEST
	class NonNull_Test
	{
		[Test]
		static void test_NonNullT()
		{
			{
				NonNull<Classe> a = default(NonNull<Classe>);
				Testeur.TesteÉchecPrévu(() => a.Value);
				a = new Classe();
				//Testeur.TesteÉchecPrévu(() => a == null);
				var b = new NonNull<Classe>();
				Testeur.TesteDifférence(a, b);
				a = new Classe() { I = 1 };
				Testeur.TesteÉgalité(a.Value.I, 1);
				b.Value = new Classe() { I = 1 };
				Testeur.TesteVérité(() => a != b);
				var c = a;
				Testeur.TesteVérité(() => a == c);
			}

		}
		internal class Classe
		{
			internal int I;
		}
	}
#endif


#if false // useless
	[System.Runtime.InteropServices.ComVisible(true)]
	public static class NonNull
	{
		[System.Runtime.InteropServices.ComVisible(true)]
		public static int Compare<T>(NonNull<T> n1, NonNull<T> n2) where T : struct
		{
			if (n1.HasValue)
			{
				if (n2.HasValue) return Comparer<T>.Default.Compare(n1._Value, n2._Value);
				return 1;
			}
			if (n2.HasValue) return -1;
			return 0;
		}

		[System.Runtime.InteropServices.ComVisible(true)]
		public static bool Equals<T>(NonNull<T> n1, NonNull<T> n2) where T : struct
		{
			if (n1.HasValue)
			{
				if (n2.HasValue) return EqualityComparer<T>.Default.Equals(n1._Value, n2._Value);
				return false;
			}
			if (n2.HasValue) return false;
			return true;
		}

		// If the type provided is not a NonNull Type, return null.
		// Otherwise, returns the underlying type of the NonNull type
		public static Type GetUnderlyingType(Type readonlyeType)
		{
			if ((object)readonlyeType == null)
			{
				throw new ArgumentNullException("readonlyType");
			}
			Contract.EndContractBlock();
			Type result = null;
			if (readonlyeType.IsGenericType && !readonlyeType.IsGenericTypeDefinition)
			{
				// instantiated generic type only                
				Type genericType = readonlyeType.GetGenericTypeDefinition();
				if (Object.ReferenceEquals(genericType, typeof(NonNull<>)))
				{
					result = readonlyeType.GetGenericArguments()[0];
				}
			}
			return result;
		}

	}
#endif

	/// <summary>
	/// Certifies a string is always defined (non-null and non empty).
	/// <para>
	/// IMPORTANT:
	/// Never initialize a blank structure, using default(NonEmpty).
	/// And never let a NonEmpty in a field without initializing it with a reference.
	/// </para>
	/// </summary>
	public struct DefinedString : IValue<string>, IEquatable<DefinedString>
	{
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		static readonly string erreur = string.Format(CultureInfo.InvariantCulture, "{0} has not been built with a value.", typeof(DefinedString).Name);

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		String _Value; // can not be null, unless the structure has never been initialized.

		/// <summary>
		/// Gets or sets the value.
		/// </summary>
		/// <exception cref="System.InvalidOperationException">When trying to get an undefined value.</exception>
		/// <exception cref="System.ArgumentNullException">When trying to set a null reference as the value.</exception>
		/// <exception cref="System.ArgumentException">When trying to set String.Empty as the value.</exception>
		public String Value
		{
			get
			{
				if (this._Value == null /*|| this._Value==string.Empty*/)
					throw new InvalidOperationException(erreur);
				return this._Value;
			}
			set
			{
				if (value == null)
					throw new ArgumentNullException();
				if (value == string.Empty)
					throw new ArgumentException();
				this._Value = value;
			}
		}

		/// <summary>
		/// Creates a defined string reference.
		/// </summary>
		/// <param name="value"></param>
		public DefinedString(String value)
		{
			if (value == null)
				throw new ArgumentNullException();
			if (value == string.Empty)
				throw new ArgumentException();
			this._Value = value;
		}

		/// <summary>
		/// True is a == b.
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		public static bool operator ==(DefinedString a, DefinedString b)
		{
			if (object.ReferenceEquals(a._Value, null) || object.ReferenceEquals(b._Value, null))
				return false;
			return a._Value == b._Value;

			/*if (a._Value == b._Value)
				return true;

#if true
			return comparer.Equals(a._Value, b._Value);
#else
			if (a._Value is IEquatable<String>)
				return ((IEquatable<String>)a._Value).Equals(b._Value);
			else
				if (a._Value is IEqualityComparer<String>)
					return ((IEqualityComparer<String>)a._Value).Equals(a._Value, b._Value);
			return object.Equals(a._Value, b._Value);
#endif*/
		}

		/// <summary>
		/// True if a != b.
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		public static bool operator !=(DefinedString a, DefinedString b)
		{
			return !(a == b);
		}

		/// <summary>
		/// True if this value equals the given object.
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public override bool Equals(object other)
		{
			if (this._Value == null)
				return false;
			if (!(other is DefinedString))
				return false;
			return this == (DefinedString)other;
		}

		/// <summary>
		/// Returns the hash code.
		/// </summary>
		/// <returns></returns>
		public override int GetHashCode()
		{
			return this._Value.GetHashCode();
		}

		/// <summary>
		/// Returns the value itself.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return this._Value;
		}

		/// <summary>
		/// Transtype a regular string to a DefinedString.
		/// </summary>
		/// <param name="value"></param>
		public static implicit operator DefinedString(String value)
		{
			return new DefinedString(value);
		}

		/// <summary>
		/// Transtype a DefinedString to a regular string, in fact by extracting the value.
		/// </summary>
		/// <param name="value"></param>
		public static implicit operator String(DefinedString value)
		{
			return value.Value;
		}

		/// <summary>
		/// True if this == other.
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public bool Equals(DefinedString other)
		{
			return this == other;
		}

		/// <summary>
		/// True if a value is defined.
		/// </summary>
		public bool HasValue
		{
			get { return this._Value != null; }
		}

		/// <summary>
		/// True if the value equals the other one.
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public bool Equals(string other)
		{
			if (this._Value == null)
				return false;
			return this.Value == other;
		}
	}

	/// <summary>
	/// Certifies an enumeration value is always defined (is one of the declared values of the enumeration).
	/// <para>
	/// IMPORTANT:
	/// Never initialize a blank structure, using default(DefinedEnum&lt;&gt;).
	/// And never let a DefinedEnum&lt;&gt; in a field without initializing it with a value.
	/// </para>
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public struct DefinedEnum<T> : IValue<T>, IEquatable<DefinedEnum<T>>
		where T : struct
	{
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		static readonly string erreur = string.Format(CultureInfo.InvariantCulture, "{0}<{1}> is not a defined value in its enumeration type.", typeof(DefinedEnum<T>).Name, typeof(T).Name);
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		static IEqualityComparer<T> comparer = EnumHelper<T>.Comparer; //EqualityComparer<T>.Default;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		static readonly bool IsAnEnumerationType = _IsAnEnumerationType(); // intended to cause an error when T is not an enumeration.

		/// <summary>
		/// True if this enumeration has the attribute <see cref="System.FlagsAttribute"/>.
		/// </summary>
		public static bool IsAFlagsEnumeration = EnumHelper<T>.HasFlagsAttribute;
		//typeof(T).GetCustomAttributes(typeof(FlagsAttribute), true).Length > 0;

		[Obfuscation(Exclude = true)]
		readonly bool isInitialized;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		T _Value; // can't be undefined.

		/// <summary>
		/// Gets or sets the value.
		/// </summary>
		/// <exception cref="System.InvalidOperationException">When trying to get the value of an uninitialized structure.</exception>
		/// <exception cref="System.ArgumentException">When trying to set a value that is not explicitly declared in the enumeration.</exception>
		public T Value
		{
			get
			{
				if (!this.isInitialized)
					throw new InvalidOperationException(erreur);
				return this._Value;
			}
			set
			{
				if (!IsDefined(value))
					throw new ArgumentException();
				this._Value = value;
			}
		}

		/// <summary>
		/// Creates a DefinedEnum using the given value.
		/// </summary>
		/// <param name="value"></param>
		/// <exception cref="System.ArgumentException">When the value is not explicitly declared in the enumeration.</exception>
		public DefinedEnum(T value)
		{
			if (!IsDefined(value))
				throw new ArgumentException();
			this._Value = value;
			this.isInitialized = true;
		}

		static bool _IsAnEnumerationType()
		{
			if (typeof(T).IsEnum())
				return true;
			throw new TypeAccessException(string.Format("Type {0} is not an enumeration.", typeof(T).Name));
		}

		/// <summary>
		/// Checks the enumeration value is defined in its enumeration type.
		/// <para>The value can be a multiple values combination if <typeparamref name="T"/> has FlagsAttribute.</para>
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static bool IsDefined(T value)
		{
			return EnumHelper<T>.IsDefinedValue(value);
		}

		/// <summary>
		/// Checks the enumeration value is one of the defined values in its enumeration type.
		/// <para>The value can not be a multiple values combination, even if <typeparamref name="T"/> has FlagsAttribute.</para>
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static bool IsDeclaredAsAUniqueValue(T value)
		{
			return EnumHelper<T>.IsDeclaredAsAUniqueValue(value);
		}

		[Obsolete("Use IsDefinedAUniqueValue")]
		internal static bool IsDefinedAsOneValue(T value)
		{
			return EnumHelper<T>.IsDefinedAsOneValue(value);
		}

		/// <summary>
		/// True if a == b.
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		public static bool operator ==(DefinedEnum<T> a, DefinedEnum<T> b)
		{
			if (!a.isInitialized || !b.isInitialized)
				return false;
			return comparer.Equals(a._Value, b._Value);
		}

		/// <summary>
		/// True if a != b.
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		public static bool operator !=(DefinedEnum<T> a, DefinedEnum<T> b)
		{
			return !(a == b);
		}

		/// <summary>
		/// True if this equals the other object.
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public override bool Equals(object other)
		{
			if (!this.isInitialized)
				return false;
			if (!(other is DefinedEnum<T>))
				return false;
			return this == (DefinedEnum<T>)other;
		}

		/// <summary>
		/// Returns the hash code.
		/// </summary>
		/// <returns></returns>
		public override int GetHashCode()
		{
			return this._Value.GetHashCode();
		}

		/// <summary>
		/// Returns the fully qualified type name of the value.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return this._Value.ToString();
		}

		/// <summary>
		/// Transtype an original value to a DefinedEnum.
		/// </summary>
		/// <param name="value"></param>
		/// <exception cref="System.ArgumentException">When the <paramref name="value"/> is not explicitly declared in the enumeration.</exception>
		public static implicit operator DefinedEnum<T>(T value)
		{
			return new DefinedEnum<T>(value);
		}

		/// <summary>
		/// Transtype a DefinedEnum to the original type by extracting its value.
		/// </summary>
		/// <param name="value"></param>
		/// <exception cref="System.InvalidOperationException">If the structure is uninitialized.</exception>
		public static implicit operator T(DefinedEnum<T> value)
		{
			return value.Value;
		}

		/// <summary>
		/// True if this == other.
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public bool Equals(DefinedEnum<T> other)
		{
			return this == other;
		}

		/// <summary>
		/// True if there is a defined value.
		/// </summary>
		public bool HasValue
		{
			get { return this.isInitialized; }
		}

		/// <summary>
		/// True if this value equals the other.
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public bool Equals(T other)
		{
			if (!this.isInitialized)
				return false;
			return comparer.Equals(this._Value, other);
		}
	}

	#region Bounded

	/// <summary>
	/// A value from a bounded sequence.
	/// <para>
	/// <list type="bullet">
	/// <item>The <see cref="IBounded{T}.Value"/> must be greater, or equal depending on <see cref="IBounded{T}.RangeIncludesTheMinimum"/>, to <see cref="IBounded{T}.Minimum"/>.</item>
	/// <item>The <see cref="IBounded{T}.Value"/> must be lower, or equal depending on <see cref="IBounded{T}.RangeIncludesTheMaximum"/>, to <see cref="IBounded{T}.Maximum"/>.</item>
	/// </list>
	/// </para>
	/// <para>Please note the word Bounded comes from mathematics (see https://en.wikipedia.org/wiki/Bounded_set for more information).</para>
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public interface IBounded<T>
		where T : IComparable<T>, IEquatable<T>
	{
		/// <summary>
		/// The minimum value is the lower bound of the range.
		/// </summary>
		T Minimum { get; }
		/// <summary>
		/// The maximum value is the upper bound of the range.
		/// </summary>
		T Maximum { get; }
		/// <summary>
		/// True if the range includes the minimum value (the lower bound) itself.
		/// </summary>
		bool RangeIncludesTheMinimum { get; }
		/// <summary>
		/// True if the range includes the maximum value (the upper bound) itself.
		/// </summary>
		bool RangeIncludesTheMaximum { get; }
		/// <summary>
		/// The value.
		/// </summary>
		T Value { get; }
	}

	/// <summary>
	/// A value from a bounded sequence.
	/// <para>Requirement #1 for sub-types: The range must be defined once by type. Two instances of the same type can't have different ranges.</para>
	/// <para>Requirement #2 for sub-types: <see cref="Bounded{T}.Minimum"/> and <see cref="Bounded{T}.Maximum"/> must be non-null.</para>
	/// <para>
	/// <list type="bullet">
	/// <item>The <see cref="Bounded{T}.Value"/> must be greater, or equal depending on <see cref="Bounded{T}.RangeIncludesTheMinimum"/>, to <see cref="Bounded{T}.Minimum"/>.</item>
	/// <item>The <see cref="Bounded{T}.Value"/> must be lower, or equal depending on <see cref="Bounded{T}.RangeIncludesTheMaximum"/>, to <see cref="Bounded{T}.Maximum"/>.</item>
	/// </list>
	/// </para>
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public abstract class Bounded<T> : IBounded<T>
		where T : IComparable<T>, IEquatable<T>
	{
		readonly T _Value;

		/// <summary>
		/// The minimum value is the lower bound of the range.
		/// <para>Condition: The class must return a static value. Two instances of the same class can't have different ranges.
		/// Example: <code>public T Minimum =&gt; 0;</code></para>
		/// </summary>
		public abstract T Minimum { get; }
		/// <summary>
		/// The maximum value is the upper bound of the range.
		/// <para>Condition: The class must return a static value. Two instances of the same class can't have different ranges.
		/// Example: <code>public T Maximum =&gt; 100;</code></para>
		/// </summary>
		public abstract T Maximum { get; }
		/// <summary>
		/// True if the range includes the minimum value (the lower bound) itself.
		/// <para>Condition: The class must return a static value. Two instances of the same class can't have different ranges.
		/// Example: <code>public bool RangeIncludesTheMinimum =&gt; true;</code></para>
		/// </summary>
		public abstract bool RangeIncludesTheMinimum { get; }
		/// <summary>
		/// True if the range includes the maximum value (the upper bound) itself.
		/// <para>Condition: The class must return a static value. Two instances of the same class can't have different ranges.
		/// Example: <code>public bool RangeIncludesTheMaximum =&gt; true;</code></para>
		/// </summary>
		public abstract bool RangeIncludesTheMaximum { get; }
		/// <summary>
		/// The value.
		/// <para>This value is immutable, it is defined in the class constructor.</para>
		/// </summary>
		public T Value { get { return this._Value; } }

		/// <summary>
		/// Creates a value.
		/// </summary>
		/// <param name="value"></param>
		/// <exception cref="ArgumentOutOfRangeException">The value is not is the range.</exception>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public Bounded(T value)
		{
			if (
			value.CompareTo(Minimum) < 0
			|| (!RangeIncludesTheMinimum && value.CompareTo(Minimum) == 0)
			|| value.CompareTo(Maximum) > 0
			|| (!RangeIncludesTheMaximum && value.CompareTo(Maximum) == 0)
				)
				throw new ArgumentOutOfRangeException();
			this._Value = value;
		}

		/// <summary>
		/// Private constructor that does not check the range.
		/// </summary>
		/// <param name="value"></param>
		/// <param name="checkRange">Unused. Just to allow a second constructor (a matter of method signatures).</param>
		protected Bounded(T value, bool checkRange)
		{
			this._Value = value;
		}
		/// <summary>
		/// Creates a simple range generator.
		/// This generator in turn lets you create simple bounded values.
		/// </summary>
		/// <param name="minimum"></param>
		/// <param name="maximum"></param>
		/// <param name="rangeIncludesTheMinimum"></param>
		/// <param name="rangeIncludesTheMaximum"></param>
		/// <returns></returns>
		public static IRangeGenerator<T> CreateASimpleRange(T minimum, T maximum, bool rangeIncludesTheMinimum, bool rangeIncludesTheMaximum)
		{
			return new RangeGenerator<T>(minimum, maximum, rangeIncludesTheMinimum, rangeIncludesTheMaximum);
		}

		/// <summary>
		/// True if the given <paramref name="value"/> is in the range.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public bool IsAValidValue(T value)
		{
			if (value == null)
				return false;
			return !(value.CompareTo(Minimum) < 0
			|| (!RangeIncludesTheMinimum && value.Equals(Minimum))
			|| value.CompareTo(Maximum) > 0
			|| (!RangeIncludesTheMaximum && value.Equals(Maximum)));
		}
		/// <summary>
		/// True if the given <paramref name="value"/> is in the range.
		/// </summary>
		/// <param name="value"></param>
		/// <param name="minimum"></param>
		/// <param name="maximum"></param>
		/// <param name="rangeIncludesTheMinimum"></param>
		/// <param name="rangeIncludesTheMaximum"></param>
		/// <returns></returns>
		public static bool IsAValidValue(T value, T minimum, T maximum, bool rangeIncludesTheMinimum, bool rangeIncludesTheMaximum)
		{
			return !(
				value == null
				|| minimum == null
				|| maximum == null
				|| value.CompareTo(minimum) < 0
				|| (!rangeIncludesTheMinimum && value.Equals(minimum))
				|| value.CompareTo(maximum) > 0
				|| (!rangeIncludesTheMaximum && value.Equals(maximum)));
		}

		/// <summary>
		/// Compare values.
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		public static bool operator ==(Bounded<T> a, Bounded<T> b)
		{
			if (object.Equals(a, null))
				if (object.Equals(b, null))
					return true;
				else
					return false;
			if (object.Equals(a._Value, null))
				if (object.Equals(b._Value, null))
					return true;
				else
					return false;
			return a._Value.Equals(b._Value);
		}

		/// <summary>
		/// Compare values.
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		public static bool operator !=(Bounded<T> a, Bounded<T> b)
		{
			return !(a == b);
		}

		/// <summary>
		/// Compare values.
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		public static bool operator ==(Bounded<T> a, T b)
		{
			if (object.Equals(a, null))
				return false;
			if (object.Equals(a._Value, null))
				if (object.Equals(b, null))
					return true;
				else
					return false;
			return a._Value.Equals(b);
		}

		/// <summary>
		/// Compare values.
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		public static bool operator !=(Bounded<T> a, T b)
		{
			return !(a == b);
		}

		/// <summary>
		/// Transtypes a bounded value to the original type by extracting the value itself.
		/// </summary>
		/// <param name="value"></param>
		/// <exception cref="System.InvalidOperationException">If the value is undefined.</exception>
		public static implicit operator T(Bounded<T> value)
		{
			return value._Value;
		}

#if false // model
		/// <summary>
		/// Transtypes an original type to a bounded value.
		/// </summary>
		/// <param name="value"></param>
		/// <exception cref="System.InvalidOperationException">If the value is undefined.</exception>
		public static implicit operator Range<T>(T value)
		{
			return new Range<T>() { Truc = value };
		}
#endif

		/// <summary>
		/// Returns a string that represents the value. Or null if the value is null.
		/// </summary>
		/// <returns>The string that represents the value. Or null if the value is null.</returns>
		public override string ToString()
		{
			if (_Value == null)
				return null;
			return _Value.ToString();
		}

		/// <summary>
		/// Returns the hash code, or zero if the value is null.
		/// </summary>
		/// <returns></returns>
		public override int GetHashCode()
		{
			if (_Value == null)
				return 0;
			return _Value.GetHashCode();
		}

		/// <summary>
		/// Compare values.
		/// <para>Ranges are not compared.</para>
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public override bool Equals(object obj)
		{
			if (obj is T)
				return this == (T)obj;
			if (obj is Bounded<T>)
				return this == (Bounded<T>)obj;

			return false;
		}

		/// <summary>
		/// Compare values.
		/// <para>Ranges are not compared.</para>
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public bool Equals(T other)
		{
			return this == other;
		}

		/// <summary>
		/// Compare this value to the other.
		/// Then returns 0 is they are equal, -1 if this value is lower than the other, or +1 if this value is greater than the other.
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		/// <exception cref="System.InvalidOperationException">The current value is null.</exception>
		/// <exception cref="System.ArgumentNullException"></exception>
		public int CompareTo(Bounded<T> other)
		{
			if (other == null)
				throw new ArgumentNullException();
			if (this._Value == null)
				throw new InvalidOperationException("The current value is null.");
			return this._Value.CompareTo(other._Value);
		}

		/// <summary>
		/// Compare this value to the other.
		/// Then returns 0 is they are equal, -1 if this value is lower than the other, or +1 if this value is greater than the other.
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		/// <exception cref="System.InvalidOperationException">The current value is null.</exception>
		public int CompareTo(T other)
		{
			if (this._Value == null)
				throw new InvalidOperationException("The current value is null.");
			return this._Value.CompareTo(other);
		}

		/// <summary>
		/// True if this value equals the other one.
		/// False if one (or two) of the values is undefined.
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public bool Equals(Bounded<T> other)
		{
			return this == other;
		}
	}

	#region Simple range generator and values

	/// <summary>
	/// A simple range generator.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public interface IRangeGenerator<T>
		where T : IComparable<T>, IEquatable<T>
	{
		/// <summary>
		/// True if the value is in the range.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		bool IsAValidValue(T value);

		/// <summary>
		/// Creates a new bounded value.
		/// </summary>
		/// <param name="value"></param>
		/// <returns>The value that is certified to be in the range.</returns>
		/// <exception cref="System.ArgumentOutOfRangeException">The value is not is the range.</exception>
		IBounded<T> NewValue(T value);

		/// <summary>
		/// Creates a new bounded value.
		/// </summary>
		/// <param name="value"></param>
		/// <returns>The value that is certified to be in the range and to be non-null.</returns>
		/// <exception cref="System.ArgumentOutOfRangeException">The value is not is the range.</exception>
		NonNull<IBounded<T>> NewValueAsNonNull(T value);

		/// <summary>
		/// Try to create a new bounded value.
		/// </summary>
		/// <param name="value"></param>
		/// <returns>The value that is certified to be in the range, or null if the <paramref name="value"/> is not in the range.</returns>
		IBounded<T> TryCreateValue(T value);
	}

	internal class RangeGenerator<T> : IRangeGenerator<T>
		where T : IComparable<T>, IEquatable<T>
	{
		internal readonly T Minimum;
		internal readonly T Maximum;
		internal readonly bool RangeIncludesTheMinimum;
		internal readonly bool RangeIncludesTheMaximum;

		internal RangeGenerator(T minimum, T maximum, bool rangeIncludesTheMinimum, bool rangeIncludesTheMaximum)
		{
			if (minimum == null)
				throw new ArgumentNullException(nameof(minimum));
			if (maximum == null)
				throw new ArgumentNullException(nameof(maximum));
			Minimum = minimum; Maximum = maximum; RangeIncludesTheMinimum = rangeIncludesTheMinimum; RangeIncludesTheMaximum = rangeIncludesTheMaximum;
		}

		public IBounded<T> NewValue(T value)
		{
			return new SimpleBoundedValue<T>(this, value);
		}

		public NonNull<IBounded<T>> NewValueAsNonNull(T value)
		{
			return new NonNull<IBounded<T>>(new SimpleBoundedValue<T>(this, value));
		}

		public IBounded<T> TryCreateValue(T value)
		{
			if (!IsAValidValue(value))
				return null;
			return new SimpleBoundedValue<T>(this, value, false);
		}

		public bool IsAValidValue(T value)
		{
			return !(
				value == null
				|| Minimum == null
				|| Maximum == null
				|| value.CompareTo(Minimum) < 0
				|| (!RangeIncludesTheMinimum && value.Equals(Minimum))
				|| value.CompareTo(Maximum) > 0
				|| (!RangeIncludesTheMaximum && value.Equals(Maximum)));
		}

	}

	internal class SimpleBoundedValue<T> : Bounded<T>
			where T : IComparable<T>, IEquatable<T>
	{
		readonly RangeGenerator<T> Generator;

		public override T Minimum => Generator.Minimum;

		public override T Maximum => Generator.Maximum;

		public override bool RangeIncludesTheMinimum => true;

		public override bool RangeIncludesTheMaximum => false;

		internal SimpleBoundedValue(RangeGenerator<T> generator, T value)
			: base(value, false)
		{
			this.Generator = generator;
			if (!this.IsAValidValue(value))
				throw new ArgumentOutOfRangeException();
		}
		internal SimpleBoundedValue(RangeGenerator<T> generator, T value, bool checkRange)
			: base(value, false)
		{
			this.Generator = generator;
		}
	}

	#endregion Simple range generator and values

#if false // old intent.
	/// <summary>
	/// A ranged value.
	/// <para>
	/// The value must be greater, or equal depending on RangeIncludesTheMinimum, to Minimum.
	/// The value must be lower, or equal depending on RangeIncludesTheMaximum, to Maximum.
	/// </para>
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public struct Ranged<T> : IValue<T>, IComparable<Ranged<T>>, IComparable<T>, IEquatable<Ranged<T>>
		where T : IComparable<T>, IEquatable<T>
	{
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		T _Value;
		[Obfuscation(Exclude = true)]
		bool isInitialized;

		/// <summary>
		/// The minimum value of the range.
		/// </summary>
		public readonly T Minimum;
		/// <summary>
		/// The maximum value of the range.
		/// </summary>
		public readonly T Maximum;
		/// <summary>
		/// True if the range includes the minimum value itself.
		/// </summary>
		public readonly bool RangeIncludesTheMinimum;
		/// <summary>
		/// True if the range includes the maximum value itself.
		/// </summary>
		public readonly bool RangeIncludesTheMaximum;

		/// <summary>
		/// Gets or sets the value.
		/// </summary>
		/// <exception cref="System.InvalidOperationException">When trying to get an undefined value.</exception>
		/// <exception cref="System.ArgumentOutOfRangeException">When trying to set a value that is not in the range.</exception>
		public T Value
		{
			get
			{
				if (!isInitialized)
					throw new InvalidOperationException("The structure was not initialized (constructed).");
				return _Value;
			}
			set
			{
				if (
				value.CompareTo(Minimum) < 0
				|| (!RangeIncludesTheMinimum && value.CompareTo(Minimum) == 0)
				|| value.CompareTo(Maximum) > 0
				|| (!RangeIncludesTheMaximum && value.CompareTo(Maximum) == 0)
					)
					throw new ArgumentOutOfRangeException();
				_Value = value;
			}
		}

		/// <summary>
		/// Creates a new ranged value.
		/// <para>The minimum and maximum values are included in the range.</para>
		/// </summary>
		/// <param name="value"></param>
		/// <param name="minimum"></param>
		/// <param name="maximum"></param>
		public Ranged(T value, T minimum, T maximum)
			: this(value, minimum, maximum, true, true)
		{ }

		/// <summary>
		/// Creates a new ranged value.
		/// </summary>
		/// <param name="value"></param>
		/// <param name="minimum"></param>
		/// <param name="maximum"></param>
		/// <param name="rangeIncludesTheMinimum"></param>
		/// <param name="rangeIncludesTheMaximum"></param>
		public Ranged(T value, T minimum, T maximum, bool rangeIncludesTheMinimum, bool rangeIncludesTheMaximum)
		{
			isInitialized = true;
			Minimum = minimum;
			Maximum = maximum;
			RangeIncludesTheMinimum = rangeIncludesTheMinimum;
			RangeIncludesTheMaximum = rangeIncludesTheMaximum;
			if (
			value.CompareTo(Minimum) < 0
			|| (!RangeIncludesTheMinimum && value.CompareTo(Minimum) == 0)
			|| value.CompareTo(Maximum) > 0
			|| (!RangeIncludesTheMaximum && value.CompareTo(Maximum) == 0)
				)
				throw new ArgumentOutOfRangeException();
			_Value = value;
		}

		/// <summary>
		/// Compare values.
		/// <para>Ranges are not compared.</para>
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		public static bool operator ==(Ranged<T> a, Ranged<T> b)
		{
			if (!a.isInitialized || !b.isInitialized)
				return false;
			return a.Value.Equals(b._Value);
		}

		/// <summary>
		/// Compare values.
		/// <para>Ranges are not compared.</para>
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		public static bool operator !=(Ranged<T> a, Ranged<T> b)
		{
			return !(a == b);
		}

		/// <summary>
		/// Copies this Ranged and sets a new value to the clone.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		/// <exception cref="InvalidOperationException">This Ranged is not initialized.</exception>
		/// <exception cref="ArgumentOutOfRangeException">The new value is not in the range.</exception>
		public Ranged<T> Clone(T value)
		{
			if (!this.isInitialized)
				throw new InvalidOperationException();
			var clone = this;
			clone.Value = value;
			return clone;
		}

		/// <summary>
		/// Transtype a ranged value to the original type by extracting the value itself.
		/// </summary>
		/// <param name="value"></param>
		/// <exception cref="System.InvalidOperationException">If the value is undefined.</exception>
		public static implicit operator T(Ranged<T> value)
		{
			return value.Value;
		}

		// Note: there is no transtyping from T to Ranged<T> because we would miss the range.

		/// <summary>
		/// Returns a string that represents the value. Or "&lt;unintialized&gt;" if the value is undefined.
		/// </summary>
		/// <returns>The string that represents the value. Or "&lt;unintialized&gt;" if the value is undefined.</returns>
		public override string ToString()
		{
			if (!isInitialized)
				return "<unintialized>";
			return _Value.ToString();
		}

		/// <summary>
		/// Returns the hash code, or zero if the value is undefined.
		/// </summary>
		/// <returns></returns>
		public override int GetHashCode()
		{
			if (!isInitialized)
				return 0;
			return _Value.GetHashCode();
		}

		/// <summary>
		/// Compare values.
		/// <para>Ranges are not compared.</para>
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public override bool Equals(object obj)
		{
			if (!isInitialized)
				return false;
			if (!(obj is T))
				return false;
			return _Value.Equals(obj);
		}

		/// <summary>
		/// True if the value is defined.
		/// </summary>
		public bool HasValue
		{
			get { return this.isInitialized; }
		}

		/// <summary>
		/// Compare values.
		/// <para>Ranges are not compared.</para>
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public bool Equals(T other)
		{
			if (!isInitialized)
				return false;
			return this._Value.Equals(other);
		}

		/// <summary>
		/// Compare this value to the other.
		/// Then returns 0 is they are equal, -1 if this value is lower than the other, or +1 if this value is greater than the other.
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		/// <exception cref="System.InvalidOperationException">If either this value or the other value are undefined.</exception>
		public int CompareTo(Ranged<T> other)
		{
			if (!isInitialized || !other.isInitialized)
				throw new InvalidOperationException("The structure was not initialized (constructed).");
			return this._Value.CompareTo(other);
		}

		/// <summary>
		/// Compare this value to the other.
		/// Then returns 0 is they are equal, -1 if this value is lower than the other, or +1 if this value is greater than the other.
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		/// <exception cref="System.InvalidOperationException">If either this value or the other value are undefined.</exception>
		public int CompareTo(T other)
		{
			if (!isInitialized)
				throw new InvalidOperationException("The structure was not initialized (constructed).");
			return this._Value.CompareTo(other);
		}

		/// <summary>
		/// True if this value equals the other one.
		/// False if one (or two) of the values is undefined.
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public bool Equals(Ranged<T> other)
		{
			if (!isInitialized || !other.isInitialized)
				return false;
			return this._Value.Equals(other.Value);
		}
	}
#endif

	#endregion Bounded

}