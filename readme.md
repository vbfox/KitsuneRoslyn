Test Diagnostics
================

This project is my [Roslyn](https://roslyn.codeplex.com/) test project for diagnostics and code fixes.

The diagnostics actually implemented are :

* No `String.Empty` (Use `""` instead) **With Fix**
* No `new Guid()` (Use `Guid.Empty` instead) **With Fix**
* Don't use `String.Concat` where a single string or `string.Format` can be used **With Fix**

Examples
--------

The following code :

```csharp
var str = String.Empty;
var guid = new Guid();
var singleString = string.Concat("Foo", "Bar");
var formatString = string.Concat("Foo", str, "Bar");
```

become after replacing everything :
```csharp
var str = "";
var guid = Guid.Empty;
var singleString = "FooBar";
var formatString = string.Format("Foo{0}Bar", str);
```