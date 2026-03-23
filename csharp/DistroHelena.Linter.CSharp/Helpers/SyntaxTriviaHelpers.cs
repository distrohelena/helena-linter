using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DistroHelena.Linter.CSharp.Helpers;

/// <summary>
/// Provides shared helpers for working with syntax trivia and blank-line formatting.
/// </summary>
public static class SyntaxTriviaHelpers
{
    /// <summary>
    /// Determines whether a trivia list already contains a blank line between statements.
    /// </summary>
    /// <param name="triviaList">The trivia list to inspect.</param>
    /// <returns><c>true</c> when at least two end-of-line markers are present; otherwise <c>false</c>.</returns>
    public static bool HasBlankLine(SyntaxTriviaList triviaList)
    {
        int endOfLineCount = 0;

        foreach (SyntaxTrivia trivia in triviaList)
        {
            if (trivia.IsKind(SyntaxKind.EndOfLineTrivia))
            {
                endOfLineCount++;

                continue;
            }

            if (!trivia.IsKind(SyntaxKind.WhitespaceTrivia))
            {
                endOfLineCount = 0;
            }

            if (endOfLineCount >= 2)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Determines whether a statement is already preceded by a blank line.
    /// </summary>
    /// <param name="statement">The statement to inspect.</param>
    /// <returns><c>true</c> when the statement has a blank line before it; otherwise <c>false</c>.</returns>
    public static bool HasBlankLineBefore(StatementSyntax statement)
    {
        return HasBlankLine(statement.GetLeadingTrivia());
    }

    /// <summary>
    /// Determines whether two sibling statements are separated by a blank line.
    /// </summary>
    /// <param name="currentStatement">The current statement.</param>
    /// <param name="nextStatement">The following sibling statement.</param>
    /// <returns><c>true</c> when the statements are separated by a blank line; otherwise <c>false</c>.</returns>
    public static bool HasBlankLineBetween(StatementSyntax currentStatement, StatementSyntax nextStatement)
    {
        SyntaxTriviaList combinedTrivia = currentStatement.GetLastToken().TrailingTrivia.AddRange(nextStatement.GetLeadingTrivia());
        return HasBlankLine(combinedTrivia);
    }

    /// <summary>
    /// Inserts one blank line before the supplied statement when one is not already present.
    /// </summary>
    /// <param name="root">The syntax root containing the target statement.</param>
    /// <param name="statement">The statement that should gain a blank line before it.</param>
    /// <returns>The updated syntax root.</returns>
    public static SyntaxNode InsertBlankLineBeforeStatement(SyntaxNode root, StatementSyntax statement)
    {
        SyntaxTriviaList leadingTrivia = statement.GetLeadingTrivia();

        if (HasBlankLine(leadingTrivia))
        {
            return root;
        }

        string endOfLineText = GetEndOfLineText(leadingTrivia);
        SyntaxTrivia endOfLineTrivia = SyntaxFactory.EndOfLine(endOfLineText);
        int insertionIndex = GetBlankLineInsertionIndex(leadingTrivia);
        SyntaxTriviaList updatedLeadingTrivia = leadingTrivia.Insert(insertionIndex, endOfLineTrivia);
        StatementSyntax updatedStatement = statement.WithLeadingTrivia(updatedLeadingTrivia);
        return root.ReplaceNode(statement, updatedStatement);
    }

    /// <summary>
    /// Determines the trivia index where the inserted blank line should be placed.
    /// </summary>
    /// <param name="leadingTrivia">The statement's current leading trivia.</param>
    /// <returns>The insertion index for an additional end-of-line trivia item.</returns>
    private static int GetBlankLineInsertionIndex(SyntaxTriviaList leadingTrivia)
    {
        for (int index = 0; index < leadingTrivia.Count; index++)
        {
            if (leadingTrivia[index].IsKind(SyntaxKind.EndOfLineTrivia))
            {
                return index + 1;
            }
        }

        return 0;
    }

    /// <summary>
    /// Resolves the end-of-line text to use when inserting a blank line.
    /// </summary>
    /// <param name="leadingTrivia">The statement's current leading trivia.</param>
    /// <returns>The end-of-line text to preserve existing line-ending style.</returns>
    private static string GetEndOfLineText(SyntaxTriviaList leadingTrivia)
    {
        foreach (SyntaxTrivia trivia in leadingTrivia)
        {
            if (trivia.IsKind(SyntaxKind.EndOfLineTrivia))
            {
                return trivia.ToString();
            }
        }

        return "\n";
    }
}
