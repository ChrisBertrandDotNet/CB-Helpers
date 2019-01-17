# CB.Data.EnumHelper.cs

## class `EnumHelper<T>`

Gets information about an enumeration (of type T).

Notable members:
- `HasFlagsAttribute`  
True is the enumeration can be treated as a bit field; that is, a set of flags.
- `IsDefinedValue(value)`  
Checks if the enumeration value is defined in its enumeration type.
- `IsDefinedAsAUniqueValue(value)`  
Similar to the previous function, except the value can not be a multiple values combination, even if T has FlagsAttribute.
- `Values`  
All the declared values of this enumeration.

---

[Go back to the table of contents](../readme.md)

---
Copyright (c) [Christophe Bertrand](https://chrisbertrand.net)  
https://github.com/ChrisBertrandDotNet/CB-Helpers
