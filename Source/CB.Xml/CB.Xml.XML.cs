
// Copyright Christophe Bertrand
// https://github.com/ChrisBertrandDotNet/CB-Helpers
// https://chrisbertrand.net

/* Requires:
 * CB.Execution\CB.Execution.Return.cs
. CB.Reflection\CB.Reflection.TypeEx.cs
 */

/*
Helpers for XML files.
 */

using CB.Execution;
using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
#if NETFX_CORE
/*using Windows.Storage;
using Windows.Storage.Streams;
using Windows.Foundation;
using Windows.ApplicationModel.Resources.Core;*/
#endif

namespace CB.Xml
{
	/// <summary>
	/// Helpers for XML files.
	/// </summary>
	public static class XMLExtensions
	{
		/// <summary>
		/// Fixes the line-feeds in the tags text.
		/// <para>Nethertheless, the line feeds are not removed, just modified.</para>
		/// <para>It's useful with software like XML Notepad that transforms line feeds &amp;#10; in \r\n in the text nodes.</para>
		/// </summary>
		/// <param name="xml"></param>
		/// <param name="sourceForm"></param>
		/// <param name="targetForm"></param>
		/// <returns></returns>
		public static string FixLineFeedsInTextNodes(string xml, string sourceForm = "\r\n", string targetForm = "&#10;")
		{
			const string closingXml = "?>";

			var ret = new StringBuilder((int)(xml.Length * 1.1));
			int iCurrent = 0;
			while (true) // Passes xml anounces like <?xml version="1.0" encoding="UTF-8"?> <?xml-stylesheet href="transformation.xsl" type="text/xsl"?> <?screen mode?> <?instruction for processing?>
			{
				var iXml = xml.IndexOf(closingXml, iCurrent);
				if (iXml < 0)
					break;
				iXml += closingXml.Length;
				ret.Append(xml.Substring(iCurrent, iXml - iCurrent));
				iCurrent = iXml;
			}
			while (iCurrent < xml.Length)
			{
				var iClosingTag = xml.IndexOf('>', iCurrent);
				if (iClosingTag < 0)
					break;
				iClosingTag++;
				ret.Append(xml.Substring(iCurrent, iClosingTag - iCurrent)); // text in tag.
				iCurrent = iClosingTag;

				var iOpeningTag = xml.IndexOf('<', iCurrent);
				if (iOpeningTag < 0)
					break;

				var textContent = xml.Substring(iCurrent, iOpeningTag - iCurrent);
				var trim = textContent.Trim();
				if (trim.Length != 0)
					textContent = textContent.Replace(sourceForm, targetForm);

				ret.Append(textContent);
				iCurrent = iOpeningTag;
			}
			ret.Append(xml.Substring(iCurrent)); // Remaining.
			return ret.ToString();
		}

		/// <summary>
		/// CB: Returns the entire value, including xml sub-elements as a text.
		/// </summary>
		/// <param name="element"></param>
		/// <returns></returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Ne pas supprimer d'objets plusieurs fois")]
		public static string GetCompleteValue(this XElement element)
		{
			if (!element.HasElements)
				return element.Value;

			#region Equivalent to XNode.ToString(SaveOptions)
			string t;
			using (StringWriter sw = new StringWriter(CultureInfo.InvariantCulture))
			{
				XmlWriterSettings ws = new XmlWriterSettings();
				ws.OmitXmlDeclaration = true;
				ws.NamespaceHandling |= NamespaceHandling.OmitDuplicates;
				using (XmlWriter w = XmlWriter.Create(sw, ws))
				{
					element.WriteTo(w);
				}
				t = sw.ToString();
			}
			#endregion Equivalent to XNode.ToString(SaveOptions)

			// removes the main tag, the one of the element:
			t = t.Substring(t.IndexOf('>') + 1);
			t = t.Substring(0, t.LastIndexOf('<'));
			return t;
		}

		/// <summary>
		/// CB: Saves the xml text code to a file.
		/// </summary>
		/// <param name="xElement"></param>
		/// <param name="fileName"></param>
		public static ReturnError SaveToFile(this XElement xElement, string fileName)
		{
			try
			{
#if !NETFX_CORE // WPF
				xElement.Save(fileName);
#else
				Windows.Storage.StorageFolder répertoire = Windows.Storage.ApplicationData.Current.LocalFolder;
				Windows.Storage.StorageFile fichier = répertoire.CreateFileAsync(fileName, Windows.Storage.CreationCollisionOption.ReplaceExisting).AsTask().Result;
				using (var stream = fichier.OpenAsync(Windows.Storage.FileAccessMode.ReadWrite).AsTask().Result)
				{
					xElement.Save(stream.AsStreamForWrite());
				}
#endif
			}
			catch (Exception e)
			{
				return new ReturnError(e, false);
			}
			return ReturnError.Success;
		}
#if false//NETFX_CORE
		static async void _SaveToFile(XElement xElement, string fileName)
		{
			Windows.Storage.StorageFolder répertoire = Windows.Storage.ApplicationData.Current.LocalFolder;
			Windows.Storage.StorageFile fichier = await répertoire.CreateFileAsync(fileName, Windows.Storage.CreationCollisionOption.ReplaceExisting);
			using (var stream = await fichier.OpenAsync(Windows.Storage.FileAccessMode.ReadWrite))
			{
				xElement.Save(stream.AsStreamForWrite());
			}
		}
#endif
	}

}