using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DistroHelena.Linter.CSharp.Helpers;

/// <summary>
/// Provides shared statement-navigation helpers for sibling-statement analyzers.
/// </summary>
public static class StatementSequenceHelpers
{
    /// <summary>
    /// Returns the next sibling statement in the same block when one exists.
    /// </summary>
    /// <param name="statement">The current statement.</param>
    /// <returns>The next statement in the containing block, or <c>null</c> when none exists.</returns>
    public static StatementSyntax? GetNextStatement(StatementSyntax statement)
    {
        if (statement.Parent is not BlockSyntax block)
        {
            return null;
        }

        SyntaxList<StatementSyntax> statements = block.Statements;
        int statementIndex = statements.IndexOf(statement);

        if (statementIndex < 0 || statementIndex + 1 >= statements.Count)
        {
            return null;
        }

        return statements[statementIndex + 1];
    }

    /// <summary>
    /// Returns the previous sibling statement in the same block when one exists.
    /// </summary>
    /// <param name="statement">The current statement.</param>
    /// <returns>The previous statement in the containing block, or <c>null</c> when none exists.</returns>
    public static StatementSyntax? GetPreviousStatement(StatementSyntax statement)
    {
        if (statement.Parent is not BlockSyntax block)
        {
            return null;
        }

        SyntaxList<StatementSyntax> statements = block.Statements;
        int statementIndex = statements.IndexOf(statement);

        if (statementIndex <= 0)
        {
            return null;
        }

        return statements[statementIndex - 1];
    }
}
