# CB.Data.EmptyArray.cs

## class `EmptyArray<T>`

Gives an empty array of type T.

We preserve resources since we build only one empty array by type. That is better than building a new empty array regularly.

Get the empty array by reading this field:
```C#
EmptyArray<T>.Empty
```

---

[Go back to the table of contents](../readme.md)

---
Copyright (c) [Christophe Bertrand](https://chrisbertrand.net)  
https://github.com/ChrisBertrandDotNet/CB-Helpers
