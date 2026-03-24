# Go Helena Linter

This module provides Helena's Go analysis package through `golang.org/x/tools/go/analysis`.
It is analyzer-only: there is no standalone CLI, no `golangci-lint` integration layer, and no
runtime outside normal Go analyzer consumers.

## Scope

The first release focuses on the readability and control-flow rules that make sense in Go. The
`control-body-braces` rule is intentionally omitted because Go already requires braces for control
flow bodies.

## Package Shape

- `bundle/` exposes the default Helena analyzer hook; at scaffold time `Recommended()` returns an
  empty analyzer list until later tasks add analyzers
- `analyzers/` is reserved for one analyzer per Helena rule
- `internal/diag/` contains diagnostic-rule helpers
- `internal/testx/` contains test helpers for analyzer suggested fixes
- `testdata/` will hold `analysistest` fixtures and fix-output samples

## Supported Rules

The bundled analyzer set is expected to grow to cover:

- `control-block-following-spacing`
- `declaration-leading-spacing`
- `declaration-spacing`
- `early-return`
- `exit-spacing`
- `if-else-if-chain`
- `if-following-spacing`
- `if-leading-spacing`
- `multiline-block-layout`
- `redundant-else-if`

## Verification

Run the Go package tests from the `go/` submodule directory:

```bash
cd go
go test ./...
```
