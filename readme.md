# _CB Helpers_ is a set of programming helpers for .NET

This library is a set of useful types and functions I use in my open source projects.

In addition to many simple helpers, this library proposes a specific programming style:
- [Error codes philosophy](Documentation/CB.Execution.Return.cs.md) (instead of exceptions).
- [Certified values](Documentation/CB.Data.Certify.cs.md) (such as non-null references, non-empty strings and bounded values).

## The libraries

- **CB Helpers dotnet**  
This is the main library for .NET 4.5.
- **CB Helpers WPF**  
This is an additional library for WPF (on .NET 4.5).
- **CB Helpers WindowsForms**  
This is an additional library for Windows Forms (on .NET 4.5).

## The plan

In the *future*, this library will be improved in order to totally respect these requirements:
- Be concurrent (or thread-safe at least).
- Be portable.
- Apply the [error codes philosophy](Documentation/CB.Execution.Return.cs.md) (instead of exceptions).
- Return [certified values](Documentation/CB.Data.Certify.cs.md) (such as non-null references and bounded values). [Will depend on C# 8 too]
- Apply a (non-published yet) equivalent of the [contract conditions](Documentation/CB.Validation.ContractConditions.cs.md) that will make it compatible with the Error codes philosophy.
- Apply the concept of [parallel consistency](Documentation/CB.Parallelism.ConsistentData.cs.md).
- Apply a custom version of testing procedures. (not available yet).

## Content in short, by file names

- [__CB.Data.Base32.cs__](Documentation/CB.Data.Base32.cs.md)  
Similar to base64 but more human-manageable.
- [__CB.Data.BlockingCollectionAsICollection.cs__](Documentation/CB.Data.BlockingCollectionAsICollection.cs.md)  
Presents `System.Collections.Concurrent.BlockingCollection` as a `ICollection<T>`.
- [__CB.Data.Certify.cs__](Documentation/CB.Data.Certify.cs.md)  
**Gives non-null certified references, plus related structures that certify their data is defined and valid.**
- [__CB.Data.Comparer.cs__](Documentation/CB.Data.Comparer.cs.md)  
Implements common comparisons for your class: `== , !=, <, <=, >, >=`, etc.
- [__CB.Data.EmptyArray.cs__](Documentation/CB.Data.EmptyArray.cs.md)  
Gives an empty array of type T.
- [__CB.Data.EnumHelper.cs__](Documentation/CB.Data.EnumHelper.cs.md)  
Gets information about an enumeration (of type T).
- [__CB.Data.ImmutableDictionary.cs__](Documentation/CB.Data.ImmutableDictionary.cs.md)  
This is a read-only dictionary.
- [__CB.Data.ImmutableList.cs__](Documentation/CB.Data.ImmutableList.cs.md)  
This is a read-only list/array.
- [__CB.Data.NullableAny.cs__](Documentation/CB.Data.NullableAny.cs.md)  
Similar to `System.Nullable<T>`, but applicable to both class and structure types.
- [__CB.Execution.Return.cs__](Documentation/CB.Execution.Return.cs.md)  
**Here, the philosophy is to return codes on errors, not Exceptions.**
- [__CB.Files.cs__](Documentation/CB.Files.cs.md)  
Some portable helpers for file access.
- [__CB.Files.INI.cs__](Documentation/CB.Files.INI.cs.md)  
Manages INI files for settings.
- [__CB.Graphics.GraphicsExtensions.SetPixelFormat.cs__](Documentation/CB.Graphics.GraphicsExtensions.SetPixelFormat.cs.md)  
Helpers for graphic classes.
- [__CB.Graphics.PixelBGR32.cs__](Documentation/CB.Graphics.PixelBGR32.cs.md)  
ABGR pixel in a Int32.
- [__CB.OperatingSystem.EnvironmentEx.cs__](Documentation/CB.OperatingSystem.EnvironmentEx.cs.md)  
Helpers about the operating system's environment in general (similar to `System.Environment`).
- [__CB.Parallelism.ConsistentData.cs__](Documentation/CB.Parallelism.ConsistentData.cs.md)  
**This is a way to manage data concurrently without any lock, thanks to consistency.**
- [__CB.Processus.ProcessEx.cs__](Documentation/CB.Processus.ProcessEx.cs.md)  
Helpers for `System.Diagnostics.Process`.
- [__CB.Processus.ProcessEx.SetDebugPrivilege.cs__](Documentation/CB.Processus.ProcessEx.SetDebugPrivilege.cs.md)  
Obtains Debug privilege for the current process.
- [__CB.Reflection.MemberInfoOf.cs__](Documentation/CB.Reflection.MemberInfoOf.cs.md)  
Retreives the `System.Reflection.MemberInfo` of a member, parameter, etc..
- [__CB.Reflection.NameOf.cs__](Documentation/CB.Reflection.NameOf.cs.md)  
Similar to `nameof`but applicable to C# ≤ 5 (and VS ≤ 2013).
- [__CB.Reflection.Reflection.cs__](Documentation/CB.Reflection.Reflection.cs.md)  
Helpers for reflection classes such as `MemberInfo`.
- [__CB.Reflection.TypeEx.cs__](Documentation/CB.Reflection.TypeEx.cs.md)  
Portable reflection helpers for Type.
- [__CB.Text.StringWithHashCode.cs__](Documentation/CB.Text.StringWithHashCode.cs.md)  
Cached string classes that optimize comparisons (useful to speedup string dictionaries).
- [__CB.Text.Unicode.cs__](Documentation/CB.Text.Unicode.cs.md)  
Helpers for Unicode texts.
- [__CB.Validation.ContractConditions.cs__](Documentation/CB.Validation.ContractConditions.cs.md)  
**Do conditional checks, similar to the contracts (see  `System.Diagnostics.Contracts.Contract`), but with different (and more comprehensible) names and a few other differences.**
- [__CB.WindowsForms.FolderSelectDialog.cs__](Documentation/CB.WindowsForms.FolderSelectDialog.cs.md)  
Helpers for Windows Forms (such as `FolderSelectDialog`).
- [__CB.WPF.ClipboardImage.cs__](Documentation/CB.WPF.ClipboardImage.cs.md)  
Helpers for images in the clipboard.
- [__CB.WPF.Image.SetImageDisplaySize.cs__](Documentation/CB.WPF.Image.SetImageDisplaySize.cs.md)  
Helpers for images.
- [__CB.Xaml.XamlHelper.cs__](Documentation/CB.Xaml.XamlHelper.cs.md) [WPF + Windows Store]  
Helpers for rich texts, events and XAML trees.
- [__CB.XamlCB.Xaml.BindingHelper.cs__](Documentation/CB.XamlCB.Xaml.BindingHelper.cs.md) [WPF + Windows Store]  
Helpers for Bindings.
- [__CB.Xml.AssemblyToXsd.cs__](Documentation/CB.Xml.AssemblyToXsd.cs.md)  
Extracts a XSD schema from the types an Assembly file (`.exe` or `.dll`) declares.
- [__CB.Xml.XML.cs__](Documentation/CB.Xml.XML.cs.md)  
Helpers for XML files.



## Release notes

Version 1.2
2019-01-xx
- xxx

Version 1.1
2019-01-17
- Many files added. Most are still quite simple.
- Every code file is paired with a markdown documentation file.

Version 1.0
2019-01-06
- First version.
- Only one file, just to check the structure.

## To do

- Add `CB_DOTNET\_TESTS\Test.cs` in order to activate auto-testing.
- Add tests for all code files.
- Synchronize with UniversalSerializer ?

---
Copyright (c) [Christophe Bertrand](https://chrisbertrand.net)  
https://github.com/ChrisBertrandDotNet/CB-Helpers
