using DistroHelena.Linter.CSharp.Diagnostics;
using System.Threading.Tasks;
using Xunit;
using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.CodeFixVerifier<
    DistroHelena.Linter.CSharp.Analyzers.IfLeadingSpacingAnalyzer,
    DistroHelena.Linter.CSharp.CodeFixes.IfLeadingSpacingCodeFixProvider>;

namespace DistroHelena.Linter.CSharp.Tests.Analyzers;

/// <summary>
/// Covers analyzer and code-fix behavior for blank lines before <c>if</c> statements.
/// </summary>
public class IfLeadingSpacingAnalyzerTests
{
    /// <summary>
    /// Verifies a blank line is inserted before an <c>if</c> statement that follows another statement.
    /// </summary>
    [Fact]
    public async Task VerifyCodeFixAsync_AddsBlankLineBeforeIfStatement()
    {
        const string source = """
            class Sample
            {
                void Run(bool flag)
                {
                    int count = 1;
                    {|#0:if|} (flag)
                    {
                        System.Console.WriteLine(count);
                    }
                }
            }
            """;

        const string fixedSource = """
            class Sample
            {
                void Run(bool flag)
                {
                    int count = 1;

                    if (flag)
                    {
                        System.Console.WriteLine(count);
                    }
                }
            }
            """;

        await VerifyCS.VerifyCodeFixAsync(
            source,
            VerifyCS.Diagnostic(HelenaDiagnosticDescriptors.IfLeadingSpacing).WithLocation(0),
            fixedSource);
    }

    /// <summary>
    /// Verifies the first <c>if</c> statement in a block remains valid.
    /// </summary>
    [Fact]
    public async Task VerifyAnalyzerAsync_AllowsFirstIfStatement()
    {
        const string source = """
            class Sample
            {
                void Run(bool flag)
                {
                    if (flag)
                    {
                        System.Console.WriteLine(flag);
                    }
                }
            }
            """;

        await VerifyCS.VerifyAnalyzerAsync(source);
    }
}
