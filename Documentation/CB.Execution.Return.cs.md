# CB.Execution.Return.cs

Here, the philosophy is to return codes on errors, not Exceptions.

## Why ?

First, Exceptions have drawbacks:
- Non-caught exceptions crash applications in the wild.
- Caught exceptions are very slow and resource-hungry.

Second problem:  
Normally, programmers should catch separately all exceptions a function (or procedure) can throw.  
For example, ``System.IO.File.Open()`` can throw 9 different exceptions. Every call should be followed by 9 catches.  
In practice, I never see any programmer doing that.  
At best, they just catch the all 9 exceptions at once using a global ``catch { }``, which can be seen as a bad practice (not discriminant enough).

All well-designed programming languages should enforce programmers to manage all errors a function can produce.  
Of course, that is so annoying and repetitive that in the practice programming languages do not enforce anything at all.  
In short, that practice is highly error-prone and probably causes a lot of problems ("bugs" for entomologists), but it takes into account the laziness that affects all human beings (including programmers).

## The proposal
- Functions (methods) return error codes on expected errors. For example when a needed file does not exist.
	- Every function declares, using a specific attribute, a list of error codes it can return.
- Every function call manages *all* the errors that function declares (using a switch-case code block).
- Exceptions are thrown only on unexpected errors, in fact on programming errors. Ideally, they should happen only when debugging ("insect-tracking" ?).

Of course, that affects only your own source code. So you still have to catch Exceptions from external libraries, such as the dotnet and Core frameworks, that apply the regular exception scheme.

### Main structures
- ``ReturnSuccess`` is a common error code set.  
You can return it in a procedure (that would normally return `void`).
- ``Return<T,E>`` is a return structure that contains both a return value and an error code. The value can be null.
- ``Ret<T>`` is a shortcut for ``Return<T, ReturnSuccess>``.
- ``ReturnNonNull<T,E>`` is similar to ``Return<T,E>``, but the value can't be null (except if there is an error, of course).
- ``RetNonNull<T>`` is a shortcut for ``ReturnNonNull<T, ReturnSuccess>``.

### Simple example
A function:
```C#
[ReturnSuccessCodes(ReturnSuccess.ArgumentOutOfRange)]
Ret<double> Sqrt(double d)
{
  if (d < 0.0) return ReturnSuccess.ArgumentOutOfRange;
  return Math.Sqrt(d);
}
```
How we call the function:
```C#
var a = this.Sqrt(0.0);
switch(a.ErrorCode)
{
  case ReturnSuccess.ArgumentOutOfRange:
    a = -1; break;
}
```
### Example with a procedure
A procedure:
```C#
[ReturnSuccessCodes(ReturnSuccess.ArgumentOutOfRange)]
ReturnSuccess DisplayLine(int lineNumber)
{
  if (lineNumber < 0) return ReturnSuccess.ArgumentOutOfRange;
  ... // do some work
  return ReturnSuccess.Success;
}
```
How we call the procedure:
```C#
var e = this.DisplayLine(100);
if (e == ReturnSuccess.ArgumentOutOfRange)
  ... // Take the error in consideration.
```
### Example with a customized error code set
The error set:
```C#
enum MyErrorCodes
{ NotInitialized, Success, StringIsEmpty, StringIsNull }
```
A function that returns errors from the set:
```C#
[ReturnCodes((int)MyErrorCodes.StringIsEmpty, (int)MyErrorCodes.StringIsNull)]
static ReturnNonNull<String, MyErrorCodes> CertifiesStringIsDefined(string p)
{
  if (p == null)
    return new ReturnNonNull<string, MyErrorCodes>(MyErrorCodes.StringIsNull);
  if (p == string.Empty)
    return new ReturnNonNull<string, MyErrorCodes>(MyErrorCodes.StringIsEmpty);
  return new ReturnNonNull<string, MyErrorCodes>(p);
}
```
A function call:
```C#
var s = CertifiesStringIsDefined("good");
switch (s.ErrorCode)
{
  case MyErrorCodes.StringIsEmpty:
    s = "!Empty!"; break;
  case MyErrorCodes.StringIsNull:
    s = "!Null!"; break;
}
```
### Additional structures
- The structure ``ReturnError`` contains both a ReturnSuccess and an _Exception_.  
  It can be used when a function returns a detailed exception message: for example "Write access to file **x.y** is denied because it is open by process **z**".

---

[Go back to the table of contents](../readme.md)

---
Copyright (c) [Christophe Bertrand](https://chrisbertrand.net)  
https://github.com/ChrisBertrandDotNet/CB-Helpers

