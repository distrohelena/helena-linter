using System.Collections.Immutable;
using DistroHelena.Linter.CSharp.Diagnostics;
using DistroHelena.Linter.CSharp.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace DistroHelena.Linter.CSharp.Analyzers;

/// <summary>
/// Reports supported opportunities to rewrite control flow into early-return guard clauses.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class EarlyReturnAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    /// The diagnostics supported by this analyzer.
    /// </summary>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(HelenaDiagnosticDescriptors.EarlyReturn);

    /// <summary>
    /// Configures the analyzer to inspect <c>if</c> statements for early-return rewrites.
    /// </summary>
    /// <param name="context">The Roslyn analyzer initialization context.</param>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeIfStatement, Microsoft.CodeAnalysis.CSharp.SyntaxKind.IfStatement);
    }

    /// <summary>
    /// Reports a diagnostic when the current <c>if</c> statement matches a supported early-return rewrite pattern.
    /// </summary>
    /// <param name="context">The syntax-node analysis context.</param>
    private static void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not Microsoft.CodeAnalysis.CSharp.Syntax.IfStatementSyntax ifStatement)
        {
            return;
        }

        if (EarlyReturnPatternDetector.TryDetect(ifStatement) is null)
        {
            return;
        }

        Diagnostic diagnostic = Diagnostic.Create(
            HelenaDiagnosticDescriptors.EarlyReturn,
            ifStatement.IfKeyword.GetLocation());

        context.ReportDiagnostic(diagnostic);
    }
}
