# TypeScript Multiline Block Layout Design

## Goal
Add a TypeScript Helena Linter rule that forces non-empty block bodies onto multiple lines while allowing empty blocks to remain single-line.

## Scope
The rule applies to TypeScript block syntax such as function bodies, control-flow blocks, try/catch/finally blocks, and class static blocks. It does not target object literals or type literals.

## Behavior
- `if (flag) { return; }` becomes a multiline block.
- `try { current.close(); } catch {}` becomes multiline for the non-empty `try` block while leaving the empty `catch {}` unchanged.
- Empty blocks like `{}` remain valid on one line.

## Rule Shape
- Rule name: `@distrohelena/linter/multiline-block-layout`
- Fix-first rule with deterministic source rewrite.
- Added to the recommended config.

## Verification
- Rule unit tests for valid/invalid/fixed output.
- Export and recommended-config coverage.
- Smoke test remains green.
