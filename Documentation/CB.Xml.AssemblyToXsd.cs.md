# CB.Xml.AssemblyToXsd.cs

Extracts a XSD schema from the types an Assembly file (`.exe` or `.dll`) declares.

## class `AssemblyToXsd`

Example:
```C#
var outputPath = Path.GetTempPath();
var assemblies = new string[] { Assembly.GetExecutingAssembly().Location };
var outputMessages = new List<string>();

var exported = AssemblyToXsd.ExportTypesAsSchemas(assemblies, outputPath, outputMessages);
```

---

[Go back to the table of contents](../readme.md)

---
Copyright (c) [Christophe Bertrand](https://chrisbertrand.net)  
https://github.com/ChrisBertrandDotNet/CB-Helpers
