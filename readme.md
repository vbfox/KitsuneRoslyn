Kitsune Roslyn
==============

> Kitsune (狐) is the Japanese word for fox. Foxes are a common subject of Japanese folklore; in English, kitsune refers to them in this context. Stories depict them as intelligent beings and as possessing magical abilities that increase with their age and wisdom. Foremost among these is the ability to assume human form.
>
> [Wikipedia](https://en.wikipedia.org/wiki/Kitsune)

Kitsune Roslyn is a compilation of Diagnostics for the C♯ language in Visual Studio 2015 made using [Roslyn](https://roslyn.codeplex.com/).    
The extension is [on the Gallery](https://visualstudiogallery.msdn.microsoft.com/59375d68-e57e-4deb-831d-eb49cf083c51) and can be installed directly in Visual Studio from the `Tools | Extensions and Updates…` menu.

Visual Studio 2015 support two different ways to allow code fixes in the light bulb:

* Diagnostics created by inheriting `DiagnosticAnalyzer`.
  They can appear visually as squiggles in the editor and even when no squiggles are used the lightbulb
  appear in the margin when the cursor is over a potential fix.  
* Refactorings created by inheriting `CodeRefactoringProvider`.
  They never appear visually by themselves without being invoked with `Ctrl+;` but they can use the
  current selection of the user (*Extract Method* wouldn't be possible without it)

The choice was made for this library to implement the majority of it as Diagnostics for ease of use but
it might be split latter in 2 versions if Diagnostics are slowing down Visual Studio too much.

Currently supported:

* [Conversion between the different type of properties](#conversion-between-the-different-type-of-properties) (Expression, initializer and statements)
* [Usage of `var` or type specified explicitly](#usage-of-var-or-type-specified-explicitly)
* [String formatting conversions](#string-formatting-conversions) (Between `+`, `string.Format` and `string.Concat`)
* [Usage of canonical Empty values](#usage-of-canonical-empty-values) (For `string` and `Guid`)

Conversion between the different type of properties
---------------------------------------------------

_Documentation in progress…_

Usage of `var` or type specified explicitly
-------------------------------------------

_Documentation in progress…_

String formatting conversions
-----------------------------

### Convert from `string.concat`

This rule matches calls to `string.Concat` that provide strings or objects directly as argument.

The associated code fix suggest to replace it with either a single string if possible or with a call to
`string.Format`.

![Avoid string.Concat replace with string][NoStringConcatString]

![Avoid string.Concat replace with string.Format][NoStringConcatFormat]

**Default level**: Warning

### Convert from string concatenation

This rule matches concatenation of strings using the `+` operator.

The associated code fix suggest to replace it with either a single string if possible or with a call to
`string.Format`.

![Avoid string.Concat replace with string][StringConcatOperatorString]

![Avoid string.Concat replace with string.Format][StringConcatOperatorFormat]

**Default level**: Hidden

Usage of canonical Empty values
-------------------------------

### Replace `new Guid()` with `Guid.Empty`

This rule matches calls to `new Guid()` and the associated code fix suggest to replace it with
a call to `Guid.Empty`.

The rationale is that `Guid.Empty` is more expressive than the default constructor to represent
a GUID with only zeroes inside.

![Avoid new Guid()][NoNewGuid]

**Default level**: Warning

### Replace `string.Empty` with `""`

This rule matches calls to `string.Empty` and the associated code fix suggest to replace it with
the equivalent literal `""`.

The rationale is that there is no real reason for using `string.Empty`, the potential performance
hit is insignificant and it is just a more verbose form of writing `""`.

![Avoid String.Empty][NoStringEmpty]

**Default level**: Warning

[NoNewGuid]: https://github.com/vbfox/RoslynDiagnostics/raw/master/ReadmePictures/NoNewGuid.png
[NoStringEmpty]: https://github.com/vbfox/RoslynDiagnostics/raw/master/ReadmePictures/NoStringEmpty.png
[NoStringConcatString]: https://github.com/vbfox/RoslynDiagnostics/raw/master/ReadmePictures/NoStringConcatString.png
[NoStringConcatFormat]: https://github.com/vbfox/RoslynDiagnostics/raw/master/ReadmePictures/NoStringConcatFormat.png
[StringConcatOperatorString]: https://github.com/vbfox/RoslynDiagnostics/raw/master/ReadmePictures/StringConcatOperatorString.png
[StringConcatOperatorFormat]: https://github.com/vbfox/RoslynDiagnostics/raw/master/ReadmePictures/StringConcatOperatorFormat.png