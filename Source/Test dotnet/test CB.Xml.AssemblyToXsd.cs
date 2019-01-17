
// Copyright Christophe Bertrand
// https://github.com/ChrisBertrandDotNet/CB-Helpers
// https://chrisbertrand.net

using CB.Xml;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

public partial class TestHelpers
{
	public void Test_CB_Xml_AssemblyToXsd_cs()
	{
		var outputPath = Path.GetTempPath();
		var assemblies = new string[] { Assembly.GetExecutingAssembly().Location };
		var outputMessages = new List<string>();

		var exported = AssemblyToXsd.ExportTypesAsSchemas(assemblies, outputPath, outputMessages);
	}
}