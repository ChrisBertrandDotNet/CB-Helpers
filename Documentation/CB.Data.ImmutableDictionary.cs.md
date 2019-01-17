# CB.Data.ImmutableDictionary.cs

## class `ImmutableDictionary<K,V>`

This is a read-only dictionary.

Notes:
- Unlike `ReadOnlyDictionary<,>`, it is not a wrapper: items are copied.
- It implements `IReadOnlyDictionary<,>`, *not* `IDictionary<,>`.
- This class is thread-safe and concurrent.  
It uses `System.Collections.Concurrent.ConcurrentDictionary<,>` internally.

---

[Go back to the table of contents](../readme.md)

---
Copyright (c) [Christophe Bertrand](https://chrisbertrand.net)  
https://github.com/ChrisBertrandDotNet/CB-Helpers
