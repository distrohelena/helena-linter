using System.Collections.Immutable;
using DistroHelena.Linter.CSharp.Diagnostics;
using DistroHelena.Linter.CSharp.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DistroHelena.Linter.CSharp.Analyzers;

/// <summary>
/// Reports complementary <c>else if</c> branches that should be written as plain <c>else</c>.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class RedundantElseIfAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    /// The diagnostics supported by this analyzer.
    /// </summary>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(HelenaDiagnosticDescriptors.RedundantElseIf);

    /// <summary>
    /// Configures the analyzer to inspect <c>else</c> clauses in C# syntax trees.
    /// </summary>
    /// <param name="context">The Roslyn analyzer initialization context.</param>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeElseClause, Microsoft.CodeAnalysis.CSharp.SyntaxKind.ElseClause);
    }

    /// <summary>
    /// Reports a diagnostic when an <c>else if</c> condition is an exact complement of the preceding <c>if</c>.
    /// </summary>
    /// <param name="context">The syntax-node analysis context.</param>
    private static void AnalyzeElseClause(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not ElseClauseSyntax elseClause ||
            elseClause.Statement is not IfStatementSyntax elseIfStatement ||
            elseClause.Parent is not IfStatementSyntax containingIf)
        {
            return;
        }

        if (!ConditionComparisonHelpers.IsComplementaryCondition(containingIf.Condition, elseIfStatement.Condition))
        {
            return;
        }

        TextSpan diagnosticSpan = TextSpan.FromBounds(
            elseClause.ElseKeyword.SpanStart,
            elseIfStatement.CloseParenToken.Span.End);

        Location diagnosticLocation = Location.Create(elseClause.SyntaxTree, diagnosticSpan);
        Diagnostic diagnostic = Diagnostic.Create(HelenaDiagnosticDescriptors.RedundantElseIf, diagnosticLocation);
        context.ReportDiagnostic(diagnostic);
    }
}
