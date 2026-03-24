# Helena Linter

Helena Linter is a multi-package repository for Helena's readability and control-flow lint rules.

It currently contains:

- [TypeScript package](./typescript/README.md): `@distrohelena/linter`
- [C# package](./csharp/README.md): `DistroHelena.Linter.CSharp`
- [Java package](./java/README.md): Helena Checkstyle rules packaged in
  `helena-linter-checkstyle`
- [Go package](./go/README.md): `github.com/distrohelena/helena-linter/go`, the Helena Go
  analyzer bundle and per-rule packages

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

### Java

The Java package is published as the `helena-linter-checkstyle` Checkstyle extension jar and
provides the Helena Java readability rules for Gradle, Maven, or raw Checkstyle consumers.

Published coordinates:

```text
com.distrohelena:helena-linter-checkstyle:0.1.0
```

Use it from a Gradle project with:

```kotlin
dependencies {
  checkstyle(project(":helena-linter-checkstyle"))
}
```

See [java/README.md](./java/README.md) for the full rule list, consumer instructions, and local
verification commands.

### Go

The Go package is published as the `github.com/distrohelena/helena-linter/go` module and provides
Helena analyzers for Go projects through `golang.org/x/tools/go/analysis`.

Import `github.com/distrohelena/helena-linter/go/analyzers/<rule>` when you want one analyzer, or
import `github.com/distrohelena/helena-linter/go/bundle` to wire the recommended analyzer set into
your own `singlechecker` or `multichecker` tool.

See [go/README.md](./go/README.md) for the package scope, supported-rule list, and local
verification command.

## Repository Layout

- [`typescript/`](./typescript/) contains the npm package and ESLint rule implementation.
- [`csharp/`](./csharp/) contains the Roslyn analyzer package, tests, and sample consumer.
- [`go/`](./go/) contains the Go analyzer package and shared helper scaffolding.
- [`docs/`](./docs/) contains design and planning notes for repository evolution.
