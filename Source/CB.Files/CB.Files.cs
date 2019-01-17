
// Copyright Christophe Bertrand
// https://github.com/ChrisBertrandDotNet/CB-Helpers
// https://chrisbertrand.net

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

#region Addition for Windows Runtime
#if NETFX_CORE
namespace System.IO
{
	public class DriveNotFoundException : DirectoryNotFoundException
	{ }
}
#endif // NETFX_CORE
#endregion Addition for Windows Runtime

namespace CB.Files
{


	/// <summary>
	/// Represents an existing directory (at the time of instanciation).
	/// </summary>
	public class ExistingDirectory
	{
		/// <summary>
		/// The directory that was given as the class constructor parameter.
		/// </summary>
		public readonly string OriginalDirectory;
		/// <summary>
		/// The full path, determined at the moment of the class construction.
		/// </summary>
		public readonly string FullPath;

		/// <summary>
		/// True if the file still exists, now.
		/// </summary>
		public bool StillExists
		{ get { return Directory.Exists(this.FullPath); } }

		/// <summary>
		/// Creates an instance from an existing directory.
		/// <para>Various exceptions can occur. If you need an exception-less creation, please use the static function <see cref="ExistingDirectory.GetDirectory(string)"/>.</para>
		/// <para>The FullPath is determined now and will stay constant.</para>
		/// </summary>
		/// <param name="directory"></param>
		/// <exception cref="DriveNotFoundException"></exception>
		/// <exception cref="DirectoryNotFoundException"></exception>
		public ExistingDirectory(string directory)
		{
			var di = new DirectoryInfo(directory);
			this.FullPath = di.FullName;
			if (!Path.GetPathRoot(this.FullPath).Contains(Path.VolumeSeparatorChar.ToString()))
				throw new System.IO.DriveNotFoundException();
			if (!Directory.Exists(this.FullPath))
				throw new DirectoryNotFoundException();
			this.OriginalDirectory = directory;
		}

		/// <summary>
		/// Creates an ExistingDirectory. Or returns null in case of a problem (the directory does not exist, or is not accessible, or its full path can not be determined, or the parameter was null or empty, or any other reason.
		/// <para>It does not throw exceptions. If you need exceptions, please use <see cref="ExistingDirectory.ExistingDirectory(string)">the constructor</see>.</para>
		/// </summary>
		/// <param name="directory"></param>
		/// <returns></returns>
		public static ExistingDirectory GetDirectory(string directory)
		{
			try
			{
				return new ExistingDirectory(directory);
			}
			catch
			{
				return null;
			}
		}

		/// <summary>
		/// Instances an existing directory.
		/// <para>If you need an error code constructor, please use <see cref="ExistingDirectory.GetDirectory(string)"/></para>
		/// </summary>
		/// <param name="temp"></param>
		public static implicit operator ExistingDirectory(string temp) // 'implicit' is nice for function parameters.
		{
			return new ExistingDirectory(temp);
		}

		/// <summary>
		/// Returns the full path.
		/// </summary>
		/// <param name="c"></param>
		public static explicit operator string(ExistingDirectory c)
		{
			return c.FullPath;
		}

		/// <summary>
		/// True if a == b.
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		public static bool operator ==(ExistingDirectory a, ExistingDirectory b)
		{
			return object.ReferenceEquals(a, b) || (!object.ReferenceEquals(a, null) && !object.ReferenceEquals(b, null)
					&& a.FullPath == b.FullPath);
		}
		/// <summary>
		/// True if a != b.
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		public static bool operator !=(ExistingDirectory a, ExistingDirectory b)
		{
			return !(a == b);
		}
		/// <summary>
		/// True if this equals the <paramref name="obj"/>.
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public override bool Equals(object obj)
		{
			var obj2 = obj as ExistingDirectory;
			if (object.ReferenceEquals(obj2, null))
				return false;
			return this == obj2;
		}

		/// <summary>
		/// Returns hash code.
		/// </summary>
		/// <returns></returns>
		public override int GetHashCode()
		{
			return this.FullPath.GetHashCode();
		}
		/// <summary>
		/// Returns a string that represents the directory.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return this.FullPath;
		}
	}

	/// <summary>
	/// Helpers functions for files.
	/// </summary>
	public static class FileEx
	{
		/// <summary>
		/// Checks the syntax validity as a file name.
		/// <para>It does not check the file existence.</para>
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public static bool IsValidFileName(string name)
		{
			if (string.IsNullOrEmpty(name))
				return false;
			if (name.Contains(".."))
				return false;
			const string caractèresInterdits = "\\/:*?\"<>|";
			foreach (var interdit in caractèresInterdits)
				if (name.Contains(interdit))
					return false;
			return true;
		}

		/// <summary>
		/// A portable comparer for file-system paths.
		/// <para>On Linux and macOS, the comparer takes letter case into account. On Windows, it ignores letter case.</para>
		/// <para>Starting and ending spaces, and quotation marks are removed before comparison.</para>
		/// <para>Relative paths are not resolved.</para>
		/// <para>Path syntax and existence are not checked.</para>
		/// </summary>
		public static readonly IEqualityComparer<string> PathComparer = new PortablePathComparer();

		/// <summary>
		/// A portable comparer for pure file names. Paths are ignored.
		/// <para>On Linux and macOS, the comparer takes letter case into account. On Windows, it ignores letter case.</para>
		/// <para>Starting and ending spaces, and quotation marks are removed before comparison.</para>
		/// <para>Relative paths are not resolved.</para>
		/// <para>Path syntax and existence are not checked.</para>
		/// </summary>
		public static readonly IEqualityComparer<string> FileNameComparer = new PortableFileNameComparer();

		/// <summary>
		/// Removes from the first file list the files that are in the second file list.
		/// </summary>
		/// <param name="files">The first list. It will be modified</param>
		/// <param name="otherFiles">The second list.</param>
		/// <param name="ignorePath">If true, the file names are compared, ignoring the file paths.</param>
		/// <returns>The number of removed files.</returns>
		public static int RemoveFiles(this IList<string> files, ICollection<string> otherFiles, bool ignorePath)
		{
			var comparateur = ignorePath ? CB.Files.FileEx.FileNameComparer : CB.Files.FileEx.PathComparer;

			int début = files.Count;
			for (int i = 0; i < files.Count; i++)
			{
				if (otherFiles.Contains(files[i], comparateur))
					files.RemoveAt(i--);
			}
			return début - files.Count;
		}

		class PortablePathComparer : IEqualityComparer<string>
		{
			readonly IEqualityComparer<string> _PathComparer =
					Environment.OSVersion.Platform == PlatformID.MacOSX || Environment.OSVersion.Platform == PlatformID.Unix ? StringComparer.CurrentCulture : StringComparer.CurrentCultureIgnoreCase;

			public bool Equals(string x, string y)
			{
				x = supprimeGuillemets(x);
				y = supprimeGuillemets(y);
				return _PathComparer.Equals(x, y);
			}

			static string supprimeGuillemets(string x)
			{
				x = x.Trim();
				if (x.Length >= 2 && x[0] == '\"' && x[x.Length - 1] == '\"')
					x = x.Substring(1, x.Length - 2);
				return x;
			}

			public int GetHashCode(string obj)
			{
				return 0;
			}
		}

		/// <summary>
		/// Compares file names, but ignore their path.
		/// </summary>
		class PortableFileNameComparer : IEqualityComparer<string>
		{
			readonly IEqualityComparer<string> _PathComparer =
				Environment.OSVersion.Platform == PlatformID.MacOSX || Environment.OSVersion.Platform == PlatformID.Unix ? StringComparer.CurrentCulture : StringComparer.CurrentCultureIgnoreCase;

			public bool Equals(string x, string y)
			{
				x = Path.GetFileName(supprimeGuillemets(x));
				y = Path.GetFileName(supprimeGuillemets(y));
				return _PathComparer.Equals(x, y);
			}

			static string supprimeGuillemets(string x)
			{
				x = x.Trim();
				if (x.Length >= 2 && x[0] == '\"' && x[x.Length - 1] == '\"')
					x = x.Substring(1, x.Length - 2);
				return x;
			}

			public int GetHashCode(string obj)
			{
				return 0;
			}
		}

	}
}