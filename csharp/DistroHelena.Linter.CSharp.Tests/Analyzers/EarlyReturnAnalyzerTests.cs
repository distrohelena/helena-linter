using DistroHelena.Linter.CSharp.Diagnostics;
using System.Threading.Tasks;
using Xunit;
using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.CodeFixVerifier<
    DistroHelena.Linter.CSharp.Analyzers.EarlyReturnAnalyzer,
    DistroHelena.Linter.CSharp.CodeFixes.EarlyReturnCodeFixProvider>;

namespace DistroHelena.Linter.CSharp.Tests.Analyzers;

/// <summary>
/// Covers analyzer and code-fix behavior for early-return rewrites.
/// </summary>
public class EarlyReturnAnalyzerTests
{
    /// <summary>
    /// Verifies a wrapped happy-path block is inverted into a guard clause when the following sibling exits.
    /// </summary>
    [Fact]
    public async Task VerifyCodeFixAsync_InvertsWrappedHappyPathIntoGuardClause()
    {
        const string source = """
            class Sample
            {
                void Run(bool flag)
                {
                    {|#0:if|} (flag)
                    {
                        System.Console.WriteLine(flag);
                        System.Console.WriteLine("done");
                    }
                    return;
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

                    System.Console.WriteLine(flag);
                    System.Console.WriteLine("done");
                }
            }
            """;

        await VerifyCS.VerifyCodeFixAsync(
            source,
            VerifyCS.Diagnostic(HelenaDiagnosticDescriptors.EarlyReturn).WithLocation(0),
            fixedSource);
    }

    /// <summary>
    /// Verifies an <c>if / else</c> with an exiting <c>if</c> branch is rewritten to a guard clause.
    /// </summary>
    [Fact]
    public async Task VerifyCodeFixAsync_RewritesIfElseWhenIfBranchExits()
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
                        System.Console.WriteLine(flag);
                        System.Console.WriteLine("done");
                    }
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

                    System.Console.WriteLine(flag);
                    System.Console.WriteLine("done");
                }
            }
            """;

        await VerifyCS.VerifyCodeFixAsync(
            source,
            VerifyCS.Diagnostic(HelenaDiagnosticDescriptors.EarlyReturn).WithLocation(0),
            fixedSource);
    }

    /// <summary>
    /// Verifies an <c>if / else</c> with an exiting <c>else</c> branch is rewritten to an inverted guard clause.
    /// </summary>
    [Fact]
    public async Task VerifyCodeFixAsync_RewritesIfElseWhenElseBranchExits()
    {
        const string source = """
            class Sample
            {
                void Run(bool flag)
                {
                    {|#0:if|} (flag)
                    {
                        System.Console.WriteLine(flag);
                        System.Console.WriteLine("done");
                    }
                    else
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

                    System.Console.WriteLine(flag);
                    System.Console.WriteLine("done");
                }
            }
            """;

        await VerifyCS.VerifyCodeFixAsync(
            source,
            VerifyCS.Diagnostic(HelenaDiagnosticDescriptors.EarlyReturn).WithLocation(0),
            fixedSource);
    }

    /// <summary>
    /// Verifies the analyzer ignores conditional branches that do not contain an exiting path.
    /// </summary>
    [Fact]
    public async Task VerifyAnalyzerAsync_IgnoresIfElseWhenNeitherBranchExits()
    {
        const string source = """
            class Sample
            {
                void Run(bool flag, bool otherFlag)
                {
                    if (flag)
                    {
                        System.Console.WriteLine(flag);
                    }
                    else
                    {
                        System.Console.WriteLine(otherFlag);
                    }
                }
            }
            """;

        await VerifyCS.VerifyAnalyzerAsync(source);
    }
}
