using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DistroHelena.Linter.CSharp.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DistroHelena.Linter.CSharp.CodeFixes;

/// <summary>
/// Provides shared code-fix operations for statement-spacing rules.
/// </summary>
public static class StatementSpacingCodeFixSupport
{
    /// <summary>
    /// Inserts a blank line before the statement following the statement addressed by a diagnostic.
    /// </summary>
    /// <param name="document">The document being updated.</param>
    /// <param name="diagnosticLocation">The location of the reported diagnostic.</param>
    /// <param name="cancellationToken">The cancellation token for the async operation.</param>
    /// <returns>The updated document when a sibling statement is found; otherwise the original document.</returns>
    public static async Task<Document> InsertBlankLineBeforeNextStatementAsync(
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
        StatementSyntax? anchorStatement = targetNode.AncestorsAndSelf().OfType<StatementSyntax>().FirstOrDefault();

        if (anchorStatement is null)
        {
            return document;
        }

        StatementSyntax? nextStatement = StatementSequenceHelpers.GetNextStatement(anchorStatement);

        if (nextStatement is null)
        {
            return document;
        }

        SyntaxNode updatedRoot = SyntaxTriviaHelpers.InsertBlankLineBeforeStatement(root, nextStatement);
        return document.WithSyntaxRoot(updatedRoot);
    }

    /// <summary>
    /// Inserts a blank line before the statement addressed by a diagnostic.
    /// </summary>
    /// <param name="document">The document being updated.</param>
    /// <param name="diagnosticLocation">The location of the reported diagnostic.</param>
    /// <param name="cancellationToken">The cancellation token for the async operation.</param>
    /// <returns>The updated document when the targeted statement is found; otherwise the original document.</returns>
    public static async Task<Document> InsertBlankLineBeforeCurrentStatementAsync(
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
        StatementSyntax? targetStatement = targetNode.AncestorsAndSelf().OfType<StatementSyntax>().FirstOrDefault();

        if (targetStatement is null)
        {
            return document;
        }

        SyntaxNode updatedRoot = SyntaxTriviaHelpers.InsertBlankLineBeforeStatement(root, targetStatement);
        return document.WithSyntaxRoot(updatedRoot);
    }
}
