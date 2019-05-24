
// Copyright Christophe Bertrand
// https://github.com/ChrisBertrandDotNet/CB-Helpers
// https://chrisbertrand.net

using CB.Files;

public partial class TestHelpers
{
	public void Test_CB_Files_cs()
	{
		#region ExistingDirectory

		string winDir = System.Environment.SystemDirectory;// ex: @""C:\WINDOWS\system32"";
		Release.Assert(!string.IsNullOrEmpty(winDir));
		var w1 = new ExistingDirectory(winDir); // Throws an exception if the directory does not exists.
		Release.Assert(ExistingDirectory.GetDirectory(winDir) != null); // returns null if the directory does not exists.

		#endregion ExistingDirectory
	}
}