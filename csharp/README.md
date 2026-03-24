# C# Helena Linter

This folder contains the Roslyn analyzer package for Helena’s C# readability rules.

## Package

The analyzer package lives in `DistroHelena.Linter.CSharp` and is intended to be consumed from C# projects through a `PackageReference` or analyzer project reference.

## First-Pass Rules

Examples below show the intended shape for each analyzer and code fix.

### `HLN001` `redundant-else-if`

Rewrites complementary `else if` branches to plain `else`.

Bad:

```csharp
if (value is null)
{
    HandleMissing();
}
else if (value is not null)
{
    HandleValue(value);
}
```

Good:

```csharp
if (value is null)
{
    HandleMissing();
}
else
{
    HandleValue(value);
}
```

### `HLN002` `if-following-spacing`

Inserts a blank line after a completed `if` or `if / else` chain before the next sibling statement.

Bad:

```csharp
if (ready)
{
    Run();
}
Cleanup();
```

Good:

```csharp
if (ready)
{
    Run();
}

Cleanup();
```

### `HLN003` `control-block-following-spacing`

Inserts a blank line after `for`, `while`, `do`, `switch`, and `try` statements before the next sibling statement.

Bad:

```csharp
for (var i = 0; i < items.Length; i++)
{
    Process(items[i]);
}
Finish();
```

Good:

```csharp
for (var i = 0; i < items.Length; i++)
{
    Process(items[i]);
}

Finish();
```

### `HLN004` `exit-spacing`

Inserts a blank line before `return`, `throw`, `break`, and `continue`.

Bad:

```csharp
UpdateState();
return value;
```

Good:

```csharp
UpdateState();

return value;
```

### `HLN005` `declaration-spacing`

Inserts a blank line after local declarations before the next sibling statement.

Bad:

```csharp
var result = Compute();
Render(result);
```

Good:

```csharp
var result = Compute();

Render(result);
```

### `HLN006` `declaration-leading-spacing`

Inserts a blank line before local declarations that follow non-declaration statements.

Bad:

```csharp
LogStart();
var result = Compute();
```

Good:

```csharp
LogStart();

var result = Compute();
```

### `HLN007` `if-leading-spacing`

Inserts a blank line before `if` statements that follow earlier sibling statements.

Bad:

```csharp
Prepare();
if (ready)
{
    Run();
}
```

Good:

```csharp
Prepare();

if (ready)
{
    Run();
}
```

### `HLN008` `if-else-if-chain`

Folds sibling `if` statements into an `else if` chain when the earlier branch definitely exits.

Bad:

```csharp
if (primary)
{
    return "a";
}

if (secondary)
{
    return "b";
}
```

Good:

```csharp
if (primary)
{
    return "a";
}
else if (secondary)
{
    return "b";
}
```

### `HLN009` `early-return`

Rewrites supported `if` and `if / else` control-flow shapes into early-return guard clauses.

Bad:

```csharp
if (IsValid(user))
{
    Save(user);
    return true;
}

return false;
```

Good:

```csharp
if (!IsValid(user))
{
    return false;
}

Save(user);
return true;
```

### `HLN010` `control-body-braces`

Wraps single-statement control-flow bodies such as `if`, `else`, `for`, `foreach`, `while`, `do`, `using`, `lock`, and `fixed` in braces.

Bad:

```csharp
if (ready)
    Run();
```

Good:

```csharp
if (ready)
{
    Run();
}
```

### `HLN011` `multiline-block-layout`

Requires non-empty code blocks to use multiline layout. Empty blocks may remain single-line, and object, collection, and anonymous initializer braces are excluded.

Bad:

```csharp
if (ready) { Run(); }
```

Good:

```csharp
if (ready)
{
    Run();
}
```

## Deferred Rules

There are no remaining deferred Helena rules in the current C# package scope.

## Local Commands

```bash
dotnet build DistroHelena.Linter.CSharp.sln
dotnet test DistroHelena.Linter.CSharp.sln
dotnet build samples/SampleConsumer.csproj
```

## Sample Consumer

See `samples/` for a minimal analyzer consumer project that references the local analyzer project as a Roslyn analyzer.
