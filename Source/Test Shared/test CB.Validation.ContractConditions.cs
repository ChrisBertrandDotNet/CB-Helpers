
// Copyright Christophe Bertrand
// https://github.com/ChrisBertrandDotNet/CB-Helpers
// https://chrisbertrand.net

using CB.Validation;

public partial class TestHelpers
{
	public void Test_CB_Validation_ContractConditions_cs()
	{
		var h = AddHello("Hi. ");

		var t = BuildTextUsingMachineName("Hello {0} !");
	}

	#region Different usage between public and private accesses

	public string AddHello(string a)
	{
		ReleaseCheck.ParameterIsDefined(a); // Always checks, because the function is public.
		return AddStrings(a, "Hello");
	}

	private string AddStrings(string a, string b)
	{
		DebugCheck.ParameterIsDefined(a); // An exception implies a programming error.
		DebugCheck.ParameterIsDefined(b);
		return a + b;
	}

	#endregion Different usage between public and private accesses

	public string BuildTextUsingMachineName(string Message) // Builds kind of "Hello Server232 !".
	{
		ReleaseCheck.ParameterStringIsDefined(Message); // Checks the string is not null and is not empty.
		ReleaseCheck.Parameter(Message.Contains("{0}")); // Checks a specific condition.

		var machineName = System.Environment.MachineName;
		ReleaseCheck.InCodeStringIsDefined(machineName); // a condition in the function body itself.

		var builtMessage = string.Format(Message, machineName);

		DebugCheck.AfterCodeReferenceIsDefined(builtMessage); // Optional final check. Only on Debug compilation.
		return builtMessage;
	}


}