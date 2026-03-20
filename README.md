# `@distrohelena/linter`

TypeScript-only Helena lint rules for modern ESLint flat config.

## Install

```bash
npm install -D @distrohelena/linter eslint typescript @typescript-eslint/parser
```

## Usage

```ts
import helenaLinter from "@distrohelena/linter";

export default [...helenaLinter.configs.recommended];
```

This enables the canonical Helena rule IDs:

- `@distrohelena/linter/declaration-spacing`
- `@distrohelena/linter/early-return`
- `@distrohelena/linter/if-else-if-chain`
- `@distrohelena/linter/if-following-spacing`
- `@distrohelena/linter/redundant-else-if`

## Per-Rule Imports

```ts
import declarationSpacingRule from "@distrohelena/linter/declaration-spacing";
import earlyReturnRule from "@distrohelena/linter/early-return";
import ifElseIfChainRule from "@distrohelena/linter/if-else-if-chain";
import ifFollowingSpacingRule from "@distrohelena/linter/if-following-spacing";
import redundantElseIfRule from "@distrohelena/linter/redundant-else-if";
```

## Included Rules

- `@distrohelena/linter/declaration-spacing`
  Requires a blank line after a declaration statement before the next sibling statement.
- `@distrohelena/linter/early-return`
  Flags wrapped happy-path `if` blocks that should be rewritten as guard clauses.
- `@distrohelena/linter/if-else-if-chain`
  Flags collapsed null checks and sibling `if` branches that should be written as `else if`.
- `@distrohelena/linter/if-following-spacing`
  Requires a blank line after a completed `if` or `if / else if / else` chain before the next sibling statement.
- `@distrohelena/linter/redundant-else-if`
  Flags `else if` branches that are just the exact complement of the previous `if`, which should be plain `else`.

## Scope

- TypeScript only
- ESLint flat config only
- No legacy `.eslintrc` support

## Local Verification

```bash
npm test -- --runInBand
npm run build
npm run lint
npm run lint:smoke
```
