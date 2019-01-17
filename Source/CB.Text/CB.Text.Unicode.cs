
// Copyright Christophe Bertrand.
// https://chrisbertrand.net
// https://github.com/ChrisBertrandDotNet

/* Requires:
 */


/*
	Unicode can be quite complex.
	. Code Points are parts of 'characters' (graphemes).
	. Text Elements are 'characters' (graphemes). They may be generated combining several Code Points.
 */

/* Project's symbols:
* Console : (nothing)
* WPF : (nothing)
* Windows 8.1 for Windows Store : NETFX_CORE;WINDOWS_APP
* Windows 8 universel for Windows (Store) : NETFX_CORE;WINDOWS_APP
* Windows 8 universel for Windows Phone (Store) : NETFX_CORE;WINDOWS_PHONE_APP
* Windows 10 UWP (Store) : NETFX_CORE;WINDOWS_UWP
*/

#if WINDOWS_APP || WINDOWS_PHONE_APP
#define WINDOWS_8_STORE // Windows 8, not Windows 10.
#endif

/* TEST:
			string s = "\U0001D162\U0001D181"; // 🌀 El Niño
				//"\U0001F300 El Ni\u006E\u0303o";
				//"\U0001D162\U0001D181".Normalize();
			var codePoints = Unicode.ToCodePoints(s);
			var codePoints2 = Unicode.ToCodePoints2(s).ToArray();
			var textElements = Unicode.ToTextElements(s);
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CB.Text
{
	/// <summary>
	/// Helpers for Unicode texts.
	/// </summary>
	public static class Unicode
	{
		/// <summary>
		/// Returns code points.
		/// </summary>
		/// <param name="str"></param>
		/// <returns></returns>
		public static int[] ToCodePoints(string str) // https://stackoverflow.com/a/28155130
		{
			if (str == null)
				throw new ArgumentNullException("str");

			var codePoints = new List<int>(str.Length);
			for (int i = 0; i < str.Length; i++)
			{
				codePoints.Add(Char.ConvertToUtf32(str, i));
				if (Char.IsHighSurrogate(str[i]))
					i += 1;
			}

			return codePoints.ToArray();
		}

		/// <summary>
		/// Returns code points (alternate method).
		/// </summary>
		/// <param name="s"></param>
		/// <returns></returns>
		public static IEnumerable<int> ToCodePoints2(string s) // https://stackoverflow.com/a/38460959
		{
			var utf32 = new UTF32Encoding(!BitConverter.IsLittleEndian, false, true);
			var bytes = utf32.GetBytes(s);
			return Enumerable.Range(0, bytes.Length / 4).Select(i => BitConverter.ToInt32(bytes, i * 4));
		}

		/// <summary>
		/// Returns text elements.
		/// </summary>
		/// <param name="s"></param>
		/// <returns></returns>
		public static int[] ToTextElements(string s) // https://stackoverflow.com/a/687451
		{
			if (!s.IsNormalized())
			{
				s = s.Normalize();
			}

			List<int> chars = new List<int>((s.Length * 3) / 2);

			var ee = System.Globalization.StringInfo.GetTextElementEnumerator(s);

			while (ee.MoveNext())
			{
				string e = ee.GetTextElement();
				chars.Add(char.ConvertToUtf32(e, 0));
			}

			return chars.ToArray();
		}
	}
}