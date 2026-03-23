using DistroHelena.Linter.CSharp.Helpers;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xunit;

namespace DistroHelena.Linter.CSharp.Tests.Helpers;

/// <summary>
/// Covers complementary condition detection used by the redundant else-if rule.
/// </summary>
public class ConditionComparisonHelpersTests
{
    /// <summary>
    /// Verifies the helper detects null-complement comparisons.
    /// </summary>
    [Fact]
    public void IsComplementaryCondition_ReturnsTrue_ForNullComparisonPair()
    {
        ExpressionSyntax first = SyntaxFactory.ParseExpression("value == null");
        ExpressionSyntax second = SyntaxFactory.ParseExpression("value != null");

        bool result = ConditionComparisonHelpers.IsComplementaryCondition(first, second);

        Assert.True(result);
    }

    /// <summary>
    /// Verifies the helper detects boolean negation complements.
    /// </summary>
    [Fact]
    public void IsComplementaryCondition_ReturnsTrue_ForBooleanNegationPair()
    {
        ExpressionSyntax first = SyntaxFactory.ParseExpression("!flag");
        ExpressionSyntax second = SyntaxFactory.ParseExpression("flag");

        bool result = ConditionComparisonHelpers.IsComplementaryCondition(first, second);

        Assert.True(result);
    }

    /// <summary>
    /// Verifies the helper does not treat unrelated conditions as complementary.
    /// </summary>
    [Fact]
    public void IsComplementaryCondition_ReturnsFalse_ForDifferentExpressions()
    {
        ExpressionSyntax first = SyntaxFactory.ParseExpression("count == 0");
        ExpressionSyntax second = SyntaxFactory.ParseExpression("count > 0");

        bool result = ConditionComparisonHelpers.IsComplementaryCondition(first, second);

        Assert.False(result);
    }
}
