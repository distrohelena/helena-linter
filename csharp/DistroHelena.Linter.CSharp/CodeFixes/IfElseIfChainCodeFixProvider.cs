using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DistroHelena.Linter.CSharp.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;

namespace DistroHelena.Linter.CSharp.CodeFixes;

/// <summary>
/// Folds sibling <c>if</c> statements into an <c>else if</c> chain.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(IfElseIfChainCodeFixProvider))]
[Shared]
public sealed class IfElseIfChainCodeFixProvider : CodeFixProvider
{
    /// <summary>
    /// The diagnostic identifiers this provider can fix.
    /// </summary>
    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(Diagnostics.HelenaDiagnosticDescriptors.IfElseIfChain.Id);

    /// <summary>
    /// The fix-all provider used for batch application.
    /// </summary>
    public override FixAllProvider GetFixAllProvider()
    {
        return WellKnownFixAllProviders.BatchFixer;
    }

    /// <summary>
    /// Registers a code action that folds the sibling <c>if</c> into an <c>else if</c> chain.
    /// </summary>
    /// <param name="context">The code-fix registration context.</param>
    public override Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        Diagnostic diagnostic = context.Diagnostics.First();

        context.RegisterCodeFix(
            CodeAction.Create(
                title: "Fold into else-if chain",
                createChangedDocument: cancellationToken =>
                    FoldIntoElseIfChainAsync(context.Document, diagnostic.Location, cancellationToken),
                equivalenceKey: CodeFixConstants.BatchEquivalenceKey),
            diagnostic);

        return Task.CompletedTask;
    }

    /// <summary>
    /// Rewrites the targeted sibling <c>if</c> into an <c>else if</c> attached to the previous sibling.
    /// </summary>
    /// <param name="document">The document being updated.</param>
    /// <param name="diagnosticLocation">The location of the reported diagnostic.</param>
    /// <param name="cancellationToken">The cancellation token for the async operation.</param>
    /// <returns>The updated document when the sibling chain can be folded; otherwise the original document.</returns>
    private static async Task<Document> FoldIntoElseIfChainAsync(
        Document document,
        Location diagnosticLocation,
        CancellationToken cancellationToken)
    {
        SyntaxNode? root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

        if (root is null)
        {
            return document;
        }

        SyntaxNode targetNode = root.FindNode(diagnosticLocation.SourceSpan, getInnermostNodeForTie: true);

        if (targetNode.AncestorsAndSelf().OfType<IfStatementSyntax>().FirstOrDefault() is not IfStatementSyntax currentIfStatement ||
            currentIfStatement.Parent is not BlockSyntax blockSyntax ||
            StatementSequenceHelpers.GetPreviousStatement(currentIfStatement) is not IfStatementSyntax previousIfStatement ||
            previousIfStatement.Else is not null ||
            !ControlFlowExitStatementAnalyzer.DoesStatementDefinitelyExit(previousIfStatement.Statement))
        {
            return document;
        }

        SyntaxToken elseKeyword = CreateElseKeyword(currentIfStatement);
        IfStatementSyntax nestedIfStatement = currentIfStatement.WithLeadingTrivia(SyntaxFactory.Space);
        ElseClauseSyntax elseClause = SyntaxFactory.ElseClause(elseKeyword, nestedIfStatement);
        IfStatementSyntax updatedPreviousIfStatement = previousIfStatement
            .WithElse(elseClause)
            .WithAdditionalAnnotations(Formatter.Annotation);

        int previousStatementIndex = blockSyntax.Statements.IndexOf(previousIfStatement);
        int currentStatementIndex = blockSyntax.Statements.IndexOf(currentIfStatement);

        if (previousStatementIndex < 0 || currentStatementIndex < 0 || currentStatementIndex != previousStatementIndex + 1)
        {
            return document;
        }

        SyntaxList<StatementSyntax> updatedStatements = blockSyntax.Statements.Replace(previousIfStatement, updatedPreviousIfStatement);
        updatedStatements = updatedStatements.RemoveAt(currentStatementIndex);
        BlockSyntax updatedBlockSyntax = blockSyntax.WithStatements(updatedStatements).WithAdditionalAnnotations(Formatter.Annotation);
        SyntaxNode updatedRoot = root.ReplaceNode(blockSyntax, updatedBlockSyntax);
        Document updatedDocument = document.WithSyntaxRoot(updatedRoot);
        return await Formatter.FormatAsync(updatedDocument, cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Creates the <c>else</c> keyword token while preserving transferred comment trivia from the folded sibling statement.
    /// </summary>
    /// <param name="currentIfStatement">The sibling <c>if</c> statement being folded.</param>
    /// <returns>An <c>else</c> keyword token ready for Roslyn formatting.</returns>
    private static SyntaxToken CreateElseKeyword(IfStatementSyntax currentIfStatement)
    {
        SyntaxTriviaList leadingTrivia = GetTransferredElseTrivia(currentIfStatement.GetLeadingTrivia());
        return SyntaxFactory.Token(leadingTrivia, SyntaxKind.ElseKeyword, SyntaxFactory.TriviaList(SyntaxFactory.Space));
    }

    /// <summary>
    /// Determines which leading trivia items should move onto the generated <c>else</c> keyword.
    /// </summary>
    /// <param name="leadingTrivia">The original leading trivia from the folded sibling <c>if</c>.</param>
    /// <returns>The trivia that should be preserved before the generated <c>else</c> keyword.</returns>
    private static SyntaxTriviaList GetTransferredElseTrivia(SyntaxTriviaList leadingTrivia)
    {
        bool containsStructuredTrivia = leadingTrivia.Any((trivia) =>
            !trivia.IsKind(SyntaxKind.WhitespaceTrivia) &&
            !trivia.IsKind(SyntaxKind.EndOfLineTrivia));

        if (!containsStructuredTrivia)
        {
            return default;
        }

        return leadingTrivia;
    }
}
