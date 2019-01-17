# CB.Files.INI.cs

Manages INI files for settings.

Please note this class is *not* thread-safe at the moment.

## class `IniParser`

Example:
```C#
var parser = new IniParser(@"C:\test.ini");
     
String newMessage;
     
newMessage = parser.GetSetting("appsettings", "msgpart1");
newMessage += parser.GetSetting("appsettings", "msgpart2");
newMessage += parser.GetSetting("punctuation", "ex");
     
Console.WriteLine(newMessage);
```

---

[Go back to the table of contents](../readme.md)

---
Copyright (c) [Christophe Bertrand](https://chrisbertrand.net)  
https://github.com/ChrisBertrandDotNet/CB-Helpers
