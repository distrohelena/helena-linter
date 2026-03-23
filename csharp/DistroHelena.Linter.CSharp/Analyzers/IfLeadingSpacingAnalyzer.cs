using System.Collections.Immutable;
using DistroHelena.Linter.CSharp.Diagnostics;
using DistroHelena.Linter.CSharp.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace DistroHelena.Linter.CSharp.Analyzers;

/// <summary>
/// Reports missing blank lines before <c>if</c> statements that follow sibling statements.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class IfLeadingSpacingAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    /// The diagnostics supported by this analyzer.
    /// </summary>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(HelenaDiagnosticDescriptors.IfLeadingSpacing);

    /// <summary>
    /// Configures the analyzer to inspect top-level <c>if</c> statements inside blocks.
    /// </summary>
    /// <param name="context">The Roslyn analyzer initialization context.</param>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeIfStatement, Microsoft.CodeAnalysis.CSharp.SyntaxKind.IfStatement);
    }

    /// <summary>
    /// Reports a diagnostic when an <c>if</c> statement follows another sibling statement without a blank line.
    /// </summary>
    /// <param name="context">The syntax-node analysis context.</param>
    private static void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not IfStatementSyntax ifStatement ||
            ifStatement.Parent is not BlockSyntax)
        {
            return;
        }

        StatementSyntax? previousStatement = StatementSequenceHelpers.GetPreviousStatement(ifStatement);

        if (previousStatement is null || SyntaxTriviaHelpers.HasBlankLineBetween(previousStatement, ifStatement))
        {
            return;
        }

        Diagnostic diagnostic = Diagnostic.Create(
            HelenaDiagnosticDescriptors.IfLeadingSpacing,
            ifStatement.IfKeyword.GetLocation());

        context.ReportDiagnostic(diagnostic);
    }
}
