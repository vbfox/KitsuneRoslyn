Test Diagnostics
================

This project is my [Roslyn](https://roslyn.codeplex.com/) test project for diagnostics and code fixes.

The diagnostics actually implemented are :

* No `String.Empty` (Use `""` instead) **With Fix**
* No `new Guid()` (Use `Guid.Empty` instead) **With Fix**
* Don't use `String.Concat` where a single string or `string.Format` can be used
