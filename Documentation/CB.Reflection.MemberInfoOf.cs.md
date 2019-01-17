# CB.Reflection.MemberInfoOf.cs

Retreives the `System.Reflection.MemberInfo` of a member, parameter, etc..

Note: These functions use reflection, therefore they can be slow.

## class `MemberInfoOf`

Gets the name of an instance or static function:
```C#
void InstanceFunction()
{
  var thisInstanceFunctionName = MemberInfoOf.Function((Action)this.InstanceFunction).Name;
}
static double StaticFunction(int a)
{
  var staticFunctionName = MemberInfoOf.Function((Func<int, double>)StaticFunction).Name;
  return a;
}
```
Gets  information about an instance field:
```C#
public class TestClass
{
  int Field1;

  void test()
  {
    var fieldType = MemberInfoOf.InstanceField<TestClass, int>(a => a.Field1).FieldType;
    var fieldName = MemberInfoOf.InstanceField<TestClass, int>(a => a.Field1).Name;
  }
}
```
Gets the MemberInfo of an instance member (field or property).
```C#
var instanceFieldName = MemberInfoOf.InstanceFieldOrProperty<TestClass, int> (u => u.Field1).Name;
```
```
Gets the MemberInfo of an instance (not static) function:
```C#
var instanceFunctionName = MemberInfoOf.InstanceFunction<TestClass> (u => (Action)u.InstanceFunction);
```
Note: you do not need an actual instance.

Gets the information about an instance property:
```C#
public class TestClass
{
  int Property1 { get; }

  void test()
  {
    var propertyName = MemberInfoOf.InstanceProperty<TestClass, int>(a => a.Property1).Name;
    var propertyType = MemberInfoOf.InstanceProperty<TestClass, int>(a => a.Property1).PropertyType;
  }
}
Gets the name of a function parameter:
```C#
static void Main(string[] args)
{
  var argsName = MemberInfoOf.Parameter(() => args).Name;
}
```
Gets the MemberInfo of a static data member (field or property):
```C#
public class TestClass //Test_MemberInfoOf
{
  static int StaticField1;

  void test()
  {
    var staticFieldName = MemberInfoOf.StaticFieldOrProperty(() => TestClass.StaticField1).Name;
  }
}
```
A complete example:
```C#
public class TestClass
{
  int Field1;
  int Property1 { get; }
  static int StaticField1;

  void test()
  {
    var fieldName = MemberInfoOf.InstanceField<TestClass, int>(a => a.Field1).Name;
    var fieldType = MemberInfoOf.InstanceField<TestClass, int>(a => a.Field1).FieldType;

    var propertyName = MemberInfoOf.InstanceProperty<TestClass, int>(a => a.Property1).Name;
    var propertyType = MemberInfoOf.InstanceProperty<TestClass, int>(a => a.Property1).PropertyType;

    var instanceFieldName = MemberInfoOf.InstanceFieldOrProperty<TestClass, int> (u => u.Field1).Name;

    var thisInstanceFunctionName = MemberInfoOf.Function((Action)this.InstanceFunction).Name;
    var staticFunctionName = MemberInfoOf.Function((Func<int, double>)TestClass.StaticFunction).Name;

    var instanceFunctionName = MemberInfoOf.InstanceFunction<TestClass> (u => (Action)u.InstanceFunction);

    var staticFieldName = MemberInfoOf.StaticFieldOrProperty(() => TestClass.StaticField1).Name;
  }

  static void Main(string[] args)
  {
    var argsName = MemberInfoOf.Parameter(() => args);
  }

  void InstanceFunction()
  {
    
  }

  static double StaticFunction(int a)
  {
    return a;
  }

}
```

---

[Go back to the table of contents](../readme.md)

---
Copyright (c) [Christophe Bertrand](https://chrisbertrand.net)  
https://github.com/ChrisBertrandDotNet/CB-Helpers
