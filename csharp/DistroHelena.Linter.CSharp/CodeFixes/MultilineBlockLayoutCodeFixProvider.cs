using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Text;

namespace DistroHelena.Linter.CSharp.CodeFixes;

/// <summary>
/// Rewrites non-empty blocks so their contents are laid out across multiple lines.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(MultilineBlockLayoutCodeFixProvider))]
[Shared]
public sealed class MultilineBlockLayoutCodeFixProvider : CodeFixProvider
{
    /// <summary>
    /// The diagnostic identifiers this provider can fix.
    /// </summary>
    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(Diagnostics.HelenaDiagnosticDescriptors.MultilineBlockLayout.Id);

    /// <summary>
    /// The fix-all provider used for batch application.
    /// </summary>
    public override FixAllProvider GetFixAllProvider()
    {
        return WellKnownFixAllProviders.BatchFixer;
    }

    /// <summary>
    /// Registers a code action that rewrites the targeted block to multiline layout.
    /// </summary>
    /// <param name="context">The code-fix registration context.</param>
    public override Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        Diagnostic diagnostic = context.Diagnostics.First();

        context.RegisterCodeFix(
            CodeAction.Create(
                title: "Use multiline block layout",
                createChangedDocument: cancellationToken =>
                    RewriteBlockAsync(context.Document, diagnostic.Location, cancellationToken),
                equivalenceKey: CodeFixConstants.BatchEquivalenceKey),
            diagnostic);

        return Task.CompletedTask;
    }

    /// <summary>
    /// Rewrites the targeted block so its braces and contents occupy separate lines.
    /// </summary>
    /// <param name="document">The document being updated.</param>
    /// <param name="diagnosticLocation">The location of the reported diagnostic.</param>
    /// <param name="cancellationToken">The cancellation token for the async operation.</param>
    /// <returns>The updated document when a target block is found; otherwise the original document.</returns>
    private static async Task<Document> RewriteBlockAsync(
        Document document,
        Location diagnosticLocation,
        CancellationToken cancellationToken)
    {
        SyntaxNode? root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        SourceText? sourceText = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);

        if (root is null || sourceText is null)
        {
            return document;
        }

        SyntaxToken diagnosticToken = root.FindToken(diagnosticLocation.SourceSpan.Start);
        BlockSyntax? block = diagnosticToken.Parent?.AncestorsAndSelf().OfType<BlockSyntax>().FirstOrDefault();

        if (block is null || block.Statements.Count == 0)
        {
            return document;
        }

        string endOfLineText = GetEndOfLineText(sourceText);
        BlockSyntax updatedBlock = CreateMultilineBlock(block, endOfLineText)
            .WithAdditionalAnnotations(Formatter.Annotation);

        SyntaxNode updatedRoot = root.ReplaceNode(block, updatedBlock);
        Document updatedDocument = document.WithSyntaxRoot(updatedRoot);
        return await Formatter.FormatAsync(updatedDocument, cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Rewrites a single-line block into a multiline block while preserving trivia.
    /// </summary>
    /// <param name="block">The block being rewritten.</param>
    /// <param name="endOfLineText">The line-ending text used by the current document.</param>
    /// <returns>The rewritten block syntax.</returns>
    private static BlockSyntax CreateMultilineBlock(BlockSyntax block, string endOfLineText)
    {
        SyntaxTrivia endOfLineTrivia = SyntaxFactory.EndOfLine(endOfLineText);
        SyntaxList<StatementSyntax> updatedStatements = SyntaxFactory.List(
            block.Statements.Select((statement, index) =>
                index < block.Statements.Count - 1
                    ? AppendEndOfLine(statement, endOfLineTrivia)
                    : statement));

        return block
            .WithOpenBraceToken(AppendEndOfLine(block.OpenBraceToken, endOfLineTrivia))
            .WithStatements(updatedStatements)
            .WithCloseBraceToken(PrependEndOfLine(block.CloseBraceToken, endOfLineTrivia));
    }

    /// <summary>
    /// Appends a line break to the trailing trivia of a statement when one is not already present.
    /// </summary>
    /// <param name="statement">The statement to update.</param>
    /// <param name="endOfLineTrivia">The line-ending trivia to append.</param>
    /// <returns>The updated statement.</returns>
    private static StatementSyntax AppendEndOfLine(StatementSyntax statement, SyntaxTrivia endOfLineTrivia)
    {
        if (HasEndOfLine(statement.GetTrailingTrivia()))
        {
            return statement;
        }

        return statement.WithTrailingTrivia(statement.GetTrailingTrivia().Add(endOfLineTrivia));
    }

    /// <summary>
    /// Appends a line break to the trailing trivia of a syntax token when one is not already present.
    /// </summary>
    /// <param name="token">The token to update.</param>
    /// <param name="endOfLineTrivia">The line-ending trivia to append.</param>
    /// <returns>The updated token.</returns>
    private static SyntaxToken AppendEndOfLine(SyntaxToken token, SyntaxTrivia endOfLineTrivia)
    {
        if (HasEndOfLine(token.TrailingTrivia))
        {
            return token;
        }

        return token.WithTrailingTrivia(token.TrailingTrivia.Add(endOfLineTrivia));
    }

    /// <summary>
    /// Prepends a line break to the leading trivia of a syntax token when one is not already present.
    /// </summary>
    /// <param name="token">The token to update.</param>
    /// <param name="endOfLineTrivia">The line-ending trivia to prepend.</param>
    /// <returns>The updated token.</returns>
    private static SyntaxToken PrependEndOfLine(SyntaxToken token, SyntaxTrivia endOfLineTrivia)
    {
        if (HasEndOfLine(token.LeadingTrivia))
        {
            return token;
        }

        return token.WithLeadingTrivia(token.LeadingTrivia.Insert(0, endOfLineTrivia));
    }

    /// <summary>
    /// Determines whether a trivia list already contains a line break.
    /// </summary>
    /// <param name="triviaList">The trivia list to inspect.</param>
    /// <returns><c>true</c> when the trivia list already includes an end-of-line marker; otherwise <c>false</c>.</returns>
    private static bool HasEndOfLine(SyntaxTriviaList triviaList)
    {
        foreach (SyntaxTrivia trivia in triviaList)
        {
            if (trivia.IsKind(SyntaxKind.EndOfLineTrivia))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Resolves the line-ending text used by the current source file.
    /// </summary>
    /// <param name="sourceText">The current document text.</param>
    /// <returns>The line-ending string to preserve the file's newline style.</returns>
    private static string GetEndOfLineText(SourceText sourceText)
    {
        string source = sourceText.ToString();

        return source.IndexOf("\r\n", System.StringComparison.Ordinal) >= 0 ? "\r\n" : "\n";
    }
}
