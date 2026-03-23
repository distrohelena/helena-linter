# C# Roslyn Linter Design

## Summary

Add a new `csharp` folder to the `helena-linter` repository that contains a Roslyn analyzer package for C# projects. The first pass should focus on the Helena rules that are safe to auto-fix deterministically in Roslyn, leaving the more invasive control-flow rewrites for a later phase.

## Goals

- Create a plug-and-play Roslyn analyzer package for C#.
- Keep the package scoped to C# only.
- Ship analyzers and code fixes for the easiest Helena rules first.
- Mirror the TypeScript package intent without forcing a one-to-one internal implementation.
- Keep the package structured so additional Helena rules can be added incrementally.

## Non-Goals

- Full parity with every TypeScript rule in the first pass.
- A standalone CLI rewriter outside normal .NET tooling.
- Language support beyond C#.
- Large semantic rewrites that are risky to auto-fix in the initial release.

## Recommended First-Pass Rule Set

The first package version should include:

1. `if-following-spacing`
2. `control-block-following-spacing`
3. `exit-spacing`
4. `declaration-spacing`
5. `declaration-leading-spacing`
6. `if-leading-spacing`
7. `redundant-else-if`

These rules are mostly syntax-driven and have deterministic code-fix behavior in Roslyn.

## Deferred Rules

These should be left for a later iteration:

- `early-return`
- `if-else-if-chain`

Both are valuable, but their code fixes are more invasive in C# and need stricter trivia and behavior-preservation handling.

## Repository Layout

The repository root should contain:

- `typescript/`
- `csharp/`
- shared repo documentation as needed

The `csharp` folder should contain a small .NET solution:

- `DistroHelena.Linter.CSharp/`
- `DistroHelena.Linter.CSharp.Tests/`
- `DistroHelena.Linter.CSharp.sln`

## Package Shape

The C# package should be a standard Roslyn analyzer NuGet package intended for `PackageReference` use in C# projects.

Expected package characteristics:

- analyzer library targeting current supported Roslyn/.NET analyzer conventions
- code fix providers for every included first-pass rule
- package metadata and README aligned with the TypeScript package branding
- diagnostic IDs in a Helena namespace such as `HLN001`, `HLN002`, and so on

## Internal Architecture

### Analyzer Layer

Each rule should have its own analyzer class and, when appropriate, its own code fix provider:

- `IfFollowingSpacingAnalyzer`
- `IfFollowingSpacingCodeFixProvider`
- `ControlBlockFollowingSpacingAnalyzer`
- `ControlBlockFollowingSpacingCodeFixProvider`
- `ExitSpacingAnalyzer`
- `ExitSpacingCodeFixProvider`
- `DeclarationSpacingAnalyzer`
- `DeclarationSpacingCodeFixProvider`
- `DeclarationLeadingSpacingAnalyzer`
- `DeclarationLeadingSpacingCodeFixProvider`
- `IfLeadingSpacingAnalyzer`
- `IfLeadingSpacingCodeFixProvider`
- `RedundantElseIfAnalyzer`
- `RedundantElseIfCodeFixProvider`

### Shared Support

The package should include focused shared helpers:

- `DiagnosticDescriptors`
  Centralizes IDs, titles, categories, and messages.
- `SyntaxTriviaHelpers`
  Handles blank-line detection, indentation, and trivia replacement.
- `StatementSequenceHelpers`
  Finds sibling statements and statement containers safely.
- `ConditionComparisonHelpers`
  Detects complementary `if / else if` conditions for the redundant `else if` rule.

These helpers should stay generic and rule-agnostic.

## Behavior Model

### Diagnostics

First-pass diagnostics should be tuned for a fixer-first workflow:

- prefer non-noisy defaults such as `Info` or `Hidden`
- provide clear messages that describe the resulting cleanup
- avoid speculative diagnostics without a safe code fix

### Code Fixes

Code fixes should:

- preserve comments and trivia
- preserve indentation style already present in the file
- avoid rewriting beyond the minimal necessary syntax region
- avoid semantic transforms unless the pattern is exact and proven safe

## Testing Strategy

The `DistroHelena.Linter.CSharp.Tests` project should use Roslyn analyzer/code-fix testing packages.

Each included rule should have:

- valid examples
- invalid examples
- fixed output assertions
- trivia-preservation cases
- nested block cases where relevant

Recommended coverage themes:

- `if / else` chains
- loops and `switch`
- `try / catch / finally`
- declarations followed by normal statements
- exit statements such as `return`, `throw`, `break`, and `continue`

## Documentation

The `csharp` package should have its own README covering:

- install instructions via NuGet
- supported rules
- the fact that the first pass intentionally covers the easiest auto-fixable rules
- how the Roslyn rule names map to the Helena style rules from the TypeScript package

## Verification

Before calling the package complete, the following should pass from `csharp`:

- `dotnet test`
- `dotnet build`

## Future Extension Path

Once the first pass is stable, the next rules to add should be:

1. `if-else-if-chain`
2. `early-return`

Those should reuse the same helper infrastructure instead of adding rule-specific ad hoc syntax walking.
