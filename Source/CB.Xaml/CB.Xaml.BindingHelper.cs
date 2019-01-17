
// Copyright Christophe Bertrand
// https://github.com/ChrisBertrandDotNet/CB-Helpers
// https://chrisbertrand.net

/*
Lets use an XML file that is common to both WPF and Windows Store.
Sometimes we have to use custom controls, therfore a special name space in the XAML.
XAML: xmlns:cbxaml="clr-namespace:CB.Xaml"
*/

/* rappel des symboles de projet:
 * Console : (rien)
 * WPF : (rien, mais devrait toujours définir WPF)
 * Android: (rien, mais devrait toujours définir ANDROID)
 * Windows 8.1 pour Windows Store : NETFX_CORE;WINDOWS_APP
 * Windows 8 universel pour Windows (Store) : NETFX_CORE;WINDOWS_APP
 * Windows 8 universel pour Windows Phone (Store) : NETFX_CORE;WINDOWS_PHONE_APP
 * Windows 10 UWP (Store) : NETFX_CORE;WINDOWS_UWP
*/
#if WINDOWS_APP || WINDOWS_PHONE_APP
#define WINDOWS_8_STORE // Windows 8, mais non Windows 10.
#endif
#if false
#if !WPF && !ANDROID && !NETFX_CORE && !WINDOWS_APP && !WINDOWS_PHONE_APP && !WINDOWS_UWP
#error You have to declare the GUI's type (such as 'WPF'), as a conditional compilation symbol in the project settings.
#endif
#endif

using System.Windows;
#if NETFX_CORE
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
#else // WPF
using System.Windows.Data;
#endif

// The following line lets you display an XAML without prefix (but compilation does not accecpt that):
//[assembly: System.Windows.Markup.XmlnsDefinition("http://schemas.microsoft.com/winfx/2006/xaml/presentation", "CB.CommonXaml")]

namespace CB.Xaml
{
	/// <summary>
	/// Helpers for Bindings.
	/// </summary>
	public static class BindingHelper
	{
		/// <summary>
		/// CB: Evaluates the binding value.
		/// </summary>
		/// <param name="b"></param>
		/// <param name="ret"></param>
		/// <returns></returns>
		public static bool GetValue(this BindingBase b, out object ret)
		{
			return BindingEvaluator.GetBindingValue(b, out ret);
		}

		/// <summary>
		/// CB: Evaluates the binding value.
		/// </summary>
		/// <param name="b"></param>
		/// <param name="ret"></param>
		/// <returns></returns>
		public static bool GetValue<T>(this BindingBase b, out T ret)
		{
			return BindingEvaluator<T>.GetBindingValue(b, out ret);
		}

#if NETFX_CORE
		// Anyway, UWP does not allow checking the validity of the binding.
		/// <summary>
		/// CB: Evaluates the binding value.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="b"></param>
		/// <returns></returns>
		public static T GetValue<T>(this BindingBase b)
		{
			T val;
			BindingEvaluator<T>.GetBindingValue(b, out val);
			return val;
		}

		// Anyway, UWP does not allow checking the validity of the binding.
		/// <summary>
		/// CB: Evaluates the binding value.
		/// </summary>
		/// <param name="b"></param>
		/// <returns></returns>
		public static object GetValue(this BindingBase b)
		{
			object val;
			BindingEvaluator.GetBindingValue(b, out val);
			return val;
		}
#endif

		class BindingEvaluator : DependencyObject
		{
			public object Value
			{
				get { return (object)GetValue(ValueProperty); }
				set { SetValue(ValueProperty, value); }
			}

			public static readonly DependencyProperty ValueProperty =
				DependencyProperty.Register("Value", typeof(object), typeof(BindingEvaluator),
#if NETFX_CORE
					new PropertyMetadata(null));
#else // WPF
					new UIPropertyMetadata(null));
#endif

			internal static bool GetBindingValue(BindingBase b, out object ret)
			{
				BindingEvaluator d = new BindingEvaluator();
#if NETFX_CORE
				BindingOperations.SetBinding(d, BindingEvaluator.ValueProperty, b);
				ret = d.Value;
				return true; // en fait j'ignore si ça a marché.
#else // WPF
				var beb = BindingOperations.SetBinding(d, BindingEvaluator.ValueProperty, b);
				var ok = beb.Status == BindingStatus.Active;
				ret = d.Value;
				BindingOperations.ClearBinding(d, BindingEvaluator.ValueProperty);
				return ok;
#endif
			}
		}

		class BindingEvaluator<T> : DependencyObject
		{
			public T Value
			{
				get { return (T)GetValue(ValueProperty); }
				set { SetValue(ValueProperty, value); }
			}

			public static readonly DependencyProperty ValueProperty =
				DependencyProperty.Register("Value", typeof(T), typeof(BindingEvaluator<T>),
#if NETFX_CORE
					new PropertyMetadata(null));
#else // WPF
					new UIPropertyMetadata(null));
#endif

			internal static bool GetBindingValue(BindingBase b, out T ret)
			{
				BindingEvaluator<T> d = new BindingEvaluator<T>();
#if NETFX_CORE
				BindingOperations.SetBinding(d, BindingEvaluator.ValueProperty, b);
				var retObject = d.Value;
				ret = retObject is T ? (T)retObject : default(T);
				return true; // In fact, I don't know if it worked.
#else // WPF
				var beb = BindingOperations.SetBinding(d, BindingEvaluator<T>.ValueProperty, b);
				var ok = beb.Status == BindingStatus.Active;
				var retObject = d.Value;
				BindingOperations.ClearBinding(d, BindingEvaluator<T>.ValueProperty);
				ret = retObject is T ? (T)retObject : default(T);
				return ok;
#endif
			}
		}
	}
}