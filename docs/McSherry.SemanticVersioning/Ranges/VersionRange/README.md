# `VersionRange` class

```c#
[Serializable]
[CLSCompliant(true)]
public sealed class VersionRange : IComparable<SemanticVersion>
```

**Namespace:** [`McSherry.SemanticVersioning.Ranges`][1]  
**Minimum Version:** 1.1.0

Represents a range of acceptable versions. This class cannot be
inherited.

[1]: ../


## Constructors

- **[VersionRange(String)][2]**  
  Creates a version range from a string representing the range.
  
[2]: ./ctor(String).md

## Instance Properties

- [**MemoizationAgent**][2A]  
  The cache used to memoize the results of [VersionRange][2B] satisfaction
  methods.
- [**SynchronizationObject**][2C]  
  The object used in synchronising accesses to the [MemoizationAgent][2A].

[2A]: ./MemoizationAgent.md
[2B]:./
[2C]: ./SynchronizationObject.md

## Instance Methods

- **[SatisfiedBy(SemanticVersion)][3]**  
  Determines whether the current version range is satisfied by
  a specified [SemanticVersion][4].
- **[SatisfiedBy(SemanticVersion[])][5]**  
  Determines whether the current range is satisfied by all
  specified [SemanticVersion][4] instances.
- **[`IComparable<SemanticVersion>` CompareTo(SemanticVersion)][5A]**  
  Determines whether the specified [SemanticVersion][4] is outside the bounds of the current version range.
[3]: ./SatisfiedBy(SemanticVersion).md
[4]: ../SemanticVersion
[5]: ./SatisfiedBy(SemanticVersion).md
[5A]:./CompareTo(SemanticVersion).md


## Static Methods

- **[Parse(String)][6]**  
  Parses a version range from a string.
- **[TryParse(String, out VersionRange)][7]**  
  Attempts to parse a version range from a string.

[6]: ./Parse(String).md
[7]: ./TryParse(String,VersionRange).md


## Remarks

A version range specifies a set of semantic versions that are
acceptable, and is used to check that a given semantic version
fits within this set.

Version ranges use the [`node-semver`][8] syntax for ranges.
Specifically, ranges are based on the specification as it was
written for the [`v6.0.0`][9] release of `node-semver`.

The full basic and advanced range syntaxes are supported, but
there are minor differences in how 'X-ranges' are handled. Unlike
with `node-semver`, this class will reject ranges where the wildcard
(`x`, `X`, or `*` character) is followed by another version component
or by pre-release identifiers or metadata. As `node-semver` appears
to ignore anything that follows a wildcard, this has no real impact
on functionality.

In addition, for backwards compatibility, an empty version range
will be considered invalid. `node-semver` treats this as equivalent
to `*`.

[8]: https://github.com/npm/node-semver
[9]: https://github.com/npm/node-semver/blob/v6.0.0/README.md#ranges