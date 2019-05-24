
// Copyright Christophe Bertrand
// https://github.com/ChrisBertrandDotNet/CB-Helpers
// https://chrisbertrand.net

using System.Windows;
using System.Windows.Media;

namespace CB.WPF
{
	/// <summary>
	/// Helpers for images.
	/// </summary>
	public static class Screen
	{
		/// <summary>
		/// CB: Converts a screen size to a Xaml size, depending on the DPI settings of the screen.
		/// </summary>
		/// <param name="visual">A Window, for example.</param>
		/// <param name="width"></param>
		/// <param name="height"></param>
		public static Size SizeFromScreenToVisual(this Visual visual, double width, double height)
		{
			if (visual != null)
			{
				var source = PresentationSource.FromVisual(visual);
				if (source != null)
				{
					var ct = source.CompositionTarget;
					var scaleX = ct.TransformToDevice.M11;
					var scaleY = ct.TransformToDevice.M22;
					return new Size(width / scaleX, height / scaleY);
				}
			}
			return default(Size);
		}

		/// <summary>
		/// CB: Converts a screen size to a Xaml size, depending on the DPI settings of the screen.
		/// </summary>
		/// <param name="visual">A Window, for example.</param>
		/// <param name="size"></param>
		/// <returns></returns>
		public static Size SizeFromScreenToVisual(this Visual visual, Size size)
		{
			return SizeFromScreenToVisual(visual, size.Width, size.Height);
		}

		/// <summary>
		/// CB: Converts a Xaml size to a screen size, depending on the DPI settings of the screen.
		/// </summary>
		/// <param name="visual">A Window, for example.</param>
		/// <param name="width"></param>
		/// <param name="height"></param>
		public static Size SizeFromVisualToScreen(this Visual visual, double width, double height)
		{
			if (visual != null)
			{
				var source = PresentationSource.FromVisual(visual);
				if (source != null)
				{
					var ct = source.CompositionTarget;
					var scaleX = ct.TransformToDevice.M11;
					var scaleY = ct.TransformToDevice.M22;
					return new Size(width * scaleX, height * scaleY);
				}
			}
			return default(Size);
		}

		/// <summary>
		/// CB: Converts a Xaml size to a screen size, depending on the DPI settings of the screen.
		/// </summary>
		/// <param name="visual">A Window, for example.</param>
		/// <param name="size"></param>
		/// <returns></returns>
		public static Size SizeFromVisualToScreen(this Visual visual, Size size)
		{
			return SizeFromVisualToScreen(visual, size.Width, size.Height);
		}

	}
}