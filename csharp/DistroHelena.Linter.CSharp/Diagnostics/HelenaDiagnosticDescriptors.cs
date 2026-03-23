using Microsoft.CodeAnalysis;

namespace DistroHelena.Linter.CSharp.Diagnostics;

/// <summary>
/// Exposes the shared diagnostic descriptors used by the first-pass Helena rules.
/// </summary>
public static class HelenaDiagnosticDescriptors
{
    /// <summary>
    /// Diagnostic for complementary <c>else if</c> branches that should be written as <c>else</c>.
    /// </summary>
    public static readonly DiagnosticDescriptor RedundantElseIf = CreateDescriptor(
        id: "HLN001",
        title: "Use else instead of complementary else-if",
        messageFormat: "Replace this complementary else-if with else");

    /// <summary>
    /// Diagnostic for missing blank lines after an <c>if</c> chain.
    /// </summary>
    public static readonly DiagnosticDescriptor IfFollowingSpacing = CreateDescriptor(
        id: "HLN002",
        title: "Add a blank line after if blocks",
        messageFormat: "Add a blank line after this if block");

    /// <summary>
    /// Diagnostic for missing blank lines after non-if control-flow blocks.
    /// </summary>
    public static readonly DiagnosticDescriptor ControlBlockFollowingSpacing = CreateDescriptor(
        id: "HLN003",
        title: "Add a blank line after control blocks",
        messageFormat: "Add a blank line after this control block");

    /// <summary>
    /// Diagnostic for missing blank lines before exit statements.
    /// </summary>
    public static readonly DiagnosticDescriptor ExitSpacing = CreateDescriptor(
        id: "HLN004",
        title: "Add a blank line before exit statements",
        messageFormat: "Add a blank line before this exit statement");

    /// <summary>
    /// Diagnostic for missing blank lines after declarations.
    /// </summary>
    public static readonly DiagnosticDescriptor DeclarationSpacing = CreateDescriptor(
        id: "HLN005",
        title: "Add a blank line after declarations",
        messageFormat: "Add a blank line after this declaration");

    /// <summary>
    /// Diagnostic for missing blank lines before declarations.
    /// </summary>
    public static readonly DiagnosticDescriptor DeclarationLeadingSpacing = CreateDescriptor(
        id: "HLN006",
        title: "Add a blank line before declarations",
        messageFormat: "Add a blank line before this declaration");

    /// <summary>
    /// Diagnostic for missing blank lines before <c>if</c> statements.
    /// </summary>
    public static readonly DiagnosticDescriptor IfLeadingSpacing = CreateDescriptor(
        id: "HLN007",
        title: "Add a blank line before if statements",
        messageFormat: "Add a blank line before this if statement");

    /// <summary>
    /// Diagnostic for sibling <c>if</c> statements that should be folded into an <c>else if</c> chain.
    /// </summary>
    public static readonly DiagnosticDescriptor IfElseIfChain = CreateDescriptor(
        id: "HLN008",
        title: "Use else-if for sibling if statements",
        messageFormat: "Fold this sibling if into an else-if chain");

    /// <summary>
    /// Diagnostic for control-flow patterns that should be rewritten as early-return guard clauses.
    /// </summary>
    public static readonly DiagnosticDescriptor EarlyReturn = CreateDescriptor(
        id: "HLN009",
        title: "Rewrite as early return",
        messageFormat: "Rewrite this control flow as an early-return guard clause");

    /// <summary>
    /// Creates a descriptor using Helena's shared defaults.
    /// </summary>
    /// <param name="id">The diagnostic identifier.</param>
    /// <param name="title">The analyzer title.</param>
    /// <param name="messageFormat">The diagnostic message.</param>
    /// <returns>A descriptor configured for hidden diagnostics with code fixes.</returns>
    private static DiagnosticDescriptor CreateDescriptor(string id, string title, string messageFormat)
    {
        return new DiagnosticDescriptor(
            id,
            title,
            messageFormat,
            HelenaDiagnosticCategories.Readability,
            DiagnosticSeverity.Hidden,
            isEnabledByDefault: true);
    }
}
