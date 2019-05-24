
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
#if !WPF && !ANDROID && !NETFX_CORE && !WINDOWS_APP && !WINDOWS_PHONE_APP && !WINDOWS_UWP
#error You have to define a conditional compilation symbol in your project settings (section Build), probably WPF.
#endif

using System;
using System.ComponentModel;
using System.Linq;
#if WPF
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Reflection;
using System.Diagnostics;
using System.Windows.Threading;
#endif
#if NETFX_CORE
using Windows.Foundation.Metadata;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;
#endif

// The following line lets you display an XAML without prefix (but compilation does not accecpt that):
//[assembly: System.Windows.Markup.XmlnsDefinition("http://schemas.microsoft.com/winfx/2006/xaml/presentation", "CB.CommonXaml")]

#pragma warning disable IDE0019 // Utiliser les critères spéciaux

namespace CB.Xaml
{

	#region RichText
	/* Exemple:
	<cbxaml:RichText MinWidth="400">
			<Paragraph FontSize="20">
				<Italic>Blue</Italic> as sky
			</Paragraph>
	</cbxaml:RichText>
	 */
#if WPF
	/// <summary>
	/// Equivalent to RichTextBlock in Store XAML, but done by RichTextBox in WPF.
	/// </summary>
	[ContentProperty("Blocks")]
	public class RichText : RichTextBox
	{
		readonly FlowDocument doc = new FlowDocument();

		/// <summary>
		/// Gets the top-level System.Windows.Documents.Block elements of the contents of the System.Windows.Documents.FlowDocument.
		/// </summary>
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public BlockCollection Blocks
		{
			get { return this.doc.Blocks; }
		}

		/// <summary>
		/// Initializes with a new <see cref="System.Windows.Documents.FlowDocument"/>.
		/// </summary>
		public RichText()
		{
			base.Document = doc;
		}
	}
#endif
#if NETFX_CORE
	/// <summary>
	/// Equivalent to RichTextBlock.
	/// </summary>
	[ContentProperty(Name = "Blocks")]
	public class RichText : ContentControl
	{
#if !DEBUG
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
#endif
		readonly RichTextBlock rtb=new RichTextBlock(); // On ne peut pas hériter de RichTextBlock, alors on l'inclut.

		public double BaselineOffset { get { return this.rtb.BaselineOffset; } }
		public BlockCollection Blocks { get { return this.rtb.Blocks; } }
		public new int CharacterSpacing { get { return this.rtb.CharacterSpacing; } set { this.rtb.CharacterSpacing=value; } }
		public TextPointer ContentEnd { get { return rtb.ContentEnd; } }
		public TextPointer ContentStart { get { return rtb.ContentStart; } }
		public new FontFamily FontFamily { get { return rtb.FontFamily; } set { this.rtb.FontFamily = value; } }
		public new double FontSize { get { return rtb.FontSize; } set { this.rtb.FontSize = value; } }
		public new FontStretch FontStretch { get { return rtb.FontStretch; } set { this.rtb.FontStretch = value; } }
		public new FontStyle FontStyle { get { return rtb.FontStyle; } set { this.rtb.FontStyle = value; } }
		public new FontWeight FontWeight { get { return rtb.FontWeight; } set { this.rtb.FontWeight = value; } }
		public new Brush Foreground { get { return rtb.Foreground; } set { this.rtb.Foreground = value; } }
		public bool HasOverflowContent { get { return rtb.HasOverflowContent; } }
		public bool IsColorFontEnabled { get { return rtb.IsColorFontEnabled; } set { this.rtb.IsColorFontEnabled=value; } }
		public bool IsTextSelectionEnabled { get { return rtb.IsTextSelectionEnabled; } set { this.rtb.IsTextSelectionEnabled=value; } }
		public double LineHeight { get { return rtb.LineHeight; } set { this.rtb.LineHeight=value; } }
		public LineStackingStrategy LineStackingStrategy { get { return rtb.LineStackingStrategy; } set { this.rtb.LineStackingStrategy=value; } }
		public int MaxLines { get { return rtb.MaxLines; } set { this.rtb.MaxLines=value; } }
		public OpticalMarginAlignment OpticalMarginAlignment { get { return rtb.OpticalMarginAlignment; } set { this.rtb.OpticalMarginAlignment=value; } }
		public RichTextBlockOverflow OverflowContentTarget { get { return rtb.OverflowContentTarget; } set { this.rtb.OverflowContentTarget=value; } }
		public new Thickness Padding { get { return rtb.Padding; } set { this.rtb.Padding = value; } }
		public string SelectedText { get { return rtb.SelectedText; } }
		public TextPointer SelectionEnd { get { return rtb.SelectionEnd; } }
		public SolidColorBrush SelectionHighlightColor { get { return rtb.SelectionHighlightColor; } set { this.rtb.SelectionHighlightColor=value; } }
		public TextPointer SelectionStart { get { return rtb.SelectionStart; } }
		public TextAlignment TextAlignment { get { return rtb.TextAlignment; } set { this.rtb.TextAlignment=value; } }
		public double TextIndent { get { return rtb.TextIndent; } set { this.rtb.TextIndent=value; } }
		public TextLineBounds TextLineBounds { get { return rtb.TextLineBounds; } set { this.rtb.TextLineBounds=value; } }
		public TextReadingOrder TextReadingOrder { get { return rtb.TextReadingOrder; } set { this.rtb.TextReadingOrder=value; } }
		public TextTrimming TextTrimming { get { return rtb.TextTrimming; } set { this.rtb.TextTrimming=value; } }
		public TextWrapping TextWrapping { get { return rtb.TextWrapping; } set { this.rtb.TextWrapping=value; } }

		public new static DependencyProperty CharacterSpacingProperty { get { return RichTextBlock.CharacterSpacingProperty; } }
		public new static DependencyProperty FontFamilyProperty { get { return RichTextBlock.FontFamilyProperty; } }
		public new static DependencyProperty FontSizeProperty { get { return RichTextBlock.FontSizeProperty; } }
		public new static DependencyProperty FontStretchProperty { get { return RichTextBlock.FontStretchProperty; } }
		public new static DependencyProperty FontStyleProperty { get { return RichTextBlock.FontStyleProperty; } }
		public new static DependencyProperty FontWeightProperty { get { return RichTextBlock.FontWeightProperty; } }
		public new static DependencyProperty ForegroundProperty { get { return RichTextBlock.ForegroundProperty; } }
		public static DependencyProperty HasOverflowContentProperty { get { return RichTextBlock.HasOverflowContentProperty; } }
		public static DependencyProperty IsColorFontEnabledProperty { get { return RichTextBlock.IsColorFontEnabledProperty; } }
		public static DependencyProperty IsTextSelectionEnabledProperty { get { return RichTextBlock.IsTextSelectionEnabledProperty; } }
		public static DependencyProperty LineHeightProperty { get { return RichTextBlock.LineHeightProperty; } }
		public static DependencyProperty LineStackingStrategyProperty { get { return RichTextBlock.LineStackingStrategyProperty; } }
		public static DependencyProperty MaxLinesProperty { get { return RichTextBlock.MaxLinesProperty; } }
		public static DependencyProperty OpticalMarginAlignmentProperty { get { return RichTextBlock.OpticalMarginAlignmentProperty; } }
		public static DependencyProperty OverflowContentTargetProperty { get { return RichTextBlock.OverflowContentTargetProperty; } }
		public new static DependencyProperty PaddingProperty { get { return RichTextBlock.PaddingProperty; } }
		public static DependencyProperty SelectedTextProperty { get { return RichTextBlock.SelectedTextProperty; } }
		public static DependencyProperty SelectionHighlightColorProperty { get { return RichTextBlock.SelectionHighlightColorProperty; } }
		public static DependencyProperty TextAlignmentProperty { get { return RichTextBlock.TextAlignmentProperty; } }
		public static DependencyProperty TextIndentProperty { get { return RichTextBlock.TextIndentProperty; } }
		public static DependencyProperty TextLineBoundsProperty { get { return RichTextBlock.TextLineBoundsProperty; } }
		public static DependencyProperty TextReadingOrderProperty { get { return RichTextBlock.TextReadingOrderProperty; } }
		public static DependencyProperty TextTrimmingProperty { get { return RichTextBlock.TextTrimmingProperty; } }
		public static DependencyProperty TextWrappingProperty { get { return RichTextBlock.TextWrappingProperty; } }

		public RichText()
		{
			base.Content = this.rtb;
		}
	}
#endif
	#endregion RichText

	/// <summary>
	/// Helpers for XAML.
	/// </summary>
	public static class XamlHelper
	{
		/// <summary>
		/// CB: Finds the first control of the given type <typeparamref name="T"/> in the path of the parent controls.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="container"></param>
		/// <param name="includesThisControl"></param>
		/// <returns></returns>
		public static T FindFirstParentControlByType<T>(this FrameworkElement container, bool includesThisControl = false) where T : FrameworkElement
		{
			FrameworkElement found = null;
			TraverseControlTreeUp(container,
				e =>
				{
					if (e != null && e is T)
					{
						found = e;
						return true;
					}
					return false;
				},
			includesThisControl
				);
			return found as T;
		}

		/// <summary>
		/// CB: Traverses the control tree below this one (<paramref name="container"/>), searching for the first control that is of the given type (<typeparamref name="T"/>).
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="container"></param>
		/// <param name="includesThisControl">Includes <paramref name="container"/> in the search.</param>
		/// <returns></returns>
		public static T FindFirstSubControlByType<T>(this FrameworkElement container, bool includesThisControl = false) where T : FrameworkElement
		{
			FrameworkElement found = null;
			TraverseControlTreeDown(container,
				e =>
				{
					if (e != null && e is T)
					{
						found = e;
						return true;
					}
					return false;
				},
			includesThisControl
				);
			return found as T;
		}

#if WPF
		/// <summary>
		/// CB: Obtains the sub-objects of this control.
		/// <para>Returns null if there is no sub-controls.</para>
		/// </summary>
		/// <param name="control"></param>
		/// <returns></returns>
		public static object[] GetChildrenObjects(this FrameworkElement control)
		{
			if (control == null)
				return null;
			if (!control.IsLoaded)
				throw new InvalidOperationException("Not loaded");
			{
				var cc = control as ContentControl;
				if (cc != null)
				{
					var contenu = cc.Content;
					if (contenu != null)
						return new object[] { contenu };
				}
			}
			{
				var c = control as Panel;
				if (c != null)
				{
					var sousContrôles = c.Children;
					if (sousContrôles.Count != 0)
					{
						var ret = new object[sousContrôles.Count];
						for (int i = 0; i < sousContrôles.Count; i++)
							ret[i] = sousContrôles[i];
						return ret;
					}
				}
			}
			return null;
		}

		/// <summary>
		/// CB: Obtains the sub-controls of this control.
		/// <para>Returns null if there is no sub-controls.</para>
		/// </summary>
		/// <param name="control"></param>
		/// <returns></returns>
		public static UIElement[] GetChildrenControls(this FrameworkElement control)
		{
			if (control == null)
				return null;
			/*if (!control.IsLoaded)
				throw new InvalidOperationException("Not loaded");*/
			{
				var cc = control as ContentControl;
				if (cc != null)
				{
					var contenu = cc.Content as UIElement;
					if (contenu != null)
						return new UIElement[] { contenu };
				}
			}
			{
				var c = control as Panel;
				if (c != null)
				{
					var sousContrôles = c.Children;
					if (sousContrôles.Count != 0)
					{
						var ret = new UIElement[sousContrôles.Count];
						for (int i = 0; i < sousContrôles.Count; i++)
							ret[i] = sousContrôles[i];
						return ret;
					}
				}
			}
			return null;
		}

		/// <summary>
		/// CB: Obtains the control at the top or the control tree.
		/// </summary>
		/// <param name="control"></param>
		/// <param name="checkIsLoaded">If true, checks that all control are loaded.
		/// <para>When a control is not loaded, its Parent is probably null.</para>
		/// </param>
		/// <returns></returns>
		public static FrameworkElement GetRootControl(this FrameworkElement control, bool checkIsLoaded = true)
		{
			while (control != null)
			{
				var p = control.Parent as FrameworkElement;
				if (checkIsLoaded && p == null && !control.IsLoaded)
					throw new InvalidOperationException("Not loaded");
				if (p == null)
					return control;
				control = p;
			}
			return null;
		}
#endif // WPF

		/// <summary>
		/// CB: Tells if the IDE's Xaml visual designer is running the executing assembly.
		/// </summary>
		/// <returns></returns>
		public static bool IsXamlDesignerRunning
		{
			get
			{
				if (!_IsXamlDesignerRunning.HasValue)
#if WPF
					_IsXamlDesignerRunning = System.Reflection.Assembly.GetExecutingAssembly().Location.Contains("VisualStudio");
#endif
#if SILVERLIGHT
				_IsXamlDesignerRunning = System.ComponentModel.DesignerProperties.IsInDesignTool;
#endif
#if NETFX_CORE
				_IsXamlDesignerRunning = Windows.ApplicationModel.DesignMode.DesignModeEnabled;
#endif
				return _IsXamlDesignerRunning.Value;
			}
		}
		static bool? _IsXamlDesignerRunning;

#if WPF
		#region Loaded gérant les Exceptions
		/// <summary>
		/// An event handler for XAML.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="control"></param>
		/// <param name="e"></param>
		public delegate void XamlRoutedEventHandler<T>(T control, RoutedEventArgs e) where T : FrameworkElement;

		/// <summary>
		/// CB: Gets a Loaded event that manages Exceptions even on 64-bit operating systems.
		/// <para>The problem with Loaded event is it does not let catch an exception on 64-bit systems.</para>
		/// <para>If the control is a Window, please consider using ContentRendered instead.</para>
		/// <para>You can consider overriding EndInit too, as it is called after properties initialization by the xaml designer (XDesProc.exe).</para>
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="control"></param>
		/// <param name="handler"></param>
		public static void Loaded_InterceptAndManageExceptions<T>(this T control, XamlRoutedEventHandler<T> handler)
			where T : FrameworkElement
		{
			control.Loaded += (sender, e) => ControlLoaded<T>((T)sender, e, handler);
		}

		[Obfuscation(Exclude = true)]
		static void ControlLoaded<T>(T control, RoutedEventArgs e, XamlRoutedEventHandler<T> handler)
			where T : FrameworkElement
		{
			try
			{
				handler(control, e);
			}
			catch (Exception ex)
			{
				if (Environment.Is64BitOperatingSystem)   // 64bit machines are unable to properly throw the errors during a Page_Loaded event.
				{
					if (Debugger.IsAttached)
						Debugger.Break();
					var loaderExceptionWorker = new System.ComponentModel.BackgroundWorker();
					loaderExceptionWorker.DoWork += ((exceptionWorkerSender, runWorkerCompletedEventArgs) => { runWorkerCompletedEventArgs.Result = runWorkerCompletedEventArgs.Argument; });
					loaderExceptionWorker.RunWorkerCompleted += ((exceptionWorkerSender, runWorkerCompletedEventArgs) => { throw (Exception)runWorkerCompletedEventArgs.Result; });
					loaderExceptionWorker.RunWorkerAsync(ex);
				}
				else
					throw;
			}
		}

		#endregion Loaded gérant les Exceptions

		/// <summary>
		/// CB: Translates a rect relative to this element to coordinates that are relative to the specified element.
		/// </summary>
		/// <param name="control"></param>
		/// <param name="rect"></param>
		/// <param name="relativeTo"></param>
		/// <returns></returns>
		public static Rect TranslateRect(this UIElement control, Rect rect, UIElement relativeTo)
		{
			var point = control.TranslatePoint(new Point(rect.X, rect.Y), relativeTo);
			return new Rect(point, rect.Size);
		}

		/// <summary>
		/// CB: Traverses the control upward.
		/// </summary>
		/// <param name="control"></param>
		/// <param name="traverser"></param>
		/// <param name="includesThisControl"></param>
		public static void TraverseControlTreeUp(this FrameworkElement control, Func<FrameworkElement, bool> traverser, bool includesThisControl = false)
		{
			if (control == null || traverser == null)
				throw new ArgumentNullException();
			_TraverseControlTreeUp(control, traverser, includesThisControl);
		}
		static void _TraverseControlTreeUp(FrameworkElement control, Func<FrameworkElement, bool> traverser, bool includesThisControl)
		{
			if (includesThisControl)
				if (traverser(control))
					return;

			var parent = control.Parent as FrameworkElement;
			if (parent != null)
				_TraverseControlTreeUp(parent, traverser, true);
		}

		/// <summary>
		/// CB: Traverses the entire control tree below this control.
		/// </summary>
		/// <param name="control"></param>
		/// <param name="traverser">Returns true to end the travel.</param>
		/// <param name="includesThisControl"></param>
		public static void TraverseControlTreeDown(this FrameworkElement control, Func<FrameworkElement, bool> traverser, bool includesThisControl = false)
		{
			if (control == null || traverser == null)
				throw new ArgumentNullException();
			_TraverseControlTreeDown(control, traverser, includesThisControl);
		}
		static void _TraverseControlTreeDown(FrameworkElement control, Func<FrameworkElement, bool> traverser, bool includesThisControl)
		{
			if (includesThisControl)
				if (traverser(control))
					return;
			var sousContrôles = control.GetChildrenControls();
			if (sousContrôles != null)
				for (int i = 0; i < sousContrôles.Length; i++)
				{
					var c = sousContrôles[i] as FrameworkElement;
					if (c != null)
						_TraverseControlTreeDown(c, traverser, true);
				}
		}

		/// <summary>
		/// CB: Gets the text of this control, whatever type is the control.
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public static string GetControlText(this FrameworkElement item)
		{
			{
				var c = item as ContentControl;
				if (c != null && c.Content is FrameworkElement)
					return ((FrameworkElement)c.Content).GetControlText();
			}
			{
				var c = item as TextBox;
				if (c != null)
					return c.Text;
			}
			{
				var c = item as TextBlock;
				if (c != null)
					return c.Text;
			}
			{
				var f = GetPlainText.Value;
				if (f != null)
				{
					if (item.CheckAccess())
					{
						return f.Invoke(item, null) as string;
					}
					else
					{
						return (string)item.Dispatcher.Invoke(DispatcherPriority.Send, new TimeSpan(0, 0, 0, 0, 20), new DispatcherOperationCallback((object o) => f.Invoke(item, null)), null);
					}
				}
			}
			{
				var t = item.ToString();
				var ntype = item.GetType().FullName + ": ";
				if (t.IndexOf(ntype) == 0)
					return t.Substring(ntype.Length);
				return t;
			}
		}
		static Lazy<MethodInfo> GetPlainText = new Lazy<MethodInfo>(() => typeof(FrameworkElement).GetMethod("GetPlainText", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public));

		/// <summary>
		/// CB: Gets the text of this item control/object, whatever type is the control.
		/// </summary>
		/// <param name="itemsControl"></param>
		/// <param name="item"></param>
		/// <returns></returns>
		public static string GetItemText(this ItemsControl itemsControl, object item)
		{
			if (item == null)
				throw new ArgumentNullException("item");
			if (item is FrameworkElement)
			{
				{
					var c = item as ContentControl;
					if (c != null)
						if (itemsControl == null)
							throw new ArgumentNullException("itemsControl");
						else
							return itemsControl.GetItemText(c.Content);
				}
				{
					var c = item as TextBox;
					if (c != null)
						return c.Text;
				}
				{
					var c = item as TextBlock;
					if (c != null)
						return c.Text;
				}
			}
			{
				var f = GetPrimaryTextFromItem.Value;
				if (f != null)
					return f.Invoke(null, new object[] { itemsControl, item }) as string;
			}
			{
				var t = item.ToString();
				var ntype = item.GetType().FullName + ": ";
				if (t.IndexOf(ntype) == 0)
					return t.Substring(ntype.Length);
				return t;
			}
		}
		static Lazy<MethodInfo> GetPrimaryTextFromItem = new Lazy<MethodInfo>(() => typeof(TextSearch).GetMethod("GetPrimaryTextFromItem", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic));

#endif // WPF

	}


}

#pragma warning restore IDE0019 // Utiliser les critères spéciaux
