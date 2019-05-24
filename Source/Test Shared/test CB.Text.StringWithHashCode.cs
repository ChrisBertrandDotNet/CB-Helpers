
// Copyright Christophe Bertrand
// https://github.com/ChrisBertrandDotNet/CB-Helpers
// https://chrisbertrand.net

using CB.Text;

public partial class TestHelpers
{
	public void Test_CB_Text_StringWithHashCode_cs()
	{
		const string text = "Hello!";
		StringWithCache t1 = text; // Adds the text to the cache and calculates the hash code.
		StringWithCache t2 = text;  // Gets the hash code from the cache.

		StringWithHashCode t3 = new string('0', 1000000); // Builds a big string and calculate its hash code (that takes a while).
		var hc = t3.HashCode; // Gets the hash code directly, no calculations needed.
	}
}