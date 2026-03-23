using DistroHelena.Linter.CSharp.Diagnostics;
using System.Threading.Tasks;
using Xunit;
using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.CodeFixVerifier<
    DistroHelena.Linter.CSharp.Analyzers.ControlBlockFollowingSpacingAnalyzer,
    DistroHelena.Linter.CSharp.CodeFixes.ControlBlockFollowingSpacingCodeFixProvider>;

namespace DistroHelena.Linter.CSharp.Tests.Analyzers;

/// <summary>
/// Covers analyzer and code-fix behavior for blank lines after non-if control blocks.
/// </summary>
public class ControlBlockFollowingSpacingAnalyzerTests
{
    /// <summary>
    /// Verifies a missing blank line after a <c>for</c> loop is inserted.
    /// </summary>
    [Fact]
    public async Task VerifyCodeFixAsync_AddsBlankLineAfterForStatement()
    {
        const string source = """
            class Sample
            {
                void Run()
                {
                    {|#0:for|} (int index = 0; index < 1; index++)
                    {
                        continue;
                    }
                    int result = 1;
                }
            }
            """;

        const string fixedSource = """
            class Sample
            {
                void Run()
                {
                    for (int index = 0; index < 1; index++)
                    {
                        continue;
                    }

                    int result = 1;
                }
            }
            """;

        await VerifyCS.VerifyCodeFixAsync(
            source,
            VerifyCS.Diagnostic(HelenaDiagnosticDescriptors.ControlBlockFollowingSpacing).WithLocation(0),
            fixedSource);
    }

    /// <summary>
    /// Verifies a missing blank line after a <c>while</c> loop is inserted.
    /// </summary>
    [Fact]
    public async Task VerifyCodeFixAsync_AddsBlankLineAfterWhileStatement()
    {
        const string source = """
            class Sample
            {
                void Run(bool flag)
                {
                    {|#0:while|} (flag)
                    {
                        break;
                    }
                    int result = 1;
                }
            }
            """;

        const string fixedSource = """
            class Sample
            {
                void Run(bool flag)
                {
                    while (flag)
                    {
                        break;
                    }

                    int result = 1;
                }
            }
            """;

        await VerifyCS.VerifyCodeFixAsync(
            source,
            VerifyCS.Diagnostic(HelenaDiagnosticDescriptors.ControlBlockFollowingSpacing).WithLocation(0),
            fixedSource);
    }

    /// <summary>
    /// Verifies a missing blank line after a <c>do</c> loop is inserted.
    /// </summary>
    [Fact]
    public async Task VerifyCodeFixAsync_AddsBlankLineAfterDoStatement()
    {
        const string source = """
            class Sample
            {
                void Run(bool flag)
                {
                    {|#0:do|}
                    {
                        break;
                    }
                    while (flag);
                    int result = 1;
                }
            }
            """;

        const string fixedSource = """
            class Sample
            {
                void Run(bool flag)
                {
                    do
                    {
                        break;
                    }
                    while (flag);

                    int result = 1;
                }
            }
            """;

        await VerifyCS.VerifyCodeFixAsync(
            source,
            VerifyCS.Diagnostic(HelenaDiagnosticDescriptors.ControlBlockFollowingSpacing).WithLocation(0),
            fixedSource);
    }

    /// <summary>
    /// Verifies a missing blank line after a <c>switch</c> statement is inserted.
    /// </summary>
    [Fact]
    public async Task VerifyCodeFixAsync_AddsBlankLineAfterSwitchStatement()
    {
        const string source = """
            class Sample
            {
                void Run(int count)
                {
                    {|#0:switch|} (count)
                    {
                        case 0:
                            break;
                    }
                    int result = 1;
                }
            }
            """;

        const string fixedSource = """
            class Sample
            {
                void Run(int count)
                {
                    switch (count)
                    {
                        case 0:
                            break;
                    }

                    int result = 1;
                }
            }
            """;

        await VerifyCS.VerifyCodeFixAsync(
            source,
            VerifyCS.Diagnostic(HelenaDiagnosticDescriptors.ControlBlockFollowingSpacing).WithLocation(0),
            fixedSource);
    }

    /// <summary>
    /// Verifies a missing blank line after a <c>try</c> statement is inserted.
    /// </summary>
    [Fact]
    public async Task VerifyCodeFixAsync_AddsBlankLineAfterTryStatement()
    {
        const string source = """
            class Sample
            {
                void Run(int count)
                {
                    {|#0:try|}
                    {
                        System.Console.WriteLine(count);
                    }
                    catch
                    {
                        System.Console.WriteLine(count);
                    }
                    int result = 1;
                }
            }
            """;

        const string fixedSource = """
            class Sample
            {
                void Run(int count)
                {
                    try
                    {
                        System.Console.WriteLine(count);
                    }
                    catch
                    {
                        System.Console.WriteLine(count);
                    }

                    int result = 1;
                }
            }
            """;

        await VerifyCS.VerifyCodeFixAsync(
            source,
            VerifyCS.Diagnostic(HelenaDiagnosticDescriptors.ControlBlockFollowingSpacing).WithLocation(0),
            fixedSource);
    }

    /// <summary>
    /// Verifies already-spaced control blocks remain valid.
    /// </summary>
    [Fact]
    public async Task VerifyAnalyzerAsync_AllowsExistingBlankLine()
    {
        const string source = """
            class Sample
            {
                void Run(bool flag)
                {
                    while (flag)
                    {
                        break;
                    }

                    int count = 1;
                }
            }
            """;

        await VerifyCS.VerifyAnalyzerAsync(source);
    }
}
