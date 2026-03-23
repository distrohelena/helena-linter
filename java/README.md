# Helena Java Checkstyle Package

This workspace hosts Helena's Java lint rules as a Checkstyle extension jar.

## Package

The package lives in `helena-linter-checkstyle` and is intended to be consumed as a local
Checkstyle dependency during development or as a published artifact once the package is released.

## Gradle Usage

Add the Helena Checkstyle jar to the `checkstyle` configuration and point Checkstyle at the bundled
Helena ruleset:

```kotlin
dependencies {
    checkstyle(project(":helena-linter-checkstyle"))
}

checkstyle {
    configFile = rootProject.file("helena-linter-checkstyle/src/main/resources/helena_checks.xml")
}
```

The sample consumer in `samples/` uses the same setup with a copied ruleset at
`samples/config/checkstyle/checkstyle.xml`.

## Raw Checkstyle Usage

If you run Checkstyle directly, put both the Checkstyle distribution and the Helena jar on the
classpath, then pass the Helena ruleset file with `-c`:

```bash
java -cp "checkstyle-10.21.4-all.jar:helena-linter-checkstyle-0.1.0.jar" \
  com.puppycrawl.tools.checkstyle.Main \
  -c helena-linter-checkstyle/src/main/resources/helena_checks.xml \
  samples/src/main/java
```

## Full Rule List

- `ControlBodyBracesCheck` - requires braces around executable control-flow bodies.
- `ControlBlockFollowingSpacingCheck` - requires a blank line after completed `for`, `while`,
  `do`, `switch`, and `try` blocks.
- `DeclarationLeadingSpacingCheck` - requires a blank line before local declarations that follow
  non-declaration statements.
- `DeclarationSpacingCheck` - requires a blank line after local declarations before the next
  sibling statement.
- `EarlyReturnCheck` - flags wrapped happy-path `if` and `if / else` shapes that should become
  guard clauses.
- `ExitSpacingCheck` - requires a blank line before `return`, `throw`, `break`, and `continue`.
- `IfElseIfChainCheck` - folds sibling `if` statements into an `else if` chain when the earlier
  branch definitely exits.
- `IfFollowingSpacingCheck` - requires a blank line after a completed `if` chain before the next
  sibling statement.
- `IfLeadingSpacingCheck` - requires a blank line before `if` statements that follow earlier
  sibling statements.
- `RedundantElseIfCheck` - flags complementary `else if` branches that should be written as
  `else`.

## Local Verification

```bash
./gradlew test
./gradlew build
./gradlew :samples:checkstyleMain
```

## Layout

- `helena-linter-checkstyle/` contains the custom Checkstyle extension jar and bundled ruleset.
- `samples/` contains a tiny Java consumer project wired to the local Helena jar and config copy.
