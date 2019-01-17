
// Copyright Christophe Bertrand
// https://github.com/ChrisBertrandDotNet/CB-Helpers
// https://chrisbertrand.net

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CB.WPF
{
	/// <summary>
	/// Helpers for images.
	/// </summary>
	public static class Images
	{
		/// <summary>
		/// Adapts the displayed image size to the DPI setting of the screen in Windows.
		/// </summary>
		/// <param name="window"></param>
		/// <param name="image"></param>
		/// <param name="width"></param>
		/// <param name="height"></param>
		public static void SetImageDisplaySize(Visual window, Image image, double width, double height)
		{
			var source = PresentationSource.FromVisual(window); // Here, 'this' is the window.
			var scaleX = source.CompositionTarget.TransformToDevice.M11;
			var scaleY = source.CompositionTarget.TransformToDevice.M22;

			image.Width = width / scaleX;
			image.Height = height / scaleY;
		}
	}

}