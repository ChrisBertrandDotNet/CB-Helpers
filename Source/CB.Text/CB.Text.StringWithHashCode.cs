
// Copyright Christophe Bertrand
// https://github.com/ChrisBertrandDotNet/CB-Helpers
// https://chrisbertrand.net

/* Requires:
 */

/*

Cached string classes that optimize comparisons.
That is useful to speedup string dictionaries.
 */

/* TODO: add a cache for string comparisons, so that dictionaries are as fast as possible.
Because dictionaries need to compare entire string when searching or adding a key, and this comparison is very slow.
I should add such a cache to StringWithCache.
- constructor StringWithCache(string s, IEqualityComparer<string> equalityComparer=null, ICompare<string> differenceComparer=null)
- Every comparison of two strings should be added to the cache.
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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace CB.Text
{
	/// <summary>
	/// A string and its pre-computed hash code.
	/// <para>The pre-computed hash code accelerates comparisons (when strings are different).</para>
	/// <para>Useful to speedup string dictionaries.</para>
	/// <para>Like <see cref="System.String"/>, this value is immutable (invariable).</para>
	/// </summary>
	public struct StringWithHashCode : IEquatable<StringWithHashCode>, IComparable<StringWithHashCode>
	{
		/// <summary>
		/// The main value is the string.
		/// <para>Can be null.</para>
		/// </summary>
		public readonly string Value;
		/// <summary>
		/// The hash code of our string.
		/// </summary>
		public readonly int HashCode;
		//readonly int firstBytes;
		/// <summary>
		/// Gets the number of characters in the string.
		/// </summary>
		public int Length { get { return Value.Length; } }
		/// <summary>
		/// True is the string is not null.
		/// </summary>
		public bool HasValue { get { return Value != null; } }
		/// <summary>
		/// An empty <see cref="CB.Text.StringWithHashCode"/>.
		/// </summary>
		public static readonly StringWithHashCode Empty = new StringWithHashCode(string.Empty);
		/// <summary>
		/// Builds a new <see cref="CB.Text.StringWithHashCode"/>.
		/// </summary>
		/// <param name="value"></param>
		public StringWithHashCode(string value)
		{
			if (value != null)
			{
				this.Value = value;
				this.HashCode = value.GetHashCode();
			}
			else
			{
				this.Value = null;
				this.HashCode = 0;
			}
		}
		/// <summary>
		/// Gets the hash code of the string, or 0 if the string is null.
		/// </summary>
		/// <returns></returns>
		public override int GetHashCode()
		{
			return this.HashCode;
		}
		/// <summary>
		/// Returns the string itself, or null if the string is null.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return Value;
		}
		/// <summary>
		/// True if the string equals the <paramref name="obj"/>.
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public override bool Equals(object obj)
		{
			if (obj == null)
				return false;
			if (obj is StringWithHashCode)
				return this == (StringWithHashCode)obj;
			if (obj is string)
				return this.Value == (string)obj;
			return base.Equals(obj);
		}
		/// <summary>
		/// True if a == b.
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		public static bool operator ==(StringWithHashCode a, StringWithHashCode b)
		{
			if (object.ReferenceEquals(a.Value, b.Value))
				return true;
			if (a.HashCode == b.HashCode)
				if (a.Length == b.Length)
					return a.Value == b.Value;
			return false;
		}
		/// <summary>
		/// True if a != b.
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		public static bool operator !=(StringWithHashCode a, StringWithHashCode b)
		{
			return !(a == b);
		}
		/// <summary>
		/// True if a == b.
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		public static bool operator ==(StringWithHashCode a, string b)
		{
			if (a.Length != b.Length)
				return false;
			return a.Value == b;
		}
		/// <summary>
		/// True if a != b.
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		public static bool operator !=(StringWithHashCode a, string b)
		{
			return !(a == b);
		}
		/// <summary>
		/// True if the string equals the <paramref name="other"/>.
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public bool Equals(StringWithHashCode other)
		{
			return this == other;
		}
		/// <summary>
		/// Retuns 0 if the strings are equal, or if both have no value.
		/// Otherwize, compare positions in the sort order (see <see cref="System.String.CompareTo(string)"/> for more information).
		/// Retuns -1 if this string is less than the <paramref name="other"/>, or if this string has no value while the <paramref name="other"/> has a value.
		/// Retuns 1 if this string is more than the <paramref name="other"/>, or if this string has a value while the <paramref name="other"/> has no value.
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public int CompareTo(StringWithHashCode other)
		{
			if (!this.HasValue && !other.HasValue)
				return 0;
			if (!this.HasValue && other.HasValue)
				return -1;
			if (this.HasValue && !other.HasValue)
				return 1;
			return this.Value.CompareTo(other.Value);
		}
		/// <summary>
		/// Transtype the string to a <see cref="CB.Text.StringWithHashCode"/>.
		/// </summary>
		/// <param name="s"></param>
		public static implicit operator StringWithHashCode(string s)
		{
			return new StringWithHashCode(s);
		}
		/// <summary>
		/// Transtype the <see cref="CB.Text.StringWithHashCode"/> to a string.
		/// </summary>
		/// <param name="s"></param>
		public static implicit operator string(StringWithHashCode s)
		{
			return s.Value;
		}
	}

	/// <summary>
	/// CB: A string and its pre-computed hash code. A cache memorizes equality comparisons (but not <see ref="CB.Text.StringWithCache.CompareTo(StringWithCache)"/>).
	/// <para>Useful when comparing big strings that are often equal.</para>
	/// <para>Like <see cref="System.String"/>, this value is immutable (invariable).</para>
	/// </summary>
	public struct StringWithCache : IEquatable<StringWithCache>, IComparable<StringWithCache>
	{
		/// <summary>
		/// The main value is the string.
		/// <para>Can be null.</para>
		/// </summary>
		public readonly string Value;
		/// <summary>
		/// The hash code of the string.
		/// <para>This value is cached (calculated once for all and memorized accross instances).</para>
		/// </summary>
		public readonly int HashCode;

		static readonly Dictionary<WeakReference<string>, S2> unicityCache = new Dictionary<WeakReference<string>, S2>(new compWR());
		static Stopwatch stopWatch;
		/// <summary>
		/// Gets the number of characters in the string.
		/// </summary>
		public int Length { get { return Value.Length; } } // Test .NET 4.7 : String.Length is cached (apparently there is an internal field).
		/// <summary>
		/// True is the string is not null.
		/// </summary>
		public bool HasValue { get { return Value != null; } }
		/// <summary>
		/// An empty <see cref="CB.Text.StringWithCache"/>.
		/// </summary>
		public static readonly StringWithCache Empty = new StringWithCache(string.Empty);

		struct S2
		{
			internal WeakReference<string> TextWR;
			internal int HashCode;
		}

		class compWR : IEqualityComparer<WeakReference<string>>
		{
			public bool Equals(WeakReference<string> x, WeakReference<string> y)
			{
				string t1, t2;
				if (x.TryGetTarget(out t1))
					if (y.TryGetTarget(out t2))
						return t1 == t2;
				return false;
			}

			public int GetHashCode(WeakReference<string> obj)
			{
				string t;
				if (obj.TryGetTarget(out t))
					return t.GetHashCode();
				return 0;
			}
		}

		StringWithCache(string value, bool noCache)
		{
			this.Value = value;
			this.HashCode = value.GetHashCode();
		}
		/// <summary>
		/// Builds a new <see cref="CB.Text.StringWithCache"/>.
		/// </summary>
		/// <param name="value"></param>
		public StringWithCache(string value)
		{
			if (value != null)
			{
				this.Value = null;
				this.HashCode = 0;
				S2 v2;
				WeakReference<string> wr = new WeakReference<string>(value);
				bool créer = !unicityCache.TryGetValue(wr, out v2);
				if (!créer)
				{
					if (v2.TextWR.TryGetTarget(out this.Value))
					{
						this.HashCode = v2.HashCode;
					}
					else
					{
						unicityCache.Remove(wr);
						créer = true;
					}
				}
				if (créer)
				{
					if (stopWatch==null)
					{
						stopWatch = new Stopwatch();
						stopWatch.Start();
					}
					else
					{
						if (stopWatch.ElapsedMilliseconds > 1000.0)
						{
							// delete useless weak references:
							string s;
							foreach (var item in unicityCache)
								if (!item.Key.TryGetTarget(out s))
									unicityCache.Remove(item.Key);
							// reset the stopwatch:
							stopWatch.Reset();
							stopWatch.Start();
						}
					}
					v2 = new S2() { HashCode = value.GetHashCode(), TextWR = wr };
					unicityCache.Add(wr, v2);
					this.Value = value;
					this.HashCode = v2.HashCode;
				}
			}
			else
			{
				this.Value = null;
				this.HashCode = 0;
			}
		}
		/// <summary>
		/// Gets the hash code of the string, or 0 if the string is null.
		/// <para>This value is cached (calculated once for all and memorized accross instances).</para>
		/// </summary>
		/// <returns></returns>
		public override int GetHashCode()
		{
			return this.HashCode;
		}
		/// <summary>
		/// Returns the string itself, or null if the string is null.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return Value;
		}
		/// <summary>
		/// True if the string equals the <paramref name="obj"/>.
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public override bool Equals(object obj)
		{
			if (obj == null)
				return false;
			if (obj is StringWithCache)
				return this == (StringWithCache)obj;
			if (obj is string)
				return this.Value == (string)obj;
			return base.Equals(obj);
		}
		/// <summary>
		/// True if the string equals the <paramref name="other"/>.
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public bool Equals(StringWithCache other)
		{
			return this == other;
		}
		/// <summary>
		/// True if a == b.
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		public static bool operator ==(StringWithCache a, StringWithCache b)
		{
			if (object.ReferenceEquals(a.Value, b.Value))
				return true;
			if (a.HashCode == b.HashCode)
				if (a.Length == b.Length)
					return a.Value == b.Value;
			return false;
		}
		/// <summary>
		/// True if a != b.
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		public static bool operator !=(StringWithCache a, StringWithCache b)
		{
			return !(a == b);
		}
		/// <summary>
		/// True if a == b.
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		public static bool operator ==(StringWithCache a, string b)
		{
			if (a.Length != b.Length)
				return false;
			return a.Value == b;
		}
		/// <summary>
		/// True if a != b.
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		public static bool operator !=(StringWithCache a, string b)
		{
			return !(a == b);
		}
		/// <summary>
		/// Transtype the string to a <see cref="CB.Text.StringWithCache"/>.
		/// </summary>
		/// <param name="s"></param>
		public static implicit operator StringWithCache(string s)
		{
			return new StringWithCache(s);
		}
		/// <summary>
		/// Transtype the <see cref="CB.Text.StringWithCache"/> to a string.
		/// </summary>
		/// <param name="s"></param>
		public static implicit operator string(StringWithCache s)
		{
			return s.Value;
		}


#if DEBUG
		internal static class TestClass
		{
			internal static void TestDeVitesse()
			{
				var r = new Random();
				var t = new string(new char[100].Select(c => (char)r.Next(256)).ToArray());
				var tc2 = t.ToArray();
				//tc2[tc2.Length - 1] = '.';
				string t2 = new string(tc2);
				bool egal = t == t2;
				bool mr = object.ReferenceEquals(t, t2);
				//if (!egal || mr)
				if (mr)
					throw new Exception();
				//t2 = t2.Substring(1);


				/*HashSet<WeakReference< string>> h = new HashSet<WeakReference<string>>(new compWR());
				h.Add(new WeakReference<string>( t));
				var wt1 = new WeakReference<string>(t);
				var contientDéjà1 = h.Contains(wt1);
				var wt2 = new WeakReference<string>(t2);
				var contientDéjà2 = h.Contains(wt2);*/

				for (int i = 0; i < 3; i++)
				{
					testStringGetHashCode(t);
					testStringWithCacheGetHashCode(t);
				}
				for (int i = 0; i < 3; i++)
				{
					testStringCompare(t, t2);
					testStringWithCacheCompare(t, t2);
				}
				for (int i = 0; i < 3; i++)
				{
					testStringLength(t);
					testStringWithCacheLength(t);
				}
			}

			/*class compWR : IEqualityComparer<WeakReference<string>>
			{
				public bool Equals(WeakReference<string> x, WeakReference<string> y)
				{
					string t1, t2;
					if (x.TryGetTarget(out t1))
						if (y.TryGetTarget(out t2))
							return t1 == t2;
					return false;
				}

				public int GetHashCode(WeakReference<string> obj)
				{
					string t;
					if (obj.TryGetTarget(out t))
						return t.GetHashCode();
					return 0;
				}
			}*/

			static void testStringGetHashCode(string t)
			{
				var sw = new Stopwatch();
				sw.Start();
				int n = 0;
				for (int i = 0; i < 10000000; i++)
					n = n + t.GetHashCode();
				sw.Stop();
				Console.WriteLine("GetHashCode string: temps= {0} ms, r={1}.", sw.ElapsedMilliseconds, n);
			}

			static void testStringWithCacheGetHashCode(string t)
			{
				var sw = new Stopwatch();
				var swhc = new StringWithCache(t);
				sw.Start();
				int n = 0;
				for (int i = 0; i < 10000000; i++)
					n = n + swhc.GetHashCode();
				sw.Stop();
				Console.WriteLine("GetHashCode StringWithCache: temps= {0} ms, r={1}.", sw.ElapsedMilliseconds, n);
			}

			static void testStringCompare(string t, string t2)
			{
				var sw = new Stopwatch();
				sw.Start();
				int n = 0;
				for (int i = 0; i < 10000000; i++)
					if (t == t2)
						n++;
				sw.Stop();
				Console.WriteLine("== string: temps= {0} ms, r={1}.", sw.ElapsedMilliseconds, n);
			}

			static void testStringWithCacheCompare(string t, string t2)
			{
				var sw = new Stopwatch();
				var swhc = new StringWithCache(t);
				var swhc2 = new StringWithCache(t2);
				sw.Start();
				int n = 0;
				for (int i = 0; i < 10000000; i++)
					if (swhc == swhc2)
						n++;
				sw.Stop();
				Console.WriteLine("== StringWithCache: temps= {0} ms, r={1}.", sw.ElapsedMilliseconds, n);
			}

			static void testStringLength(string t)
			{
				var sw = new Stopwatch();
				sw.Start();
				int n = 0;
				for (int i = 0; i < 10000000; i++)
					n += t.Length;
				sw.Stop();
				Console.WriteLine("Length string: temps= {0} ms, r={1}.", sw.ElapsedMilliseconds, n);
			}

			static void testStringWithCacheLength(string t)
			{
				var sw = new Stopwatch();
				var swhc = new StringWithCache(t);
				sw.Start();
				int n = 0;
				for (int i = 0; i < 10000000; i++)
					n += swhc.Length;
				sw.Stop();
				Console.WriteLine("Length StringWithCache: temps= {0} ms, r={1}.", sw.ElapsedMilliseconds, n);
			}
		}
#endif
		/// <summary>
		/// Retuns 0 if the strings are equal, or if both have no value.
		/// Otherwize, compare positions in the sort order (see <see cref="System.String.CompareTo(string)"/> for more information).
		/// Retuns -1 if this string is less than the <paramref name="other"/>, or if this string has no value while the <paramref name="other"/> has a value.
		/// Retuns 1 if this string is more than the <paramref name="other"/>, or if this string has a value while the <paramref name="other"/> has no value.
		/// <para>Warning: this comparison is not cached (unlike the equality comparison).</para>
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public int CompareTo(StringWithCache other)
		{
			if (!this.HasValue && !other.HasValue)
				return 0;
			if (!this.HasValue && other.HasValue)
				return -1;
			if (this.HasValue && !other.HasValue)
				return 1;
			return this.Value.CompareTo(other.Value);
		}
	}
}