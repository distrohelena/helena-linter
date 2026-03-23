# C# Helena Linter

This folder contains the Roslyn analyzer package for Helena’s C# readability rules.

## Package

The analyzer package lives in `DistroHelena.Linter.CSharp` and is intended to be consumed from C# projects through a `PackageReference` or analyzer project reference.

## First-Pass Rules

- `HLN001` `redundant-else-if`
  Rewrites complementary `else if` branches to plain `else`.
- `HLN002` `if-following-spacing`
  Inserts a blank line after a completed `if` or `if / else` chain before the next sibling statement.
- `HLN003` `control-block-following-spacing`
  Inserts a blank line after `for`, `while`, `do`, `switch`, and `try` statements before the next sibling statement.
- `HLN004` `exit-spacing`
  Inserts a blank line before `return`, `throw`, `break`, and `continue`.
- `HLN005` `declaration-spacing`
  Inserts a blank line after local declarations before the next sibling statement.
- `HLN006` `declaration-leading-spacing`
  Inserts a blank line before local declarations that follow non-declaration statements.
- `HLN007` `if-leading-spacing`
  Inserts a blank line before `if` statements that follow earlier sibling statements.

## Deferred Rules

These TypeScript Helena rules are still pending for the C# package:

- `if-else-if-chain`
- `early-return`

## Local Commands

```bash
dotnet build DistroHelena.Linter.CSharp.sln
dotnet test DistroHelena.Linter.CSharp.sln
dotnet build samples/SampleConsumer.csproj
```

## Sample Consumer

See `samples/` for a minimal analyzer consumer project that references the local analyzer project as a Roslyn analyzer.
