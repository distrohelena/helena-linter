using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DistroHelena.Linter.CSharp.Helpers;

/// <summary>
/// Provides shared helpers for comparing conditional expressions used by Helena analyzers.
/// </summary>
public static class ConditionComparisonHelpers
{
    /// <summary>
    /// Determines whether two condition expressions are exact complements of one another.
    /// </summary>
    /// <param name="firstCondition">The first conditional expression.</param>
    /// <param name="secondCondition">The second conditional expression.</param>
    /// <returns><c>true</c> when the expressions represent complementary branches; otherwise <c>false</c>.</returns>
    public static bool IsComplementaryCondition(ExpressionSyntax firstCondition, ExpressionSyntax secondCondition)
    {
        if (IsBooleanNegationPair(firstCondition, secondCondition))
        {
            return true;
        }

        return IsComplementaryBinaryComparison(firstCondition, secondCondition);
    }

    /// <summary>
    /// Determines whether the two expressions match a <c>!value</c> versus <c>value</c> pattern.
    /// </summary>
    /// <param name="firstCondition">The first conditional expression.</param>
    /// <param name="secondCondition">The second conditional expression.</param>
    /// <returns><c>true</c> when one expression is the negation of the other; otherwise <c>false</c>.</returns>
    private static bool IsBooleanNegationPair(ExpressionSyntax firstCondition, ExpressionSyntax secondCondition)
    {
        if (firstCondition is PrefixUnaryExpressionSyntax firstUnary &&
            firstUnary.Kind() == SyntaxKind.LogicalNotExpression)
        {
            return SyntaxFactory.AreEquivalent(firstUnary.Operand, secondCondition);
        }

        if (secondCondition is PrefixUnaryExpressionSyntax secondUnary &&
            secondUnary.Kind() == SyntaxKind.LogicalNotExpression)
        {
            return SyntaxFactory.AreEquivalent(secondUnary.Operand, firstCondition);
        }

        return false;
    }

    /// <summary>
    /// Determines whether the two expressions are complementary equality and inequality comparisons.
    /// </summary>
    /// <param name="firstCondition">The first conditional expression.</param>
    /// <param name="secondCondition">The second conditional expression.</param>
    /// <returns><c>true</c> when the comparisons differ only by equality operator polarity; otherwise <c>false</c>.</returns>
    private static bool IsComplementaryBinaryComparison(ExpressionSyntax firstCondition, ExpressionSyntax secondCondition)
    {
        if (firstCondition is not BinaryExpressionSyntax firstBinary ||
            secondCondition is not BinaryExpressionSyntax secondBinary)
        {
            return false;
        }

        return AreComplementaryEqualityKinds(firstBinary.Kind(), secondBinary.Kind()) &&
               HaveEquivalentOperands(firstBinary, secondBinary);
    }

    /// <summary>
    /// Determines whether the two operator kinds are complementary equality operators.
    /// </summary>
    /// <param name="firstKind">The first binary operator kind.</param>
    /// <param name="secondKind">The second binary operator kind.</param>
    /// <returns><c>true</c> when one operator is equality and the other is inequality; otherwise <c>false</c>.</returns>
    private static bool AreComplementaryEqualityKinds(SyntaxKind firstKind, SyntaxKind secondKind)
    {
        return (firstKind == SyntaxKind.EqualsExpression && secondKind == SyntaxKind.NotEqualsExpression) ||
               (firstKind == SyntaxKind.NotEqualsExpression && secondKind == SyntaxKind.EqualsExpression);
    }

    /// <summary>
    /// Determines whether both binary comparisons operate on the same operand pair.
    /// </summary>
    /// <param name="firstBinary">The first binary comparison.</param>
    /// <param name="secondBinary">The second binary comparison.</param>
    /// <returns><c>true</c> when the operands are equivalent in either order; otherwise <c>false</c>.</returns>
    private static bool HaveEquivalentOperands(BinaryExpressionSyntax firstBinary, BinaryExpressionSyntax secondBinary)
    {
        bool sameOrder =
            SyntaxFactory.AreEquivalent(firstBinary.Left, secondBinary.Left) &&
            SyntaxFactory.AreEquivalent(firstBinary.Right, secondBinary.Right);

        if (sameOrder)
        {
            return true;
        }

        return SyntaxFactory.AreEquivalent(firstBinary.Left, secondBinary.Right) &&
               SyntaxFactory.AreEquivalent(firstBinary.Right, secondBinary.Left);
    }
}
