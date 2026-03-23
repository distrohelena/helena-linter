using System.Collections.Immutable;
using DistroHelena.Linter.CSharp.Diagnostics;
using DistroHelena.Linter.CSharp.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace DistroHelena.Linter.CSharp.Analyzers;

/// <summary>
/// Reports missing blank lines after local declarations.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DeclarationSpacingAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    /// The diagnostics supported by this analyzer.
    /// </summary>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(HelenaDiagnosticDescriptors.DeclarationSpacing);

    /// <summary>
    /// Configures the analyzer to inspect local declarations inside blocks.
    /// </summary>
    /// <param name="context">The Roslyn analyzer initialization context.</param>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeDeclaration, Microsoft.CodeAnalysis.CSharp.SyntaxKind.LocalDeclarationStatement);
    }

    /// <summary>
    /// Reports a diagnostic when a declaration is immediately followed by another statement without a blank line.
    /// </summary>
    /// <param name="context">The syntax-node analysis context.</param>
    private static void AnalyzeDeclaration(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not LocalDeclarationStatementSyntax declarationStatement)
        {
            return;
        }

        StatementSyntax? nextStatement = StatementSequenceHelpers.GetNextStatement(declarationStatement);

        if (nextStatement is null || SyntaxTriviaHelpers.HasBlankLineBetween(declarationStatement, nextStatement))
        {
            return;
        }

        Diagnostic diagnostic = Diagnostic.Create(
            HelenaDiagnosticDescriptors.DeclarationSpacing,
            declarationStatement.Declaration.Type.GetLocation());

        context.ReportDiagnostic(diagnostic);
    }
}
