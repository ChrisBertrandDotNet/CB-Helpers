
// Copyright Christophe Bertrand
// https://github.com/ChrisBertrandDotNet/CB-Helpers
// https://chrisbertrand.net

using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CB.Graphics
{
	/// <summary>
	/// Helpers for graphic classes.
	/// </summary>
	public static class GraphicsExtensions
	{
		/// <summary>
		/// Returns an image with the right pixel format.
		/// </summary>
		/// <param name="image"></param>
		/// <param name="format"></param>
		/// <returns></returns>
		public static BitmapSource SetPixelFormat(this BitmapSource image, PixelFormat format)
		{
			if (image == null)
				return null;
			if (image.Format == format)
				return image;

			FormatConvertedBitmap newFormatedBitmapSource = new FormatConvertedBitmap();

			// BitmapSource objects like FormatConvertedBitmap can only have their properties
			// changed within a BeginInit/EndInit block.
			newFormatedBitmapSource.BeginInit();

			// Use the BitmapSource object defined above as the source for this new 
			// BitmapSource (chain the BitmapSource objects together).
			newFormatedBitmapSource.Source = image;

			// Set the new format.
			newFormatedBitmapSource.DestinationFormat = format;
			newFormatedBitmapSource.EndInit();
			return newFormatedBitmapSource;
		}
	}
}