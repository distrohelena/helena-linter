# Helena Linter

Helena Linter is a multi-package repository for Helena's readability and control-flow lint rules.

It currently contains:

- [TypeScript package](./typescript/README.md): `@distrohelena/linter`
- [C# package](./csharp/README.md): `DistroHelena.Linter.CSharp`

## Packages

### TypeScript

The TypeScript package is published as `@distrohelena/linter` and provides ESLint flat-config rules for TypeScript projects.

Install:

```bash
npm install -D @distrohelena/linter eslint typescript @typescript-eslint/parser
```

Use:

```ts
import helenaLinter from "@distrohelena/linter";

export default [...helenaLinter.configs.recommended];
```

See [typescript/README.md](./typescript/README.md) for the full rule list, per-rule imports, and local verification commands.

### C#

The C# package is published as `DistroHelena.Linter.CSharp` and provides Roslyn analyzers and code fixes for C# projects.

Use it from a project with:

```xml
<ItemGroup>
  <PackageReference Include="DistroHelena.Linter.CSharp" Version="0.1.0" PrivateAssets="all" />
</ItemGroup>
```

See [csharp/README.md](./csharp/README.md) for the current rule list, local development commands, and sample-consumer guidance.

## Repository Layout

- [`typescript/`](./typescript/) contains the npm package and ESLint rule implementation.
- [`csharp/`](./csharp/) contains the Roslyn analyzer package, tests, and sample consumer.
- [`docs/`](./docs/) contains design and planning notes for repository evolution.
