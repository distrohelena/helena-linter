# Java Checkstyle Helena Linter Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build a new `java` Checkstyle extension package inside `helena-linter` that ships the full Helena rule set for Java projects.

**Architecture:** Add a standalone Gradle-based Java package under `java/helena-linter-checkstyle` with one custom Checkstyle check per Helena rule, shared AST helper classes, a bundled Helena ruleset XML, and a sample consumer project under `java/samples`. Keep the checks focused and small, with shared logic extracted into helper classes instead of mega-checks.

**Tech Stack:** Java 21 or current locally available JDK, Gradle, Checkstyle custom checks, JUnit 5, Checkstyle test helpers, bundled XML ruleset resources.

---

### Task 1: Scaffold The Java Checkstyle Package

**Files:**
- Create: `java/README.md`
- Create: `java/settings.gradle.kts`
- Create: `java/build.gradle.kts`
- Create: `java/gradle.properties`
- Create: `java/helena-linter-checkstyle/build.gradle.kts`
- Create: `java/helena-linter-checkstyle/src/main/resources/helena_checks.xml`
- Create: `java/helena-linter-checkstyle/src/test/resources/`
- Create: `java/samples/build.gradle.kts`
- Create: `java/samples/settings.gradle.kts`
- Create: `java/samples/src/main/java/dev/distrohelena/sample/Sample.java`
- Modify: `docs/superpowers/specs/2026-03-23-java-checkstyle-linter-design.md` only if naming drift is discovered during scaffolding

- [ ] **Step 1: Create the Java package directories**

Run:
```bash
cd /home/helena/dev/helena-linter
mkdir -p java/helena-linter-checkstyle/src/main/java/dev/distrohelena/linter/checkstyle
mkdir -p java/helena-linter-checkstyle/src/main/resources
mkdir -p java/helena-linter-checkstyle/src/test/java/dev/distrohelena/linter/checkstyle
mkdir -p java/helena-linter-checkstyle/src/test/resources
mkdir -p java/samples/src/main/java/dev/distrohelena/sample
```

Expected: the Java package and sample-project folders exist.

- [ ] **Step 2: Add Gradle root files for the Java workspace**

Write `java/settings.gradle.kts` to include:
- `helena-linter-checkstyle`
- `samples`

Write `java/build.gradle.kts` so all subprojects share:
- `group = "dev.distrohelena"`
- `version = "0.1.0"`
- a consistent Java toolchain
- Maven Central repository

- [ ] **Step 3: Add the Checkstyle package build file**

Write `java/helena-linter-checkstyle/build.gradle.kts` to:
- apply `java-library`
- publish a normal jar
- depend on Checkstyle APIs
- depend on JUnit 5 for tests
- make resources include `helena_checks.xml`

- [ ] **Step 4: Add the sample project build files**

Write `java/samples/build.gradle.kts` and `java/samples/settings.gradle.kts` so the sample project can:
- compile a tiny Java class
- run Checkstyle using the local Helena Checkstyle package on its classpath

- [ ] **Step 5: Add the Java package README skeleton**

Write `java/README.md` with:
- package purpose
- planned Helena rule list
- local commands: `./gradlew test`, `./gradlew build`, `./gradlew :samples:checkstyleMain`

- [ ] **Step 6: Verify the empty Java workspace builds**

Run:
```bash
cd /home/helena/dev/helena-linter/java
./gradlew build
```

Expected: the Java workspace builds even before custom checks exist.

- [ ] **Step 7: Commit**

```bash
cd /home/helena/dev/helena-linter
git add java docs/superpowers/plans/2026-03-23-java-checkstyle-linter-implementation.md
git commit -m "feat: scaffold java checkstyle linter package"
```

### Task 2: Add Shared Checkstyle Infrastructure

**Files:**
- Create: `java/helena-linter-checkstyle/src/main/java/dev/distrohelena/linter/checkstyle/diagnostics/HelenaMessageIds.java`
- Create: `java/helena-linter-checkstyle/src/main/java/dev/distrohelena/linter/checkstyle/helpers/StatementAstNavigator.java`
- Create: `java/helena-linter-checkstyle/src/main/java/dev/distrohelena/linter/checkstyle/helpers/BlankLineAnalyzer.java`
- Create: `java/helena-linter-checkstyle/src/main/java/dev/distrohelena/linter/checkstyle/helpers/ControlFlowExitAnalyzer.java`
- Create: `java/helena-linter-checkstyle/src/main/java/dev/distrohelena/linter/checkstyle/helpers/ConditionComparisonAnalyzer.java`
- Create: `java/helena-linter-checkstyle/src/test/java/dev/distrohelena/linter/checkstyle/helpers/ConditionComparisonAnalyzerTests.java`
- Create: `java/helena-linter-checkstyle/src/test/java/dev/distrohelena/linter/checkstyle/helpers/ControlFlowExitAnalyzerTests.java`

- [ ] **Step 1: Write the helper tests first**

Add tests covering:
- complementary null comparisons
- boolean negation comparisons
- return/throw/break/continue exit detection
- non-exiting branches that should stay false

- [ ] **Step 2: Run the helper tests to verify failure**

Run:
```bash
cd /home/helena/dev/helena-linter/java
./gradlew :helena-linter-checkstyle:test --tests '*ConditionComparisonAnalyzerTests' --tests '*ControlFlowExitAnalyzerTests'
```

Expected: FAIL because the helper classes do not exist yet.

- [ ] **Step 3: Implement the shared helper classes**

Implement:
- AST sibling navigation
- blank-line detection between sibling statements
- control-flow exit detection
- complementary condition detection
- shared message IDs/constants for the checks

- [ ] **Step 4: Run the helper tests again**

Run the same command as Step 2.

Expected: PASS.

- [ ] **Step 5: Commit**

```bash
cd /home/helena/dev/helena-linter
git add java
git commit -m "feat: add java checkstyle helper infrastructure"
```

### Task 3: Implement Control-Body And Following-Spacing Checks

**Files:**
- Create: `java/helena-linter-checkstyle/src/main/java/dev/distrohelena/linter/checkstyle/checks/ControlBodyBracesCheck.java`
- Create: `java/helena-linter-checkstyle/src/main/java/dev/distrohelena/linter/checkstyle/checks/IfFollowingSpacingCheck.java`
- Create: `java/helena-linter-checkstyle/src/main/java/dev/distrohelena/linter/checkstyle/checks/ControlBlockFollowingSpacingCheck.java`
- Create: `java/helena-linter-checkstyle/src/test/java/dev/distrohelena/linter/checkstyle/checks/ControlBodyBracesCheckTests.java`
- Create: `java/helena-linter-checkstyle/src/test/java/dev/distrohelena/linter/checkstyle/checks/IfFollowingSpacingCheckTests.java`
- Create: `java/helena-linter-checkstyle/src/test/java/dev/distrohelena/linter/checkstyle/checks/ControlBlockFollowingSpacingCheckTests.java`
- Create: `java/helena-linter-checkstyle/src/test/resources/samples/control-body-braces/`
- Create: `java/helena-linter-checkstyle/src/test/resources/samples/following-spacing/`

- [ ] **Step 1: Write failing tests for `ControlBodyBracesCheck`**

Cover:
- `if`, `else`, `for`, `foreach`, `while`, `do`, `try-with-resources`, `synchronized`
- valid cases with braces
- invalid cases without braces

- [ ] **Step 2: Write failing tests for the two following-spacing checks**

Cover:
- blank line after `if / else if / else`
- blank line after `for`, `while`, `do`, `switch`, `try`
- already-spaced valid cases

- [ ] **Step 3: Run the check tests to verify failure**

Run:
```bash
cd /home/helena/dev/helena-linter/java
./gradlew :helena-linter-checkstyle:test --tests '*ControlBodyBracesCheckTests' --tests '*IfFollowingSpacingCheckTests' --tests '*ControlBlockFollowingSpacingCheckTests'
```

Expected: FAIL because the checks do not exist yet.

- [ ] **Step 4: Implement the three checks**

Use the shared helpers to:
- detect embedded control statements without braces
- detect missing blank lines after completed control blocks

- [ ] **Step 5: Run the tests again**

Run the same command as Step 3.

Expected: PASS.

- [ ] **Step 6: Commit**

```bash
cd /home/helena/dev/helena-linter
git add java
git commit -m "feat: add java control body and following spacing checks"
```

### Task 4: Implement Declaration, Exit, And If-Leading Spacing Checks

**Files:**
- Create: `java/helena-linter-checkstyle/src/main/java/dev/distrohelena/linter/checkstyle/checks/DeclarationSpacingCheck.java`
- Create: `java/helena-linter-checkstyle/src/main/java/dev/distrohelena/linter/checkstyle/checks/DeclarationLeadingSpacingCheck.java`
- Create: `java/helena-linter-checkstyle/src/main/java/dev/distrohelena/linter/checkstyle/checks/ExitSpacingCheck.java`
- Create: `java/helena-linter-checkstyle/src/main/java/dev/distrohelena/linter/checkstyle/checks/IfLeadingSpacingCheck.java`
- Create: `java/helena-linter-checkstyle/src/test/java/dev/distrohelena/linter/checkstyle/checks/DeclarationSpacingCheckTests.java`
- Create: `java/helena-linter-checkstyle/src/test/java/dev/distrohelena/linter/checkstyle/checks/DeclarationLeadingSpacingCheckTests.java`
- Create: `java/helena-linter-checkstyle/src/test/java/dev/distrohelena/linter/checkstyle/checks/ExitSpacingCheckTests.java`
- Create: `java/helena-linter-checkstyle/src/test/java/dev/distrohelena/linter/checkstyle/checks/IfLeadingSpacingCheckTests.java`

- [ ] **Step 1: Write failing tests for declaration spacing rules**

Cover:
- blank line after local declarations
- blank line before declarations after non-declaration statements
- valid already-spaced cases

- [ ] **Step 2: Write failing tests for exit and if-leading spacing**

Cover:
- blank line before `return`, `throw`, `break`, `continue`
- blank line before `if` after another sibling statement
- valid already-spaced cases

- [ ] **Step 3: Run the tests to verify failure**

Run:
```bash
cd /home/helena/dev/helena-linter/java
./gradlew :helena-linter-checkstyle:test --tests '*DeclarationSpacingCheckTests' --tests '*DeclarationLeadingSpacingCheckTests' --tests '*ExitSpacingCheckTests' --tests '*IfLeadingSpacingCheckTests'
```

Expected: FAIL because the checks do not exist yet.

- [ ] **Step 4: Implement the four checks**

Use `BlankLineAnalyzer` and `StatementAstNavigator` to compare adjacent statements and declarations.

- [ ] **Step 5: Run the tests again**

Run the same command as Step 3.

Expected: PASS.

- [ ] **Step 6: Commit**

```bash
cd /home/helena/dev/helena-linter
git add java
git commit -m "feat: add java declaration and exit spacing checks"
```

### Task 5: Implement `RedundantElseIfCheck` And `IfElseIfChainCheck`

**Files:**
- Create: `java/helena-linter-checkstyle/src/main/java/dev/distrohelena/linter/checkstyle/checks/RedundantElseIfCheck.java`
- Create: `java/helena-linter-checkstyle/src/main/java/dev/distrohelena/linter/checkstyle/checks/IfElseIfChainCheck.java`
- Create: `java/helena-linter-checkstyle/src/test/java/dev/distrohelena/linter/checkstyle/checks/RedundantElseIfCheckTests.java`
- Create: `java/helena-linter-checkstyle/src/test/java/dev/distrohelena/linter/checkstyle/checks/IfElseIfChainCheckTests.java`

- [ ] **Step 1: Write failing tests for `RedundantElseIfCheck`**

Cover:
- `if (value == null)` followed by complementary `else if (value != null)`
- `if (!flag)` followed by `else if (flag)`
- negative cases that are not exact complements

- [ ] **Step 2: Write failing tests for `IfElseIfChainCheck`**

Cover:
- sibling `if` after a `return`
- sibling `if` after a `throw`
- sibling `if` after a loop `break` or `continue`
- negative case where the first branch does not exit

- [ ] **Step 3: Run the tests to verify failure**

Run:
```bash
cd /home/helena/dev/helena-linter/java
./gradlew :helena-linter-checkstyle:test --tests '*RedundantElseIfCheckTests' --tests '*IfElseIfChainCheckTests'
```

Expected: FAIL because the checks do not exist yet.

- [ ] **Step 4: Implement both checks**

Use:
- `ConditionComparisonAnalyzer` for exact complements
- `ControlFlowExitAnalyzer` plus sibling-statement navigation for sibling `if` folding suggestions

- [ ] **Step 5: Run the tests again**

Run the same command as Step 3.

Expected: PASS.

- [ ] **Step 6: Commit**

```bash
cd /home/helena/dev/helena-linter
git add java
git commit -m "feat: add java branch structure checks"
```

### Task 6: Implement `EarlyReturnCheck`

**Files:**
- Create: `java/helena-linter-checkstyle/src/main/java/dev/distrohelena/linter/checkstyle/checks/EarlyReturnCheck.java`
- Create: `java/helena-linter-checkstyle/src/test/java/dev/distrohelena/linter/checkstyle/checks/EarlyReturnCheckTests.java`
- Modify: `java/helena-linter-checkstyle/src/main/java/dev/distrohelena/linter/checkstyle/helpers/ControlFlowExitAnalyzer.java` only if additional exit-shape support is needed

- [ ] **Step 1: Write failing tests for `EarlyReturnCheck`**

Cover:
- wrapped happy-path block followed by an exit statement
- `if / else` where the `if` branch exits
- `if / else` where the `else` branch exits
- negative case where neither branch exits

- [ ] **Step 2: Run the early-return tests to verify failure**

Run:
```bash
cd /home/helena/dev/helena-linter/java
./gradlew :helena-linter-checkstyle:test --tests '*EarlyReturnCheckTests'
```

Expected: FAIL because the check does not exist yet.

- [ ] **Step 3: Implement `EarlyReturnCheck`**

Use the shared control-flow helper to detect supported wrapped-happy-path and `if / else` guard-clause opportunities.

- [ ] **Step 4: Run the early-return tests again**

Run the same command as Step 2.

Expected: PASS.

- [ ] **Step 5: Commit**

```bash
cd /home/helena/dev/helena-linter
git add java
git commit -m "feat: add java early return check"
```

### Task 7: Bundle The Ruleset And Wire The Sample Consumer

**Files:**
- Modify: `java/helena-linter-checkstyle/src/main/resources/helena_checks.xml`
- Modify: `java/samples/build.gradle.kts`
- Modify: `java/samples/src/main/java/dev/distrohelena/sample/Sample.java`
- Create: `java/samples/config/checkstyle/checkstyle.xml`
- Modify: `java/README.md`
- Modify: `README.md` if you want the root repo landing page to mention the new Java package after implementation

- [ ] **Step 1: Add the full Helena ruleset XML**

Wire `helena_checks.xml` so it references every custom Java check in one bundled configuration.

- [ ] **Step 2: Configure the sample project to use the local Helena ruleset**

Make the sample project:
- depend on the local check jar on the Checkstyle classpath
- reference the bundled or copied Helena ruleset XML
- include source that intentionally triggers several Helena checks

- [ ] **Step 3: Run the sample consumer check task**

Run:
```bash
cd /home/helena/dev/helena-linter/java
./gradlew :samples:checkstyleMain
```

Expected: the sample project reports Helena violations from the local Java package.

- [ ] **Step 4: Update the Java README with real consumer instructions**

Document:
- Gradle usage
- Maven or raw Checkstyle usage if practical
- the full rule list
- local verification commands

- [ ] **Step 5: Run the full Java verification suite**

Run:
```bash
cd /home/helena/dev/helena-linter/java
./gradlew test
./gradlew build
./gradlew :samples:checkstyleMain
```

Expected: tests and build pass, and the sample consumer demonstrates the Helena checks.

- [ ] **Step 6: Commit**

```bash
cd /home/helena/dev/helena-linter
git add java README.md
git commit -m "feat: add java checkstyle helena package"
```
