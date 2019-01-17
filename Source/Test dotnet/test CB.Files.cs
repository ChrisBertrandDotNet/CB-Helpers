
// Copyright Christophe Bertrand
// https://github.com/ChrisBertrandDotNet/CB-Helpers
// https://chrisbertrand.net

using CB.Files;

public partial class TestHelpers
{
	public void Test_CB_Files_cs()
	{
		#region ExistingDirectory

		const string winDir= @"c:\Windows";
		var w1 = new ExistingDirectory(winDir); // Throws an exception if the directory does not exists.
		var w2 = ExistingDirectory.GetDirectory(winDir); // returns null if the directory does not exists.

		#endregion ExistingDirectory
	}
}