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
        } else if (parent.getType() != TokenTypes.SLIST) {
            return;
        }

        DetailAST previousSibling = StatementAstNavigator.getPreviousSibling(ast);

        if (previousSibling == null) {
            return;
        } else if (!ControlFlowExitAnalyzer.doesStatementDefinitelyExit(previousSibling)) {
            return;
        }

        log(ast, HelenaMessageIds.IF_ELSE_IF_CHAIN);
    }
}
