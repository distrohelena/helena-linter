using System.Collections.Generic;
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
/// Rewrites supported control-flow patterns into early-return guard clauses.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(EarlyReturnCodeFixProvider))]
[Shared]
public sealed class EarlyReturnCodeFixProvider : CodeFixProvider
{
    /// <summary>
    /// The diagnostic identifiers this provider can fix.
    /// </summary>
    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(Diagnostics.HelenaDiagnosticDescriptors.EarlyReturn.Id);

    /// <summary>
    /// The fix-all provider used for batch application.
    /// </summary>
    public override FixAllProvider GetFixAllProvider()
    {
        return WellKnownFixAllProviders.BatchFixer;
    }

    /// <summary>
    /// Registers a code action that rewrites the current pattern into an early-return guard clause.
    /// </summary>
    /// <param name="context">The code-fix registration context.</param>
    public override Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        Diagnostic diagnostic = context.Diagnostics.First();

        context.RegisterCodeFix(
            CodeAction.Create(
                title: "Rewrite as early return",
                createChangedDocument: cancellationToken =>
                    RewriteAsEarlyReturnAsync(context.Document, diagnostic.Location, cancellationToken),
                equivalenceKey: CodeFixConstants.BatchEquivalenceKey),
            diagnostic);

        return Task.CompletedTask;
    }

    /// <summary>
    /// Rewrites the targeted pattern into a guard clause and hoisted trailing statements.
    /// </summary>
    /// <param name="document">The document being updated.</param>
    /// <param name="diagnosticLocation">The location of the reported diagnostic.</param>
    /// <param name="cancellationToken">The cancellation token for the async operation.</param>
    /// <returns>The updated document when the pattern is still valid; otherwise the original document.</returns>
    private static async Task<Document> RewriteAsEarlyReturnAsync(
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

        if (targetNode.AncestorsAndSelf().OfType<IfStatementSyntax>().FirstOrDefault() is not IfStatementSyntax ifStatement ||
            ifStatement.Parent is not BlockSyntax blockSyntax ||
            EarlyReturnPatternDetector.TryDetect(ifStatement) is not EarlyReturnPattern pattern)
        {
            return document;
        }

        int ifStatementIndex = blockSyntax.Statements.IndexOf(ifStatement);

        if (ifStatementIndex < 0)
        {
            return document;
        }

        IfStatementSyntax guardIfStatement = CreateGuardIfStatement(pattern);
        List<StatementSyntax> replacementStatements = new List<StatementSyntax> { guardIfStatement };
        replacementStatements.AddRange(pattern.HoistedStatements);

        List<StatementSyntax> updatedStatements = new List<StatementSyntax>();

        for (int index = 0; index < blockSyntax.Statements.Count; index++)
        {
            StatementSyntax statement = blockSyntax.Statements[index];

            if (index == ifStatementIndex)
            {
                updatedStatements.AddRange(replacementStatements);

                continue;
            }

            if (pattern.RemovedSiblingStatement is not null && statement == pattern.RemovedSiblingStatement)
            {
                continue;
            }

            updatedStatements.Add(statement);
        }

        BlockSyntax updatedBlockSyntax = blockSyntax
            .WithStatements(SyntaxFactory.List(updatedStatements));

        if (pattern.HoistedStatements.Length > 0)
        {
            StatementSyntax firstHoistedStatement = updatedBlockSyntax.Statements[ifStatementIndex + 1];
            updatedBlockSyntax = (BlockSyntax)SyntaxTriviaHelpers.InsertBlankLineBeforeStatement(updatedBlockSyntax, firstHoistedStatement);
        }

        updatedBlockSyntax = updatedBlockSyntax.WithAdditionalAnnotations(Formatter.Annotation);

        SyntaxNode updatedRoot = root.ReplaceNode(blockSyntax, updatedBlockSyntax);
        Document updatedDocument = document.WithSyntaxRoot(updatedRoot);
        return await Formatter.FormatAsync(updatedDocument, cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Builds the guard-clause <c>if</c> statement for a detected early-return pattern.
    /// </summary>
    /// <param name="pattern">The detected pattern to rewrite.</param>
    /// <returns>The rewritten guard-clause <c>if</c> statement.</returns>
    private static IfStatementSyntax CreateGuardIfStatement(EarlyReturnPattern pattern)
    {
        StatementSyntax guardBody = CreateGuardBody(pattern.GuardStatement);

        return SyntaxFactory.IfStatement(pattern.GuardCondition, guardBody)
            .WithLeadingTrivia(pattern.IfStatement.GetLeadingTrivia())
            .WithTrailingTrivia(pattern.IfStatement.GetTrailingTrivia())
            .WithAdditionalAnnotations(Formatter.Annotation);
    }

    /// <summary>
    /// Creates the statement body for a rewritten guard clause.
    /// </summary>
    /// <param name="guardStatement">The original exiting statement to retain.</param>
    /// <returns>A block statement for single exits, or the original block when one already exists.</returns>
    private static StatementSyntax CreateGuardBody(StatementSyntax guardStatement)
    {
        if (guardStatement is BlockSyntax)
        {
            return guardStatement;
        }

        StatementSyntax normalizedGuardStatement = guardStatement
            .WithLeadingTrivia(default(SyntaxTriviaList))
            .WithTrailingTrivia(default(SyntaxTriviaList));

        return SyntaxFactory.Block(normalizedGuardStatement);
    }
}
