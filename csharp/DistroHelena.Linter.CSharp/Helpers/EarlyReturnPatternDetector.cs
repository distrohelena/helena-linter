using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DistroHelena.Linter.CSharp.Helpers;

/// <summary>
/// Detects supported early-return rewrite opportunities in C# syntax trees.
/// </summary>
public static class EarlyReturnPatternDetector
{
    /// <summary>
    /// Attempts to identify a supported early-return rewrite pattern for the supplied <c>if</c> statement.
    /// </summary>
    /// <param name="ifStatement">The <c>if</c> statement to analyze.</param>
    /// <returns>A detected pattern when the statement matches a supported rewrite shape; otherwise <c>null</c>.</returns>
    public static EarlyReturnPattern? TryDetect(IfStatementSyntax ifStatement)
    {
        if (ifStatement.Parent is not BlockSyntax)
        {
            return null;
        }

        return TryDetectIfElsePattern(ifStatement) ?? TryDetectWrappedHappyPathPattern(ifStatement);
    }

    /// <summary>
    /// Detects supported <c>if / else</c> early-return rewrites.
    /// </summary>
    /// <param name="ifStatement">The <c>if</c> statement to analyze.</param>
    /// <returns>A detected pattern when the <c>if / else</c> shape is supported; otherwise <c>null</c>.</returns>
    private static EarlyReturnPattern? TryDetectIfElsePattern(IfStatementSyntax ifStatement)
    {
        if (ifStatement.Else is not ElseClauseSyntax elseClause)
        {
            return null;
        }

        bool ifBranchExits = ControlFlowExitStatementAnalyzer.DoesStatementDefinitelyExit(ifStatement.Statement);
        bool elseBranchExits = ControlFlowExitStatementAnalyzer.DoesStatementDefinitelyExit(elseClause.Statement);

        if (ifBranchExits == elseBranchExits)
        {
            return null;
        }

        if (ifBranchExits)
        {
            ImmutableArray<StatementSyntax> hoistedStatements = GetStatements(elseClause.Statement);

            if (hoistedStatements.IsDefaultOrEmpty)
            {
                return null;
            }

            return new EarlyReturnPattern(
                EarlyReturnPatternKind.ExitingIfBranch,
                ifStatement,
                ifStatement.Condition,
                ifStatement.Statement,
                hoistedStatements,
                removedSiblingStatement: null);
        }

        ImmutableArray<StatementSyntax> invertedHoistedStatements = GetStatements(ifStatement.Statement);

        if (invertedHoistedStatements.IsDefaultOrEmpty)
        {
            return null;
        }

        return new EarlyReturnPattern(
            EarlyReturnPatternKind.ExitingElseBranch,
            ifStatement,
            ConditionNegationExpressionBuilder.BuildNegatedCondition(ifStatement.Condition),
            elseClause.Statement,
            invertedHoistedStatements,
            removedSiblingStatement: null);
    }

    /// <summary>
    /// Detects wrapped happy-path rewrites with an exiting following sibling statement.
    /// </summary>
    /// <param name="ifStatement">The <c>if</c> statement to analyze.</param>
    /// <returns>A detected pattern when the wrapped happy-path shape is supported; otherwise <c>null</c>.</returns>
    private static EarlyReturnPattern? TryDetectWrappedHappyPathPattern(IfStatementSyntax ifStatement)
    {
        if (ifStatement.Else is not null)
        {
            return null;
        }

        if (ControlFlowExitStatementAnalyzer.DoesStatementDefinitelyExit(ifStatement.Statement))
        {
            return null;
        }

        StatementSyntax? nextStatement = StatementSequenceHelpers.GetNextStatement(ifStatement);

        if (nextStatement is null || !ControlFlowExitStatementAnalyzer.DoesStatementDefinitelyExit(nextStatement))
        {
            return null;
        }

        ImmutableArray<StatementSyntax> hoistedStatements = GetStatements(ifStatement.Statement);

        if (hoistedStatements.IsDefaultOrEmpty)
        {
            return null;
        }

        return new EarlyReturnPattern(
            EarlyReturnPatternKind.WrappedHappyPath,
            ifStatement,
            ConditionNegationExpressionBuilder.BuildNegatedCondition(ifStatement.Condition),
            nextStatement,
            hoistedStatements,
            nextStatement);
    }

    /// <summary>
    /// Flattens a statement into the list of statements that should be hoisted after a guard clause.
    /// </summary>
    /// <param name="statement">The statement to flatten.</param>
    /// <returns>The flattened statement list.</returns>
    private static ImmutableArray<StatementSyntax> GetStatements(StatementSyntax statement)
    {
        if (statement is BlockSyntax blockSyntax)
        {
            return blockSyntax.Statements.ToImmutableArray();
        }

        return ImmutableArray.Create(statement);
    }
}
