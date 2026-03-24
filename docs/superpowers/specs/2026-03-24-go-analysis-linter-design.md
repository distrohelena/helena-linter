# Go Analysis Helena Linter Design

## Goal

Add a new `go/` package to `helena-linter` that delivers Helena readability and control-flow rules for Go projects through the standard `go/analysis` API.

The Go package should feel like the Go equivalent of the existing Helena packages:

- TypeScript: ESLint flat-config package
- C#: Roslyn analyzer package
- Java: Checkstyle extension package
- Go: `go/analysis` analyzer package

The initial Go release should expose analyzers only. It should not ship a standalone CLI.

## Goals

- Create a Go-native analyzer package built on `golang.org/x/tools/go/analysis`.
- Mirror the current Helena rule set wherever Go syntax supports the same intent.
- Provide auto-fixes through `SuggestedFix` where the rewrite is mechanically safe.
- Keep rules modular so consumers can run one analyzer or the full Helena bundle.
- Keep the public package shape easy to embed into future Go lint drivers.

## Non-Goals

- A standalone command-line binary in the first release.
- A `golangci-lint` plugin in the first release.
- Exact one-to-one syntax parity with languages whose grammar differs fundamentally from Go.
- Unsafe or speculative auto-fixes that require semantic guesswork.

## Repository Layout

The repository root should gain a new top-level `go/` folder.

Proposed structure:

- `go/`
  - `go.mod`
  - `README.md`
  - `bundle/`
    - exports the full Helena analyzer set
  - `analyzers/`
    - one analyzer per Helena rule
  - `internal/`
    - shared AST navigation, blank-line analysis, control-flow exit detection, condition comparison, and edit helpers
  - `testdata/`
    - `analysistest` fixtures and fix-output samples

This keeps the Go implementation isolated like the existing `typescript/`, `csharp/`, and `java/` packages.

## Public Package Shape

The Go package should publish a normal Go module that exposes:

- one `*analysis.Analyzer` per supported Helena rule
- a bundled analyzer list for consumers who want the default Helena set
- package documentation and examples in `go/README.md`

Consumers should be able to import the analyzers directly into their own `multichecker` or analyzer runner.

## Analyzer Architecture

The recommended structure is one analyzer per rule plus a bundle package.

Expected analyzer layout:

- `analyzers/controlblockfollowingspacing`
- `analyzers/declarationleadingspacing`
- `analyzers/declarationspacing`
- `analyzers/earlyreturn`
- `analyzers/exitspacing`
- `analyzers/ifelseifchain`
- `analyzers/iffollowingspacing`
- `analyzers/ifleadingspacing`
- `analyzers/multilineblocklayout`
- `analyzers/redundantelseif`

Bundle layer:

- `bundle`
  - exports something like `Recommended()` or `All()` returning `[]*analysis.Analyzer`

Shared support should live under `internal/` and stay rule-agnostic:

- statement sibling discovery
- line and blank-line analysis
- exact edit construction from token positions
- control-flow exit detection
- complementary condition comparison
- fix application helpers for block/statement rewrites

## Rule Mapping

The Go package should mirror Helena’s intent where Go syntax allows it.

### Supported Rules

- `control-block-following-spacing`
  - require a blank line after completed `for`, `switch`, `type switch`, and `select` blocks before the next sibling statement
- `declaration-leading-spacing`
  - require a blank line before local declarations that follow non-declaration statements
- `declaration-spacing`
  - require a blank line after local declarations before the next non-declaration sibling statement
- `early-return`
  - rewrite wrapped happy-path `if` shapes into guard clauses where the control flow is exact and safe
- `exit-spacing`
  - require a blank line before `return`, `break`, `continue`, and `goto`
- `if-else-if-chain`
  - fold a sibling `if` into `else if` when the earlier branch definitely exits
- `if-following-spacing`
  - require a blank line after a completed `if` or `if / else if / else` chain before the next sibling statement
- `if-leading-spacing`
  - require a blank line before an `if` statement when it follows another sibling statement
- `multiline-block-layout`
  - require non-empty code blocks to span multiple lines while allowing empty blocks on one line
- `redundant-else-if`
  - rewrite exact complementary `else if` conditions to plain `else`

### Omitted Rule

- `control-body-braces`
  - omitted in Go because the language already requires braces for control-flow bodies

The omission should be documented clearly in the Go README so consumers understand that this is intentional, not missing work.

## Auto-Fix Strategy

The Go package should prefer auto-fixes when the rewrite is deterministic and locally scoped.

Good candidates for direct `SuggestedFix` support:

- blank-line insertion and removal
- `if` to `else if` folding
- complementary `else if` to `else`
- multiline block expansion
- guard-clause rewrites when the pattern is exact

Fixes must:

- preserve existing comments
- preserve indentation style already present in the file
- avoid touching unrelated siblings
- avoid semantic rewrites unless the pattern is proven safe from syntax and local control flow

If a rule can detect a pattern but not rewrite it safely, the analyzer may report a diagnostic without a fix, but the design target is fix-first wherever practical.

## Diagnostics

Diagnostics should use stable Helena-style naming and messages so the rule set is understandable across languages.

Recommended approach:

- expose Go analyzer names matching the Helena rule names
- keep diagnostic messages action-oriented and specific
- keep messages consistent with the TypeScript, C#, and Java package descriptions where possible

## Testing Strategy

The Go package should use `analysistest` from `golang.org/x/tools/go/analysis/analysistest`.

Each rule should have:

- valid fixture files
- invalid fixture files with expected diagnostics
- fix verification for rules that emit `SuggestedFix`
- focused edge cases for comments, nested blocks, and mixed statement sequences

The test corpus should cover:

- declaration groups and single declarations
- `if / else if / else` chains
- `for`, `switch`, `type switch`, and `select`
- exit statements
- exact condition complements for `redundant-else-if`
- wrapped happy-path control flow for `early-return`

The package should also include a bundle-level smoke test that runs the recommended analyzer set against sample Go code.

## Documentation

The `go/README.md` should cover:

- module purpose and install/import instructions
- how to consume a single analyzer
- how to consume the bundled Helena analyzer set
- supported rules
- omitted rules and why
- per-rule `bad` / `good` examples, matching the style now used in the TypeScript, C#, and Java READMEs

The root `README.md` should also gain a Go package entry after implementation.

## Verification

Before the Go package is considered complete, the following should pass from `go/`:

- `go test ./...`
- any package build or vet-style smoke checks needed for sample integration

Success also requires:

- a clear Go package README
- modular analyzers with shared helper infrastructure
- bundle export for the full Helena analyzer set
- documented omission of `control-body-braces`

## Future Extension Path

Once the analyzer package is stable, the next likely additions are distribution layers rather than new rules:

1. a small example `multichecker` runner for local testing
2. optional `golangci-lint` integration guidance
3. release and versioning documentation for the Go module

Those should build on the analyzer package rather than replace it.
