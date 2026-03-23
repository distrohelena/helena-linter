using DistroHelena.Linter.CSharp.Diagnostics;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;
using Xunit;
using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.CodeFixVerifier<
    DistroHelena.Linter.CSharp.Analyzers.IfBodyBracesAnalyzer,
    DistroHelena.Linter.CSharp.CodeFixes.IfBodyBracesCodeFixProvider>;

namespace DistroHelena.Linter.CSharp.Tests.Analyzers;

#pragma warning disable CS0618

/// <summary>
/// Covers analyzer and code-fix behavior for requiring braces around control-flow bodies.
/// </summary>
public class IfBodyBracesAnalyzerTests
{
    /// <summary>
    /// Verifies single-statement control-flow bodies are wrapped in braces.
    /// </summary>
    [Fact]
    public async Task VerifyCodeFixAsync_WrapsSingleStatementControlBodiesInBraces()
    {
        const string source = """
            class Sample
            {
                unsafe void Run(bool first, bool second, object gate, System.IDisposable disposable, int[] values)
                {
                    {|#0:if|} (first)
                        return;
                    else {|#1:if|} (second)
                        System.Console.WriteLine(second);
                    {|#2:else|}
                        System.Console.WriteLine(first);
                    {|#3:for|} (int index = 0; index < values.Length; index++)
                        System.Console.WriteLine(values[index]);
                    {|#4:foreach|} (int value in values)
                        System.Console.WriteLine(value);
                    {|#5:while|} (first)
                        return;
                    {|#6:do|}
                        System.Console.WriteLine(values.Length);
                    while (second);
                    {|#7:using|} (disposable)
                        System.Console.WriteLine(disposable.GetHashCode());
                    {|#8:lock|} (gate)
                        System.Console.WriteLine(gate.GetHashCode());
                    {|#10:fixed|} (int* pointer = values)
                        {|#9:for|} (int index = 0; index < values.Length; index++)
                            System.Console.WriteLine(pointer[index]);
                }
            }
            """;

        const string fixedSource = """
            class Sample
            {
                unsafe void Run(bool first, bool second, object gate, System.IDisposable disposable, int[] values)
                {
                    if (first)
                    {
                        return;
                    }
                    else if (second)
                    {
                        System.Console.WriteLine(second);
                    }
                    else
                    {
                        System.Console.WriteLine(first);
                    }

                    for (int index = 0; index < values.Length; index++)
                    {
                        System.Console.WriteLine(values[index]);
                    }

                    foreach (int value in values)
                    {
                        System.Console.WriteLine(value);
                    }

                    while (first)
                    {
                        return;
                    }

                    do
                    {
                        System.Console.WriteLine(values.Length);
                    }
                    while (second);
                    using (disposable)
                    {
                        System.Console.WriteLine(disposable.GetHashCode());
                    }

                    lock (gate)
                    {
                        System.Console.WriteLine(gate.GetHashCode());
                    }

                    fixed (int* pointer = values)
                    {
                        for (int index = 0; index < values.Length; index++)
                        {
                            System.Console.WriteLine(pointer[index]);
                        }
                    }
                }
            }
            """;

        CSharpCodeFixTest<
            DistroHelena.Linter.CSharp.Analyzers.IfBodyBracesAnalyzer,
            DistroHelena.Linter.CSharp.CodeFixes.IfBodyBracesCodeFixProvider,
            XUnitVerifier> test = new()
        {
            TestCode = source,
            FixedCode = fixedSource,
            NumberOfFixAllIterations = 2,
        };

        test.ExpectedDiagnostics.AddRange(
            [
                VerifyCS.Diagnostic(HelenaDiagnosticDescriptors.IfBodyBraces).WithLocation(0),
                VerifyCS.Diagnostic(HelenaDiagnosticDescriptors.IfBodyBraces).WithLocation(1),
                VerifyCS.Diagnostic(HelenaDiagnosticDescriptors.IfBodyBraces).WithLocation(2),
                VerifyCS.Diagnostic(HelenaDiagnosticDescriptors.IfBodyBraces).WithLocation(3),
                VerifyCS.Diagnostic(HelenaDiagnosticDescriptors.IfBodyBraces).WithLocation(4),
                VerifyCS.Diagnostic(HelenaDiagnosticDescriptors.IfBodyBraces).WithLocation(5),
                VerifyCS.Diagnostic(HelenaDiagnosticDescriptors.IfBodyBraces).WithLocation(6),
                VerifyCS.Diagnostic(HelenaDiagnosticDescriptors.IfBodyBraces).WithLocation(7),
                VerifyCS.Diagnostic(HelenaDiagnosticDescriptors.IfBodyBraces).WithLocation(8),
                VerifyCS.Diagnostic(HelenaDiagnosticDescriptors.IfBodyBraces).WithLocation(9),
                VerifyCS.Diagnostic(HelenaDiagnosticDescriptors.IfBodyBraces).WithLocation(10),
            ]);

        await test.RunAsync();
    }

    /// <summary>
    /// Verifies already-braced control bodies do not produce diagnostics.
    /// </summary>
    [Fact]
    public async Task VerifyAnalyzerAsync_IgnoresBracedBodies()
    {
        const string source = """
            class Sample
            {
                unsafe void Run(bool first, bool second, object gate, System.IDisposable disposable, int[] values)
                {
                    if (first)
                    {
                        return;
                    }
                    else if (second)
                    {
                        System.Console.WriteLine(second);
                    }
                    else
                    {
                        System.Console.WriteLine(first);
                    }
                    for (int index = 0; index < values.Length; index++)
                    {
                        System.Console.WriteLine(values[index]);
                    }
                    foreach (int value in values)
                    {
                        System.Console.WriteLine(value);
                    }
                    while (first)
                    {
                        return;
                    }
                    do
                    {
                        System.Console.WriteLine(values.Length);
                    } while (second);
                    using (disposable)
                    {
                        System.Console.WriteLine(disposable.GetHashCode());
                    }
                    lock (gate)
                    {
                        System.Console.WriteLine(gate.GetHashCode());
                    }
                    fixed (int* pointer = values)
                    {
                        for (int index = 0; index < values.Length; index++)
                        {
                            System.Console.WriteLine(pointer[index]);
                        }
                    }
                }
            }
            """;

        await VerifyCS.VerifyAnalyzerAsync(source);
    }
}

#pragma warning restore CS0618
