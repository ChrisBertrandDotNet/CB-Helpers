# CB.WindowsForms.FolderSelectDialog.cs

Helpers for Windows Forms.

In `CB Helpers WindowsForms.dll`.

## class `FolderSelectDialog`

Lets the user select a folder.  
This class uses the regular Windows' directory selection window, unlike `System.Windows.Forms.FolderBrowserDialog` which displays a very limited window.

Example:
```C#
var dialog = new CB.WindowsForms.FolderSelectDialog
{ DirectoryPath= @"d:\" }; // initial directory
if (dialog.ShowDialog(Handle) == System.Windows.Forms.DialogResult.OK)
{
  var d = dialog.DirectoryPath;
  ...
}
```

### From WPF too
FolderSelectDialog can be called from WPF:
```C#
var hwnd = new WindowInteropHelper(this).Handle; // 'this' is a WPF Window.
var dialog = new CB.WindowsForms.FolderSelectDialog
{
  DirectoryPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
};
if (dialog.ShowDialog(hwnd) == System.Windows.Forms.DialogResult.OK)
{
  var d = dialog.DirectoryPath;
  ...
}
```
---

[Go back to the table of contents](../readme.md)

---
Copyright (c) [Christophe Bertrand](https://chrisbertrand.net)  
https://github.com/ChrisBertrandDotNet/CB-Helpers
