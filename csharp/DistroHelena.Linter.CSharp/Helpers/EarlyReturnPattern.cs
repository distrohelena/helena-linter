using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DistroHelena.Linter.CSharp.Helpers;

/// <summary>
/// Represents a supported early-return rewrite opportunity.
/// </summary>
public sealed class EarlyReturnPattern
{
    /// <summary>
    /// Initializes a new early-return pattern description.
    /// </summary>
    /// <param name="kind">The rewrite family identified for the current pattern.</param>
    /// <param name="ifStatement">The original <c>if</c> statement being rewritten.</param>
    /// <param name="guardCondition">The condition expression to use on the rewritten guard clause.</param>
    /// <param name="guardStatement">The exiting statement to retain inside the guard clause.</param>
    /// <param name="hoistedStatements">The statements to move after the guard clause.</param>
    /// <param name="removedSiblingStatement">The sibling statement removed by wrapped happy-path rewrites, when applicable.</param>
    public EarlyReturnPattern(
        EarlyReturnPatternKind kind,
        IfStatementSyntax ifStatement,
        ExpressionSyntax guardCondition,
        StatementSyntax guardStatement,
        ImmutableArray<StatementSyntax> hoistedStatements,
        StatementSyntax? removedSiblingStatement)
    {
        Kind = kind;
        IfStatement = ifStatement;
        GuardCondition = guardCondition;
        GuardStatement = guardStatement;
        HoistedStatements = hoistedStatements;
        RemovedSiblingStatement = removedSiblingStatement;
    }

    /// <summary>
    /// Gets the rewrite family identified for this pattern.
    /// </summary>
    public EarlyReturnPatternKind Kind { get; }

    /// <summary>
    /// Gets the original <c>if</c> statement being rewritten.
    /// </summary>
    public IfStatementSyntax IfStatement { get; }

    /// <summary>
    /// Gets the condition expression to use on the rewritten guard clause.
    /// </summary>
    public ExpressionSyntax GuardCondition { get; }

    /// <summary>
    /// Gets the exiting statement to retain inside the guard clause.
    /// </summary>
    public StatementSyntax GuardStatement { get; }

    /// <summary>
    /// Gets the statements that should be hoisted after the guard clause.
    /// </summary>
    public ImmutableArray<StatementSyntax> HoistedStatements { get; }

    /// <summary>
    /// Gets the sibling statement removed by wrapped happy-path rewrites, when applicable.
    /// </summary>
    public StatementSyntax? RemovedSiblingStatement { get; }
}
