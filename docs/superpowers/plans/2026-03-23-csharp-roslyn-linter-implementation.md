# C# Roslyn Linter Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build a new `csharp` Roslyn analyzer package inside `helena-linter` that ships the first pass of Helena’s easiest auto-fixable C# rules.

**Architecture:** Add a standalone `.NET` solution under `csharp` with one analyzer package project and one test project. Keep diagnostics, shared Roslyn syntax helpers, and rule-specific analyzers/code fixes split into focused files so new rules can be added without growing monoliths.

**Tech Stack:** .NET SDK 9, Roslyn analyzers/code fixes, `Microsoft.CodeAnalysis.CSharp`, `Microsoft.CodeAnalysis.CSharp.Workspaces`, `Microsoft.CodeAnalysis.CSharp.Analyzer.Testing`, xUnit or MSTest-compatible Roslyn test harness, NuGet package metadata.

---

### Task 1: Create The C# Solution Skeleton

**Files:**
- Create: `csharp/DistroHelena.Linter.CSharp.sln`
- Create: `csharp/DistroHelena.Linter.CSharp/DistroHelena.Linter.CSharp.csproj`
- Create: `csharp/DistroHelena.Linter.CSharp.Tests/DistroHelena.Linter.CSharp.Tests.csproj`
- Create: `csharp/README.md`
- Modify: `docs/superpowers/specs/2026-03-23-csharp-roslyn-linter-design.md` only if naming drift is discovered

- [ ] **Step 1: Create the solution and projects**

Run:
```bash
cd /home/helena/dev/helena-linter
mkdir -p csharp
cd csharp
dotnet new sln -n DistroHelena.Linter.CSharp
dotnet new classlib -n DistroHelena.Linter.CSharp
dotnet new xunit -n DistroHelena.Linter.CSharp.Tests
dotnet sln add DistroHelena.Linter.CSharp/DistroHelena.Linter.CSharp.csproj
dotnet sln add DistroHelena.Linter.CSharp.Tests/DistroHelena.Linter.CSharp.Tests.csproj
dotnet add DistroHelena.Linter.CSharp.Tests/DistroHelena.Linter.CSharp.Tests.csproj reference DistroHelena.Linter.CSharp/DistroHelena.Linter.CSharp.csproj
```

Expected: solution and both projects are created.

- [ ] **Step 2: Convert the analyzer project into a Roslyn analyzer package**

Modify `csharp/DistroHelena.Linter.CSharp/DistroHelena.Linter.CSharp.csproj` to:
- target a current analyzer-friendly `netstandard2.0` or `net8.0`-compatible analyzer setup chosen for Roslyn package compatibility
- include Roslyn analyzer/code-fix package references
- include NuGet metadata for package name, description, license, repository, and analyzer packaging

- [ ] **Step 3: Configure the test project for Roslyn analyzer testing**

Modify `csharp/DistroHelena.Linter.CSharp.Tests/DistroHelena.Linter.CSharp.Tests.csproj` to include:
- Roslyn C# analyzer testing packages
- the chosen unit test framework runner
- references needed for code-fix verification

- [ ] **Step 4: Add a minimal C# README**

Write `csharp/README.md` with:
- package purpose
- first-pass rule list
- local commands: `dotnet build`, `dotnet test`

- [ ] **Step 5: Verify the empty solution builds**

Run:
```bash
cd /home/helena/dev/helena-linter/csharp
dotnet build DistroHelena.Linter.CSharp.sln
dotnet test DistroHelena.Linter.CSharp.sln
```

Expected: build and test succeed with the empty scaffold.

- [ ] **Step 6: Commit**

```bash
cd /home/helena/dev/helena-linter
git add csharp docs/superpowers/plans/2026-03-23-csharp-roslyn-linter-implementation.md
git commit -m "feat: scaffold csharp roslyn linter package"
```

### Task 2: Add Shared Analyzer Infrastructure

**Files:**
- Create: `csharp/DistroHelena.Linter.CSharp/Diagnostics/HelenaDiagnosticDescriptors.cs`
- Create: `csharp/DistroHelena.Linter.CSharp/Diagnostics/HelenaDiagnosticCategories.cs`
- Create: `csharp/DistroHelena.Linter.CSharp/Helpers/SyntaxTriviaHelpers.cs`
- Create: `csharp/DistroHelena.Linter.CSharp/Helpers/StatementSequenceHelpers.cs`
- Create: `csharp/DistroHelena.Linter.CSharp/Helpers/ConditionComparisonHelpers.cs`
- Create: `csharp/DistroHelena.Linter.CSharp/CodeFixes/CodeFixConstants.cs`
- Test: `csharp/DistroHelena.Linter.CSharp.Tests/Helpers/ConditionComparisonHelpersTests.cs`

- [ ] **Step 1: Write the helper tests first**

Add `ConditionComparisonHelpersTests.cs` covering:
- `if (value == null)` vs `else if (value != null)`
- `if (!flag)` vs `else if (flag)`
- a negative case that should not count as complementary

- [ ] **Step 2: Run the helper test to verify it fails**

Run:
```bash
cd /home/helena/dev/helena-linter/csharp
dotnet test DistroHelena.Linter.CSharp.sln --filter ConditionComparisonHelpers
```

Expected: FAIL because the helper does not exist yet.

- [ ] **Step 3: Implement shared descriptor and helper classes**

Add focused classes for:
- diagnostic IDs `HLN001` through the initial first-pass set
- shared severity/category metadata
- syntax trivia spacing detection/replacement helpers
- sibling statement/container enumeration
- complementary `else if` detection

- [ ] **Step 4: Run the helper test to verify it passes**

Run:
```bash
cd /home/helena/dev/helena-linter/csharp
dotnet test DistroHelena.Linter.CSharp.sln --filter ConditionComparisonHelpers
```

Expected: PASS.

- [ ] **Step 5: Run the full solution**

Run:
```bash
cd /home/helena/dev/helena-linter/csharp
dotnet build DistroHelena.Linter.CSharp.sln
dotnet test DistroHelena.Linter.CSharp.sln
```

Expected: PASS.

- [ ] **Step 6: Commit**

```bash
cd /home/helena/dev/helena-linter
git add csharp
git commit -m "feat: add csharp analyzer infrastructure"
```

### Task 3: Implement `redundant-else-if`

**Files:**
- Create: `csharp/DistroHelena.Linter.CSharp/Analyzers/RedundantElseIfAnalyzer.cs`
- Create: `csharp/DistroHelena.Linter.CSharp/CodeFixes/RedundantElseIfCodeFixProvider.cs`
- Test: `csharp/DistroHelena.Linter.CSharp.Tests/Analyzers/RedundantElseIfAnalyzerTests.cs`

- [ ] **Step 1: Write failing analyzer and code-fix tests**

Cover:
- `if (value == null) { ... } else if (value != null) { ... }`
- `if (!flag) { ... } else if (flag) { ... }`
- fixed output uses plain `else`
- comments/trivia around `else if` are preserved

- [ ] **Step 2: Run the rule tests to verify they fail**

Run:
```bash
cd /home/helena/dev/helena-linter/csharp
dotnet test DistroHelena.Linter.CSharp.sln --filter RedundantElseIfAnalyzerTests
```

Expected: FAIL because analyzer/code fix do not exist yet.

- [ ] **Step 3: Implement the analyzer**

Detect only exact complementary `else if` patterns using `ConditionComparisonHelpers`.

- [ ] **Step 4: Implement the code fix**

Rewrite the `else if` syntax to `else` with minimal trivia changes.

- [ ] **Step 5: Run the rule tests again**

Run:
```bash
cd /home/helena/dev/helena-linter/csharp
dotnet test DistroHelena.Linter.CSharp.sln --filter RedundantElseIfAnalyzerTests
```

Expected: PASS.

- [ ] **Step 6: Commit**

```bash
cd /home/helena/dev/helena-linter
git add csharp
git commit -m "feat: add csharp redundant else-if analyzer"
```

### Task 4: Implement Blank-Line-After Rules

**Files:**
- Create: `csharp/DistroHelena.Linter.CSharp/Analyzers/IfFollowingSpacingAnalyzer.cs`
- Create: `csharp/DistroHelena.Linter.CSharp/CodeFixes/IfFollowingSpacingCodeFixProvider.cs`
- Create: `csharp/DistroHelena.Linter.CSharp/Analyzers/ControlBlockFollowingSpacingAnalyzer.cs`
- Create: `csharp/DistroHelena.Linter.CSharp/CodeFixes/ControlBlockFollowingSpacingCodeFixProvider.cs`
- Test: `csharp/DistroHelena.Linter.CSharp.Tests/Analyzers/IfFollowingSpacingAnalyzerTests.cs`
- Test: `csharp/DistroHelena.Linter.CSharp.Tests/Analyzers/ControlBlockFollowingSpacingAnalyzerTests.cs`

- [ ] **Step 1: Write failing tests for `if-following-spacing`**

Cover:
- `if` followed immediately by a sibling statement
- `if / else if / else` chain followed by a sibling statement
- already-spaced case should stay valid

- [ ] **Step 2: Write failing tests for `control-block-following-spacing`**

Cover:
- `for`, `while`, `do`, `switch`, `try`
- already-spaced case should stay valid

- [ ] **Step 3: Run the tests to verify failure**

Run:
```bash
cd /home/helena/dev/helena-linter/csharp
dotnet test DistroHelena.Linter.CSharp.sln --filter FollowingSpacingAnalyzerTests
```

Expected: FAIL.

- [ ] **Step 4: Implement the analyzers**

Use `StatementSequenceHelpers` to inspect sibling statements within the same container.

- [ ] **Step 5: Implement the code fixes**

Use `SyntaxTriviaHelpers` to insert exactly one blank line while preserving indentation.

- [ ] **Step 6: Run the tests again**

Run:
```bash
cd /home/helena/dev/helena-linter/csharp
dotnet test DistroHelena.Linter.CSharp.sln --filter FollowingSpacingAnalyzerTests
```

Expected: PASS.

- [ ] **Step 7: Commit**

```bash
cd /home/helena/dev/helena-linter
git add csharp
git commit -m "feat: add csharp following spacing analyzers"
```

### Task 5: Implement Blank-Line-Before Rules

**Files:**
- Create: `csharp/DistroHelena.Linter.CSharp/Analyzers/ExitSpacingAnalyzer.cs`
- Create: `csharp/DistroHelena.Linter.CSharp/CodeFixes/ExitSpacingCodeFixProvider.cs`
- Create: `csharp/DistroHelena.Linter.CSharp/Analyzers/DeclarationLeadingSpacingAnalyzer.cs`
- Create: `csharp/DistroHelena.Linter.CSharp/CodeFixes/DeclarationLeadingSpacingCodeFixProvider.cs`
- Create: `csharp/DistroHelena.Linter.CSharp/Analyzers/IfLeadingSpacingAnalyzer.cs`
- Create: `csharp/DistroHelena.Linter.CSharp/CodeFixes/IfLeadingSpacingCodeFixProvider.cs`
- Test: `csharp/DistroHelena.Linter.CSharp.Tests/Analyzers/ExitSpacingAnalyzerTests.cs`
- Test: `csharp/DistroHelena.Linter.CSharp.Tests/Analyzers/DeclarationLeadingSpacingAnalyzerTests.cs`
- Test: `csharp/DistroHelena.Linter.CSharp.Tests/Analyzers/IfLeadingSpacingAnalyzerTests.cs`

- [ ] **Step 1: Write failing tests for `exit-spacing`**

Cover:
- `return`
- `throw`
- `break`
- `continue`

- [ ] **Step 2: Write failing tests for `declaration-leading-spacing` and `if-leading-spacing`**

Cover:
- declaration after non-declaration statement
- `if` after non-final sibling statement
- first statement should remain valid

- [ ] **Step 3: Run the tests to verify failure**

Run:
```bash
cd /home/helena/dev/helena-linter/csharp
dotnet test DistroHelena.Linter.CSharp.sln --filter "ExitSpacingAnalyzerTests|DeclarationLeadingSpacingAnalyzerTests|IfLeadingSpacingAnalyzerTests"
```

Expected: FAIL.

- [ ] **Step 4: Implement the analyzers**

Use sibling inspection helpers instead of ad hoc tree walking per rule.

- [ ] **Step 5: Implement the code fixes**

Insert one blank line before the target statement while preserving trivia.

- [ ] **Step 6: Run the tests again**

Run:
```bash
cd /home/helena/dev/helena-linter/csharp
dotnet test DistroHelena.Linter.CSharp.sln --filter "ExitSpacingAnalyzerTests|DeclarationLeadingSpacingAnalyzerTests|IfLeadingSpacingAnalyzerTests"
```

Expected: PASS.

- [ ] **Step 7: Commit**

```bash
cd /home/helena/dev/helena-linter
git add csharp
git commit -m "feat: add csharp leading spacing analyzers"
```

### Task 6: Implement `declaration-spacing`

**Files:**
- Create: `csharp/DistroHelena.Linter.CSharp/Analyzers/DeclarationSpacingAnalyzer.cs`
- Create: `csharp/DistroHelena.Linter.CSharp/CodeFixes/DeclarationSpacingCodeFixProvider.cs`
- Test: `csharp/DistroHelena.Linter.CSharp.Tests/Analyzers/DeclarationSpacingAnalyzerTests.cs`

- [ ] **Step 1: Write the failing tests**

Cover:
- declaration followed by another statement without a blank line
- declaration followed by another declaration without the required blank line
- final declaration in a block should remain valid

- [ ] **Step 2: Run the tests to verify failure**

Run:
```bash
cd /home/helena/dev/helena-linter/csharp
dotnet test DistroHelena.Linter.CSharp.sln --filter DeclarationSpacingAnalyzerTests
```

Expected: FAIL.

- [ ] **Step 3: Implement the analyzer and code fix**

Reuse the same sibling/trivia helpers instead of duplicating spacing logic.

- [ ] **Step 4: Run the tests again**

Run:
```bash
cd /home/helena/dev/helena-linter/csharp
dotnet test DistroHelena.Linter.CSharp.sln --filter DeclarationSpacingAnalyzerTests
```

Expected: PASS.

- [ ] **Step 5: Commit**

```bash
cd /home/helena/dev/helena-linter
git add csharp
git commit -m "feat: add csharp declaration spacing analyzer"
```

### Task 7: Add Package Metadata And Consumer Docs

**Files:**
- Modify: `csharp/DistroHelena.Linter.CSharp/DistroHelena.Linter.CSharp.csproj`
- Modify: `csharp/README.md`
- Create: `csharp/samples/SampleConsumer.csproj`
- Create: `csharp/samples/Program.cs`

- [ ] **Step 1: Update package metadata**

Make sure the analyzer project includes:
- package id
- description
- repository/homepage/license metadata
- analyzer packaging settings

- [ ] **Step 2: Expand the C# README**

Document:
- install and usage
- rule list with Roslyn IDs
- first-pass scope and deferred rules
- local verification commands

- [ ] **Step 3: Add a tiny sample consumer**

Create a minimal sample project that references the analyzer project locally for smoke coverage and documentation.

- [ ] **Step 4: Build the sample consumer**

Run:
```bash
cd /home/helena/dev/helena-linter/csharp/samples
dotnet build
```

Expected: PASS.

- [ ] **Step 5: Commit**

```bash
cd /home/helena/dev/helena-linter
git add csharp
git commit -m "docs: add csharp analyzer package metadata"
```

### Task 8: Final Verification And Cleanup

**Files:**
- Modify: `csharp/**` only as needed from verification fallout

- [ ] **Step 1: Run formatting or cleanup if needed**

Run any required formatting command chosen during implementation.

- [ ] **Step 2: Run the full C# verification suite**

Run:
```bash
cd /home/helena/dev/helena-linter/csharp
dotnet build DistroHelena.Linter.CSharp.sln
dotnet test DistroHelena.Linter.CSharp.sln
```

Expected: PASS.

- [ ] **Step 3: Sanity-check repository state**

Run:
```bash
cd /home/helena/dev/helena-linter
git status --short
```

Expected: only intended `csharp` changes plus any known separate repo-layout changes.

- [ ] **Step 4: Commit final verification fixes**

```bash
cd /home/helena/dev/helena-linter
git add csharp
git commit -m "test: verify csharp analyzer package"
```
