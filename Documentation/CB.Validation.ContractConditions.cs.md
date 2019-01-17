# CB.Validation.ContractConditions.cs

Do conditional checks, similar to the contracts (see  `System.Diagnostics.Contracts.Contract`), but with different (and more comprehensible) names and a few other differences.

Note: these types are based on the "Exception philosophy" that is obviously incompatible with the "Error codes philosophy" developped in [CB.Execution.Return.cs](CB.Execution.Return.cs.md).

## Typical example

```C#
public string BuildTextUsingMachineName(string Message) // Builds kind of "Hello Server232 !".
{
  ReleaseCheck.ParameterStringIsDefined(Message); // Checks the string is not null and is not empty.
  ReleaseCheck.Parameter(Message.Contains("{0}")); // Checks a specific condition.

  var machineName = System.Environment.MachineName;
  ReleaseCheck.InCodeStringIsDefined(machineName); // a condition in the function body itself.

  var builtMessage = string.Format(Message, machineName);

  DebugCheck.AfterCodeReferenceIsDefined(builtMessage); // Optional final check. Only on Debug compilation.
  return builtMessage;
}
```

## Different usage between public and private accesses
This is a proposed programming style, and the validators it implies.
```C#
public string AddHello(string a)
{
  ReleaseCheck.ParameterIsDefined(a); // Always checks, because the function is public.
  return AddStrings(a, "Hello");
}

private string AddStrings(string a, string b)
{
  DebugCheck.ParameterIsDefined(a); // An exception implies a programming error.
  DebugCheck.ParameterIsDefined(b);
  return a + b;
}
```
Please note a subtle difference between these two functions:
- Parameters of public functions are always checked.
- Parameters of private functions are checked in Debug compilation only because they are never supposed to be null.  
There is no check on Release compilation, for optimization.

Of course this is only a suggestion.



---

[Go back to the table of contents](../readme.md)

---
Copyright (c) [Christophe Bertrand](https://chrisbertrand.net)  
https://github.com/ChrisBertrandDotNet/CB-Helpers
