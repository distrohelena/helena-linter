using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;

namespace DistroHelena.Linter.CSharp.CodeFixes;

/// <summary>
/// Rewrites complementary <c>else if</c> branches to plain <c>else</c> blocks.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RedundantElseIfCodeFixProvider))]
[Shared]
public sealed class RedundantElseIfCodeFixProvider : CodeFixProvider
{
    /// <summary>
    /// The diagnostic identifiers this provider can fix.
    /// </summary>
    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(Diagnostics.HelenaDiagnosticDescriptors.RedundantElseIf.Id);

    /// <summary>
    /// The fix-all provider used for batch application.
    /// </summary>
    public override FixAllProvider GetFixAllProvider()
    {
        return WellKnownFixAllProviders.BatchFixer;
    }

    /// <summary>
    /// Registers a code action that rewrites the complementary <c>else if</c> to <c>else</c>.
    /// </summary>
    /// <param name="context">The code-fix registration context.</param>
    public override Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        Diagnostic diagnostic = context.Diagnostics.First();

        context.RegisterCodeFix(
            CodeAction.Create(
                title: "Replace with else",
                createChangedDocument: cancellationToken => ReplaceWithElseAsync(context.Document, diagnostic.Location, cancellationToken),
                equivalenceKey: CodeFixConstants.BatchEquivalenceKey),
            diagnostic);

        return Task.CompletedTask;
    }

    /// <summary>
    /// Replaces the targeted <c>else if</c> clause with an <c>else</c> clause that reuses the existing body.
    /// </summary>
    /// <param name="document">The document being fixed.</param>
    /// <param name="diagnosticLocation">The location of the reported diagnostic.</param>
    /// <param name="cancellationToken">The cancellation token for the async operation.</param>
    /// <returns>The updated document when the target clause is found; otherwise the original document.</returns>
    private static async Task<Document> ReplaceWithElseAsync(
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
        ElseClauseSyntax? elseClause = targetNode.AncestorsAndSelf().OfType<ElseClauseSyntax>().FirstOrDefault();

        if (elseClause?.Statement is not IfStatementSyntax elseIfStatement)
        {
            return document;
        }

        StatementSyntax replacementStatement = elseIfStatement.Statement.WithLeadingTrivia(elseIfStatement.Statement.GetLeadingTrivia());
        ElseClauseSyntax replacementElseClause = elseClause
            .WithStatement(replacementStatement)
            .WithAdditionalAnnotations(Formatter.Annotation);

        SyntaxNode updatedRoot = root.ReplaceNode(elseClause, replacementElseClause);
        Document updatedDocument = document.WithSyntaxRoot(updatedRoot);
        return await Formatter.FormatAsync(updatedDocument, cancellationToken: cancellationToken).ConfigureAwait(false);
    }
}
