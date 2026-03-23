package dev.distrohelena.linter.checkstyle.checks;

import com.puppycrawl.tools.checkstyle.api.AbstractCheck;
import com.puppycrawl.tools.checkstyle.api.DetailAST;
import com.puppycrawl.tools.checkstyle.api.TokenTypes;
import dev.distrohelena.linter.checkstyle.diagnostics.HelenaMessageIds;
import dev.distrohelena.linter.checkstyle.helpers.ConditionComparisonAnalyzer;

/**
 * Reports complementary {@code else if} branches that should be written as plain {@code else} branches.
 */
public final class RedundantElseIfCheck extends AbstractCheck {

    /**
     * Creates the check that identifies redundant complementary {@code else if} branches.
     */
    public RedundantElseIfCheck() {
    }

    /**
     * Returns the token types that should be inspected for redundant {@code else if} branches.
     *
     * @return the token types the check should visit.
     */
    @Override
    public int[] getDefaultTokens() {
        return new int[] {
                TokenTypes.LITERAL_IF
        };
    }

    /**
     * Returns the token types that should be inspected for redundant {@code else if} branches.
     *
     * @return the token types the check should accept.
     */
    @Override
    public int[] getAcceptableTokens() {
        return getDefaultTokens();
    }

    /**
     * Returns the token types that must be processed by this rule.
     *
     * @return an empty token set because the check only relies on visit callbacks.
     */
    @Override
    public int[] getRequiredTokens() {
        return new int[0];
    }

    /**
     * Reports a complementary {@code else if} branch when it exactly mirrors the preceding {@code if}.
     *
     * @param ast the {@code if} statement being visited.
     */
    @Override
    public void visitToken(DetailAST ast) {
        DetailAST elseNode = ast.getParent();

        if (elseNode == null) {
            return;
        } else if (elseNode.getType() != TokenTypes.LITERAL_ELSE) {
            return;
        }

        DetailAST containingIf = elseNode.getParent();

        if (containingIf == null) {
            return;
        } else if (containingIf.getType() != TokenTypes.LITERAL_IF) {
            return;
        }

        DetailAST previousCondition = findCondition(containingIf);
        DetailAST currentCondition = findCondition(ast);

        if (previousCondition == null) {
            return;
        } else if (currentCondition == null) {
            return;
        } else if (!ConditionComparisonAnalyzer.isComplementaryCondition(previousCondition, currentCondition)) {
            return;
        }

        log(ast, HelenaMessageIds.REDUNDANT_ELSE_IF);
    }

    /**
     * Locates the comparison expression that controls an {@code if} statement.
     *
     * @param ifStatement the {@code if} statement to inspect.
     * @return the condition expression, or {@code null} when it cannot be resolved.
     */
    private static DetailAST findCondition(DetailAST ifStatement) {
        DetailAST expression = ifStatement.findFirstToken(TokenTypes.EXPR);

        if (expression == null) {
            return null;
        }

        return expression.getFirstChild();
    }
}
