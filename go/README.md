# Go Helena Linter

This module provides Helena's Go analysis package through `golang.org/x/tools/go/analysis`.
It is analyzer-only: there is no standalone CLI, no `golangci-lint` integration layer, and no
runtime outside normal Go analyzer consumers.

## Usage

Import a single analyzer when you want a focused runner:

```go
import (
	"github.com/distrohelena/helena-linter/go/analyzers/earlyreturn"
	"golang.org/x/tools/go/analysis/singlechecker"
)

func main() {
	singlechecker.Main(earlyreturn.Analyzer)
}
```

Import the bundle when you want the recommended Helena rule set:

```go
import (
	"github.com/distrohelena/helena-linter/go/bundle"
	"golang.org/x/tools/go/analysis/multichecker"
)

func main() {
	multichecker.Main(bundle.Recommended()...)
}
```

## Package Shape

- `bundle/` exposes the default Helena analyzer bundle
- `analyzers/` contains one analyzer package per supported Helena rule
- `internal/diag/` contains stable Helena rule identifiers and diagnostic helpers
- `internal/testx/` contains test helpers for analyzer suggested fixes
- `testdata/` holds `analysistest` fixtures and fix-output samples

## Supported Rules

The bundled analyzer set covers the following Go rules:

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

`control-body-braces` is intentionally omitted from the Go module because Go already requires
braces for control-flow bodies.

## Rule Examples

Examples below show the shape each rule expects, not every supported edge case.

### `control-block-following-spacing`

Requires a blank line after a completed `for`, `switch`, `type switch`, or `select` statement
before the next sibling statement.

Bad:

```go
for _, item := range items {
	process(item)
}
finish()
```

Good:

```go
for _, item := range items {
	process(item)
}

finish()
```

### `declaration-leading-spacing`

Requires a blank line before a declaration statement when the previous sibling statement is not a
declaration.

Bad:

```go
logStart()
var result = compute()
```

Good:

```go
logStart()

var result = compute()
```

### `declaration-spacing`

Requires a blank line after a declaration statement before the next sibling statement.

Bad:

```go
var result = compute()
render(result)
```

Good:

```go
var result = compute()

render(result)
```

### `early-return`

Rewrites wrapped happy-path `if` blocks and simple `if / else` shapes into guard clauses when the
rewrite is safe.

Bad:

```go
if isValid(user) {
	save(user)
	return true
}

return false
```

Good:

```go
if !isValid(user) {
	return false
}

save(user)
return true
```

### `exit-spacing`

Requires a blank line before `return`, `break`, `continue`, and `goto`.

Bad:

```go
updateState()
return value
```

Good:

```go
updateState()

return value
```

### `if-else-if-chain`

Flags sibling `if` branches that should be written as `else if`.

Bad:

```go
if primary {
	return "a"
}

if secondary {
	return "b"
}
```

Good:

```go
if primary {
	return "a"
} else if secondary {
	return "b"
}
```

### `if-following-spacing`

Requires a blank line after a completed `if` or `if / else if / else` chain before the next
sibling statement.

Bad:

```go
if ready {
	run()
}
cleanup()
```

Good:

```go
if ready {
	run()
}

cleanup()
```

### `if-leading-spacing`

Requires a blank line before an `if` statement when it has a previous sibling statement.

Bad:

```go
prepare()
if ready {
	run()
}
```

Good:

```go
prepare()

if ready {
	run()
}
```

### `multiline-block-layout`

Requires non-empty code blocks to break across lines. Empty blocks may remain on one line, and
object, collection, and anonymous initializer braces are excluded.

Bad:

```go
if ready { run() }
```

Good:

```go
if ready {
	run()
}
```

### `redundant-else-if`

Rewrites exact complementary `else if` branches to a plain `else`.

Bad:

```go
if value == nil {
	handleMissing()
} else if value != nil {
	handleValue(value)
}
```

Good:

```go
if value == nil {
	handleMissing()
} else {
	handleValue(value)
}
```

## Verification

Run the Go package tests from the `go/` submodule directory:

```bash
cd go
go test ./...
```

