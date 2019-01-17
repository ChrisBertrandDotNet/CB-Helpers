
// Copyright Christophe Bertrand
// https://github.com/ChrisBertrandDotNet/CB-Helpers
// https://chrisbertrand.net

using System;
using System.Runtime.InteropServices;

namespace CB.Graphics
{

	/// <summary>
	/// ABGR pixel in a Int32.
	/// <para>Implements comparison operators and functions.</para>
	/// </summary>
	[StructLayout(LayoutKind.Explicit)]
	public struct PixelBGR32 : IEquatable<PixelBGR32>
	{
		/// <summary>
		/// Blue component.
		/// </summary>
		[FieldOffset(0)]
		public byte Blue;
		/// <summary>
		/// Green component.
		/// </summary>
		[FieldOffset(1)]
		public byte Green;
		/// <summary>
		/// Red component.
		/// </summary>
		[FieldOffset(2)]
		public byte Red;
		/// <summary>
		/// Opcaity component.
		/// 0 = transparent. 255 = opaque.
		/// </summary>
		[FieldOffset(3)]
		public byte Opacity;
		/// <summary>
		/// Bits 0 to 7 are Blue.
		/// Bits 8 to 15 are Green.
		/// Bits 16 to 23 are Red.
		/// Bits 24 to 31 are the opacity.
		/// </summary>
		[FieldOffset(0)]
		public int AsInt32;

		/// <summary>
		/// True if a == b.
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		public static bool operator ==(PixelBGR32 a, PixelBGR32 b)
		{
			return a.AsInt32 == b.AsInt32;
		}
		/// <summary>
		/// True if a != b.
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		public static bool operator !=(PixelBGR32 a, PixelBGR32 b)
		{
			return a.AsInt32 != b.AsInt32;
		}
		/// <summary>
		/// Implicitly transtyped to an Int32.
		/// </summary>
		/// <param name="p"></param>
		public static implicit operator int(PixelBGR32 p)
		{
			return p.AsInt32;
		}
		/// <summary>
		/// Implicitly transtyped from an Int32.
		/// </summary>
		/// <param name="i"></param>
		public static implicit operator PixelBGR32(int i)
		{
			return new PixelBGR32() { AsInt32 = i };
		}
		/// <summary>
		/// True if this equals the <paramref name="obj"/>.
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public override bool Equals(object obj)
		{
			return this == (PixelBGR32)obj;
		}
		/// <summary>
		/// Returns the hash code.
		/// </summary>
		/// <returns></returns>
		public override int GetHashCode()
		{
			return this.AsInt32;
		}

		/// <summary>
		/// A simple luminosity calculus.
		/// </summary>
		public int Luminosity { get { return (int)((double)Red * 0.3 + (double)Green * 0.59 + (double)Blue * 0.11); } }

		/// <summary>
		/// = (255 - Opacity)
		/// </summary>
		public byte Transparency { get { return (byte)(255 - this.Opacity); } }

		/// <summary>
		/// 4 bytes per pixel.
		/// </summary>
		public static int Size { get { return 4; } }
		/// <summary>
		/// Transtypes from an Int32.
		/// </summary>
		/// <param name="i"></param>
		/// <returns></returns>
		public static PixelBGR32 FromInt32(int i)
		{
			return new PixelBGR32() { AsInt32 = i };
		}
		/// <summary>
		/// Builds from given the color components.
		/// </summary>
		/// <param name="r"></param>
		/// <param name="g"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		public static PixelBGR32 FromRGB(byte r, byte g, byte b)
		{
			return new PixelBGR32() { Red = r, Green = g, Blue = b };
		}
		/// <summary>
		/// Builds from the given color components and opacity.
		/// </summary>
		/// <param name="o"></param>
		/// <param name="r"></param>
		/// <param name="g"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		public static PixelBGR32 FromARGB(byte o, byte r, byte g, byte b)
		{
			return new PixelBGR32() { Opacity = o, Red = r, Green = g, Blue = b };
		}
		/// <summary>
		/// Builds from given the color components, given as Int32s.
		/// </summary>
		/// <param name="r"></param>
		/// <param name="g"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		public static PixelBGR32 FromRGB(int r, int g, int b)
		{
			return new PixelBGR32() { Red = (byte)r, Green = (byte)g, Blue = (byte)b };
		}
		/// <summary>
		/// A representation as a String.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return string.Format("Red {0}; Green {1}; Blue {2} ; Opacity {3}", this.Red, this.Green, this.Blue, this.Opacity);
		}
		/// <summary>
		/// True this equals the <paramref name="other"/>.
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public bool Equals(PixelBGR32 other)
		{
			return this == other;
		}
	}

}