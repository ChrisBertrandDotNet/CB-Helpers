
// Copyright (c) Christophe Bertrand
// https://github.com/ChrisBertrandDotNet/CB-Helpers
// https://chrisbertrand.net

/*
ClearBase32 is similar to the classic Base64 encoding, except:
- It is limited to 32 characters only.
- The character set does not include characters that could be confounded. Such as 'I' and 'l', 'I' and '1', 'l' and '1', 'O' and '0'.
- There is not markup for beginning, ending or hashing. All the text is the code.

Such code is intended to be typed manually by a user. That is why it has to be clear and simple.
*/

using System;
using System.Text;

namespace CB.Data
{
	/// <summary>
	/// Transcodes to a clearer version of the Base32 encoding.
	/// <para>The character set does not contain characters that could be confounded. Such as 'I' and '1', 'l' and '1', 'O' and '0'.</para>
	/// </summary>
	public static class ClearBase32
	{
		/// <summary>
		/// The 32 characters set.
		/// </summary>
		public const string CharacterSet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

		/// <summary>
		/// The character order set.
		/// <para>For example, CharacterOrder['A'] = 0, CharacterOrder['B'] = 1, CharacterOrder['b'] = 1</para>
		/// <para>Lower-case characters are included in the set.</para>
		/// <para>Invalid characters return -1.</para>
		/// </summary>
		public static readonly int[] CharacterOrder = CalculeNombre32DuCaractère();

		static int[] CalculeNombre32DuCaractère()
		{
			var sortie = new int[255];
			for (int i = 0; i < 255; i++)
				sortie[i] = -1;

			for (int i = 0; i < 32; i++)
			{
				var c = CharacterSet[i];
				var o = (byte)c;
				sortie[o] = i;
				var c2 = char.ToLower(c);
				if (c2 != c)
				{
					o = (byte)c2;
					sortie[o] = i;
				}
			}
			return sortie;
		}


		#region byte[] ↔ base32 (string)

		/// <summary>
		/// Encodes a byte array to a base32 string.
		/// </summary>
		/// <param name="source"></param>
		/// <returns></returns>
		/// <exception cref="System.ArgumentNullException"></exception>
		public static string Encode(byte[] source)
		{
			if (source == null)
				throw new ArgumentNullException("source");

			var tailleSortie = (source.Length * 8 + 4) / 5;
			var sb = new StringBuilder(tailleSortie);
			byte temp = 0;
			int iBit = 0;
			for (int iSource = 0; iSource < source.Length; iSource++)
			{
				var octet = source[iSource];
				for (int i = 0; i < 8; i++)
				{
					var bit = octet & 1;
					octet >>= 1;
					temp >>= 1;
					if (bit != 0)
						temp |= 16;
					iBit++;
					if (iBit >= 5)
					{
						iBit = 0;
						sb.Append(CharacterSet[temp]);
						temp = 0;
					}
				}
			}
			if (iBit != 0)
			{
				temp >>= (5 - iBit);
				sb.Append(CharacterSet[temp]);
			}
#if DEBUG
			if (sb.Length != tailleSortie)
				throw new Exception();
#endif
			return sb.ToString();
		}

		/// <summary>
		/// Decodes a base32 string to a byte array, retreiving the original data.
		/// <para>If the source has an invalid format, returns null.</para>
		/// <para>The source can contain lower and upper-case letters.</para>
		/// </summary>
		/// <param name="source"></param>
		/// <returns>The decoded byte array, or null if the source format is invalid.</returns>
		public static byte[] Decode(string source)
		{
			if (source == null)
				throw new ArgumentNullException("source");

			var tailleSortie = (source.Length * 5) / 8;
			var sortie = new byte[tailleSortie];
			var iSortie = 0;
			byte temp = 0;
			int iBit = 0;
			foreach (var c in source)
			{
				var octet32 = CharacterOrder[(byte)c];
				if (octet32 == -1)
					return null;
				for (int i = 0; i < 5; i++)
				{
					var bit = octet32 & 1;
					octet32 >>= 1;
					temp >>= 1;
					if (bit != 0)
						temp |= 128;
					iBit++;
					if (iBit >= 8)
					{
						iBit = 0;
						sortie[iSortie++] = (byte)temp;
						temp = 0;
					}
				}
			}
#if DEBUG
			if (iBit != 0 && temp != 0)
				throw new Exception();
			if (iSortie != tailleSortie)
				throw new Exception();
#endif
			return sortie;
		}

		#endregion byte[] ↔ base32 (string)

		#region string ↔ base32 (string)

		/// <summary>
		/// Encodes the source as UTF-8 then encodes it as a ClearBase32.
		/// </summary>
		/// <param name="source"></param>
		/// <returns></returns>
		/// <exception cref="System.ArgumentNullException"></exception>
		/// <exception cref="System.Text.EncoderFallbackException">An error occured during the UTF-8 encoding.</exception>
		public static string EncodeText(string source)
		{
			return Encode(UTF8Encoding.UTF8.GetBytes(source));
		}

		/// <summary>
		/// Decodes the source, then encodes the result as a UTF-8 String.
		/// </summary>
		/// <param name="source"></param>
		/// <returns></returns>
		/// <exception cref="System.ArgumentException">The source format is invalid, or the decoded data is not a valid UTF-8 string.</exception>
		/// <exception cref="System.ArgumentNullException"></exception>
		/// <exception cref="System.Text.DecoderFallbackException">The decoded data is not a valid UTF-8 string.</exception>
		public static string DecodeAsText(string source)
		{
			byte[] décodé = Decode(source);
			if (source == null)
				throw new Exception("Invalid source format. This is not a CleaBase32 encoded string.");
			return UTF8Encoding.UTF8.GetString(décodé);
		}

		#endregion string ↔ base32 (string)

		#region byte[] ↔ base32 (byte[])

		/// <summary>
		/// Encodes a byte array to a base32 string that is in turn coded as its UTF8 binary representation.
		/// </summary>
		/// <param name="source"></param>
		/// <returns></returns>
		public static byte[] EncodeToAByteArray(byte[] source)
		{
			var base32 = ClearBase32.Encode(source);
			return UTF8Encoding.UTF8.GetBytes(base32);
		}

			/// <summary>
			/// Decodes a base32 string that was coded as its UTF8 binary representation.
			/// </summary>
			/// <param name="source">An UTF8 binary representation of the base32 string.</param>
			/// <returns>The decoded byte array.</returns>
			public static byte[] DecodeFromByteArray(byte[] source)
		{
			var codé = UTF8Encoding.UTF8.GetString(source);
			var c0 = (byte)codé[0];
			if (c0 == 255)
				throw new Exception("Please suppress the BOM from begginning of the string.");
			return Decode(codé);
		}

		#endregion byte[] ↔ base32 (byte[])

		#region string ↔ base32 (byte[])

		/// <summary>
		/// Encodes a regular string to a base32 string that is in turn coded as its UTF8 binary representation.
		/// </summary>
		/// <param name="source"></param>
		/// <returns></returns>
		public static byte[] EncodeTextToAByteArray(string source)
		{
			var base32=ClearBase32.EncodeText(source);
			return UTF8Encoding.UTF8.GetBytes(base32);
		}

		/// <summary>
		/// Decodes a base32 string that was coded as its UTF8 binary representation.
		/// </summary>
		/// <param name="source"></param>
		/// <returns>The decoded regular string.</returns>
		public static string DecodeAsStringFromByteArray(byte[] source)
		{
			var décodé = DecodeFromByteArray(source);
			return UTF8Encoding.UTF8.GetString(décodé);
		}

		#endregion byte[] ↔ base32 (string)

		internal static void Teste()
		{
			// TODO: use all functions.

			for (int i = 0; i < 100000; i++)
			{
				var t = i.ToString();
				var codé = EncodeText(t);
				var décodé = DecodeAsText(codé);
				if (décodé != t)
					throw new Exception();
			}
		}

	}
}