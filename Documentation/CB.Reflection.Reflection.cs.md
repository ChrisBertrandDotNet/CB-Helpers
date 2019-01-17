# CB.Reflection.Reflection.cs

Helpers for reflection classes such as `System.Reflection.MemberInfo`.

## class `Reflection`

There is a series of static functions `IsEqualTo()` that apply to `FieldInfo`, `PropertyInfo`, `MemberInfo`, `MethodInfo` and `ParameterInfo`.  
These functions do the right comparison, unlike the framework's `FieldInfo.Equals` or `FieldInfo.==` that do not work correctly (at the moment of writing this note).

## Comparison classes

The following classes implement `IEqualityComparer<>`.  
They replace the framework's comparison functions, which are incorrect (at the moment of writing this note).

They can be used in the constructor of various collection types, as `HashSet` and `Dictionary`.

- `FieldInfoComparer`
- `MemberInfoComparer`
- `MethodInfoComparer`
- `ParameterInfoComparer`
- `PropertyInfoComparer`

---

[Go back to the table of contents](../readme.md)

---
Copyright (c) [Christophe Bertrand](https://chrisbertrand.net)  
https://github.com/ChrisBertrandDotNet/CB-Helpers
