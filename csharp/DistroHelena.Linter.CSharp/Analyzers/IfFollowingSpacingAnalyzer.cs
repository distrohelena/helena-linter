using System.Collections.Immutable;
using DistroHelena.Linter.CSharp.Diagnostics;
using DistroHelena.Linter.CSharp.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace DistroHelena.Linter.CSharp.Analyzers;

/// <summary>
/// Reports missing blank lines after completed <c>if</c> chains.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class IfFollowingSpacingAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    /// The diagnostics supported by this analyzer.
    /// </summary>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(HelenaDiagnosticDescriptors.IfFollowingSpacing);

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
    /// Reports a diagnostic when an <c>if</c> statement is immediately followed by another sibling statement.
    /// </summary>
    /// <param name="context">The syntax-node analysis context.</param>
    private static void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not IfStatementSyntax ifStatement ||
            ifStatement.Parent is not BlockSyntax)
        {
            return;
        }

        StatementSyntax? nextStatement = StatementSequenceHelpers.GetNextStatement(ifStatement);

        if (nextStatement is null || SyntaxTriviaHelpers.HasBlankLineBetween(ifStatement, nextStatement))
        {
            return;
        }

        Diagnostic diagnostic = Diagnostic.Create(
            HelenaDiagnosticDescriptors.IfFollowingSpacing,
            ifStatement.IfKeyword.GetLocation());

        context.ReportDiagnostic(diagnostic);
    }
}
