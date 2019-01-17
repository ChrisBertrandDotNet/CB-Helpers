# CB.Data.NullableAny.cs

Similar to `System.Nullable<T>`, but applicable to both class and structure types.  
Intended to let *generic* functions return nullable values that apply equally to **both** classes and structures.

Example as a return value:
```C#
NullableAny<T> AsAStringOrADouble<T>(T obj)
{
  if (obj is string || obj is double) return obj;
  else return NullableAny<T>.Undefined;
}
```
Here the returned structure can be:
- A `NullableAny<String>` where the value is a class.
- Or a `NullableAny<Double>` where the value is a structure.
- Or a `NullableAny<T>` where the value is not defined.

Example as a parameter:
```C#
string GetStringRepresentation<T>(NullableAny<T> obj)
{
  if (obj.HasValue) return obj.Value.ToString();
  else return "! undefined !";
}
```
Here this function works equally whether T is a class or a structure.

## structure `NullableAny<T>`

Note:
- The value is seen as undefined if it is null.  
I made this choice in order to be consistent with `System.Nullable<T>`.  
As a result, you can't have null as a valid defined value, even when T is a class type.
- `HasValue` returns true if there is value, **and** if the value is *not* null.
- Set value is *not* thread-safe at the moment.  
That is not a problem if you build a `NullableAny<T>` with a value and do not change the value later.  
In fact, this is the usual practice, so I don't want to slow down this structure with a lock. Of course, you may ask "why did you write a `set value` then ?". My answer is: "I don't remember, please return a few years ago and ask my old/previous body. [ a time machine is required ]".

---

[Go back to the table of contents](../readme.md)

---
Copyright (c) [Christophe Bertrand](https://chrisbertrand.net)  
https://github.com/ChrisBertrandDotNet/CB-Helpers
