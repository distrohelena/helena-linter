# CSharp and Java Multiline Block Layout Design

## Goal
Add the same multiline block layout rule to the C# and Java Helena packages so their behavior matches the existing TypeScript rule.

The rule should enforce multiline layout for non-empty code blocks while still allowing empty blocks like `{}` to remain on one line.

## Scope
The rule applies only to actual code block syntax.

Included examples:
- method and constructor bodies
- accessor bodies
- control-flow blocks such as `if`, `else`, loops, `try`, `catch`, and `finally`
- other statement or member bodies that are represented as code blocks in the language syntax

Excluded examples:
- object initializers
- collection initializers
- anonymous object initializers
- other data-literal or initializer braces that are not code blocks

This keeps the C# and Java rule aligned with the existing TypeScript `multiline-block-layout` rule.

## Shared Semantics
The rule semantics should be identical across all three language packages:

- report a non-empty block when executable content is laid out on the same line as the surrounding braces
- allow empty blocks like `{}` to remain single-line
- leave initializer-style braces untouched

Examples:

C# invalid:
```csharp
if (flag) { return; }
```

C# valid after fix:
```csharp
if (flag) {
    return;
}
```

Java invalid:
```java
try { current.close(); } catch {}
```

Java valid:
```java
try {
    current.close();
} catch {}
```

## C# Design
Add a new Roslyn analyzer and code fix under the C# package.

Planned shape:
- diagnostic id: `HLN011`
- analyzer over `BlockSyntax`
- code fix that rewrites non-empty single-line blocks into multiline blocks and then relies on Roslyn formatting

The rule should stay separate from `HLN010 control-body-braces`:
- `HLN010` answers whether a control-flow body must use braces
- `HLN011` answers whether a non-empty block must be multiline

The C# implementation should include tests for:
- single-line non-empty `if` blocks
- single-line non-empty method bodies
- mixed `try` non-empty and `catch {}` empty blocks
- valid multiline blocks
- valid empty blocks
- object initializer non-target cases

## Java Design
Add a new Checkstyle check under the Java package.

Planned shape:
- check class: `MultilineBlockLayoutCheck`
- inspect `SLIST` nodes
- report non-empty single-line code blocks
- leave empty blocks alone

The Java rule should also stay separate from `ControlBodyBracesCheck`:
- `ControlBodyBracesCheck` enforces the presence of braces for brace-optional control bodies
- `MultilineBlockLayoutCheck` enforces multiline layout once a block exists

The Java implementation should include tests for:
- single-line non-empty control-flow blocks
- single-line non-empty method bodies
- non-empty `try` plus empty `catch {}`
- valid multiline blocks
- valid empty blocks
- non-target initializer cases where applicable

## Documentation and Configuration
The new rule should be documented in:
- `csharp/README.md`
- `csharp/DistroHelena.Linter.CSharp/README.md`
- `java/README.md`

Java should also register the new check in:
- the bundled `helena_checks.xml`
- the sample consumer Checkstyle configuration

## Verification
Verification should include:

C#:
- `dotnet test csharp/DistroHelena.Linter.CSharp.sln`
- `dotnet build csharp/DistroHelena.Linter.CSharp.sln`

Java:
- `./gradlew :helena-linter-checkstyle:test`
- `./gradlew :helena-linter-checkstyle:build`

## Success Criteria
This work is complete when:
- C# and Java both have a multiline block layout rule with the same block-only semantics as TypeScript
- empty blocks remain valid on one line in both languages
- initializer braces remain out of scope in both languages
- all C# and Java tests pass
- the package documentation reflects the new rule
