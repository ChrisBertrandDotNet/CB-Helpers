# CB.Data.Certify.cs

Gives non-null certified references, plus related structures that certify their data is defined and valid.

## Concept

The fact is regular nullable references are a potential problem.  
In regular C# programming, we have to check all references a function returns, in order to ensure they are not null. Unfortunately it is very easy to forget to write this check, and the compiler does not warn or generate an error.

The concept of non-null references prevents this threat by eliminating the possibility of a null reference.

In addition to the non-null concept, this source code file contains related structures. All these structures certify their value is both defined and valid.

**NEW:** C# 8.0 now has its own non-null reference system, making the `NonNull` structure useless on this version.  
Anyway, `DefinedString`, `DefinedEnum` and `Bounded` are still useful.

## Examples

```C#
NonNull<TextInfo> GetTextInfo(NonNull<CultureInfo> culture)
{
  // No use to check the parameter: it can't be null.
  return culture.Value.TextInfo; // We certify the return value is defined.
}
DefinedString ToUpper(DefinedString s)
{
  // No use to check the parameter: it can't be null or String.Empty.
  return s.Value.ToUpper(); // We certify the return value is defined.
}
DefinedEnum<UnicodeCategory> GetUnicodeCategory(char c)
{
  return char.GetUnicodeCategory(c); // We certify the return value is a declared value of its enumeration.
}
...
var number1 = new Percentage(50); // this is a Bounded<Double>.
```

## Always initialize your structures [very important !]

You have to take extra precautions using these structures.
C# does not let us write both parametered constructors and a default (no-parameter) constructor in the same structure.  
So *you* have to make sure you never use an uninitialized structure.

The following note is for `NonNull<>`, but it is true of the other structures too.

- Never define a field of type `NonNull<>` without building an instance.
- Never initialize a local variable by `default(NonNull<>)`.

The compiler will not warn you. The error will appear on the next value reading only: in most cases, an exception will be thrown.

Example:
```C#
string UnintializedNonNull()
{
  var nullRef = default(NonNull<CultureInfo>); // Never write that !
  return nullRef.Value.Name; // Will throw an InvalidOperationException.
}
```

The reason why I did not declare these types as classes, if you want to know, is for optimization.

## structure `NonNull<T>`

Example of local variable or field:
```C#
NonNull<CultureInfo> culture = CultureInfo.CurrentUICulture; // implicitly transtyped. 'culture' is guaranted not to be null.
```

Example of function parameter:
```C#
NonNull<TextInfo> GetTextInfo(NonNull<CultureInfo> culture)
{
  // No use to check the parameter: it can't be null.
  return culture.Value.TextInfo; // We certify the return value is defined.
}
```

## structure `DefinedString`

Defines a reference to a String. This reference can't be null or String.Empty.

Example of local variable or field:
```C#
DefinedString name = CultureInfo.CurrentUICulture.Name; // implicitly transtyped. 'name' is guaranted not to be null or empty.
```

Example of function parameter:
```C#
DefinedString ToUpper(DefinedString s)
{
  // No use to check the parameter: it can't be null or String.Empty.
  return s.Value.ToUpper(); // We certify the return value is defined.
}
```
Note:  
If you want your reference can be String.Empty but not null, you can use `NonNull<string>` instead of `DefinedString`.

## structure `DefinedEnum<T>`

Here, a "declared enumeration value" is a value of an enumeration set that is explicitly declared.  
`DefinedEnum<T>` certifies such a value is a declared enumeration value.

Examples:
```C#
DefinedEnum<System.PlatformID> Windows = System.PlatformID.Win32NT; // ok, this value is declared in its enumeration.
DefinedEnum<System.PlatformID> MyOwnSystem = (System.PlatformID) 1000; // Wrong ! That will throw an ArgumentException.
```

### What about `[Flags]` ?

`System.FlagsAttribute` indicates that an enumeration can be treated as a bit field; that is, a set of flags.

A function let you know if a value is unique or is the addition of various values:
`IsDeclaredAsAUniqueValue()`.

## Bounded values

You can create a type that will certify values are in the right range.  
Commonly used with numbers, it can be applied to any value type that implements both `IComparable<T>` and `IEquatable<T>`.

### class `Bounded<T>`

The lower and upper bounds are called Minimum and Maximum. They form the range.  
Please note Minimum and Maximum can be included or excluded from the range. By default, they are included.

First we declare a bounded type:
```C#
/// <summary>
/// A number bounded in [ 0 ; 100 [
/// </summary>
internal class Percentage : Bounded<int>
{
  public override int Minimum => 0;
  public override int Maximum => 100;
  public override bool RangeIncludesTheMinimum => true;
  public override bool RangeIncludesTheMaximum => false;

  public Percentage(int value)
    : base(value)
  { }

  public static implicit operator Percentage(int value)
  {
    return new Percentage(value);
  }
}
```

Then we use it to declare values:

```C#
var number1 = new Percentage(50); // ok, 50 is between 0 and 100.
try { var number2 = new Percentage(-200); } // Wrong ! That will throw an ArgumentOutOfRangeException();
catch (ArgumentOutOfRangeException) { }
```

## If you don't want to declare a class

If you use only a few values, maybe you don't want to implement a complete class.  
You can then instanciate a range generator:
```C#
var angleRange = Bounded<double>.CreateASimpleRange(0, 360, true, false); // Serves as a range for angles.
```
then use the range to create certified values (*angles*, here):
```C#
var angle180 = angleRange.NewValue(180.0); // ok, that angle is in the range.
```
You can create values that may be certified or not:
```C#
var wrongAngle = angleRange.TryCreateValue(1000); // Wrong, the value is not in the range. The result is null.
```

If you want to certify both the value is defined (not null) and is in the range, you can use the type NonNull:
```C#
var certifiedAngle = angleRange.NewValueAsNonNull(200); // returned type: NonNull<IBounded<double>>.
```

## class `ExistingDirectory` in CB.Files.cs

Following the same idea of certified objects, but in a different code file, CB.Files.ExistingDirectory certifies the directory exists, at least at the moment of the instanciation of the class.

---

[Go back to the table of contents](../readme.md)

---
Copyright (c) [Christophe Bertrand](https://chrisbertrand.net)  
https://github.com/ChrisBertrandDotNet/CB-Helpers
