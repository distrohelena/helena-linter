package dev.distrohelena.linter.checkstyle.checks;

import com.puppycrawl.tools.checkstyle.api.AbstractCheck;
import com.puppycrawl.tools.checkstyle.api.DetailAST;
import com.puppycrawl.tools.checkstyle.api.TokenTypes;
import dev.distrohelena.linter.checkstyle.diagnostics.HelenaMessageIds;
import dev.distrohelena.linter.checkstyle.helpers.ControlFlowExitAnalyzer;
import dev.distrohelena.linter.checkstyle.helpers.StatementAstNavigator;

/**
 * Reports sibling {@code if} statements that should be folded into an {@code else if} chain.
 */
public final class IfElseIfChainCheck extends AbstractCheck {

    /**
     * Creates the check that identifies sibling {@code if} statements that should chain through {@code else if}.
     */
    public IfElseIfChainCheck() {
    }

    /**
     * Returns the token types that should be inspected for {@code else if} chain folding opportunities.
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
     * Returns the token types that should be inspected for {@code else if} chain folding opportunities.
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
     * Reports a sibling {@code if} statement when the previous statement definitively exits.
     *
     * @param ast the {@code if} statement being visited.
     */
    @Override
    public void visitToken(DetailAST ast) {
        DetailAST parent = ast.getParent();

        if (parent == null) {
            return;
        } else if (!StatementAstNavigator.isStatementContainer(parent)) {
            return;
        }

        DetailAST previousSibling = StatementAstNavigator.getPreviousSibling(ast);

        if (previousSibling == null) {
            return;
        } else if (previousSibling.getType() != TokenTypes.LITERAL_IF) {
            return;
        } else if (previousSibling.findFirstToken(TokenTypes.LITERAL_ELSE) != null) {
            return;
        }

        DetailAST thenBranch = findThenBranch(previousSibling);

        if (thenBranch == null) {
            return;
        } else if (!ControlFlowExitAnalyzer.doesStatementDefinitelyExit(thenBranch)) {
            return;
        }

        log(ast, HelenaMessageIds.IF_ELSE_IF_CHAIN);
    }

    /**
     * Locates the then-branch that follows the condition portion of an {@code if} statement.
     *
     * @param ifStatement the {@code if} statement to inspect.
     * @return the first branch statement beneath the {@code if} node, or {@code null} when absent.
     */
    private static DetailAST findThenBranch(DetailAST ifStatement) {
        DetailAST rightParen = ifStatement.findFirstToken(TokenTypes.RPAREN);

        if (rightParen == null) {
            return null;
        }

        return StatementAstNavigator.getNextSibling(rightParen);
    }
}
