using DistroHelena.Linter.CSharp.Diagnostics;
using System.Threading.Tasks;
using Xunit;
using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.CodeFixVerifier<
    DistroHelena.Linter.CSharp.Analyzers.IfFollowingSpacingAnalyzer,
    DistroHelena.Linter.CSharp.CodeFixes.IfFollowingSpacingCodeFixProvider>;

namespace DistroHelena.Linter.CSharp.Tests.Analyzers;

/// <summary>
/// Covers analyzer and code-fix behavior for blank lines after <c>if</c> chains.
/// </summary>
public class IfFollowingSpacingAnalyzerTests
{
    /// <summary>
    /// Verifies a missing blank line after a simple <c>if</c> block is inserted.
    /// </summary>
    [Fact]
    public async Task VerifyCodeFixAsync_AddsBlankLineAfterIfBlock()
    {
        const string source = """
            class Sample
            {
                void Run(bool flag)
                {
                    {|#0:if|} (flag)
                    {
                        return;
                    }
                    int count = 1;
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

                    int count = 1;
                }
            }
            """;

        await VerifyCS.VerifyCodeFixAsync(
            source,
            VerifyCS.Diagnostic(HelenaDiagnosticDescriptors.IfFollowingSpacing).WithLocation(0),
            fixedSource);
    }

    /// <summary>
    /// Verifies a completed <c>if / else</c> chain is separated from the next sibling statement.
    /// </summary>
    [Fact]
    public async Task VerifyCodeFixAsync_AddsBlankLineAfterIfElseChain()
    {
        const string source = """
            class Sample
            {
                void Run(bool flag)
                {
                    {|#0:if|} (flag)
                    {
                        return;
                    }
                    else
                    {
                        return;
                    }
                    int count = 1;
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
                    else
                    {
                        return;
                    }

                    int count = 1;
                }
            }
            """;

        await VerifyCS.VerifyCodeFixAsync(
            source,
            VerifyCS.Diagnostic(HelenaDiagnosticDescriptors.IfFollowingSpacing).WithLocation(0),
            fixedSource);
    }

    /// <summary>
    /// Verifies already-spaced <c>if</c> blocks remain valid.
    /// </summary>
    [Fact]
    public async Task VerifyAnalyzerAsync_AllowsExistingBlankLine()
    {
        const string source = """
            class Sample
            {
                void Run(bool flag)
                {
                    if (flag)
                    {
                        return;
                    }

                    int count = 1;
                }
            }
            """;

        await VerifyCS.VerifyAnalyzerAsync(source);
    }
}
