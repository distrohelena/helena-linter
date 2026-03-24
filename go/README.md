# Go Helena Linter

This module provides Helena's Go analysis package through `golang.org/x/tools/go/analysis`.
It is analyzer-only: there is no standalone CLI, no `golangci-lint` integration layer, and no
runtime outside normal Go analyzer consumers.

## Scope

The first release focuses on the readability and control-flow rules that make sense in Go. The
`control-body-braces` rule is intentionally omitted because Go already requires braces for control
flow bodies.

## Package Shape

- `bundle/` will expose the default Helena analyzer set; at scaffold time it only exports a
  placeholder `Recommended()` hook
- `analyzers/` will hold one analyzer per Helena rule
- `internal/` contains shared helpers for diagnostics, spacing, flow analysis, and tests
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

Run the Go package tests locally with:

```bash
go test ./...
```
