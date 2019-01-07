
// Copyright Christophe Bertrand
// https://github.com/ChrisBertrandDotNet/CB-Helpers
// https://chrisbertrand.net

// Inspired by http://bytes.com/topic/net/insights/797169-reading-parsing-ini-file-c
// But with many changes.

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

/* Exemple:
    public class TestParser
    {
        public static void Main()
        {
            IniParser parser = new IniParser(@"C:\test.ini");
     
            String newMessage;
     
            newMessage = parser.GetSetting("appsettings", "msgpart1");
            newMessage += parser.GetSetting("appsettings", "msgpart2");
            newMessage += parser.GetSetting("punctuation", "ex");
     
            //Returns "Hello World!"
            Console.WriteLine(newMessage);
            Console.ReadLine();
        }
    }
*/

namespace CB.Files
{

	public class IniParser
	{
		internal readonly Dictionary<SectionPair, string> keyPairs = new Dictionary<SectionPair, string>();
		private readonly String iniFilePath;
		public readonly bool SetUpperCase;

		internal struct SectionPair
		{
			internal String Section;
			internal String Key;
#if DEBUG
			public override string ToString()
			{
				return "[" + this.Section + "] : " + this.Key;
			}
#endif
		}

		private IniParser(bool SetUpperCase = true)
		{
			this.SetUpperCase = SetUpperCase;
		}

		/// <summary>
		/// Opens the INI file at the given path and enumerates the values in the IniParser.
		/// <para>If the file does not exist, just returns a new instance.</para>
		/// </summary>
		/// <param name="IniFilePath">Full path to INI file.</param>
		/// <param name="encoding"></param>
		/// <param name="SetUpperCase"></param>
		public IniParser(String IniFilePath, Encoding encoding = null, bool SetUpperCase = true)
			: this(SetUpperCase)
		{
			this.iniFilePath = IniFilePath;

			if (encoding == null)
				encoding = Encoding.UTF8;

			if (File.Exists(iniFilePath))
			{
				var texteINI = File.ReadAllLines(iniFilePath, encoding);
				this.ParseText(texteINI);
			}
		}

		/// <summary>
		/// Parses the stream and enumerates the values.
		/// </summary>
		public IniParser(Stream stream, Encoding encoding = null, bool SetUpperCase = true, bool DisposeStream = false)
		{
			List<string> lines = new List<string>();
			using (var tr = new StreamReader(stream, encoding))
			{
				string line;
				while ((line = tr.ReadLine()) != null)
					lines.Add(line);
			}
			this.ParseText(lines);
#if false
			if (encoding == null)
				encoding = Encoding.UTF8;
			this.SetUpperCase = SetUpperCase;

			TextReader iniFile = null;
			String strLine = null;
			String currentRoot = null;
			String[] keyPair = null;

			/*if (File.Exists(iniPath))
			{*/
			try
			{
				iniFile = new StreamReader(stream, encoding);

				strLine = iniFile.ReadLine();

				while (strLine != null)
				{
					strLine = strLine.Trim()/*.ToUpper()*/;

					if (strLine != "")
					{
						if (strLine.StartsWith("[") && strLine.EndsWith("]"))
						{
							currentRoot = strLine.Substring(1, strLine.Length - 2);
						}
						else
						{
							if (strLine.StartsWith("'"))
							{
								// assuming comments start with the apostrophe
								// do nothing
							}
							else
							{
								keyPair = strLine.Split(new char[] { '=' }, 2);

								SectionPair sectionPair;
								String value = null;

								if (currentRoot == null)
									currentRoot = "ROOT";

								sectionPair.Section = SetUpperCase ? currentRoot.ToUpper() : currentRoot;
								sectionPair.Key = SetUpperCase ? keyPair[0].ToUpper() : keyPair[0];

								if (keyPair.Length > 1)
									value = keyPair[1];

								keyPairs.Add(sectionPair, value);
							}
						}
					}

					strLine = iniFile.ReadLine();
				}

			}
			catch (Exception ex)
			{
				throw ex;
			}
			finally
			{
				if (iniFile != null)
					iniFile.Close();
				if (DisposeStream)
					stream.Dispose();
			}
			/*}
			else
				throw new FileNotFoundException("Unable to locate " + iniPath);*/
#endif
		}

		/// <summary>
		/// Parses a text that contains the INI lines.
		/// <para>Example: IniAsText = "[A] \n B=1"</para>
		/// </summary>
		/// <param name="SetUpperCase"></param>
		public static IniParser ParseText(string IniAsText, bool SetUpperCase = true) // Chris
		{
			var iniParser = new IniParser(SetUpperCase);
			iniParser.ParseText(IniAsText);
			return iniParser;
		}

		void ParseText(string IniAsText)
		{
			var lines = IniAsText.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries).Select(s =>
				{
					if (s.EndsWith("\r"))
						return s.Remove(s.Length - 1, 1);
					return s;
				});
			this.ParseText(lines);
		}

		void ParseText(IEnumerable<string> lines)
		{
			String currentRoot = null;
			String[] keyPair = null;

			foreach (var line in lines)
			{
				var strLine = line.Trim()/*.ToUpper()*/;

				if (strLine != "")
				{
					if (strLine.StartsWith("[") && strLine.EndsWith("]"))
					{
						currentRoot = strLine.Substring(1, strLine.Length - 2);
					}
					else
					{
						if (strLine.StartsWith("'"))
						{
							// assuming comments start with the apostrophe
							// do nothing
						}
						else
						{
							keyPair = strLine.Split(new char[] { '=' }, 2);

							SectionPair sectionPair;
							String value = null;

							if (currentRoot == null)
								currentRoot = "ROOT";

							sectionPair.Section = SetUpperCase ? currentRoot.ToUpper() : currentRoot;
							sectionPair.Key = SetUpperCase ? keyPair[0].ToUpper() : keyPair[0];

							if (keyPair.Length > 1)
								value = keyPair[1];

							keyPairs.Add(sectionPair, value);
						}
					}
				}
			}
		}

		/// <summary>
		/// Returns the value for the given section, key pair.
		/// Renvoie 'null' si pas de clé ou de section à ce nom.
		/// </summary>
		/// <param name="sectionName">Section name.</param>
		/// <param name="settingName">Key name.</param>
		public String GetSetting(String sectionName, String settingName)
		{
			SectionPair sectionPair;
			sectionPair.Section = SetUpperCase ? sectionName.ToUpper() : sectionName;
			sectionPair.Key = SetUpperCase ? settingName.ToUpper() : settingName;

			try
			{
				return (String)keyPairs[sectionPair];
			}
			catch { return null; }
		}

		/// <summary>
		/// Enumerates all lines for given section.
		/// Réalisé d'un bloc en créant une liste.
		/// </summary>
		/// <param name="sectionName">Section to enum.</param>
		public String[] EnumSection(String sectionName)
		{
			ArrayList tmpArray = new ArrayList();

			foreach (SectionPair pair in keyPairs.Keys)
			{
				if (pair.Section == (SetUpperCase ? sectionName.ToUpper() : sectionName))
					tmpArray.Add(pair.Key);
			}

			return (String[])tmpArray.ToArray(typeof(String));
		}

		/// <summary>
		/// Enumerates all lines for given section.
		/// Progressif avec un 'yield', pour les foreach.
		/// </summary>
		/// <param name="sectionName">Section to enum.</param>
		public IEnumerator<string> EnumèreSection(String sectionName)
		{
			foreach (SectionPair pair in keyPairs.Keys)
			{
				if (pair.Section == (SetUpperCase ? sectionName.ToUpper() : sectionName))
					yield return pair.Key;
			}
		}

		/// <summary>
		/// Adds or replaces a setting to the table to be saved.
		/// </summary>
		/// <param name="sectionName">Section to add under.</param>
		/// <param name="settingName">Key name to add.</param>
		/// <param name="settingValue">Value of key.</param>
		public void AddSetting(String sectionName, String settingName, String settingValue)
		{
			SectionPair sectionPair;
			sectionPair.Section = SetUpperCase ? sectionName.ToUpper() : sectionName;
			sectionPair.Key = SetUpperCase ? settingName.ToUpper() : settingName;

			if (keyPairs.ContainsKey(sectionPair))
				keyPairs.Remove(sectionPair);

			keyPairs.Add(sectionPair, settingValue);
		}

		/// <summary>
		/// Adds or replaces a setting to the table to be saved with a null value.
		/// </summary>
		/// <param name="sectionName">Section to add under.</param>
		/// <param name="settingName">Key name to add.</param>
		public void AddSetting(String sectionName, String settingName)
		{
			AddSetting(sectionName, settingName, null);
		}

		/// <summary>
		/// Remove a setting.
		/// </summary>
		/// <param name="sectionName">Section to add under.</param>
		/// <param name="settingName">Key name to add.</param>
		public void DeleteSetting(String sectionName, String settingName)
		{
			SectionPair sectionPair;
			sectionPair.Section = SetUpperCase ? sectionName.ToUpper() : sectionName;
			sectionPair.Key = SetUpperCase ? settingName.ToUpper() : settingName;

			if (keyPairs.ContainsKey(sectionPair))
				keyPairs.Remove(sectionPair);
		}

		/// <summary>
		/// Save settings to new file.
		/// </summary>
		/// <param name="newFilePath">New file path.</param>
		public bool SaveSettings(String newFilePath)
		{
			ArrayList sections = new ArrayList();
			String tmpValue = "";
			String strToSave = "";

			foreach (SectionPair sectionPair in keyPairs.Keys)
			{
				if (!sections.Contains(sectionPair.Section))
					sections.Add(sectionPair.Section);
			}

			foreach (String section in sections)
			{
				strToSave += ("[" + section + "]\r\n");

				foreach (SectionPair sectionPair in keyPairs.Keys)
				{
					if (sectionPair.Section == section)
					{
						tmpValue = (String)keyPairs[sectionPair];

						if (tmpValue != null)
							tmpValue = "=" + tmpValue;

						strToSave += (sectionPair.Key + tmpValue + "\r\n");
					}
				}

				strToSave += "\r\n";
			}

			try
			{
				TextWriter tw = new StreamWriter(newFilePath);
				tw.Write(strToSave);
				tw.Close();
			}
			catch (Exception ex)
			{
				throw ex;
			}
			return true;
		}

		/// <summary>
		/// Save settings back to ini file.
		/// </summary>
		public bool SaveSettings()
		{
			if (!string.IsNullOrEmpty(iniFilePath))
				return SaveSettings(iniFilePath);
			return false;
		}
	}

}