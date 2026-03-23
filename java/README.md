# Helena Java Checkstyle Package

This workspace hosts Helena's Java lint rules as a Checkstyle extension jar.

## Planned Rules

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

## Local Commands

Run the Java package tests:

```bash
./gradlew test
```

Build the full Java workspace:

```bash
./gradlew build
```

Run Checkstyle against the sample consumer project:

```bash
./gradlew :samples:checkstyleMain
```

## Layout

- `helena-linter-checkstyle/` contains the custom Checkstyle extension jar.
- `samples/` contains a tiny Java consumer project wired to the local Helena jar.
