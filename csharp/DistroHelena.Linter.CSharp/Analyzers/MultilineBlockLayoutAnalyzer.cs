using System.Collections.Immutable;
using DistroHelena.Linter.CSharp.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace DistroHelena.Linter.CSharp.Analyzers;

/// <summary>
/// Reports non-empty blocks that are written on a single line.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class MultilineBlockLayoutAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    /// The diagnostics supported by this analyzer.
    /// </summary>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(HelenaDiagnosticDescriptors.MultilineBlockLayout);

    /// <summary>
    /// Configures the analyzer to inspect block syntax nodes.
    /// </summary>
    /// <param name="context">The Roslyn analyzer initialization context.</param>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeBlock, SyntaxKind.Block);
    }

    /// <summary>
    /// Reports a diagnostic when a non-empty block opens and closes on the same line.
    /// </summary>
    /// <param name="context">The syntax-node analysis context.</param>
    private static void AnalyzeBlock(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not BlockSyntax block || block.Statements.Count == 0)
        {
            return;
        }

        if (!IsSingleLineBlock(block))
        {
            return;
        }

        Diagnostic diagnostic = Diagnostic.Create(
            HelenaDiagnosticDescriptors.MultilineBlockLayout,
            block.OpenBraceToken.GetLocation());

        context.ReportDiagnostic(diagnostic);
    }

    /// <summary>
    /// Determines whether the supplied block starts and ends on the same source line.
    /// </summary>
    /// <param name="block">The block being analyzed.</param>
    /// <returns><c>true</c> when the block is laid out on a single line; otherwise <c>false</c>.</returns>
    private static bool IsSingleLineBlock(BlockSyntax block)
    {
        FileLinePositionSpan openBraceSpan = block.OpenBraceToken.GetLocation().GetLineSpan();
        FileLinePositionSpan closeBraceSpan = block.CloseBraceToken.GetLocation().GetLineSpan();

        return openBraceSpan.StartLinePosition.Line == closeBraceSpan.StartLinePosition.Line;
    }
}
