# CB.OperatingSystem.EnvironmentEx.cs

Helpers about the operating system's environment in general (similar to `System.Environment`).

## class `EnvironmentEx`

- `Is64BitOperatingSystem`
True if the operating system is 64-bits (at least).
- `Is64BitProcess`
True if the process is 64-bits.
- `UserIsAdmin`
True if the current user account has an administrator role.

## class `Win32ExceptionEx`

Throws an exception with the `Marshal.GetLastWin32Error()` added at the end of the message.

---

[Go back to the table of contents](../readme.md)

---
Copyright (c) [Christophe Bertrand](https://chrisbertrand.net)  
https://github.com/ChrisBertrandDotNet/CB-Helpers
