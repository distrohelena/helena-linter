# CSharp and Java Multiline Block Layout Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add matching multiline block layout rules to the C# and Java Helena packages so they align with the existing TypeScript `multiline-block-layout` rule.

**Architecture:** Add one focused Roslyn analyzer/code-fix pair in `csharp/` and one focused Checkstyle check in `java/`. Keep the rule orthogonal to the existing braces rules: braces checks decide whether a block exists, multiline-layout checks decide whether a non-empty block must break across lines.

**Tech Stack:** C#, Roslyn analyzers and code fixes, Java, Checkstyle custom checks, .NET test projects, Gradle, JUnit 5.

---

### Task 1: Add The C# Diagnostic Surface

**Files:**
- Modify: `csharp/DistroHelena.Linter.CSharp/Diagnostics/HelenaDiagnosticDescriptors.cs`
- Modify: `csharp/README.md`
- Modify: `csharp/DistroHelena.Linter.CSharp/README.md`

- [ ] **Step 1: Add a failing C# export/documentation expectation**

Update the C# READMEs to include a placeholder line for `HLN011 multiline-block-layout`, then add the descriptor entry in comments-only form to make the missing implementation explicit.

- [ ] **Step 2: Add the `HLN011` descriptor**

Add a new descriptor to `HelenaDiagnosticDescriptors.cs` with:
- id: `HLN011`
- title: `Use multiline layout for non-empty blocks`
- message: `Rewrite this non-empty block onto multiple lines`

- [ ] **Step 3: Build the C# package to verify the descriptor compiles**

Run:
```bash
cd /home/helena/dev/helena-linter
dotnet build csharp/DistroHelena.Linter.CSharp.sln
```

Expected: PASS.

- [ ] **Step 4: Commit**

```bash
cd /home/helena/dev/helena-linter
git add csharp/DistroHelena.Linter.CSharp/Diagnostics/HelenaDiagnosticDescriptors.cs csharp/README.md csharp/DistroHelena.Linter.CSharp/README.md
git commit -m "feat: add csharp multiline block layout descriptor"
```

### Task 2: Implement The C# Multiline Block Layout Rule

**Files:**
- Create: `csharp/DistroHelena.Linter.CSharp/Analyzers/MultilineBlockLayoutAnalyzer.cs`
- Create: `csharp/DistroHelena.Linter.CSharp/CodeFixes/MultilineBlockLayoutCodeFixProvider.cs`
- Create: `csharp/DistroHelena.Linter.CSharp.Tests/Analyzers/MultilineBlockLayoutAnalyzerTests.cs`
- Modify: `csharp/DistroHelena.Linter.CSharp/CodeFixes/CodeFixConstants.cs` only if a new title constant is needed
- Modify: `csharp/DistroHelena.Linter.CSharp/Helpers/SyntaxTriviaHelpers.cs` only if the existing formatting helpers are insufficient

- [ ] **Step 1: Write the failing C# analyzer tests**

Cover:
- `if (flag) { return; }`
- `void Run() { DoWork(); }`
- `try { Close(); } catch {}`
- valid multiline blocks
- valid empty blocks
- object initializer non-target cases

- [ ] **Step 2: Run the C# analyzer test to verify failure**

Run:
```bash
cd /home/helena/dev/helena-linter
dotnet test csharp/DistroHelena.Linter.CSharp.sln --filter MultilineBlockLayoutAnalyzerTests
```

Expected: FAIL because the analyzer and code fix do not exist yet.

- [ ] **Step 3: Implement the C# analyzer**

Inspect `BlockSyntax` nodes and report only when:
- the block contains at least one statement
- the block is laid out on one line
- the block belongs to actual code-block syntax

Do not target:
- object initializers
- collection initializers
- anonymous object initializers

- [ ] **Step 4: Implement the C# code fix**

Rewrite the block into multiline form, keep the same statements, and rely on Roslyn formatting for indentation and brace placement.

- [ ] **Step 5: Run the focused C# tests again**

Run the same command as Step 2.

Expected: PASS.

- [ ] **Step 6: Run full C# verification**

Run:
```bash
cd /home/helena/dev/helena-linter
dotnet test csharp/DistroHelena.Linter.CSharp.sln
dotnet build csharp/DistroHelena.Linter.CSharp.sln
```

Expected: PASS.

- [ ] **Step 7: Commit**

```bash
cd /home/helena/dev/helena-linter
git add csharp
git commit -m "feat: add csharp multiline block layout rule"
```

### Task 3: Add The Java Rule Surface

**Files:**
- Modify: `java/helena-linter-checkstyle/src/main/resources/helena_checks.xml`
- Modify: `java/samples/config/checkstyle/checkstyle.xml`
- Modify: `java/README.md`

- [ ] **Step 1: Register the new Java check in the bundled rulesets**

Add `dev.distrohelena.linter.checkstyle.checks.MultilineBlockLayoutCheck` to:
- `java/helena-linter-checkstyle/src/main/resources/helena_checks.xml`
- `java/samples/config/checkstyle/checkstyle.xml`

- [ ] **Step 2: Document the Java rule**

Add a README entry describing:
- non-empty code blocks must be multiline
- empty blocks may remain single-line
- initializer-style braces are out of scope

- [ ] **Step 3: Run a Java resource/build sanity check**

Run:
```bash
cd /home/helena/dev/helena-linter/java
./gradlew :helena-linter-checkstyle:processResources
```

Expected: PASS.

- [ ] **Step 4: Commit**

```bash
cd /home/helena/dev/helena-linter
git add java/helena-linter-checkstyle/src/main/resources/helena_checks.xml java/samples/config/checkstyle/checkstyle.xml java/README.md
git commit -m "feat: register java multiline block layout rule"
```

### Task 4: Implement The Java Multiline Block Layout Check

**Files:**
- Create: `java/helena-linter-checkstyle/src/main/java/dev/distrohelena/linter/checkstyle/checks/MultilineBlockLayoutCheck.java`
- Create: `java/helena-linter-checkstyle/src/test/java/dev/distrohelena/linter/checkstyle/checks/MultilineBlockLayoutCheckTests.java`
- Create: `java/helena-linter-checkstyle/src/test/resources/samples/multiline-block-layout/invalid.java`
- Create: `java/helena-linter-checkstyle/src/test/resources/samples/multiline-block-layout/valid.java`
- Modify: `java/helena-linter-checkstyle/src/test/java/dev/distrohelena/linter/checkstyle/checks/CheckstyleCheckTestSupport.java` only if a helper is missing

- [ ] **Step 1: Write the failing Java tests**

Cover:
- single-line non-empty `if` block
- single-line non-empty method body
- `try { work(); } catch {}` pattern
- valid multiline blocks
- valid empty blocks

- [ ] **Step 2: Run the focused Java tests to verify failure**

Run:
```bash
cd /home/helena/dev/helena-linter/java
./gradlew :helena-linter-checkstyle:test --tests '*MultilineBlockLayoutCheckTests'
```

Expected: FAIL because the check does not exist yet.

- [ ] **Step 3: Implement `MultilineBlockLayoutCheck`**

Inspect `SLIST` nodes and report only when:
- the block is non-empty
- the block opens and closes on a single line with executable content
- the node represents an actual code block

Do not target initializer-style braces.

- [ ] **Step 4: Run the focused Java tests again**

Run the same command as Step 2.

Expected: PASS.

- [ ] **Step 5: Run full Java verification**

Run:
```bash
cd /home/helena/dev/helena-linter/java
./gradlew :helena-linter-checkstyle:test
./gradlew :helena-linter-checkstyle:build
```

Expected: PASS.

- [ ] **Step 6: Commit**

```bash
cd /home/helena/dev/helena-linter
git add java
git commit -m "feat: add java multiline block layout check"
```

### Task 5: Final Cross-Language Alignment Check

**Files:**
- Modify: `README.md` only if the root rule summaries need updating
- Modify: `typescript/README.md` only if cross-language wording drift is discovered during final review

- [ ] **Step 1: Compare the three package READMEs**

Ensure TypeScript, C#, and Java all describe the rule with the same semantics:
- non-empty code blocks must be multiline
- empty blocks may be single-line
- initializers are excluded

- [ ] **Step 2: Run final targeted verification across both implementations**

Run:
```bash
cd /home/helena/dev/helena-linter
dotnet test csharp/DistroHelena.Linter.CSharp.sln
cd /home/helena/dev/helena-linter/java
./gradlew test
```

Expected: PASS.

- [ ] **Step 3: Commit any wording-only alignment changes**

```bash
cd /home/helena/dev/helena-linter
git add README.md typescript/README.md csharp/README.md csharp/DistroHelena.Linter.CSharp/README.md java/README.md
git commit -m "docs: align multiline block layout rule wording"
```
