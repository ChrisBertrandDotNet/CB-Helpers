
// Copyright Christophe Bertrand
// https://github.com/ChrisBertrandDotNet/CB-Helpers
// https://chrisbertrand.net

/*
 * Functions and types intended to transcode between a type and a String (both directions).
 * 
 * WARNING: this file seems to be incomplete.
 * 
 */

/* TODO:
- Add functions to class StringTranscoder in order to search (by reflexion) separately the 3 static functions:
	- Return<T, ReturnSuccess> Parse(string s);
	- Return<T, ReturnSuccess> Parse(string s, IFormatProvider provider);
	- Return<T, ReturnSuccess> ParseInvariantString(string s);

- The file name should be renamed as `StringTranstyper.cs`.

*/

#if TEST
using CBdotnet.Test;
#endif
using CB.Execution;
using System;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace CB.Data
{

	/// <summary>
	/// To String transcoding functions.
	/// <para>You have to include three additional static functions:
	/// <code>public static <see cref="CB.Execution.Ret{T}"/> ParseFromInvariantString(string s)</code>
	/// and
	/// <code>public static <see cref="CB.Execution.Ret{T}"/> ParseFromString(string s)</code>
	/// and
	/// <code>public static <see cref="CB.Execution.Ret{T}"/> ParseFromString(string s, IFormatProvider provider)</code>
	/// </para>
	/// </summary>
	public interface IToString
	{
		#region static functions
#if false // C# and dotnet do not let declaring static functions in an interface, so here is their definition.
		Return<T, ReturnSuccess> Parse(string s);
		Return<T, ReturnSuccess> Parse(string s, IFormatProvider provider);
		Return<T, ReturnSuccess> ParseInvariantString(string s);
#endif
		#endregion static functions

		/// <summary>
		/// Returns a string that represents the current object, using the given formatter.
		/// </summary>
		/// <param name="provider"></param>
		/// <returns></returns>
		Ret<string> ToString(IFormatProvider provider);

		/// <summary>
		/// Returns a string that represents the current object, using the invariant formatter.
		/// </summary>
		/// <returns></returns>
		Ret<string> ToInvariantString();
	}


	/// <summary>
	/// Optimized String-to-any-type transcoders.
	/// <para>Please read <see cref="IToString"/>.</para>
	/// </summary>
	public static class StringTranscoder
	{
		/// <summary>
		/// Returns the static function that parses a String then creates an instance of the given type T.
		/// The signature of this function is <code></code>
		/// <para>Please read <see cref="IToString"/>.</para>
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static Func<string, T> GetStringToTypeFunction<T>()
		{
			var typeCode = Type.GetTypeCode(typeof(T));
			switch (typeCode)
			{
				case TypeCode.Boolean:
					return (Func<string, T>)(Delegate)(Func<string, Boolean>)Boolean.Parse;
				case TypeCode.Byte:
					return (Func<string, T>)(Delegate)(Func<string, Byte>)(t => Byte.Parse(t, CultureInfo.InvariantCulture));
				case TypeCode.Char:
					return (Func<string, T>)(Delegate)(Func<string, Char>)Char.Parse;
				case TypeCode.DateTime:
					return (Func<string, T>)(Delegate)(Func<string, DateTime>)(t => DateTime.Parse(t, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal));
				case TypeCode.DBNull:
					return null;
				case TypeCode.Decimal:
					return (Func<string, T>)(Delegate)(Func<string, Decimal>)(t => Decimal.Parse(t, CultureInfo.InvariantCulture));
				case TypeCode.Double:
					return (Func<string, T>)(Delegate)(Func<string, Double>)(t => Double.Parse(t, CultureInfo.InvariantCulture));
				case TypeCode.Empty:
					return null;
				case TypeCode.Int16:
					return (Func<string, T>)(Delegate)(Func<string, Int16>)(t => Int16.Parse(t, CultureInfo.InvariantCulture));
				case TypeCode.Int32:
					return (Func<string, T>)(Delegate)(Func<string, Int32>)(t => Int32.Parse(t, CultureInfo.InvariantCulture));
				case TypeCode.Int64:
					return (Func<string, T>)(Delegate)(Func<string, Int64>)(t => Int64.Parse(t, CultureInfo.InvariantCulture));
				case TypeCode.Object:
					if (typeof(T)==typeof(object))
						return (Func<string, T>)(Delegate)(Func<string, object>)(t => t);
					return getStringToType<T>(); // The interesting part.
				case TypeCode.SByte:
					return (Func<string, T>)(Delegate)(Func<string, SByte>)(t => SByte.Parse(t, CultureInfo.InvariantCulture));
				case TypeCode.Single:
					return (Func<string, T>)(Delegate)(Func<string, Single>)(t => Single.Parse(t, CultureInfo.InvariantCulture));
				case TypeCode.String:
					return (Func<string, T>)(Delegate)(Func<string, string>)(a => a);
				case TypeCode.UInt16:
					return (Func<string, T>)(Delegate)(Func<string, UInt16>)(t => UInt16.Parse(t, CultureInfo.InvariantCulture));
				case TypeCode.UInt32:
					return (Func<string, T>)(Delegate)(Func<string, UInt32>)(t => UInt32.Parse(t, CultureInfo.InvariantCulture));
				case TypeCode.UInt64:
					return (Func<string, T>)(Delegate)(Func<string, UInt64>)(t => UInt64.Parse(t, CultureInfo.InvariantCulture));
				default:
					throw new NotSupportedException();
			}
		}

		/// <summary>
		/// Returns null if no transcoding function was found.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		static Func<string, T> getStringToType<T>()
		{
			/*// 1st option: use my interface IStringTranscode
			{
				if (typeof(T).GetInterfaces().Any(t => t == typeof(IStringTranscodable<T>)))
					return s => ((IStringTranscodable<T>)null).ParseInvariantString(s);

			}*/

			{
				var mis = typeof(T).GetMethods(BindingFlags.Static | BindingFlags.Public);

				// 1st option: look for a function Parse(string, IFormatProvider)
				foreach (var m in mis)
				{
					if (m.IsStatic && !m.IsAbstract && m.Name == "Parse" && m.ReturnType == typeof(T))
					{
						var pars = m.GetParameters();
						if (pars.Length == 2 && pars[0].ParameterType == typeof(string) && pars[1].ParameterType == typeof(IFormatProvider))
						{
							var d = Delegate.CreateDelegate(typeof(Func<string, IFormatProvider, T>), m, false);
							if (d != null)
							{
								var f = (Func<string, IFormatProvider, T>)d;
								return (Func<string, T>)(a => f(a, CultureInfo.InvariantCulture));
							}
						}
					}
				}

				// 2nd option: look for functions Parse(string) and ParseFromInvariantString(string) (of IToString)
				foreach (var m in mis)
				{
					if (m.IsStatic && !m.IsAbstract && m.Name == "Parse" && m.ReturnType == typeof(T))
					{
						var pars = m.GetParameters();
						if (pars.Length == 1 && pars[0].ParameterType == typeof(string))
						{
							var d = Delegate.CreateDelegate(typeof(Func<string, T>), m, false);
							if (d != null)
								return (Func<string, T>)d;
						}
					}
				}

				// 3rd option: look for functions ParseFromInvariantString(string) of IToString
				foreach (var m in mis)
				{
					if (m.IsStatic && !m.IsAbstract && m.Name == "ParseFromInvariantString"/* of IToString */ && m.ReturnType == typeof(Ret<T>))
					{
						var pars = m.GetParameters();
						if (pars.Length == 1 && pars[0].ParameterType == typeof(string))
						{
							var d = Delegate.CreateDelegate(typeof(Func<string, Ret<T>>), m, false);
							if (d != null)
							{
								var f = (Func<string, Ret<T>>)d;
								return s => f(s).Value;
							}
						}
					}
				}
			}

			// 4ht options: TypeConverter.ConvertFromInvariantString()
			{
				var c = System.ComponentModel.TypeDescriptor.GetConverter(typeof(T));
				if (c != null)
				{
					if (c.CanConvertFrom(typeof(string)))
						return (Func<string, T>)(a => (T)c.ConvertFromInvariantString(a));
				}
			}

			return null;
		}

		/// <summary>
		/// Cherche fonctions ToString(IFormatProvider) et ToString().
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		static Func<T, string> getTypeToString<T>()
		{
			// 1st option: my IToString
			if (typeof(T).GetInterfaces().Any(t => t == typeof(IToString)))
				return a => ((IToString)a).ToInvariantString().Value;

			// 2nd option: IConvertible
			if (typeof(T).GetInterfaces().Any(t => t == typeof(IConvertible)))
				return a => ((IConvertible)a).ToString(CultureInfo.InvariantCulture);

			// 3rd option: ToString(IFormatProvider)
			{
				var typeCode = Type.GetTypeCode(typeof(T));
				var mis = typeof(T).GetMethods();
				foreach (var m in mis)
				{
					if (!m.IsStatic && !m.IsAbstract)
						if (m.ReturnType == typeof(string))
						{
							var pars = m.GetParameters();
							if (pars.Length == 1)
								if (pars[0].ParameterType == typeof(IFormatProvider))
								{
									var d = Delegate.CreateDelegate(typeof(Func<T, IFormatProvider, string>), null, m, false);
									if (d != null)
									{
										var f = (Func<T, IFormatProvider, string>)d;
										return (a => f(a, CultureInfo.InvariantCulture));
									}
								}
						}
				}
			}

			// 4ht option: TypeConverter.ConvertToInvariantString()
			{
				var c = System.ComponentModel.TypeDescriptor.GetConverter(typeof(T));
				if (c != null)
				{
					if (c.CanConvertTo(typeof(string)))
						return (a => c.ConvertToInvariantString(a));
				}
			}

			// Last option, a chance: ToString().
			return (Func<T, string>)(a => a.ToString()); // returns the standard ToString function, ignoring the risk of wrong formatting.
		}

#if TEST
		[Test]
		static void test()
		{
			var t = typeof(Uri);
			var c = System.ComponentModel.TypeDescriptor.GetConverter(t);
			if (c != null)
			{
				Testeur.TesteVérité(c.CanConvertTo(typeof(string)) && c.CanConvertFrom(typeof(string)));

				var f = (Func<string, Uri>)(a => (Uri)c.ConvertFromInvariantString(a));
				var f2 = (Func<Uri, string>)(a => c.ConvertToInvariantString(a));
				var f3 = (Func<Uri, string>)(c.ConvertToInvariantString);

				Uri v = new Uri("truc", UriKind.Relative);
				Testeur.TesteVérité(c.IsValid(v));

				var texte = f3(v);
				Testeur.TesteÉgalité(texte, "truc");
				var v2 = f(texte);
				Testeur.TesteLeTypePrécis<Uri>(v2, typeof(Uri));
			}
		}
#endif

		/// <summary>
		/// Always returns a function, the default Object.ToString() at worst.
		/// <para>Please read <see cref="IToString"/>.</para>
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static Func<T, string> GetTypeToStringFunction<T>()
		{
			var typeCode = Type.GetTypeCode(typeof(T));
			switch (typeCode)
			{
				case TypeCode.Boolean:
					return (Func<T, string>)(object)(Func<bool, string>)(a => a.ToString(CultureInfo.InvariantCulture));
				case TypeCode.Byte:
					return (Func<T, string>)(object)(Func<byte, string>)(a => a.ToString(CultureInfo.InvariantCulture));
				case TypeCode.Char:
					return (Func<T, string>)(object)(Func<char, string>)(a => a.ToString(CultureInfo.InvariantCulture));
				case TypeCode.DateTime:
					return (Func<T, string>)(object)(Func<DateTime, string>)(a => a.ToUniversalTime().ToString(CultureInfo.InvariantCulture));
				case TypeCode.DBNull:
					return (Func<T, string>)(object)(Func<DBNull, string>)(a => a.ToString(CultureInfo.InvariantCulture));
				case TypeCode.Decimal:
					return (Func<T, string>)(object)(Func<Decimal, string>)(a => a.ToString(CultureInfo.InvariantCulture));
				case TypeCode.Double:
					return (Func<T, string>)(object)(Func<double, string>)(a => a.ToString(CultureInfo.InvariantCulture));
				case TypeCode.Empty:
					return null;
				case TypeCode.Int16:
					return (Func<T, string>)(object)(Func<Int16, string>)(a => a.ToString(CultureInfo.InvariantCulture));
				case TypeCode.Int32:
					return (Func<T, string>)(object)(Func<int, string>)(a => a.ToString(CultureInfo.InvariantCulture));
				case TypeCode.Int64:
					return (Func<T, string>)(object)(Func<Int64, string>)(a => a.ToString(CultureInfo.InvariantCulture));
				case TypeCode.Object:
					return getTypeToString<T>();
				case TypeCode.SByte:
					return (Func<T, string>)(object)(Func<SByte, string>)(a => a.ToString(CultureInfo.InvariantCulture));
				case TypeCode.Single:
					return (Func<T, string>)(object)(Func<Single, string>)(a => a.ToString(CultureInfo.InvariantCulture));
				case TypeCode.String:
					return (Func<T, string>)(object)(Func<string, string>)(a => a);
				case TypeCode.UInt16:
					return (Func<T, string>)(object)(Func<UInt16, string>)(a => a.ToString(CultureInfo.InvariantCulture));
				case TypeCode.UInt32:
					return (Func<T, string>)(object)(Func<UInt32, string>)(a => a.ToString(CultureInfo.InvariantCulture));
				case TypeCode.UInt64:
					return (Func<T, string>)(object)(Func<UInt64, string>)(a => a.ToString(CultureInfo.InvariantCulture));
				default:
					throw new NotSupportedException();
			}
		}
	}
}