# CB.Data.ImmutableList.cs

## class `ImmutableList<T>`

This is a read-only list/array.

Notes:
- Unlike `ReadOnlyCollection<>`, it is not a wrapper: items are copied.
- It implements `IReadOnlyList<>`, *not* `IList<>`.
- This class is thread-safe and concurrent.  
It uses an array of T internally.

---

[Go back to the table of contents](../readme.md)

---
Copyright (c) [Christophe Bertrand](https://chrisbertrand.net)  
https://github.com/ChrisBertrandDotNet/CB-Helpers
