using DistroHelena.Linter.CSharp.Diagnostics;
using System.Threading.Tasks;
using Xunit;
using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.CodeFixVerifier<
    DistroHelena.Linter.CSharp.Analyzers.DeclarationSpacingAnalyzer,
    DistroHelena.Linter.CSharp.CodeFixes.DeclarationSpacingCodeFixProvider>;

namespace DistroHelena.Linter.CSharp.Tests.Analyzers;

/// <summary>
/// Covers analyzer and code-fix behavior for blank lines after declarations.
/// </summary>
public class DeclarationSpacingAnalyzerTests
{
    /// <summary>
    /// Verifies a blank line is inserted after a declaration before a non-declaration statement.
    /// </summary>
    [Fact]
    public async Task VerifyCodeFixAsync_AddsBlankLineAfterDeclarationBeforeStatement()
    {
        const string source = """
            class Sample
            {
                void Run()
                {
                    {|#0:int|} count = 1;
                    System.Console.WriteLine(count);
                }
            }
            """;

        const string fixedSource = """
            class Sample
            {
                void Run()
                {
                    int count = 1;

                    System.Console.WriteLine(count);
                }
            }
            """;

        await VerifyCS.VerifyCodeFixAsync(
            source,
            VerifyCS.Diagnostic(HelenaDiagnosticDescriptors.DeclarationSpacing).WithLocation(0),
            fixedSource);
    }

    /// <summary>
    /// Verifies consecutive declarations remain valid without an inserted blank line.
    /// </summary>
    [Fact]
    public async Task VerifyAnalyzerAsync_AllowsConsecutiveDeclarations()
    {
        const string source = """
            class Sample
            {
                void Run()
                {
                    int count = 1;
                    int total = count + 1;
                }
            }
            """;

        await VerifyCS.VerifyAnalyzerAsync(source);
    }

    /// <summary>
    /// Verifies a final declaration in a block remains valid.
    /// </summary>
    [Fact]
    public async Task VerifyAnalyzerAsync_AllowsFinalDeclaration()
    {
        const string source = """
            class Sample
            {
                void Run()
                {
                    int count = 1;
                }
            }
            """;

        await VerifyCS.VerifyAnalyzerAsync(source);
    }
}
