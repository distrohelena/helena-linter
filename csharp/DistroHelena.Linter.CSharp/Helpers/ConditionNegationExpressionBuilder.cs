using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DistroHelena.Linter.CSharp.Helpers;

/// <summary>
/// Builds negated C# condition expressions for guard-clause rewrites.
/// </summary>
public static class ConditionNegationExpressionBuilder
{
    /// <summary>
    /// Creates a negated form of the supplied condition expression.
    /// </summary>
    /// <param name="condition">The condition expression to negate.</param>
    /// <returns>A syntax node representing the negated condition.</returns>
    public static ExpressionSyntax BuildNegatedCondition(ExpressionSyntax condition)
    {
        if (condition is PrefixUnaryExpressionSyntax prefixUnaryExpression &&
            prefixUnaryExpression.Kind() == SyntaxKind.LogicalNotExpression)
        {
            return prefixUnaryExpression.Operand.WithTriviaFrom(condition);
        }

        if (condition is BinaryExpressionSyntax binaryExpression)
        {
            SyntaxKind? negatedKind = GetNegatedBinaryKind(binaryExpression.Kind());

            if (negatedKind.HasValue)
            {
                return SyntaxFactory.BinaryExpression(
                    negatedKind.Value,
                    binaryExpression.Left,
                    binaryExpression.Right).WithTriviaFrom(condition);
            }
        }

        ExpressionSyntax negatedOperand = CanNegateWithoutParentheses(condition)
            ? condition.WithoutTrivia()
            : SyntaxFactory.ParenthesizedExpression(condition.WithoutTrivia());

        return SyntaxFactory.PrefixUnaryExpression(SyntaxKind.LogicalNotExpression, negatedOperand)
            .WithTriviaFrom(condition);
    }

    /// <summary>
    /// Resolves the complementary comparison kind for supported binary conditions.
    /// </summary>
    /// <param name="kind">The original binary expression kind.</param>
    /// <returns>The negated comparison kind when supported; otherwise <c>null</c>.</returns>
    private static SyntaxKind? GetNegatedBinaryKind(SyntaxKind kind)
    {
        return kind switch
        {
            SyntaxKind.EqualsExpression => SyntaxKind.NotEqualsExpression,
            SyntaxKind.NotEqualsExpression => SyntaxKind.EqualsExpression,
            SyntaxKind.GreaterThanExpression => SyntaxKind.LessThanOrEqualExpression,
            SyntaxKind.GreaterThanOrEqualExpression => SyntaxKind.LessThanExpression,
            SyntaxKind.LessThanExpression => SyntaxKind.GreaterThanOrEqualExpression,
            SyntaxKind.LessThanOrEqualExpression => SyntaxKind.GreaterThanExpression,
            _ => null,
        };
    }

    /// <summary>
    /// Determines whether a condition can be negated without adding parentheses.
    /// </summary>
    /// <param name="condition">The condition to inspect.</param>
    /// <returns><c>true</c> when the expression can be safely prefixed with <c>!</c>; otherwise <c>false</c>.</returns>
    private static bool CanNegateWithoutParentheses(ExpressionSyntax condition)
    {
        return condition is IdentifierNameSyntax ||
               condition is InvocationExpressionSyntax ||
               condition is MemberAccessExpressionSyntax ||
               condition is ElementAccessExpressionSyntax ||
               condition is ParenthesizedExpressionSyntax;
    }
}
