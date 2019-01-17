# CB.Data.StringTranscoder.cs

Functions and types intended to transcode between a type and a String (both directions).

**WARNING, work in progress:** this file seems to be incomplete.
I should add new functions in the future, and rewrite or complete the old functions.  
Even the file name should be renamed as `StringTranstyper.cs`.

# interface `IToString`

This interface extends the concept of `System.Object.ToString()`:
- `Ret<string> ToString(IFormatProvider provider);`
- `Ret<string> ToInvariantString();`
- `static Return<T, ReturnSuccess> Parse(string s);`
- `static Return<T, ReturnSuccess> Parse(string s, IFormatProvider provider);`
- `static Return<T, ReturnSuccess> ParseInvariantString(string s);`

The first two functions are declared in the interface.  
The three last functions are static. It is impossible to declare them in the interface, so you have to copy their signature manually in your own type.


## class `StringTranscoder`

This class retreives:
- The static functions that parse a string ().  
See `static Func<string, T> GetStringToTypeFunction<T>()`.


---

[Go back to the table of contents](../readme.md)

---
Copyright (c) [Christophe Bertrand](https://chrisbertrand.net)  
https://github.com/ChrisBertrandDotNet/CB-Helpers
