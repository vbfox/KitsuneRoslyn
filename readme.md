Test Diagnostics
================

This project is my [Roslyn](https://roslyn.codeplex.com/) test project for C# diagnostics and
code fixes.

Avoid `new Guid()`
------------------

This rule matches calls to `new Guid()` and the associated code fix suggest to replace it with
a call to `Guid.Empty`.

The rationale is that `Guid.Empty` is more expressive than the default constructor to represent
a GUID with only zeroes inside.

![Avoid new Guid()][NoNewGuid]

**Default level**: Warning

Avoid `string.Empty`
--------------------

This rule matches calls to `string.Empty` and the associated code fix suggest to replace it with
the equivalent literal `""`.

The rationale is that there is no real reason for using `string.Empty`, the potential performance
hit is insignificant and it is just a more verbose form of writing `""`.

![Avoid String.Empty][NoStringEmpty]

*Note (1)*: It conflicts with **UseStringEmptyForEmptyStrings** from
[StyleCop](https://stylecop.codeplex.com/).

*Note (2)*: The performance consequence is that `""` and `string.Empty` aren't the same string
(See [Eric Lippert's blog][LippertStringInterning] for details) and it close the door to some optimization.
But even [`string.IsNullOrEmpty`][IsNullOrEmptyReferenceSource] in the framework don't take advantage
of the potential optimization.

**Default level**: Warning

Avoid `string.concat`
---------------------

This rule matches calls to `string.Concat` that provide strings or objects directly as argument.

The associated code fix suggest to replace it with either a single string if possible or with a call to
`string.Format`.

![Avoid string.Concat replace with string][NoStringConcatString]

![Avoid string.Concat replace with string.Format][NoStringConcatFormat]

**Default level**: Warning

Avoid string concatenation
--------------------------

This rule matches concatenation of strings using the `+` operator.

The associated code fix suggest to replace it with either a single string if possible or with a call to
`string.Format`.

![Avoid string.Concat replace with string][StringConcatOperatorString]

![Avoid string.Concat replace with string.Format][StringConcatOperatorFormat]

**Default level**: Hidden




[LippertStringInterning]: http://blogs.msdn.com/b/ericlippert/archive/2009/09/28/string-interning-and-string-empty.aspx
[IsNullOrEmptyReferenceSource]: http://referencesource.microsoft.com/#mscorlib/system/string.cs.html#23a8597f842071f4
[NoNewGuid]: https://github.com/vbfox/RoslynDiagnostics/raw/master/ReadmePictures/NoNewGuid.png
[NoStringEmpty]: https://github.com/vbfox/RoslynDiagnostics/raw/master/ReadmePictures/NoStringEmpty.png
[NoStringConcatString]: https://github.com/vbfox/RoslynDiagnostics/raw/master/ReadmePictures/NoStringConcatString.png
[NoStringConcatFormat]: https://github.com/vbfox/RoslynDiagnostics/raw/master/ReadmePictures/NoStringConcatFormat.png
[StringConcatOperatorString]: https://github.com/vbfox/RoslynDiagnostics/raw/master/ReadmePictures/StringConcatOperatorString.png
[StringConcatOperatorFormat]: https://github.com/vbfox/RoslynDiagnostics/raw/master/ReadmePictures/StringConcatOperatorFormat.png