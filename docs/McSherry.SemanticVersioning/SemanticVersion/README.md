# `SemanticVersion` class

```c#
[Serializable]
[CLSCompliant(true)]
public sealed class SemanticVersion : IEquatable<SemanticVersion>,  
                                      IComparable<SemanticVersion>,  
                                      IFormattable
```

**Namespace:** [McSherry.SemanticVersioning][1]  
**Minimum Version:** 1.0.0

Represents an immutable [Semantic Version][2]. This class cannot be
inherited.

[1]: /docs/McSherry.SemanticVersioning/
[2]: http://semver.org


## Constructors

- **[SemanticVersion(Int32, Int32)][3]**  
  Creates a new [SemanticVersion][7] using the provided version components,
  with [Patch][8] set to zero.
  
- **[SemanticVersion(Int32, Int32, Int32)][4]**  
  Creates a new [SemanticVersion][7] using the provided version components.
  
- **[SemanticVersion(Int32, Int32, Int32, IEnumerable(string))][5]**  
  Creates a new [SemanticVersion][7] using the provided version components
  and pre-release identifiers.
  
- **[SemanticVersion(Int32, Int32, Int32, IEnumerable(string), IEnumerable(string))][6]**  
  Creates a new [SemanticVersion][7] using the provided version components,
  pre-release identifiers, and metadata items.

[3]: ./ctor(Int32,Int32).md
[4]: ./ctor(Int32,Int32,Int32).md
[5]: ./ctor(Int32,Int32,Int32,IEnumerable(string)).md
[6]: ./ctor(Int32,Int32,Int32,IEnumerable(string),IEnumerable(string)).md

[7]: /docs/docs/McSherry.SemanticVersion/SemanticVersion/
[8]: /docs/docs/McSherry.SemanticVersion/SemanticVersion/Patch.md

## Static Fields

- [**MaxValue**][8A]  
  The highest-precedence SemanticVersion that can be represented. This field is read-only.
- [**MinValue**][8B]  
  The lowest-precedence SemanticVersion that can be represented. This field is read-only.

[8A]: ./MaxValue.md
[8B]: ./MinValue.md


## Instance Properties

- **[Major][9]**  
  The semantic version's major version component.
- **[Minor][10]**  
  The semantic version's minor version component.
- **[Patch][11]**  
  The semantic version's patch version component.
- **[Identifiers][12]**  
  The pre-release identifier components of the semantic version.
- **[Metadata][13]**  
  The build metadata components of the semantic version.
  
[9]:  ./Major.md
[10]: ./Minor.md
[11]: ./Patch.md
[12]: ./Identifiers.md
[13]: ./Metadata.md

## Static Properties

- **[MemoizationAgent][35]**  
  The cache to use to memoize the results of the 
  [SemanticVersion][7] parsing methods.
  
[35]: ./MemoizationAgent.md

## Instance Methods

- **[CompareTo(SemanticVersion)][14]**  
  Compares the current [SemanticVersion][7] with another version to
  determine relative precedence.
- **[CompatibleWith(SemanticVersion)][15]**  
  Determines whether the specified [SemanticVersion][7] is
  backwards-compatible with the current version.
- **[Deconstruct(out Int32, out Int32, out Int32)][A14]**  
  Deconstructs the current version.
- **[Deconstruct(out Int32, out Int32)][B14]**  
  Deconstructs the current version.
- **[Equals(Object)][16]**  
  Determines whether the specified object is equal to the current
  object.
- **[Equals(SemanticVersion)][17]**  
  Determines whether the specified [SemanticVersion][7] is equal to
  the current version.
- **[EquivalentTo(SemanticVersion)][18]**  
  Determines whether the specified [SemanticVersion][7] is equivalent
  to the current version.
- **[GetHashCode()][19]**  
  Returns the hash code for this instance.
- **[ToString()][20]**  
  Returns a string that represents the current [SemanticVersion][7].
- **[ToString(String)][21]**  
  Formats the value of the current [SemanticVersion][7] as specified.
  
### Explicit Interface Implementations

- **[`IFormattable` ToString(String, IFormatProvider)][22]**  
  Formats the value of the current [SemanticVersion][7] as specified.

[A14]: ./Deconstruct(Int32,Int32,Int32).md
[B14]: ./Deconstruct(Int32,Int32).md
[14]: ./CompareTo(SemanticVersion).md
[15]: ./CompatibleWith(SemanticVersion).md
[16]: ./Equals(Object).md
[17]: ./Equals(SemanticVersion).md
[18]: ./EquivalentTo(SemanticVersion).md
[19]: ./GetHashCode().md
[20]: ./ToString().md
[21]: ./ToString(String).md
[22]: ./IFormattable.ToString(String,IFormatProvider).md


## Static Methods

- [**Parse(String, ParseMode, out IEnumerator(char))**][A23]  
  Converts a string to a [SemanticVersion][7], taking into account a set
  of flags.
- **[Parse(String, ParseMode)][23]**  
  Converts a version string to a [SemanticVersion][7], taking into account
  a set of flags.
- **[Parse(String)][24]**  
  Converts a version string to a [SemanticVersion][7], only accepting the
  format given in the [Semantic Versioning specification][2].
- [**TryParse(String, ParseMode, out SemanticVersion, out IEnumerator(char))**][24A]  
  Attempts to convert a version string to a [SemanticVersion][7], taking
  into account a set of flags.
- **[TryParse(String, ParseMode, out SemanticVersion)][25]**  
  Attempts to convert a version string to a [SemanticVersion][7], taking
  into account a set of flags.
- **[TryParse(String, out SemanticVersion)][26]**  
  Attempts to convert a version string to a [SemanticVersion][7].

[A23]: ./Parse(String,ParseMode,IEnumerator(T)).md
[23]: ./Parse(String,ParseMode).md
[24]: ./Parse(String).md
[24A]: ./TryParse(String,ParseMode,SemanticVersion,IEnumerator(T)).md
[25]: ./TryParse(String,ParseMode,SemanticVersion).md
[26]: ./TryParse(String,SemanticVersion).md


## Operators

- **[Equality(SemanticVersion, SemanticVersion)][27]**  
  Determines whether the two specified [SemanticVersion][7]s are equal in
  value.
- **[Inequality(SemanticVersion, SemanticVersion)][28]**  
  Determines whether the two specified [SemanticVersion][7]s are not equal
  in value.
- **[GreaterThan(SemanticVersion, SemanticVersion)][29]**  
  Determines whether one [SemanticVersion][7] has greater precedence than
  another.
- **[LessThan(SemanticVersion, SemanticVersion)][30]**  
  Determines whether one [SemanticVersion][7] has lesser precedence than
  another.
- **[GreaterThanOrEqual(SemanticVersion, SemanticVersion)][31]**  
  Determines whether the precedence of one [SemanticVersion][7] is equal
  to or greater than the precedence of another.
- **[LessThanOrEqual(SemanticVersion, SemanticVersion)][32]**  
  Determines whether the precedence of one [SemanticVersion][7] is equal
  to or lesser than the precedence of another.
- **[Explicit(String to SemanticVersion)][33]**  
  Converts a version string to a [SemanticVersion][7], with all
  [ParseMode][34] modifiers active.

[27]: ./op_Equality(SemanticVersion,SemanticVersion).md
[28]: ./op_Inequality(SemanticVersion,SemanticVersion).md
[29]: ./op_GreaterThan(SemanticVersion,SemanticVersion).md
[30]: ./op_LessThan(SemanticVersion,SemanticVersion).md
[31]: ./op_GreaterThanOrEqual(SemanticVersion,SemanticVersion).md
[32]: ./op_LessThanOrEqual(SemanticVersion,SemanticVersion).md
[33]: ./op_Explicit(String_to_SemanticVersion).md

[34]: /docs/McSherry.SemanticVersioning/ParseMode.md


## Remarks

This class represents a Semantic Version compliant with version 2.0.0 of
the Semantic Versioning specification.

Although the specification itself imposes no limit on version numbers, this
class has the following limitations:

- The major, minor, and patch versions are represented using an `Int32`, and
  so each component's maximum value is 2,147,483,647.