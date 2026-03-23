package dev.distrohelena.linter.checkstyle.helpers;

import com.puppycrawl.tools.checkstyle.api.DetailAST;
import com.puppycrawl.tools.checkstyle.api.TokenTypes;

/**
 * Compares conditional expressions for the complementary shapes used by Helena rules.
 */
public final class ConditionComparisonAnalyzer {

    /**
     * Prevents direct instantiation.
     */
    private ConditionComparisonAnalyzer() {
    }

    /**
     * Determines whether two condition expressions are exact complements of one another.
     *
     * @param firstCondition the first conditional expression.
     * @param secondCondition the second conditional expression.
     * @return {@code true} when the expressions represent complementary branches; otherwise {@code false}.
     */
    public static boolean isComplementaryCondition(DetailAST firstCondition, DetailAST secondCondition) {
        DetailAST normalizedFirst = normalizeCondition(firstCondition);
        DetailAST normalizedSecond = normalizeCondition(secondCondition);

        if (normalizedFirst == null || normalizedSecond == null) {
            return false;
        } else if (isBooleanNegationPair(normalizedFirst, normalizedSecond)) {
            return true;
        }

        return isComplementaryBinaryComparison(normalizedFirst, normalizedSecond);
    }

    /**
     * Resolves the outer expression node into the comparison or operand subtree that matters for matching.
     *
     * @param condition the raw condition node from Checkstyle.
     * @return the comparable expression subtree.
     */
    private static DetailAST normalizeCondition(DetailAST condition) {
        while (condition != null) {
            if (condition.getType() == TokenTypes.EXPR && condition.getFirstChild() != null) {
                condition = condition.getFirstChild();
                continue;
            }

            if (condition.getType() == TokenTypes.LPAREN && condition.getFirstChild() != null) {
                condition = condition.getFirstChild();
                continue;
            }

            break;
        }

        return condition;
    }

    /**
     * Determines whether the two expressions match a {@code !value} versus {@code value} pattern.
     *
     * @param firstCondition the first conditional expression.
     * @param secondCondition the second conditional expression.
     * @return {@code true} when one expression is the negation of the other; otherwise {@code false}.
     */
    private static boolean isBooleanNegationPair(DetailAST firstCondition, DetailAST secondCondition) {
        if (firstCondition.getType() == TokenTypes.LNOT) {
            return areEquivalent(firstCondition.getFirstChild(), secondCondition);
        } else if (secondCondition.getType() == TokenTypes.LNOT) {
            return areEquivalent(secondCondition.getFirstChild(), firstCondition);
        }

        return false;
    }

    /**
     * Determines whether the two expressions are complementary equality and inequality comparisons.
     *
     * @param firstCondition the first conditional expression.
     * @param secondCondition the second conditional expression.
     * @return {@code true} when the comparisons differ only by equality operator polarity; otherwise {@code false}.
     */
    private static boolean isComplementaryBinaryComparison(
            DetailAST firstCondition,
            DetailAST secondCondition) {
        if (!isEqualityComparison(firstCondition) || !isEqualityComparison(secondCondition)) {
            return false;
        } else if (!areComplementaryEqualityKinds(firstCondition.getType(), secondCondition.getType())) {
            return false;
        }

        return haveEquivalentOperands(firstCondition, secondCondition);
    }

    /**
     * Determines whether the token type represents an equality comparison.
     *
     * @param condition the comparison node to inspect.
     * @return {@code true} when the node is an equality or inequality comparison; otherwise {@code false}.
     */
    private static boolean isEqualityComparison(DetailAST condition) {
        return condition.getType() == TokenTypes.EQUAL || condition.getType() == TokenTypes.NOT_EQUAL;
    }

    /**
     * Determines whether the two operator kinds are complementary equality operators.
     *
     * @param firstKind the first binary operator kind.
     * @param secondKind the second binary operator kind.
     * @return {@code true} when one operator is equality and the other is inequality; otherwise {@code false}.
     */
    private static boolean areComplementaryEqualityKinds(int firstKind, int secondKind) {
        return (firstKind == TokenTypes.EQUAL && secondKind == TokenTypes.NOT_EQUAL)
                || (firstKind == TokenTypes.NOT_EQUAL && secondKind == TokenTypes.EQUAL);
    }

    /**
     * Determines whether both binary comparisons operate on the same operand pair.
     *
     * @param firstCondition the first binary comparison.
     * @param secondCondition the second binary comparison.
     * @return {@code true} when the operands are equivalent in either order; otherwise {@code false}.
     */
    private static boolean haveEquivalentOperands(DetailAST firstCondition, DetailAST secondCondition) {
        boolean sameOrder = areEquivalent(firstCondition.getFirstChild(), secondCondition.getFirstChild())
                && areEquivalent(firstCondition.getLastChild(), secondCondition.getLastChild());

        if (sameOrder) {
            return true;
        }

        return areEquivalent(firstCondition.getFirstChild(), secondCondition.getLastChild())
                && areEquivalent(firstCondition.getLastChild(), secondCondition.getFirstChild());
    }

    /**
     * Compares two AST subtrees for structural equivalence.
     *
     * @param firstNode the first subtree.
     * @param secondNode the second subtree.
     * @return {@code true} when both subtrees have the same shape and token text; otherwise {@code false}.
     */
    private static boolean areEquivalent(DetailAST firstNode, DetailAST secondNode) {
        if (firstNode == null || secondNode == null) {
            return firstNode == secondNode;
        } else if (firstNode.getType() != secondNode.getType()) {
            return false;
        } else if (!firstNode.getText().equals(secondNode.getText())) {
            return false;
        }

        DetailAST firstChild = firstNode.getFirstChild();
        DetailAST secondChild = secondNode.getFirstChild();

        while (firstChild != null || secondChild != null) {
            if (!areEquivalent(firstChild, secondChild)) {
                return false;
            }

            firstChild = firstChild == null ? null : firstChild.getNextSibling();
            secondChild = secondChild == null ? null : secondChild.getNextSibling();
        }

        return true;
    }
}
