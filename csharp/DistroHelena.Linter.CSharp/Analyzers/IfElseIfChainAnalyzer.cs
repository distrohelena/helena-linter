using System.Collections.Immutable;
using DistroHelena.Linter.CSharp.Diagnostics;
using DistroHelena.Linter.CSharp.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace DistroHelena.Linter.CSharp.Analyzers;

/// <summary>
/// Reports sibling <c>if</c> statements that should be folded into an <c>else if</c> chain.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class IfElseIfChainAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    /// The diagnostics supported by this analyzer.
    /// </summary>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(HelenaDiagnosticDescriptors.IfElseIfChain);

    /// <summary>
    /// Configures the analyzer to inspect <c>if</c> statements inside blocks.
    /// </summary>
    /// <param name="context">The Roslyn analyzer initialization context.</param>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeIfStatement, Microsoft.CodeAnalysis.CSharp.SyntaxKind.IfStatement);
    }

    /// <summary>
    /// Reports a diagnostic when a sibling <c>if</c> should become an <c>else if</c>.
    /// </summary>
    /// <param name="context">The syntax-node analysis context.</param>
    private static void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not IfStatementSyntax currentIfStatement ||
            currentIfStatement.Parent is not BlockSyntax)
        {
            return;
        }

        if (StatementSequenceHelpers.GetPreviousStatement(currentIfStatement) is not IfStatementSyntax previousIfStatement)
        {
            return;
        }

        if (previousIfStatement.Else is not null)
        {
            return;
        }

        if (!ControlFlowExitStatementAnalyzer.DoesStatementDefinitelyExit(previousIfStatement.Statement))
        {
            return;
        }

        Diagnostic diagnostic = Diagnostic.Create(
            HelenaDiagnosticDescriptors.IfElseIfChain,
            currentIfStatement.IfKeyword.GetLocation());

        context.ReportDiagnostic(diagnostic);
    }
}
