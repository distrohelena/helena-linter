using DistroHelena.Linter.CSharp.Diagnostics;
using System.Threading.Tasks;
using Xunit;
using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.CodeFixVerifier<
    DistroHelena.Linter.CSharp.Analyzers.DeclarationLeadingSpacingAnalyzer,
    DistroHelena.Linter.CSharp.CodeFixes.DeclarationLeadingSpacingCodeFixProvider>;

namespace DistroHelena.Linter.CSharp.Tests.Analyzers;

/// <summary>
/// Covers analyzer and code-fix behavior for blank lines before declarations.
/// </summary>
public class DeclarationLeadingSpacingAnalyzerTests
{
    /// <summary>
    /// Verifies a blank line is inserted before a declaration that follows a non-declaration statement.
    /// </summary>
    [Fact]
    public async Task VerifyCodeFixAsync_AddsBlankLineBeforeDeclaration()
    {
        const string source = """
            class Sample
            {
                void Run()
                {
                    System.Console.WriteLine("start");
                    {|#0:int|} count = 1;
                }
            }
            """;

        const string fixedSource = """
            class Sample
            {
                void Run()
                {
                    System.Console.WriteLine("start");

                    int count = 1;
                }
            }
            """;

        await VerifyCS.VerifyCodeFixAsync(
            source,
            VerifyCS.Diagnostic(HelenaDiagnosticDescriptors.DeclarationLeadingSpacing).WithLocation(0),
            fixedSource);
    }

    /// <summary>
    /// Verifies the first declaration in a block remains valid.
    /// </summary>
    [Fact]
    public async Task VerifyAnalyzerAsync_AllowsFirstDeclaration()
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
}
