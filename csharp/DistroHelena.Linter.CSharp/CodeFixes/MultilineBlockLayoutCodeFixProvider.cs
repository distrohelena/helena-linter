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

        string updatedBlockText = CreateMultilineBlockText(block, sourceText);
        BlockSyntax updatedBlock = (BlockSyntax)SyntaxFactory.ParseStatement(updatedBlockText)
            .WithAdditionalAnnotations(Formatter.Annotation);

        SyntaxNode updatedRoot = root.ReplaceNode(block, updatedBlock);
        Document updatedDocument = document.WithSyntaxRoot(updatedRoot);
        return await Formatter.FormatAsync(updatedDocument, cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Builds the multiline text used to replace a single-line block.
    /// </summary>
    /// <param name="block">The block being rewritten.</param>
    /// <param name="sourceText">The current document text.</param>
    /// <returns>The multiline block text.</returns>
    private static string CreateMultilineBlockText(BlockSyntax block, SourceText sourceText)
    {
        string source = sourceText.ToString();
        string newline = source.IndexOf("\r\n", System.StringComparison.Ordinal) >= 0 ? "\r\n" : "\n";
        string outerIndentation = GetLineIndentation(block.SpanStart, sourceText);
        string innerIndentation = outerIndentation + "    ";
        int innerStart = block.OpenBraceToken.Span.End;
        int innerLength = block.CloseBraceToken.SpanStart - innerStart;
        string innerText = sourceText.ToString(TextSpan.FromBounds(innerStart, innerStart + innerLength)).Trim();

        return $"{outerIndentation}{{{newline}{innerIndentation}{innerText}{newline}{outerIndentation}}}";
    }

    /// <summary>
    /// Resolves the indentation used at the start of the line containing the supplied position.
    /// </summary>
    /// <param name="position">The absolute source position.</param>
    /// <param name="sourceText">The current document text.</param>
    /// <returns>The whitespace prefix for the containing line.</returns>
    private static string GetLineIndentation(int position, SourceText sourceText)
    {
        TextLine line = sourceText.Lines.GetLineFromPosition(position);
        string lineText = sourceText.ToString(TextSpan.FromBounds(line.Start, position));

        int indentationLength = 0;

        while (indentationLength < lineText.Length && char.IsWhiteSpace(lineText[indentationLength]))
        {
            indentationLength++;
        }

        return lineText.Substring(0, indentationLength);
    }
}
