using System.Collections.Immutable;
using DistroHelena.Linter.CSharp.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace DistroHelena.Linter.CSharp.Analyzers;

/// <summary>
/// Reports control-flow bodies that are not wrapped in braces.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class IfBodyBracesAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    /// The diagnostics supported by this analyzer.
    /// </summary>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(HelenaDiagnosticDescriptors.IfBodyBraces);

    /// <summary>
    /// Configures the analyzer to inspect control statements and <c>else</c> clauses in C# syntax trees.
    /// </summary>
    /// <param name="context">The Roslyn analyzer initialization context.</param>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(
            AnalyzeStatementWithEmbeddedBody,
            SyntaxKind.IfStatement,
            SyntaxKind.ForStatement,
            SyntaxKind.ForEachStatement,
            SyntaxKind.ForEachVariableStatement,
            SyntaxKind.WhileStatement,
            SyntaxKind.DoStatement,
            SyntaxKind.UsingStatement,
            SyntaxKind.LockStatement,
            SyntaxKind.FixedStatement);
        context.RegisterSyntaxNodeAction(AnalyzeElseClause, SyntaxKind.ElseClause);
    }

    /// <summary>
    /// Reports a diagnostic when a control statement body uses an embedded statement instead of a block.
    /// </summary>
    /// <param name="context">The syntax-node analysis context.</param>
    private static void AnalyzeStatementWithEmbeddedBody(SyntaxNodeAnalysisContext context)
    {
        if (TryGetEmbeddedStatement(context.Node) is not StatementSyntax embeddedStatement ||
            embeddedStatement is BlockSyntax)
        {
            return;
        }

        Diagnostic diagnostic = Diagnostic.Create(
            HelenaDiagnosticDescriptors.IfBodyBraces,
            GetDiagnosticLocation(context.Node));

        context.ReportDiagnostic(diagnostic);
    }

    /// <summary>
    /// Reports a diagnostic when an <c>else</c> body uses an embedded statement instead of a block.
    /// </summary>
    /// <param name="context">The syntax-node analysis context.</param>
    private static void AnalyzeElseClause(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not ElseClauseSyntax elseClause ||
            elseClause.Statement is BlockSyntax ||
            elseClause.Statement is IfStatementSyntax)
        {
            return;
        }

        Diagnostic diagnostic = Diagnostic.Create(
            HelenaDiagnosticDescriptors.IfBodyBraces,
            elseClause.ElseKeyword.GetLocation());

        context.ReportDiagnostic(diagnostic);
    }

    /// <summary>
    /// Returns the embedded statement body owned by a control statement, when available.
    /// </summary>
    /// <param name="node">The syntax node to inspect.</param>
    /// <returns>The embedded statement body, or <c>null</c> when the node does not own one.</returns>
    private static StatementSyntax? TryGetEmbeddedStatement(SyntaxNode node)
    {
        return node switch
        {
            IfStatementSyntax ifStatement => ifStatement.Statement,
            ForStatementSyntax forStatement => forStatement.Statement,
            ForEachStatementSyntax forEachStatement => forEachStatement.Statement,
            ForEachVariableStatementSyntax forEachVariableStatement => forEachVariableStatement.Statement,
            WhileStatementSyntax whileStatement => whileStatement.Statement,
            DoStatementSyntax doStatement => doStatement.Statement,
            UsingStatementSyntax usingStatement => usingStatement.Statement,
            LockStatementSyntax lockStatement => lockStatement.Statement,
            FixedStatementSyntax fixedStatement => fixedStatement.Statement,
            _ => null,
        };
    }

    /// <summary>
    /// Resolves the control keyword location that should host the diagnostic.
    /// </summary>
    /// <param name="node">The control statement node being analyzed.</param>
    /// <returns>The location of the statement's leading keyword token.</returns>
    private static Location GetDiagnosticLocation(SyntaxNode node)
    {
        return node switch
        {
            IfStatementSyntax ifStatement => ifStatement.IfKeyword.GetLocation(),
            ForStatementSyntax forStatement => forStatement.ForKeyword.GetLocation(),
            ForEachStatementSyntax forEachStatement => forEachStatement.ForEachKeyword.GetLocation(),
            ForEachVariableStatementSyntax forEachVariableStatement => forEachVariableStatement.ForEachKeyword.GetLocation(),
            WhileStatementSyntax whileStatement => whileStatement.WhileKeyword.GetLocation(),
            DoStatementSyntax doStatement => doStatement.DoKeyword.GetLocation(),
            UsingStatementSyntax usingStatement => usingStatement.UsingKeyword.GetLocation(),
            LockStatementSyntax lockStatement => lockStatement.LockKeyword.GetLocation(),
            FixedStatementSyntax fixedStatement => fixedStatement.FixedKeyword.GetLocation(),
            _ => node.GetLocation(),
        };
    }
}
