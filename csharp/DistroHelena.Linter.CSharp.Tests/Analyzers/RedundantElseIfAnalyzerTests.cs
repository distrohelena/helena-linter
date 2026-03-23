using DistroHelena.Linter.CSharp.Diagnostics;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.CodeFixVerifier<
    DistroHelena.Linter.CSharp.Analyzers.RedundantElseIfAnalyzer,
    DistroHelena.Linter.CSharp.CodeFixes.RedundantElseIfCodeFixProvider>;

namespace DistroHelena.Linter.CSharp.Tests.Analyzers;

/// <summary>
/// Covers analyzer and code-fix behavior for complementary <c>else if</c> branches.
/// </summary>
public class RedundantElseIfAnalyzerTests
{
    /// <summary>
    /// Verifies a null-comparison complementary branch is rewritten to <c>else</c>.
    /// </summary>
    [Fact]
    public async Task VerifyCodeFixAsync_RewritesComplementaryNullComparison()
    {
        const string source = """
            class Sample
            {
                void Run(string? value)
                {
                    if (value == null)
                    {
                        return;
                    }
                    {|#0:else if (value != null)|}
                    {
                        return;
                    }
                }
            }
            """;

        const string fixedSource = """
            class Sample
            {
                void Run(string? value)
                {
                    if (value == null)
                    {
                        return;
                    }
                    else
                    {
                        return;
                    }
                }
            }
            """;

        DiagnosticResult expected = VerifyCS.Diagnostic(HelenaDiagnosticDescriptors.RedundantElseIf)
            .WithLocation(0);

        await VerifyCS.VerifyCodeFixAsync(source, expected, fixedSource);
    }

    /// <summary>
    /// Verifies a boolean-negation complementary branch is rewritten to <c>else</c> without dropping comments.
    /// </summary>
    [Fact]
    public async Task VerifyCodeFixAsync_PreservesTriviaAroundComplementaryElseIf()
    {
        const string source = """
            class Sample
            {
                void Run(bool flag)
                {
                    if (!flag)
                    {
                        return;
                    }
                    // Keep this note.
                    {|#0:else if (flag)|}
                    {
                        return;
                    }
                }
            }
            """;

        const string fixedSource = """
            class Sample
            {
                void Run(bool flag)
                {
                    if (!flag)
                    {
                        return;
                    }
                    // Keep this note.
                    else
                    {
                        return;
                    }
                }
            }
            """;

        DiagnosticResult expected = VerifyCS.Diagnostic(HelenaDiagnosticDescriptors.RedundantElseIf)
            .WithLocation(0);

        await VerifyCS.VerifyCodeFixAsync(source, expected, fixedSource);
    }

    /// <summary>
    /// Verifies unrelated conditions do not produce a diagnostic.
    /// </summary>
    [Fact]
    public async Task VerifyAnalyzerAsync_IgnoresNonComplementaryElseIf()
    {
        const string source = """
            class Sample
            {
                void Run(int count)
                {
                    if (count == 0)
                    {
                        return;
                    }
                    else if (count > 0)
                    {
                        return;
                    }
                }
            }
            """;

        await VerifyCS.VerifyAnalyzerAsync(source);
    }
}
