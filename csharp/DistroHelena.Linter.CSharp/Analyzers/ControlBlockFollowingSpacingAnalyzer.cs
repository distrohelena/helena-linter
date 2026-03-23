using System.Collections.Immutable;
using DistroHelena.Linter.CSharp.Diagnostics;
using DistroHelena.Linter.CSharp.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace DistroHelena.Linter.CSharp.Analyzers;

/// <summary>
/// Reports missing blank lines after non-<c>if</c> control-flow blocks.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ControlBlockFollowingSpacingAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    /// The diagnostics supported by this analyzer.
    /// </summary>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(HelenaDiagnosticDescriptors.ControlBlockFollowingSpacing);

    /// <summary>
    /// Configures the analyzer to inspect loop, switch, and try statements inside blocks.
    /// </summary>
    /// <param name="context">The Roslyn analyzer initialization context.</param>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(
            AnalyzeControlStatement,
            SyntaxKind.ForStatement,
            SyntaxKind.ForEachStatement,
            SyntaxKind.ForEachVariableStatement,
            SyntaxKind.WhileStatement,
            SyntaxKind.DoStatement,
            SyntaxKind.SwitchStatement,
            SyntaxKind.TryStatement);
    }

    /// <summary>
    /// Reports a diagnostic when a tracked control statement is immediately followed by another sibling statement.
    /// </summary>
    /// <param name="context">The syntax-node analysis context.</param>
    private static void AnalyzeControlStatement(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not StatementSyntax statement ||
            statement.Parent is not BlockSyntax)
        {
            return;
        }

        StatementSyntax? nextStatement = StatementSequenceHelpers.GetNextStatement(statement);

        if (nextStatement is null || SyntaxTriviaHelpers.HasBlankLineBetween(statement, nextStatement))
        {
            return;
        }

        Location diagnosticLocation = GetDiagnosticLocation(statement);
        Diagnostic diagnostic = Diagnostic.Create(
            HelenaDiagnosticDescriptors.ControlBlockFollowingSpacing,
            diagnosticLocation);

        context.ReportDiagnostic(diagnostic);
    }

    /// <summary>
    /// Resolves the keyword location that should host the diagnostic for a tracked control statement.
    /// </summary>
    /// <param name="statement">The control statement being analyzed.</param>
    /// <returns>The location of the statement's leading keyword token.</returns>
    private static Location GetDiagnosticLocation(StatementSyntax statement)
    {
        return statement switch
        {
            ForStatementSyntax forStatement => forStatement.ForKeyword.GetLocation(),
            ForEachStatementSyntax forEachStatement => forEachStatement.ForEachKeyword.GetLocation(),
            ForEachVariableStatementSyntax forEachVariableStatement => forEachVariableStatement.ForEachKeyword.GetLocation(),
            WhileStatementSyntax whileStatement => whileStatement.WhileKeyword.GetLocation(),
            DoStatementSyntax doStatement => doStatement.DoKeyword.GetLocation(),
            SwitchStatementSyntax switchStatement => switchStatement.SwitchKeyword.GetLocation(),
            TryStatementSyntax tryStatement => tryStatement.TryKeyword.GetLocation(),
            _ => statement.GetLocation(),
        };
    }
}
