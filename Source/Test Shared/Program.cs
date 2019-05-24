
// Copyright Christophe Bertrand
// https://github.com/ChrisBertrandDotNet/CB-Helpers
// https://chrisbertrand.net

using System;
using System.Diagnostics;

namespace Test_Shared
{
	class Program
	{
		static void Main(string[] args)
		{
			var th = new TestHelpers();
			th.Test_CB_Data_Certify_cs();
			th.Test_CB_Data_NullableAny_cs();
			th.Test_CB_Execution_Return_cs();
			th.Test_CB_Files_cs();
			th.test_CB_Parallelism_ConsistentData_cs();
			th.Test_CB_Parallelism_PositionList_cs();
			th.Test_CB_Reflection_MemberInfoOf_cs();
			th.Test_CB_Text_StringWithHashCode_cs();
			th.Test_CB_Validation_ContractConditions_cs();
			th.Test_CB_Xml_AssemblyToXsd_cs();

			Console.WriteLine("No error.");

			if (Debugger.IsAttached)
			{
				Console.WriteLine("Press any key to close this app.");
				Console.ReadKey();
			}
		}
	}
}