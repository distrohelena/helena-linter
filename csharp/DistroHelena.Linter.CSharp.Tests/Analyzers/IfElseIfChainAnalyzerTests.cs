using DistroHelena.Linter.CSharp.Diagnostics;
using System.Threading.Tasks;
using Xunit;
using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.CodeFixVerifier<
    DistroHelena.Linter.CSharp.Analyzers.IfElseIfChainAnalyzer,
    DistroHelena.Linter.CSharp.CodeFixes.IfElseIfChainCodeFixProvider>;

namespace DistroHelena.Linter.CSharp.Tests.Analyzers;

/// <summary>
/// Covers analyzer and code-fix behavior for folding sibling <c>if</c> statements into <c>else if</c> chains.
/// </summary>
public class IfElseIfChainAnalyzerTests
{
    /// <summary>
    /// Verifies a sibling <c>if</c> becomes <c>else if</c> when the first branch returns.
    /// </summary>
    [Fact]
    public async Task VerifyCodeFixAsync_FoldsSiblingIfAfterReturn()
    {
        const string source = """
            class Sample
            {
                void Run(bool flag, bool otherFlag)
                {
                    if (flag)
                    {
                        return;
                    }
                    {|#0:if|} (otherFlag)
                    {
                        System.Console.WriteLine(otherFlag);
                    }
                }
            }
            """;

        const string fixedSource = """
            class Sample
            {
                void Run(bool flag, bool otherFlag)
                {
                    if (flag)
                    {
                        return;
                    }
                    else if (otherFlag)
                    {
                        System.Console.WriteLine(otherFlag);
                    }
                }
            }
            """;

        await VerifyCS.VerifyCodeFixAsync(
            source,
            VerifyCS.Diagnostic(HelenaDiagnosticDescriptors.IfElseIfChain).WithLocation(0),
            fixedSource);
    }

    /// <summary>
    /// Verifies loop exits also trigger sibling folding.
    /// </summary>
    [Fact]
    public async Task VerifyCodeFixAsync_FoldsSiblingIfAfterBreak()
    {
        const string source = """
            class Sample
            {
                void Run(bool flag, bool otherFlag)
                {
                    while (true)
                    {
                        if (flag)
                        {
                            break;
                        }
                        {|#0:if|} (otherFlag)
                        {
                            System.Console.WriteLine(otherFlag);
                        }
                    }
                }
            }
            """;

        const string fixedSource = """
            class Sample
            {
                void Run(bool flag, bool otherFlag)
                {
                    while (true)
                    {
                        if (flag)
                        {
                            break;
                        }
                        else if (otherFlag)
                        {
                            System.Console.WriteLine(otherFlag);
                        }
                    }
                }
            }
            """;

        await VerifyCS.VerifyCodeFixAsync(
            source,
            VerifyCS.Diagnostic(HelenaDiagnosticDescriptors.IfElseIfChain).WithLocation(0),
            fixedSource);
    }

    /// <summary>
    /// Verifies comments between sibling <c>if</c> statements are preserved when folding.
    /// </summary>
    [Fact]
    public async Task VerifyCodeFixAsync_PreservesCommentBetweenSiblingIfStatements()
    {
        const string source = """
            class Sample
            {
                void Run(bool flag, bool otherFlag)
                {
                    if (flag)
                    {
                        return;
                    }
                    // Keep this context.
                    {|#0:if|} (otherFlag)
                    {
                        System.Console.WriteLine(otherFlag);
                    }
                }
            }
            """;

        const string fixedSource = """
            class Sample
            {
                void Run(bool flag, bool otherFlag)
                {
                    if (flag)
                    {
                        return;
                    }
                    // Keep this context.
                    else if (otherFlag)
                    {
                        System.Console.WriteLine(otherFlag);
                    }
                }
            }
            """;

        await VerifyCS.VerifyCodeFixAsync(
            source,
            VerifyCS.Diagnostic(HelenaDiagnosticDescriptors.IfElseIfChain).WithLocation(0),
            fixedSource);
    }

    /// <summary>
    /// Verifies non-exiting sibling <c>if</c> statements remain untouched.
    /// </summary>
    [Fact]
    public async Task VerifyAnalyzerAsync_IgnoresSiblingIfWhenFirstBranchDoesNotExit()
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
                    if (otherFlag)
                    {
                        System.Console.WriteLine(otherFlag);
                    }
                }
            }
            """;

        await VerifyCS.VerifyAnalyzerAsync(source);
    }
}
