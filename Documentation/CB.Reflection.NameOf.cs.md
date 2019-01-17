# CB.Reflection.NameOf.cs

Similar to `nameof`but applicable to C# ≤ 5 (and VS ≤ 2013).

Note: These functions use reflection, therefore are much slower than `nameof`.

## class `NameOf`

Gets the name of an instance or static function.
```C#
var thisInstanceFunctionName = NameOf.Function((Action)this.InstanceFunction);
var staticFunctionName = NameOf.Function((Func<string, int>)TestClass.StaticFunction);
```
Note: for an instance function on a class type, you need an actual instance (null is not valid).

Gets the name of an instance member (field or property):
```C#
var instanceFieldName = NameOf<Program, int>(u => u.InstanceField);
```
Note: you do not need an actual instance.

Gets the name of an instance (not static) function:
```C#
var instanceFunctionName = NameOf.InstanceFunction<Program>(u => (Func<string, int>)u.InstanceFunction);
```
Note: you do not need an actual instance.

Gets the name of a function parameter:
```C#
var argsName = NameOf.Parameter(() => args);
```
Gets the name of a static data member (field or property):
```C#
var staticFieldName = NameOf.StaticFieldOrProperty(() => Program.StaticField);
```

---

[Go back to the table of contents](../readme.md)

---
Copyright (c) [Christophe Bertrand](https://chrisbertrand.net)  
https://github.com/ChrisBertrandDotNet/CB-Helpers
