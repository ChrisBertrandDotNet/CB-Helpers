
// Copyright Christophe Bertrand
// https://github.com/ChrisBertrandDotNet/CB-Helpers
// https://chrisbertrand.net

/* Requires:
. CB.Reflection\CB.Reflection.TypeEx.cs
*/

/*
 */

using System;
using System.Collections.Generic;
using System.Linq;

namespace CB.Data
{
	/// <summary>
	/// Fast tests on Enum.
	/// </summary>
	/// <typeparam name="T">T must be an enumeration type.</typeparam>
	public static class EnumHelper<T>
		where T : struct
	{
		/// <summary>
		/// All the declared values of this enumeration. The returned list is cached.
		/// </summary>
		public static readonly IReadOnlyList<T> Values = new CB.Data.ImmutableList<T>((T[])Enum.GetValues(typeof(T)));

		/// <summary>
		/// The real type that actually stores values.
		/// By default, enumerations are stores as Int32.
		/// </summary>
		public static readonly Type UnderlyingType = Enum.GetUnderlyingType(typeof(T));

		/// <summary>
		/// True if the real storing type is Int64 or UInt64.
		/// </summary>
		public static readonly bool UnderlyingTypeIs64Bits = UnderlyingType == typeof(Int64) || UnderlyingType == typeof(UInt64);

		/// <summary>
		/// If the real storing type is Int64 or UInt64, returns an array of all declared values. Otherwize, returns null.
		/// </summary>
		public static readonly UInt64[] ValuesAsUInt64 = UnderlyingTypeIs64Bits ? Values.Select(v => Convert.ToUInt64(v)).ToArray() : null;

		/// <summary>
		/// If the real storing type is not 64-bits, returns an array of all declared values. Otherwize, returns null.
		/// </summary>
		public static readonly UInt32[] ValuesAsUInt32 = UnderlyingTypeIs64Bits ? null : Values.Select(v => Convert.ToUInt32(v)).ToArray();

		/// <summary>
		/// True is the enumeration can be treated as a bit field; that is, a set of flags.
		/// </summary>
		public static readonly bool HasFlagsAttribute = typeof(T).GetCustomAttributes(typeof(FlagsAttribute), false).Length > 0;

		/// <summary>
		/// A class that can be used as a value comparer.
		/// <para>This class implements <see cref="System.Collections.Generic.EqualityComparer{T}"/>.</para>
		/// </summary>
		public static readonly IEqualityComparer<T> Comparer = EqualityComparer<T>.Default;

#if DEBUG
		static EnumHelper()
		{
			if (!typeof(T).IsEnum)
				//throw new Exception(string.Format("Type {0} is not an enumeration.", typeof(T).FullName));
				System.Diagnostics.Debug.WriteLine(string.Format("Can't use {0} on type {1} because the latter is not an enumeration.", typeof(EnumHelper<T>).FullName, typeof(T).FullName));
		}
#endif

		/// <summary>
		/// True if v1 is equal to v2.
		/// </summary>
		/// <param name="v1"></param>
		/// <param name="v2"></param>
		/// <returns></returns>
		public static bool Equal(T v1, T v2)
		{
			return Comparer.Equals(v1, v2);
		}

		/// <summary>
		/// Checks the enumeration value is defined in its enumeration type.
		/// <para>The value can be a multiple values combination if <typeparamref name="T"/> has FlagsAttribute.</para>
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static bool IsDefinedValue(T value)
		{
			if (Values.Contains(value))
				return true;
			// check if the enumeration is a [Flags] and if its value is a combination of various valid values:
			if (UnderlyingTypeIs64Bits)
			{
				ulong reste = Convert.ToUInt64(value);
				if (HasFlagsAttribute && reste != 0ul)
				{
					for (int i = 0; i < ValuesAsUInt64.Length; i++)
					{
						var vUl = ValuesAsUInt64[i];

						if ((reste & vUl) == vUl)
						{
							reste ^= vUl;
							if (reste == 0ul)
								return true;
						}
					}
				}
			}
			else
			{
				ulong reste = Convert.ToUInt32(value);
				if (HasFlagsAttribute && reste != 0ul)
				{
					for (int i = 0; i < ValuesAsUInt32.Length; i++)
					{
						var vUl = ValuesAsUInt32[i];

						if ((reste & vUl) == vUl)
						{
							reste ^= vUl;
							if (reste == 0ul)
								return true;
						}
					}
				}
			}
			return false;
		}

		/// <summary>
		/// Checks the enumeration value is one of the defined values in its enumeration type.
		/// <para>The value can not be a multiple values combination, even if <typeparamref name="T"/> has FlagsAttribute.</para>
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static bool IsDeclaredAsAUniqueValue(T value)
		{
			if (UnderlyingTypeIs64Bits)
			{
				ulong v = Convert.ToUInt64(value);
				for (int i = 0; i < ValuesAsUInt64.Length; i++)
					if (ValuesAsUInt64[i] == v)
						return true;
			}
			else
			{
				ulong v = Convert.ToUInt32(value);
				for (int i = 0; i < ValuesAsUInt32.Length; i++)
					if (ValuesAsUInt32[i] == v)
						return true;
			}
			return false;
		}

		[Obsolete("Use IsDeclaredAsAUniqueValue()")]
		internal static bool IsDefinedAsOneValue(T value)
		{
			return IsDeclaredAsAUniqueValue(value);
		}
	}
}