package dev.distrohelena.linter.checkstyle.checks;

import com.puppycrawl.tools.checkstyle.api.AbstractCheck;
import com.puppycrawl.tools.checkstyle.api.DetailAST;
import com.puppycrawl.tools.checkstyle.api.TokenTypes;
import dev.distrohelena.linter.checkstyle.diagnostics.HelenaMessageIds;
import dev.distrohelena.linter.checkstyle.helpers.BlankLineAnalyzer;
import dev.distrohelena.linter.checkstyle.helpers.StatementAstNavigator;

/**
 * Requires a blank line after a completed {@code if} chain when another statement follows.
 */
public final class IfFollowingSpacingCheck extends AbstractCheck {

    /**
     * Creates the check that enforces blank spacing after {@code if} chains.
     */
    public IfFollowingSpacingCheck() {
    }

    /**
     * Returns the token types that should be inspected for blank spacing after an {@code if} chain.
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
     * Returns the token types that should be inspected for blank spacing after an {@code if} chain.
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
     * Reports an {@code if} chain that is followed immediately by another statement.
     *
     * @param ast the {@code if} statement being visited.
     */
    @Override
    public void visitToken(DetailAST ast) {
        if (ast.getParent() != null && ast.getParent().getType() == TokenTypes.LITERAL_ELSE) {
            return;
        }

        DetailAST nextSibling = StatementAstNavigator.getNextSibling(ast);

        if (nextSibling != null && nextSibling.getType() == TokenTypes.LITERAL_IF) {
            return;
        }

        DetailAST terminalNode = StatementAstNavigator.getLastDescendant(ast);

        if (terminalNode != null
                && !BlankLineAnalyzer.hasBlankLineAfter(terminalNode, getFileContents())) {
            log(ast, HelenaMessageIds.IF_FOLLOWING_SPACING);
        }
    }
}
