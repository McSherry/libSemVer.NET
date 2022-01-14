# `VersionRange.CompareTo(SemanticVersion)` method

```c#
public int CompareTo(
    SemanticVersion semver
    )
```

**Namespace:** [`McSherry.SemanticVersioning.Ranges`][1]  
**Minimum version:** 1.4.0
**Implements:** `System.IComparable<SemanticVersion>.CompareTo`

[1]: ../

Determines whether the specified [SemanticVersion][2] is outside the bounds of the current version range.

[2]: ../SemanticVersion


### Parameters

- **`semver`**  
  **Type:** [`McSherry.SemanticVersioning.SemanticVersion`][2]  
  The [SemanticVersion][2] to compare to the current version range.

### Return Value

**Type:** `System.Int32`

| Return value      | Condition                                                    |
| ----------------- | ------------------------------------------------------------ |
| Greater than zero | When _`semver`_ has higher precedence than all versions that satisfy the current version range. |
| Zero              | When _`semver`_ satisfies the current version range, is neither greater than nor less than all versions that satisfy the current version range, or is `null`. |
| Less than zero    | When _`semver`_ has lower precedence than all versions that satisfy the current version range. |