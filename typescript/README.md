# `@distrohelena/linter`

TypeScript-only Helena lint rules for modern ESLint flat config.

Repository: https://github.com/distrohelena/helena-linter

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

- `@distrohelena/linter/control-body-braces`
- `@distrohelena/linter/control-block-following-spacing`
- `@distrohelena/linter/declaration-leading-spacing`
- `@distrohelena/linter/declaration-spacing`
- `@distrohelena/linter/early-return`
- `@distrohelena/linter/exit-spacing`
- `@distrohelena/linter/if-else-if-chain`
- `@distrohelena/linter/if-leading-spacing`
- `@distrohelena/linter/if-following-spacing`
- `@distrohelena/linter/redundant-else-if`

## Per-Rule Imports

```ts
import controlBodyBracesRule from "@distrohelena/linter/control-body-braces";
import controlBlockFollowingSpacingRule from "@distrohelena/linter/control-block-following-spacing";
import declarationLeadingSpacingRule from "@distrohelena/linter/declaration-leading-spacing";
import declarationSpacingRule from "@distrohelena/linter/declaration-spacing";
import earlyReturnRule from "@distrohelena/linter/early-return";
import exitSpacingRule from "@distrohelena/linter/exit-spacing";
import ifElseIfChainRule from "@distrohelena/linter/if-else-if-chain";
import ifLeadingSpacingRule from "@distrohelena/linter/if-leading-spacing";
import ifFollowingSpacingRule from "@distrohelena/linter/if-following-spacing";
import redundantElseIfRule from "@distrohelena/linter/redundant-else-if";
```

## Included Rules

- `@distrohelena/linter/control-body-braces`
  Requires braces for `if`, `else`, `for`, `for...in`, `for...of`, `while`, and `do...while` bodies.
- `@distrohelena/linter/control-block-following-spacing`
  Requires a blank line after a completed `for`, `while`, `do`, `switch`, or `try` statement before the next sibling statement.
- `@distrohelena/linter/declaration-leading-spacing`
  Requires a blank line before a declaration statement when the previous sibling statement is not a declaration.
- `@distrohelena/linter/declaration-spacing`
  Requires a blank line after a declaration statement before the next sibling statement.
- `@distrohelena/linter/early-return`
  Flags wrapped happy-path `if` blocks that should be rewritten as guard clauses.
- `@distrohelena/linter/exit-spacing`
  Requires a blank line before `return`, `throw`, `break`, and `continue` statements.
- `@distrohelena/linter/if-else-if-chain`
  Flags sibling `if` branches that should be written as `else if`.
- `@distrohelena/linter/if-leading-spacing`
  Requires a blank line before an `if` statement when it has a previous sibling statement.
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
