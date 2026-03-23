using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;

namespace DistroHelena.Linter.CSharp.CodeFixes;

/// <summary>
/// Inserts blank lines after completed <c>if</c> chains when a following sibling statement exists.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(IfFollowingSpacingCodeFixProvider))]
[Shared]
public sealed class IfFollowingSpacingCodeFixProvider : CodeFixProvider
{
    /// <summary>
    /// The diagnostic identifiers this provider can fix.
    /// </summary>
    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(Diagnostics.HelenaDiagnosticDescriptors.IfFollowingSpacing.Id);

    /// <summary>
    /// The fix-all provider used for batch application.
    /// </summary>
    public override FixAllProvider GetFixAllProvider()
    {
        return WellKnownFixAllProviders.BatchFixer;
    }

    /// <summary>
    /// Registers a code action that inserts a blank line before the following sibling statement.
    /// </summary>
    /// <param name="context">The code-fix registration context.</param>
    public override Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        Diagnostic diagnostic = context.Diagnostics.First();

        context.RegisterCodeFix(
            CodeAction.Create(
                title: "Add blank line after if block",
                createChangedDocument: cancellationToken =>
                    StatementSpacingCodeFixSupport.InsertBlankLineBeforeNextStatementAsync(
                        context.Document,
                        diagnostic.Location,
                        cancellationToken),
                equivalenceKey: CodeFixConstants.BatchEquivalenceKey),
            diagnostic);

        return Task.CompletedTask;
    }
}
