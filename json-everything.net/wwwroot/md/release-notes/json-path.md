# [0.4.0](https://github.com/gregsdennis/json-everything/pull/372) {#release-path-0.4.0}

- Updated function definition
  - list parameter sets to support type checking and overloads rather than a minimum and maximum parameter count
  - add return type to support type checking within expressions
- fixes around `TryParse()` to ensure that it doesn't throw exceptions (at least not from this code)
- more fixes based on [expanded tests](https://github.com/jsonpath-standard/jsonpath-compliance-test-suite/pull/12) in the compliance test suite

# [0.3.3](https://github.com/gregsdennis/json-everything/pull/367) {#release-path-0.3.3}

I added a bunch of tests to the JSON Path test suite and found a few bugs in filter expressions. This release fixes those bugs.

- Some copy/paste/forgot-to-edit issues with `<`/`<=`/`>`/`>=`
- Not extracting string values correctly in comparisons
- Not treating `null` values correctly when evaluating local paths (e.g. `@.a`)

# [0.3.2](https://github.com/gregsdennis/json-everything/pull/366) {#release-path-0.3.2}

[#364](https://github.com/gregsdennis/json-everything/issues/364) - `count()`, `match()`, and `search()` functions are not registered and so cannot be parsed.  Thanks to [@amis92](https://github.com/amis92) for finding and reporting this.

# [0.3.1](https://github.com/gregsdennis/json-everything/commit/3248441c00dd97cf4a8c66fff6b2462682a3bb8e) {#release-path-0.3.1}

Fixed an issue in `search` and `match` function evaluation.

# [0.3.0](https://github.com/gregsdennis/json-everything/pull/362) {#release-path-0.3.0}

[#281](https://github.com/gregsdennis/json-everything/issues/281) - Complete rewrite to support `JsonNode` and align with specification.
[#351](https://github.com/gregsdennis/json-everything/issues/351) - Fixes issues parsing `$.['foo']` (basically removes support since the `.[]` syntax is invalid).

Changes of note:

- Expression functions are supported and custom functions can be created.
- `.length` has been replaced by `length()` function.
- Available functions:
  - `length()` - Length of values (objects, arrays, & strings)
  - `count()` - Length of nodelists
  - `match()` - Precise regex matching (uses implicit anchoring)
  - `search()` - General regex matching (does not use implicit anchoring, more like JSON Schema's `pattern` keyword)
- Expression engine is more robust.
- No context object needed for evaluation.
- Removed experimental feature of `$ref` resolution since it's not in the spec.  (See the JSON Referencing proposals from the JSON Schema group at https://github.com/json-schema-org/referencing)

# [0.2.1](https://github.com/gregsdennis/json-everything/commit/1ba018b86c14dc94c55206062250735356491460) {#release-path-0.2.1}

[#243](https://github.com/gregsdennis/json-everything/pull/243) - Updated System.Text.Json to version 6.

Fixed issue with parsing recursive descents into quoted-index property names, e.g. `$..['foo']`.  Previous versions added two recursive descent selectors, which resulted in duplicate entries in the results.

# [0.2.0](https://github.com/gregsdennis/json-everything/pull/242) {#release-path-0.2.0}

Result locations should be expressed in Normalized Paths, not JSON Pointers.

# [0.1.14](https://github.com/gregsdennis/json-everything/pull/182) {#release-path-0.1.14}

Updated JsonPointer.Net to v2.0.0.  Please see [release notes](./json-pointer.md) for that library as it contains breaking changes.

# [0.1.13](https://github.com/gregsdennis/json-everything/pull/161) {#release-path-0.1.13}

Added experimential support for referencing data in secondary documents.

# [0.1.12](https://github.com/gregsdennis/json-everything/pull/161) {#release-path-0.1.12}

Added support for unary-not operator, e.g.

# [0.1.11](https://github.com/gregsdennis/json-everything/pull/159) {#release-path-0.1.11}

- Parenthetical support for nested expressions.
- Better stringifying.

# [0.1.10](https://github.com/gregsdennis/json-everything/pull/158) {#release-path-0.1.10}

Added support for comparisons against all JSON literals in expressions.

# [0.1.9](https://github.com/gregsdennis/json-everything/pull/155) {#release-path-0.1.9}

Better error reporting during parsing.

# [0.1.8](https://github.com/gregsdennis/json-everything/pull/150) {#release-path-0.1.8}

[#151](https://github.com/gregsdennis/json-everything/pull/151) - Parsing expressions with numeric values that contain `9` fails.

# [0.1.7](https://github.com/gregsdennis/json-everything/pull/133) {#release-path-0.1.7}

[#132](https://github.com/gregsdennis/json-everything/pull/132) - Fixed some memory management issues around `JsonDocument` and `JsonElement`.  Thanks to [@ddunkin](https://github.com/ddunkin) for finding and fixing these.

# [0.1.6](https://github.com/gregsdennis/json-everything/pull/122) {#release-path-0.1.6}

Updated expression parsing to handle whitespace better.

# [0.1.5](https://github.com/gregsdennis/json-everything/pull/75) {#release-path-0.1.5}

Added support for nullable reference types.

# [0.1.4](https://github.com/gregsdennis/json-everything/pull/61) {#release-path-0.1.4}

Signed the DLL for strong name compatibility.

# 0.1.3 (no PR) {#release-path-0.1.3}

Fixed issue with parsing property path segments that end in `z`, `Z`, and `9` (e.g. `$.baz`).

# [0.1.2](https://github.com/gregsdennis/json-everything/pull/45) {#release-path-0.1.2}

Added debug symbols to package.  No functional change.

# [0.1.1](https://github.com/gregsdennis/json-everything/pull/13) {#release-path-0.1.1}

Better single-quote support.

# 0.1.0 {#release-path-0.1.0}

Initial release.