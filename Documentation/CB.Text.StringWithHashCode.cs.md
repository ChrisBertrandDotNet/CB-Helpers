# CB.Text.StringWithHashCode.cs


## class `StringWithHashCode`

This class stores a String as a value, and memorizes the hash code of the string.  
That is useful to speedup string dictionaries.

Example:
```C#
StringWithHashCode t3 = new string('0', 1000000); // Builds a big string and calculate its hash code (that takes a while).
var hc = t3.HashCode; // Gets the hash code directly, no calculations needed.
```

## class `StringWithCache`

Similar to the previous class except here there is a static (common) string cache that avoids duplicate hash code calculations.  
It is useful for big strings, and for massive duplication of strings.

For example:
```C#
const string text = "Hello!";
StringWithCache t1 = text; // Adds the text to the cache and calculates the hash code.
StringWithCache t2 = text;	// Gets the hash code from the cache.
```

Note: This cache works on weak references, so the garbage collection is preserved.

---

[Go back to the table of contents](../readme.md)

---
Copyright (c) [Christophe Bertrand](https://chrisbertrand.net)  
https://github.com/ChrisBertrandDotNet/CB-Helpers
