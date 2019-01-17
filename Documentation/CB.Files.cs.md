# CB.Files.cs

Some portable helpers for file access.

## class `ExistingDirectory`

Represents an existing directory (at the time of the class instanciation).
Unlike `System.IO.DirectoryInfo` which can be instanciated on a non-existing directory.

You can instanciate differently along your favorite concept, exceptions or [error codes](CB.Execution.Return.cs.md):
```C#
const string winDir= @"c:\Windows";
var w1 = new ExistingDirectory(winDir); // Throws an exception if the directory does not exists.
var w2 = ExistingDirectory.GetDirectory(winDir); // returns null if the directory does not exists.
```

## class `FileEx`

Notable static fiels:
- `FileNameComparer`  
A portable (Windows, Unix, etc) comparer for pure **file names**. Paths are ignored.  
This is a `IEqualityComparer<string>` you can use in collections classes (such as dictionaries).
- `PathComparer`  
A portable (Windows, Unix, etc) comparer for file-system **paths**.  
This is a `IEqualityComparer<string>` too.

Useful static function:
- `bool IsValidFileName(string name)`  
Checks the syntax validity as a file name.  
It is portable (Windows, Unix, etc).

Useful class extension:
- `static int RemoveFiles(this IList<string> files, ICollection<string> otherFiles, bool ignorePath)`  
Removes from the first file list the files that are in the second file list.  
Uses the portable file and path comparers.


---

[Go back to the table of contents](../readme.md)

---
Copyright (c) [Christophe Bertrand](https://chrisbertrand.net)  
https://github.com/ChrisBertrandDotNet/CB-Helpers
