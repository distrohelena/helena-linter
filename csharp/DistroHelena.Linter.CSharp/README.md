# DistroHelena.Linter.CSharp

Roslyn analyzers and code fixes for Helena’s C# readability rules.

## Included Rule IDs

- `HLN001` redundant else-if
- `HLN002` if following spacing
- `HLN003` control-block following spacing
- `HLN004` exit spacing
- `HLN005` declaration spacing
- `HLN006` declaration-leading spacing
- `HLN007` if-leading spacing
- `HLN008` if-else-if-chain
- `HLN009` early-return
- `HLN010` control-body-braces
- `HLN011` multiline-block-layout

## Usage

Install the package in a C# project:

```xml
<ItemGroup>
  <PackageReference Include="DistroHelena.Linter.CSharp" Version="0.1.0" PrivateAssets="all" />
</ItemGroup>
```

Or reference the project directly as an analyzer during local development:

```xml
<ItemGroup>
  <ProjectReference Include="../DistroHelena.Linter.CSharp/DistroHelena.Linter.CSharp.csproj"
                    OutputItemType="Analyzer"
                    ReferenceOutputAssembly="false" />
</ItemGroup>
```

## Multiline Block Layout

`HLN011` requires non-empty code blocks to be written across multiple lines.

- Empty blocks may remain single-line.
- Object, collection, and anonymous object initializer braces are excluded from this rule.
