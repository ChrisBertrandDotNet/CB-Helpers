# CB.Data.Comparer.cs

## class `ComparerAndOperators<>`

Implements common comparisons for your class: `== , !=, <, <=, >, >=)`.

Implements `IComparer<T>`, `IEqualityComparer<T>`, `IComparable<T>`.

You just have to implement `Equals` and `CompareTo`.

Example of usage:
```C#
class Int32Comparer : CB.Data.ComparerAndOperators<Int32Comparer>
{
  public readonly int Value;

  public Int32Comparer(int value)
  {
    this.Value = value;
  }

  public override int CompareTo(Int32Comparer other)
  {
    return this.Value.CompareTo(other.Value);
  }

  public override bool Equals(Int32Comparer other)
  {
    return this.Value == other.Value;
  }

  [Test]
  static void ComparerAndOperators_Test()
  {
    var a5 = new Int32Comparer(5);
    var b5 = new Int32Comparer(5);
    var deux = new Int32Comparer(2);
    var dix = new Int32Comparer(10);
    var zéro = new Int32Comparer(0);
    Tester.CheckTruth(a5 > zéro);
    Tester.CheckTruth(a5 >= zéro);
    Tester.CheckTruth(a5 >= b5);
    Tester.CheckTruth(a5 <= b5);
    Tester.CheckIfFalse(a5 <= zéro);
    Tester.CheckIfFalse(a5 < zéro);
    Tester.CheckTruth(a5 == b5);
    Tester.CheckTruth(a5 != deux);
    Tester.CheckTruth(a5.Equals(b5));
    Tester.CheckTruth(a5.CompareTo(deux) == 1);
    Tester.CheckTruth(a5.CompareTo(b5) == 0);
    Tester.CheckTruth(a5.CompareTo(dix) == -1);
  }
}
```
---

[Go back to the table of contents](../readme.md)

---
Copyright (c) [Christophe Bertrand](https://chrisbertrand.net)  
https://github.com/ChrisBertrandDotNet/CB-Helpers
