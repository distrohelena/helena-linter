using DistroHelena.Linter.CSharp.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;
using System.Threading.Tasks;
using Xunit;
using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.CodeFixVerifier<
    DistroHelena.Linter.CSharp.Analyzers.MultilineBlockLayoutAnalyzer,
    DistroHelena.Linter.CSharp.CodeFixes.MultilineBlockLayoutCodeFixProvider>;

namespace DistroHelena.Linter.CSharp.Tests.Analyzers;

#pragma warning disable CS0618

/// <summary>
/// Covers analyzer and code-fix behavior for multiline layout of non-empty blocks.
/// </summary>
public class MultilineBlockLayoutAnalyzerTests
{
    /// <summary>
    /// Verifies a single-line <c>if</c> body is rewritten onto multiple lines.
    /// </summary>
    [Fact]
    public async Task VerifyCodeFixAsync_FormatsIfBodyOntoMultipleLines()
    {
        const string source = """
            class Sample
            {
                void Run(bool flag)
                {
                    if (flag) {|#0:{|} return; }
                }
            }
            """;

        const string fixedSource = """
            class Sample
            {
                void Run(bool flag)
                {
                    if (flag)
                    {
                        return;
                    }
                }
            }
            """;

        await VerifyCS.VerifyCodeFixAsync(
            source,
            VerifyCS.Diagnostic(HelenaDiagnosticDescriptors.MultilineBlockLayout).WithLocation(0),
            fixedSource);
    }

    /// <summary>
    /// Verifies a single-line method body is rewritten onto multiple lines.
    /// </summary>
    [Fact]
    public async Task VerifyCodeFixAsync_FormatsMethodBodyOntoMultipleLines()
    {
        const string source = """
            class Sample
            {
                void Run() {|#0:{|} System.Console.WriteLine(); }
            }
            """;

        const string fixedSource = """
            class Sample
            {
                void Run()
                {
                    System.Console.WriteLine();
                }
            }
            """;

        await VerifyCS.VerifyCodeFixAsync(
            source,
            VerifyCS.Diagnostic(HelenaDiagnosticDescriptors.MultilineBlockLayout).WithLocation(0),
            fixedSource);
    }

    /// <summary>
    /// Verifies a single-line <c>try</c> body is rewritten while leaving an empty <c>catch</c> block valid.
    /// </summary>
    [Fact]
    public async Task VerifyCodeFixAsync_FormatsTryBlockOntoMultipleLines()
    {
        const string source = """
            class Sample
            {
                void Run()
                {
                    try {|#0:{|} System.Console.WriteLine(); } catch {}
                }
            }
            """;

        const string fixedSource = """
            class Sample
            {
                void Run()
                {
                    try
                    {
                        System.Console.WriteLine();
                    }
                    catch { }
                }
            }
            """;

        await VerifyCS.VerifyCodeFixAsync(
            source,
            VerifyCS.Diagnostic(HelenaDiagnosticDescriptors.MultilineBlockLayout).WithLocation(0),
            fixedSource);
    }

    /// <summary>
    /// Verifies the code fix preserves trailing comments attached to the closing brace.
    /// </summary>
    [Fact]
    public async Task VerifyCodeFixAsync_PreservesTrailingCommentAfterBlock()
    {
        const string source = """
            class Sample
            {
                void Run()
                {
                    if (true) {|#0:{|} Foo(); } // keep
                }

                void Foo()
                {
                }
            }
            """;

        const string fixedSource = """
            class Sample
            {
                void Run()
                {
                    if (true)
                    {
                        Foo();
                    } // keep
                }

                void Foo()
                {
                }
            }
            """;

        await VerifyCS.VerifyCodeFixAsync(
            source,
            VerifyCS.Diagnostic(HelenaDiagnosticDescriptors.MultilineBlockLayout).WithLocation(0),
            fixedSource);
    }

    /// <summary>
    /// Verifies the code fix splits multiple statements onto separate lines inside a block.
    /// </summary>
    [Fact]
    public async Task VerifyCodeFixAsync_SplitsMultipleStatementsOntoSeparateLines()
    {
        const string source = """
            class Sample
            {
                void Run() {|#0:{|} Foo(); Bar(); }

                void Foo()
                {
                }

                void Bar()
                {
                }
            }
            """;

        const string fixedSource = """
            class Sample
            {
                void Run()
                {
                    Foo();
                    Bar();
                }

                void Foo()
                {
                }

                void Bar()
                {
                }
            }
            """;

        await VerifyCS.VerifyCodeFixAsync(
            source,
            VerifyCS.Diagnostic(HelenaDiagnosticDescriptors.MultilineBlockLayout).WithLocation(0),
            fixedSource);
    }

    /// <summary>
    /// Verifies blocks that are already multiline or empty remain valid.
    /// </summary>
    [Fact]
    public async Task VerifyAnalyzerAsync_AllowsMultilineAndEmptyBlocks()
    {
        const string source = """
            class Sample
            {
                void Run()
                {
                    if (true)
                    {
                        return;
                    }

                    void Local()
                    {
                    }

                    try
                    {
                        System.Console.WriteLine();
                    }
                    catch
                    {
                    }
                }
            }
            """;

        await VerifyCS.VerifyAnalyzerAsync(source);
    }

    /// <summary>
    /// Verifies object initializer braces are not treated as code blocks.
    /// </summary>
    [Fact]
    public async Task VerifyAnalyzerAsync_IgnoresObjectInitializers()
    {
        const string source = """
            class Sample
            {
                void Run()
                {
                    var point = new Point { X = 1, Y = 2 };
                }
            }

            class Point
            {
                public int X { get; set; }
                public int Y { get; set; }
            }
            """;

        await VerifyCS.VerifyAnalyzerAsync(source);
    }
}

#pragma warning restore CS0618
