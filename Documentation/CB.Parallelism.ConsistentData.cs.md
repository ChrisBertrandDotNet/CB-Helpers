# CB.Parallelism.ConsistentData.cs

This is a way to manage data concurrently without any lock, thanks to consistency.

Here *consistency* means every class contains a data set we can't modify partly.
For example, you want to brighten a color. The color components (red, green and blue) can't be modified one after the other because a parallel task could see inconstancies at moments. We have to set all color components at once.

The solution here is to let a upper class store immutable (invariable) data sets. The only way to modify such a data is to copy, modify the copy then replace the set as a whole in the upper class.

In other words, I call *consistency* a variable where its data is a succession of immutable sets along the time.

Please note this concept works fine on small data sets but can be slow and resource-hungry on big sets (such as a big document in an instance).

## class `Consistent<T>`

### Example with a range

First, we declare a basic data class:
```C#
public class MyRangeData
{
  public readonly int Minimum, Maximum;
  public MyRangeData(int minimum, int maximum)
  {
    if (maximum < minimum)
      throw new InconsistentArgumentsException();
    Minimum = minimum; Maximum = maximum;
  }
}
```
Please note the data set (that contains `Minimum` and `Maximum`) is immutable.

Second, we declare the upper class that stores and manages the consistent variable data:
```C#
public class MyRange : Consistent<MyRangeData>
{
  public MyRange(int minimum, int maximum)
    : base(new MyRangeData(minimum, maximum))
  { }

  public MyRangeData EnlargeRange(int difference)
  {
    var current = this.Data;
    var enlarged = new MyRangeData(current.Minimum - difference, current.Maximum + difference);
    this.Data = enlarged;
    return enlarged;
  }
}
```
Please note the function `EnlargeRange` that modifies consistently the data.

Third, we use our class:
```C#
var range1 = new MyRange(100, 200);
var range2 = range1.EnlargeRange(10); // Gets the new (consistent) range: [ 90 ; 210 ].
```
As a result, `range2` is consistent, even in presence of parallel tasks that may have modified the data of `range1` during the process.

### Example with color components

First, we declare a basic data class:
```C#
public class MyColorData
{
  public readonly int Red, Green, Blue;

  public MyColorData(int red, int green, int blue)
  { Red = red; Green = green; Blue = blue; }
}
```
Please note this data is immutable.

Second, we declare the class that stores and manages the consistent variable data:
```C#
public class MyColor : Consistent<MyColorData>
{
  public MyColor(int red, int green, int blue)
    : base(new MyColorData(red, green, blue))
  { }

  public MyColorData Lighten(int difference)
  {
    var current = this.Data;
    var Lightened = new MyColorData(current.Red + difference, current.Green + difference, current.Blue + difference);
    this.Data = Lightened;
    return Lightened;
  }
}
```
Please note the function `Lighten` that modifies consistently the data.

Third, we use our class:
```C#
var color1 = new MyColor(100,100,100);
var color2 = color1.Lighten(50);
```
As a result, `color2` is consistent, even in presence of parallel tasks that may have modified the data of `color1` during the process.

## class `InconsistentArgumentsException`

You can use this exception when your function detects that the caller tries to set inconsistent data values.

Exemple:
```C#
public class MyRangeData
{
  public readonly int Minimum, Maximum;
  public MyRangeData(int minimum, int maximum)
  {
    if (maximum < minimum)
      throw new InconsistentArgumentsException();
    Minimum = minimum; Maximum = maximum;
  }
}
```
It is a good idea to check the consistency directly in the basic data class, rather than in the upper class.




---

[Go back to the table of contents](../readme.md)

---
Copyright (c) [Christophe Bertrand](https://chrisbertrand.net)  
https://github.com/ChrisBertrandDotNet/CB-Helpers
