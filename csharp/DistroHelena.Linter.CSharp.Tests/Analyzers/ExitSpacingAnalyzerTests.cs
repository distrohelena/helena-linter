using DistroHelena.Linter.CSharp.Diagnostics;
using System.Threading.Tasks;
using Xunit;
using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.CodeFixVerifier<
    DistroHelena.Linter.CSharp.Analyzers.ExitSpacingAnalyzer,
    DistroHelena.Linter.CSharp.CodeFixes.ExitSpacingCodeFixProvider>;

namespace DistroHelena.Linter.CSharp.Tests.Analyzers;

/// <summary>
/// Covers analyzer and code-fix behavior for blank lines before exit statements.
/// </summary>
public class ExitSpacingAnalyzerTests
{
    /// <summary>
    /// Verifies a blank line is inserted before a <c>return</c> statement.
    /// </summary>
    [Fact]
    public async Task VerifyCodeFixAsync_AddsBlankLineBeforeReturn()
    {
        const string source = """
            class Sample
            {
                int Run()
                {
                    int count = 1;
                    {|#0:return|} count;
                }
            }
            """;

        const string fixedSource = """
            class Sample
            {
                int Run()
                {
                    int count = 1;

                    return count;
                }
            }
            """;

        await VerifyCS.VerifyCodeFixAsync(
            source,
            VerifyCS.Diagnostic(HelenaDiagnosticDescriptors.ExitSpacing).WithLocation(0),
            fixedSource);
    }

    /// <summary>
    /// Verifies a blank line is inserted before a <c>throw</c> statement.
    /// </summary>
    [Fact]
    public async Task VerifyCodeFixAsync_AddsBlankLineBeforeThrow()
    {
        const string source = """
            using System;

            class Sample
            {
                void Run()
                {
                    int count = 1;
                    {|#0:throw|} new InvalidOperationException(count.ToString());
                }
            }
            """;

        const string fixedSource = """
            using System;

            class Sample
            {
                void Run()
                {
                    int count = 1;

                    throw new InvalidOperationException(count.ToString());
                }
            }
            """;

        await VerifyCS.VerifyCodeFixAsync(
            source,
            VerifyCS.Diagnostic(HelenaDiagnosticDescriptors.ExitSpacing).WithLocation(0),
            fixedSource);
    }

    /// <summary>
    /// Verifies a blank line is inserted before a <c>break</c> statement.
    /// </summary>
    [Fact]
    public async Task VerifyCodeFixAsync_AddsBlankLineBeforeBreak()
    {
        const string source = """
            class Sample
            {
                void Run(bool flag)
                {
                    while (flag)
                    {
                        int count = 1;
                        {|#0:break|};
                    }
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
                        int count = 1;

                        break;
                    }
                }
            }
            """;

        await VerifyCS.VerifyCodeFixAsync(
            source,
            VerifyCS.Diagnostic(HelenaDiagnosticDescriptors.ExitSpacing).WithLocation(0),
            fixedSource);
    }

    /// <summary>
    /// Verifies a blank line is inserted before a <c>continue</c> statement.
    /// </summary>
    [Fact]
    public async Task VerifyCodeFixAsync_AddsBlankLineBeforeContinue()
    {
        const string source = """
            class Sample
            {
                void Run()
                {
                    for (int index = 0; index < 1; index++)
                    {
                        int count = index;
                        {|#0:continue|};
                    }
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
                        int count = index;

                        continue;
                    }
                }
            }
            """;

        await VerifyCS.VerifyCodeFixAsync(
            source,
            VerifyCS.Diagnostic(HelenaDiagnosticDescriptors.ExitSpacing).WithLocation(0),
            fixedSource);
    }
}
