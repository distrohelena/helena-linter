using System.Collections.Immutable;
using DistroHelena.Linter.CSharp.Diagnostics;
using DistroHelena.Linter.CSharp.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace DistroHelena.Linter.CSharp.Analyzers;

/// <summary>
/// Reports missing blank lines before exit statements.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ExitSpacingAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    /// The diagnostics supported by this analyzer.
    /// </summary>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(HelenaDiagnosticDescriptors.ExitSpacing);

    /// <summary>
    /// Configures the analyzer to inspect exit statements inside blocks.
    /// </summary>
    /// <param name="context">The Roslyn analyzer initialization context.</param>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(
            AnalyzeExitStatement,
            SyntaxKind.ReturnStatement,
            SyntaxKind.ThrowStatement,
            SyntaxKind.BreakStatement,
            SyntaxKind.ContinueStatement);
    }

    /// <summary>
    /// Reports a diagnostic when an exit statement is not visually separated from the prior sibling statement.
    /// </summary>
    /// <param name="context">The syntax-node analysis context.</param>
    private static void AnalyzeExitStatement(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not StatementSyntax statement)
        {
            return;
        }

        StatementSyntax? previousStatement = StatementSequenceHelpers.GetPreviousStatement(statement);

        if (previousStatement is null || SyntaxTriviaHelpers.HasBlankLineBetween(previousStatement, statement))
        {
            return;
        }

        Diagnostic diagnostic = Diagnostic.Create(
            HelenaDiagnosticDescriptors.ExitSpacing,
            GetKeywordLocation(statement));

        context.ReportDiagnostic(diagnostic);
    }

    /// <summary>
    /// Resolves the leading keyword location for a tracked exit statement.
    /// </summary>
    /// <param name="statement">The exit statement being analyzed.</param>
    /// <returns>The location of the statement's exit keyword token.</returns>
    private static Location GetKeywordLocation(StatementSyntax statement)
    {
        return statement switch
        {
            ReturnStatementSyntax returnStatement => returnStatement.ReturnKeyword.GetLocation(),
            ThrowStatementSyntax throwStatement => throwStatement.ThrowKeyword.GetLocation(),
            BreakStatementSyntax breakStatement => breakStatement.BreakKeyword.GetLocation(),
            ContinueStatementSyntax continueStatement => continueStatement.ContinueKeyword.GetLocation(),
            _ => statement.GetLocation(),
        };
    }
}
