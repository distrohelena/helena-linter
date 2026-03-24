# Helena Java Checkstyle Package

This workspace hosts Helena's Java lint rules as a Checkstyle extension jar.

## Package

The package lives in `helena-linter-checkstyle` and is intended to be consumed as a local
Checkstyle dependency during development or as a published artifact once the package is released.

Published coordinates:

```text
com.distrohelena:helena-linter-checkstyle:0.1.0
```

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

The sample source intentionally violates multiple Helena rules, so
`./gradlew :samples:checkstyleMain` is expected to fail and print Helena diagnostics. That task is
the end-to-end proof that the bundled ruleset and sample wiring work.

## Raw Checkstyle Usage

If you run Checkstyle directly, put both the Checkstyle distribution and the Helena jar on the
classpath, then pass the Helena ruleset file with `-c`:

```bash
java -cp "checkstyle-10.21.4-all.jar:helena-linter-checkstyle-0.1.0.jar" \
  com.puppycrawl.tools.checkstyle.Main \
  -c helena-linter-checkstyle/src/main/resources/helena_checks.xml \
  samples/src/main/java
```

## Publishing

Create the publication artifacts:

```bash
./gradlew :helena-linter-checkstyle:publishToMavenLocal
```

The package now exposes a standard Maven publication named `mavenJava` with:
- artifact id `helena-linter-checkstyle`
- sources jar
- javadoc jar
- MIT license metadata
- POM metadata pointing to the GitHub repository

If you want to publish to a remote Maven repository later, add the target repository under the
`publishing.repositories` block in `helena-linter-checkstyle/build.gradle.kts`.

### Maven Central

The Gradle build is wired for the Sonatype Central Portal staging API and provides a wrapper task:

```bash
./gradlew :helena-linter-checkstyle:publishToSonatypeCentral
```

Before running that task:

1. Register and verify the `com.distrohelena` namespace in Sonatype Central Portal using a DNS TXT
   record on `distrohelena.com`.
2. Generate a Central Portal user token.
3. Create or export a PGP private key for artifact signing.
4. Put the credentials in `~/.gradle/gradle.properties` or your shell environment:

```properties
centralPortalUsername=...
centralPortalPassword=...
signingKeyFile=/absolute/path/to/private-key.asc
signingPassword=...
```

Environment variable equivalents:

```bash
export CENTRAL_PORTAL_USERNAME=...
export CENTRAL_PORTAL_PASSWORD=...
export SIGNING_KEY_FILE=/absolute/path/to/private-key.asc
export SIGNING_PASSWORD=...
```

Optional settings:

- `signingKey` or `SIGNING_KEY` if you prefer to pass the armored private key inline.
- `signingKeyId` or `SIGNING_KEY_ID` if your key tooling needs an explicit key id.
- `centralPublishingType=automatic` to release automatically after validation, or
  `centralPublishingType=user_managed` to review the deployment manually in the Portal UI.

## Full Rule List

Examples below show the shape each Checkstyle rule expects.

### `ControlBodyBracesCheck`

Requires braces around executable control-flow bodies.

Bad:

```java
if (ready)
    run();
```

Good:

```java
if (ready) {
    run();
}
```

### `ControlBlockFollowingSpacingCheck`

Requires a blank line after completed `for`, `while`, `do`, `switch`, and `try` blocks.

Bad:

```java
for (String item : items) {
    process(item);
}
finish();
```

Good:

```java
for (String item : items) {
    process(item);
}

finish();
```

### `DeclarationLeadingSpacingCheck`

Requires a blank line before local declarations that follow non-declaration statements.

Bad:

```java
logStart();
int result = compute();
```

Good:

```java
logStart();

int result = compute();
```

### `DeclarationSpacingCheck`

Requires a blank line after local declarations before the next sibling statement.

Bad:

```java
int result = compute();
render(result);
```

Good:

```java
int result = compute();

render(result);
```

### `EarlyReturnCheck`

Flags wrapped happy-path `if` and `if / else` shapes that should become guard clauses.

Bad:

```java
if (isValid(user)) {
    save(user);
    return true;
}

return false;
```

Good:

```java
if (!isValid(user)) {
    return false;
}

save(user);
return true;
```

### `ExitSpacingCheck`

Requires a blank line before `return`, `throw`, `break`, and `continue`.

Bad:

```java
updateState();
return value;
```

Good:

```java
updateState();

return value;
```

### `IfElseIfChainCheck`

Folds sibling `if` statements into an `else if` chain when the earlier branch definitely exits.

Bad:

```java
if (primary) {
    return "a";
}

if (secondary) {
    return "b";
}
```

Good:

```java
if (primary) {
    return "a";
} else if (secondary) {
    return "b";
}
```

### `IfFollowingSpacingCheck`

Requires a blank line after a completed `if` chain before the next sibling statement.

Bad:

```java
if (ready) {
    run();
}
cleanup();
```

Good:

```java
if (ready) {
    run();
}

cleanup();
```

### `IfLeadingSpacingCheck`

Requires a blank line before `if` statements that follow earlier sibling statements.

Bad:

```java
prepare();
if (ready) {
    run();
}
```

Good:

```java
prepare();

if (ready) {
    run();
}
```

### `MultilineBlockLayoutCheck`

Requires non-empty code blocks to be written across multiple lines. Empty blocks may remain
single-line, and object, collection, and anonymous initializer braces are excluded.

Bad:

```java
if (ready) { run(); }
```

Good:

```java
if (ready) {
    run();
}
```

### `RedundantElseIfCheck`

Flags complementary `else if` branches that should be written as `else`.

Bad:

```java
if (value == null) {
    handleMissing();
} else if (value != null) {
    handleValue(value);
}
```

Good:

```java
if (value == null) {
    handleMissing();
} else {
    handleValue(value);
}
```

## Local Verification

```bash
./gradlew test
./gradlew build
./gradlew :samples:checkstyleMain
```

## Layout

- `helena-linter-checkstyle/` contains the custom Checkstyle extension jar and bundled ruleset.
- `samples/` contains a tiny Java consumer project wired to the local Helena jar and config copy.
