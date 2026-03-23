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
/// Wraps embedded control-flow statements in braces.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(IfBodyBracesCodeFixProvider))]
[Shared]
public sealed class IfBodyBracesCodeFixProvider : CodeFixProvider
{
    /// <summary>
    /// The diagnostic identifiers this provider can fix.
    /// </summary>
    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(Diagnostics.HelenaDiagnosticDescriptors.IfBodyBraces.Id);

    /// <summary>
    /// The fix-all provider used for batch application.
    /// </summary>
    public override FixAllProvider GetFixAllProvider()
    {
        return WellKnownFixAllProviders.BatchFixer;
    }

    /// <summary>
    /// Registers a code action that wraps the targeted body in braces.
    /// </summary>
    /// <param name="context">The code-fix registration context.</param>
    public override Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        Diagnostic diagnostic = context.Diagnostics.First();

        context.RegisterCodeFix(
            CodeAction.Create(
                title: "Wrap control body in braces",
                createChangedDocument: cancellationToken => WrapIfBodyInBracesAsync(context.Document, diagnostic.Location, cancellationToken),
                equivalenceKey: CodeFixConstants.BatchEquivalenceKey),
            diagnostic);

        return Task.CompletedTask;
    }

    /// <summary>
    /// Wraps the targeted control-flow body in a block.
    /// </summary>
    /// <param name="document">The document being updated.</param>
    /// <param name="diagnosticLocation">The location of the reported diagnostic.</param>
    /// <param name="cancellationToken">The cancellation token for the async operation.</param>
    /// <returns>The updated document when the target node is found; otherwise the original document.</returns>
    private static async Task<Document> WrapIfBodyInBracesAsync(
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
        SyntaxNode updatedRoot = root;

        if (targetNode.AncestorsAndSelf().OfType<ElseClauseSyntax>().FirstOrDefault() is ElseClauseSyntax elseClause &&
            elseClause.Statement is not BlockSyntax &&
            elseClause.Statement is not IfStatementSyntax)
        {
            BlockSyntax block = CreateBlock(elseClause.Statement);
            ElseClauseSyntax updatedElseClause = elseClause
                .WithStatement(block)
                .WithAdditionalAnnotations(Formatter.Annotation);

            updatedRoot = root.ReplaceNode(elseClause, updatedElseClause);
        }
        else if (TryGetEmbeddableOwner(targetNode) is StatementSyntax ownerStatement &&
                 TryGetEmbeddedStatement(ownerStatement) is StatementSyntax embeddedStatement &&
                 embeddedStatement is not BlockSyntax)
        {
            BlockSyntax block = CreateBlock(embeddedStatement);
            StatementSyntax updatedOwnerStatement = ReplaceEmbeddedStatement(ownerStatement, block)
                .WithAdditionalAnnotations(Formatter.Annotation);

            updatedRoot = root.ReplaceNode(ownerStatement, updatedOwnerStatement);
        }
        else
        {
            return document;
        }

        Document updatedDocument = document.WithSyntaxRoot(updatedRoot);
        return await Formatter.FormatAsync(updatedDocument, cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Creates a block around the supplied embedded statement.
    /// </summary>
    /// <param name="statement">The embedded statement that should be wrapped.</param>
    /// <returns>A new block containing the original statement.</returns>
    private static BlockSyntax CreateBlock(StatementSyntax statement)
    {
        return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Block(statement)
            .WithAdditionalAnnotations(Formatter.Annotation);
    }

    /// <summary>
    /// Finds the nearest control statement owner whose embedded body can be wrapped.
    /// </summary>
    /// <param name="targetNode">The node reported by the diagnostic lookup.</param>
    /// <returns>The owning control statement, or <c>null</c> when none matches.</returns>
    private static StatementSyntax? TryGetEmbeddableOwner(SyntaxNode targetNode)
    {
        return targetNode.AncestorsAndSelf().OfType<StatementSyntax>().FirstOrDefault((statement) =>
            statement is IfStatementSyntax ||
            statement is ForStatementSyntax ||
            statement is ForEachStatementSyntax ||
            statement is ForEachVariableStatementSyntax ||
            statement is WhileStatementSyntax ||
            statement is DoStatementSyntax ||
            statement is UsingStatementSyntax ||
            statement is LockStatementSyntax ||
            statement is FixedStatementSyntax);
    }

    /// <summary>
    /// Returns the embedded statement body for a control statement owner.
    /// </summary>
    /// <param name="ownerStatement">The owning control statement.</param>
    /// <returns>The embedded body, or <c>null</c> when the statement is unsupported.</returns>
    private static StatementSyntax? TryGetEmbeddedStatement(StatementSyntax ownerStatement)
    {
        return ownerStatement switch
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
    /// Replaces the embedded statement body for a supported control statement owner.
    /// </summary>
    /// <param name="ownerStatement">The owning control statement.</param>
    /// <param name="replacementBody">The replacement block body.</param>
    /// <returns>The updated owner statement.</returns>
    private static StatementSyntax ReplaceEmbeddedStatement(
        StatementSyntax ownerStatement,
        BlockSyntax replacementBody)
    {
        return ownerStatement switch
        {
            IfStatementSyntax ifStatement => ifStatement.WithStatement(replacementBody),
            ForStatementSyntax forStatement => forStatement.WithStatement(replacementBody),
            ForEachStatementSyntax forEachStatement => forEachStatement.WithStatement(replacementBody),
            ForEachVariableStatementSyntax forEachVariableStatement => forEachVariableStatement.WithStatement(replacementBody),
            WhileStatementSyntax whileStatement => whileStatement.WithStatement(replacementBody),
            DoStatementSyntax doStatement => doStatement.WithStatement(replacementBody),
            UsingStatementSyntax usingStatement => usingStatement.WithStatement(replacementBody),
            LockStatementSyntax lockStatement => lockStatement.WithStatement(replacementBody),
            FixedStatementSyntax fixedStatement => fixedStatement.WithStatement(replacementBody),
            _ => ownerStatement,
        };
    }
}
