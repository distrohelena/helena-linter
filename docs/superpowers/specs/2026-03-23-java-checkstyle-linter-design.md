# Java Checkstyle Helena Linter Design

## Goal

Add a third package under `java/` that delivers Helena's readability and control-flow rules for Java projects through Checkstyle.

The Java package should be plug-and-play for consumers in the same spirit as the existing TypeScript and C# packages:

- TypeScript: ESLint flat-config package
- C#: Roslyn analyzer package
- Java: Checkstyle extension package

## Repository Layout

The Java package should live under a new top-level `java/` folder.

Proposed structure:

- `java/helena-linter-checkstyle/`
  - custom Checkstyle checks
  - shared AST helpers
  - bundled Helena ruleset XML
  - build and publication metadata
- `java/samples/`
  - minimal Java consumer project wired to use the local Helena Checkstyle package
- `java/README.md`
  - package-specific installation, configuration, and rule documentation

This keeps ecosystems separate and mirrors the existing `typescript/` and `csharp/` package split.

## Public Package Shape

The Java package should publish a jar containing:

- one custom Checkstyle check per Helena rule
- a bundled `helena_checks.xml` ruleset consumers can reference
- metadata needed for normal Java package publication and consumption

The initial public package should be a Checkstyle extension jar, not a standalone CLI.

## Rule Mapping

Every Helena rule should have a dedicated Java Checkstyle implementation class.

Planned rules:

- `ControlBodyBracesCheck`
- `ControlBlockFollowingSpacingCheck`
- `DeclarationSpacingCheck`
- `DeclarationLeadingSpacingCheck`
- `ExitSpacingCheck`
- `IfLeadingSpacingCheck`
- `IfFollowingSpacingCheck`
- `IfElseIfChainCheck`
- `RedundantElseIfCheck`
- `EarlyReturnCheck`

This one-rule-per-class structure keeps rule ownership clear, aligns with the rest of the repo, and makes documentation straightforward.

## Shared Infrastructure

The Java package should include shared helper classes for:

- sibling statement navigation in Checkstyle ASTs
- brace detection for embedded statement bodies
- blank-line and line-distance analysis
- complementary condition detection for `redundant-else-if`
- control-flow exit detection for `if-else-if-chain` and `early-return`

These helpers should stay generic and reusable across checks, but each actual rule must remain a dedicated check class.

## Rule Semantics

The Java rules should stay as close as practical to the TypeScript and C# semantics.

Formatting/readability rules:

- require braces around control-flow bodies that can omit braces
- require blank lines around declarations, exits, and control-flow boundaries
- require blank lines before `if` where appropriate

Structural/control-flow rules:

- `RedundantElseIfCheck`
  - report `else if` branches that are exact complements of the previous `if`
- `IfElseIfChainCheck`
  - report sibling `if` statements that should be folded into an `else if` chain when the earlier branch exits
- `EarlyReturnCheck`
  - report supported wrapped happy-path or `if / else` patterns that should become guard clauses

Unlike the TypeScript and C# packages, these Java rules will be analyze-only in the first pass because Checkstyle does not provide code fixes.

## Consumer Configuration

Consumers should be able to:

1. add the Helena Checkstyle jar to their Checkstyle classpath
2. reference the bundled Helena ruleset XML
3. run Checkstyle from their normal Gradle or Maven setup

The Java README should include both Gradle and Maven integration examples if practical, plus a minimal raw Checkstyle configuration example.

## Build and Tooling

Gradle is the recommended build tool for the Java package.

Reasons:

- straightforward Java library packaging
- easy sample-project wiring
- simple local verification commands
- no need to introduce a second Java build tool unless required later

The package should still produce normal artifacts consumable from Maven/Gradle ecosystems.

## Testing Strategy

The Java package should include:

- unit tests for each custom check
- shared helper tests where the logic is non-trivial
- a sample consumer project to prove end-to-end usage

Verification should include:

- `gradle test`
- build/package command for the Checkstyle jar
- sample consumer check run using the generated package

## Success Criteria

The Java package is complete when:

- `java/` contains a standalone Checkstyle extension package
- all Helena rules are represented by dedicated Java check classes
- the package builds and tests cleanly
- the sample Java project can use the Helena Checkstyle package successfully
- the repo has a clear root README plus a Java-specific README that explains usage
