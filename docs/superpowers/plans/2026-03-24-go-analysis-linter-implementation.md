# Go Analysis Helena Linter Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build a new `go` analyzer package inside `helena-linter` that ships the Helena readability and control-flow rules for Go projects through `go/analysis`, with safe auto-fixes wherever the rewrite is deterministic.

**Architecture:** Add a standalone Go module under `go/` with one analyzer package per supported Helena rule, a shared `internal/` helper layer for AST traversal and edit construction, and a `bundle` package exporting the default Helena analyzer set. Keep rule implementations small and test-first, with reusable helpers for blank-line detection, condition comparison, control-flow exit analysis, and suggested-fix verification.

**Tech Stack:** Go 1.24.x or the repo’s chosen supported Go toolchain, `golang.org/x/tools/go/analysis`, `analysistest`, standard-library `go/ast` and `go/token`, Markdown docs.

**Prerequisite:** Install a Go toolchain before starting implementation. `go version` currently fails in this workspace because Go is not installed.

---

### Task 1: Scaffold The Go Module And Package Layout

**Files:**
- Create: `go/go.mod`
- Create: `go/README.md`
- Create: `go/bundle/all.go`
- Create: `go/internal/diag/rules.go`
- Create: `go/internal/testx/suggestedfix.go`
- Create: `go/analyzers/`
- Create: `go/testdata/src/`
- Modify: `README.md`
- Modify: `docs/superpowers/specs/2026-03-24-go-analysis-linter-design.md` only if naming drift is discovered during scaffolding

- [ ] **Step 1: Create the Go package directories**

Run:
```bash
cd /home/helena/dev/helena-linter
mkdir -p go/bundle
mkdir -p go/internal/diag
mkdir -p go/internal/testx
mkdir -p go/analyzers
mkdir -p go/testdata/src
```

Expected: the `go/` module directories exist.

- [ ] **Step 2: Initialize the Go module**

Write `go/go.mod` with:
- module path `github.com/distrohelena/helena-linter/go`
- the chosen Go version
- dependency on `golang.org/x/tools`

- [ ] **Step 3: Add the README skeleton and root README entry**

Write `go/README.md` with:
- package purpose
- analyzer-only scope
- note that `control-body-braces` is intentionally omitted in Go
- placeholder supported-rule list
- local verification command `go test ./...`

Modify `README.md` to add the new Go package alongside TypeScript, C#, and Java.

- [ ] **Step 4: Add the bundle and diagnostic ID skeleton**

Write:
- `go/bundle/all.go` exporting a placeholder `All()` or `Recommended()` function
- `go/internal/diag/rules.go` defining stable Helena rule names and diagnostic message helpers
- `go/internal/testx/suggestedfix.go` with a small helper for asserting suggested-fix output in analyzer tests

- [ ] **Step 5: Verify the empty Go module resolves**

Run:
```bash
cd /home/helena/dev/helena-linter/go
go test ./...
```

Expected: PASS with only the scaffold in place.

- [ ] **Step 6: Commit**

```bash
cd /home/helena/dev/helena-linter
git add go README.md docs/superpowers/plans/2026-03-24-go-analysis-linter-implementation.md
git commit -m "feat: scaffold go analysis linter package"
```

### Task 2: Add Shared AST, Spacing, Flow, And Condition Helpers

**Files:**
- Create: `go/internal/astx/statements.go`
- Create: `go/internal/textx/blanklines.go`
- Create: `go/internal/fixx/edits.go`
- Create: `go/internal/flow/exit.go`
- Create: `go/internal/exprx/complements.go`
- Create: `go/internal/textx/blanklines_test.go`
- Create: `go/internal/flow/exit_test.go`
- Create: `go/internal/exprx/complements_test.go`

- [ ] **Step 1: Write the helper tests first**

Add tests covering:
- sibling statement discovery within block statements
- blank-line detection between adjacent statements
- detection of definite exits for `return`, `break`, `continue`, and `goto`
- exact complementary conditions such as `value == nil` vs `value != nil` and `!flag` vs `flag`

- [ ] **Step 2: Run the helper tests to verify failure**

Run:
```bash
cd /home/helena/dev/helena-linter/go
go test ./internal/textx ./internal/flow ./internal/exprx
```

Expected: FAIL because the helper implementations do not exist yet.

- [ ] **Step 3: Implement the shared helper packages**

Implement:
- `astx` helpers for sibling statement access
- `textx` helpers for blank-line and line-range analysis
- `fixx` helpers for building replacement edits from token positions
- `flow` helpers for exact local exit detection
- `exprx` helpers for complementary condition comparison

- [ ] **Step 4: Run the helper tests again**

Run the same command as Step 2.

Expected: PASS.

- [ ] **Step 5: Commit**

```bash
cd /home/helena/dev/helena-linter
git add go
git commit -m "feat: add go analyzer helper infrastructure"
```

### Task 3: Implement Declaration Spacing Rules

**Files:**
- Create: `go/analyzers/declarationleadingspacing/analyzer.go`
- Create: `go/analyzers/declarationleadingspacing/analyzer_test.go`
- Create: `go/analyzers/declarationspacing/analyzer.go`
- Create: `go/analyzers/declarationspacing/analyzer_test.go`
- Create: `go/testdata/src/declarationleadingspacing/invalid.go`
- Create: `go/testdata/src/declarationleadingspacing/valid.go`
- Create: `go/testdata/src/declarationspacing/invalid.go`
- Create: `go/testdata/src/declarationspacing/valid.go`

- [ ] **Step 1: Write failing tests for `declaration-leading-spacing`**

Cover:
- a declaration immediately after a non-declaration statement
- already-spaced valid cases
- comment-preservation around the inserted blank line

- [ ] **Step 2: Write failing tests for `declaration-spacing`**

Cover:
- declarations immediately followed by non-declaration statements
- declaration groups
- already-spaced valid cases

- [ ] **Step 3: Run the declaration rule tests to verify failure**

Run:
```bash
cd /home/helena/dev/helena-linter/go
go test ./analyzers/declarationleadingspacing ./analyzers/declarationspacing
```

Expected: FAIL because the analyzers do not exist yet.

- [ ] **Step 4: Implement the two analyzers with suggested fixes**

Use `astx`, `textx`, and `fixx` to:
- detect adjacent statements in the same block
- distinguish declaration statements from non-declaration statements
- add exactly one blank line while preserving indentation and comments

- [ ] **Step 5: Run the declaration rule tests again**

Run the same command as Step 3.

Expected: PASS.

- [ ] **Step 6: Commit**

```bash
cd /home/helena/dev/helena-linter
git add go
git commit -m "feat: add go declaration spacing analyzers"
```

### Task 4: Implement If, Control-Block, And Exit Spacing Rules

**Files:**
- Create: `go/analyzers/ifleadingspacing/analyzer.go`
- Create: `go/analyzers/ifleadingspacing/analyzer_test.go`
- Create: `go/analyzers/iffollowingspacing/analyzer.go`
- Create: `go/analyzers/iffollowingspacing/analyzer_test.go`
- Create: `go/analyzers/controlblockfollowingspacing/analyzer.go`
- Create: `go/analyzers/controlblockfollowingspacing/analyzer_test.go`
- Create: `go/analyzers/exitspacing/analyzer.go`
- Create: `go/analyzers/exitspacing/analyzer_test.go`
- Create: `go/testdata/src/ifleadingspacing/invalid.go`
- Create: `go/testdata/src/ifleadingspacing/valid.go`
- Create: `go/testdata/src/iffollowingspacing/invalid.go`
- Create: `go/testdata/src/iffollowingspacing/valid.go`
- Create: `go/testdata/src/controlblockfollowingspacing/invalid.go`
- Create: `go/testdata/src/controlblockfollowingspacing/valid.go`
- Create: `go/testdata/src/exitspacing/invalid.go`
- Create: `go/testdata/src/exitspacing/valid.go`

- [ ] **Step 1: Write failing tests for `if-leading-spacing` and `if-following-spacing`**

Cover:
- `if` following another statement without a blank line
- `if / else if / else` chains followed by sibling statements without a blank line
- already-correct spacing

- [ ] **Step 2: Write failing tests for `control-block-following-spacing` and `exit-spacing`**

Cover:
- `for`, `switch`, `type switch`, and `select` followed immediately by another statement
- `return`, `break`, `continue`, and `goto` without a leading blank line
- valid already-spaced cases

- [ ] **Step 3: Run the spacing-rule tests to verify failure**

Run:
```bash
cd /home/helena/dev/helena-linter/go
go test ./analyzers/ifleadingspacing ./analyzers/iffollowingspacing ./analyzers/controlblockfollowingspacing ./analyzers/exitspacing
```

Expected: FAIL because the analyzers do not exist yet.

- [ ] **Step 4: Implement the four analyzers with safe whitespace fixes**

Use the shared helpers to:
- identify same-block sibling boundaries
- skip cases that already contain a blank line
- emit one suggested fix that inserts exactly one blank line

- [ ] **Step 5: Run the spacing-rule tests again**

Run the same command as Step 3.

Expected: PASS.

- [ ] **Step 6: Commit**

```bash
cd /home/helena/dev/helena-linter
git add go
git commit -m "feat: add go control and if spacing analyzers"
```

### Task 5: Implement `multiline-block-layout`

**Files:**
- Create: `go/analyzers/multilineblocklayout/analyzer.go`
- Create: `go/analyzers/multilineblocklayout/analyzer_test.go`
- Create: `go/testdata/src/multilineblocklayout/invalid.go`
- Create: `go/testdata/src/multilineblocklayout/valid.go`

- [ ] **Step 1: Write failing tests for `multiline-block-layout`**

Cover:
- single-line non-empty `if`, `for`, and `switch` blocks
- empty blocks that should remain valid
- blocks containing only comments that should remain valid or follow the chosen documented behavior

- [ ] **Step 2: Run the multiline-block-layout tests to verify failure**

Run:
```bash
cd /home/helena/dev/helena-linter/go
go test ./analyzers/multilineblocklayout
```

Expected: FAIL because the analyzer does not exist yet.

- [ ] **Step 3: Implement the analyzer and fix**

Detect non-empty single-line blocks and rewrite them to multiline layout using token positions and existing indentation context.

- [ ] **Step 4: Run the multiline-block-layout tests again**

Run the same command as Step 2.

Expected: PASS.

- [ ] **Step 5: Commit**

```bash
cd /home/helena/dev/helena-linter
git add go
git commit -m "feat: add go multiline block layout analyzer"
```

### Task 6: Implement `redundant-else-if`

**Files:**
- Create: `go/analyzers/redundantelseif/analyzer.go`
- Create: `go/analyzers/redundantelseif/analyzer_test.go`
- Create: `go/testdata/src/redundantelseif/invalid.go`
- Create: `go/testdata/src/redundantelseif/valid.go`

- [ ] **Step 1: Write failing tests for `redundant-else-if`**

Cover:
- `if value == nil { ... } else if value != nil { ... }`
- `if !flag { ... } else if flag { ... }`
- unrelated `else if` conditions that must remain valid
- trivia-preserving conversion from `else if` to `else`

- [ ] **Step 2: Run the redundant-else-if tests to verify failure**

Run:
```bash
cd /home/helena/dev/helena-linter/go
go test ./analyzers/redundantelseif
```

Expected: FAIL because the analyzer does not exist yet.

- [ ] **Step 3: Implement the analyzer and fix**

Use `exprx` complementary-condition helpers to:
- detect only exact complements
- rewrite the `else if` node to plain `else`
- preserve the existing `else` body and nearby comments

- [ ] **Step 4: Run the redundant-else-if tests again**

Run the same command as Step 2.

Expected: PASS.

- [ ] **Step 5: Commit**

```bash
cd /home/helena/dev/helena-linter
git add go
git commit -m "feat: add go redundant else-if analyzer"
```

### Task 7: Implement `if-else-if-chain`

**Files:**
- Create: `go/analyzers/ifelseifchain/analyzer.go`
- Create: `go/analyzers/ifelseifchain/analyzer_test.go`
- Create: `go/testdata/src/ifelseifchain/invalid.go`
- Create: `go/testdata/src/ifelseifchain/valid.go`

- [ ] **Step 1: Write failing tests for `if-else-if-chain`**

Cover:
- a sibling `if` following an exiting `if` branch
- nested `if` statements that must not be folded
- comments and whitespace around the join point
- already-correct `else if` chains

- [ ] **Step 2: Run the if-else-if-chain tests to verify failure**

Run:
```bash
cd /home/helena/dev/helena-linter/go
go test ./analyzers/ifelseifchain
```

Expected: FAIL because the analyzer does not exist yet.

- [ ] **Step 3: Implement the analyzer and fix**

Use `flow` and `astx` helpers to:
- confirm the earlier branch definitely exits
- confirm the following sibling is an `if`
- rewrite the boundary into `else if` without disturbing comments

- [ ] **Step 4: Run the if-else-if-chain tests again**

Run the same command as Step 2.

Expected: PASS.

- [ ] **Step 5: Commit**

```bash
cd /home/helena/dev/helena-linter
git add go
git commit -m "feat: add go if-else-if-chain analyzer"
```

### Task 8: Implement `early-return`

**Files:**
- Create: `go/analyzers/earlyreturn/analyzer.go`
- Create: `go/analyzers/earlyreturn/analyzer_test.go`
- Create: `go/testdata/src/earlyreturn/invalid.go`
- Create: `go/testdata/src/earlyreturn/valid.go`

- [ ] **Step 1: Write failing tests for `early-return`**

Cover:
- wrapped happy-path `if` blocks followed by a fallback `return`
- `if / else` forms that can be inverted to a guard clause safely
- cases with comments or side effects that should remain diagnostic-free if the rewrite would be ambiguous

- [ ] **Step 2: Run the early-return tests to verify failure**

Run:
```bash
cd /home/helena/dev/helena-linter/go
go test ./analyzers/earlyreturn
```

Expected: FAIL because the analyzer does not exist yet.

- [ ] **Step 3: Implement the analyzer and fix**

Use `flow`, `exprx`, and `fixx` helpers to:
- recognize exact safe guard-clause patterns
- invert conditions when needed
- rewrite the minimal local region while preserving comments

- [ ] **Step 4: Run the early-return tests again**

Run the same command as Step 2.

Expected: PASS.

- [ ] **Step 5: Commit**

```bash
cd /home/helena/dev/helena-linter
git add go
git commit -m "feat: add go early-return analyzer"
```

### Task 9: Wire The Bundle And Finish Documentation

**Files:**
- Modify: `go/bundle/all.go`
- Create: `go/bundle/all_test.go`
- Modify: `go/README.md`
- Modify: `README.md`

- [ ] **Step 1: Write failing tests for the analyzer bundle**

Add tests asserting that:
- the bundle exports the full supported rule set
- `control-body-braces` is intentionally absent
- analyzer names are stable and unique

- [ ] **Step 2: Run the bundle tests to verify failure**

Run:
```bash
cd /home/helena/dev/helena-linter/go
go test ./bundle
```

Expected: FAIL because the placeholder bundle does not yet export the full set.

- [ ] **Step 3: Implement the full analyzer bundle**

Update `go/bundle/all.go` so the exported slice includes:
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

- [ ] **Step 4: Finish the Go and root READMEs**

Expand `go/README.md` with:
- import examples for one analyzer and the full bundle
- the supported-rule list
- the omitted-rule note for `control-body-braces`
- per-rule `bad` / `good` examples

Update `README.md` so the root package list and package descriptions include the Go module.

- [ ] **Step 5: Run the bundle tests again**

Run the same command as Step 2.

Expected: PASS.

- [ ] **Step 6: Commit**

```bash
cd /home/helena/dev/helena-linter
git add go README.md
git commit -m "feat: document and bundle go analyzers"
```

### Task 10: Full Verification And Final Cleanup

**Files:**
- Modify: any `go/` file touched above only if verification reveals defects
- Modify: `go/README.md` only if command output or supported-rule wording needs correction

- [ ] **Step 1: Run the full Go test suite**

Run:
```bash
cd /home/helena/dev/helena-linter/go
go test ./...
```

Expected: PASS.

- [ ] **Step 2: Review public docs for naming drift**

Check:
- `go/README.md`
- `README.md`
- `docs/superpowers/specs/2026-03-24-go-analysis-linter-design.md`

Expected: rule names, omitted rules, and bundle usage wording all match the implementation.

- [ ] **Step 3: Commit any verification fixes**

```bash
cd /home/helena/dev/helena-linter
git add go README.md docs/superpowers/specs/2026-03-24-go-analysis-linter-design.md
git commit -m "chore: finish go analyzer verification"
```

- [ ] **Step 4: Capture final evidence**

Record in the final handoff:
- the exact `go test ./...` result
- the final supported analyzer list
- the omitted `control-body-braces` rationale
