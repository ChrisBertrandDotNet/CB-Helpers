# CB.Data.BlockingCollectionAsICollection.cs

## class `BlockingCollectionAsICollection<>`

 Presents `System.Collections.Concurrent.BlockingCollection` as a `ICollection<T>`.
This way, you have a concurrent blocking collection that is a regular collection.

**Warning:** the function `Remove(item)` is not supported (since `BlockingCollection` itself does not support this feature).

---

[Go back to the table of contents](../readme.md)

---
Copyright (c) [Christophe Bertrand](https://chrisbertrand.net)  
https://github.com/ChrisBertrandDotNet/CB-Helpers
