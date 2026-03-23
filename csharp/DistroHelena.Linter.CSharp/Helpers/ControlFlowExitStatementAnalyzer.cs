using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DistroHelena.Linter.CSharp.Helpers;

/// <summary>
/// Evaluates whether statements definitely exit the current control-flow path.
/// </summary>
public static class ControlFlowExitStatementAnalyzer
{
    /// <summary>
    /// Determines whether the supplied statement definitely exits via <c>return</c>, <c>throw</c>, <c>break</c>, or <c>continue</c>.
    /// </summary>
    /// <param name="statement">The statement to evaluate.</param>
    /// <returns><c>true</c> when the statement definitely exits the current path; otherwise <c>false</c>.</returns>
    public static bool DoesStatementDefinitelyExit(StatementSyntax statement)
    {
        return statement switch
        {
            ReturnStatementSyntax => true,
            ThrowStatementSyntax => true,
            BreakStatementSyntax => true,
            ContinueStatementSyntax => true,
            BlockSyntax block => DoesBlockDefinitelyExit(block),
            IfStatementSyntax ifStatement => DoesIfStatementDefinitelyExit(ifStatement),
            _ => false,
        };
    }

    /// <summary>
    /// Determines whether a block definitely exits by walking its statements in order.
    /// </summary>
    /// <param name="block">The block to evaluate.</param>
    /// <returns><c>true</c> when an executed statement definitely exits the block; otherwise <c>false</c>.</returns>
    private static bool DoesBlockDefinitelyExit(BlockSyntax block)
    {
        foreach (StatementSyntax statement in block.Statements)
        {
            if (DoesStatementDefinitelyExit(statement))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Determines whether both branches of an <c>if</c> statement definitely exit.
    /// </summary>
    /// <param name="ifStatement">The <c>if</c> statement to evaluate.</param>
    /// <returns><c>true</c> when both branches definitely exit; otherwise <c>false</c>.</returns>
    private static bool DoesIfStatementDefinitelyExit(IfStatementSyntax ifStatement)
    {
        if (ifStatement.Else is null)
        {
            return false;
        }

        return DoesStatementDefinitelyExit(ifStatement.Statement) &&
               DoesStatementDefinitelyExit(ifStatement.Else.Statement);
    }
}
