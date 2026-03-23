package dev.distrohelena.linter.checkstyle.checks;

import com.puppycrawl.tools.checkstyle.api.AbstractCheck;
import com.puppycrawl.tools.checkstyle.api.DetailAST;
import com.puppycrawl.tools.checkstyle.api.TokenTypes;
import dev.distrohelena.linter.checkstyle.diagnostics.HelenaMessageIds;
import dev.distrohelena.linter.checkstyle.helpers.BlankLineAnalyzer;
import dev.distrohelena.linter.checkstyle.helpers.StatementAstNavigator;

/**
 * Requires a blank line before top-level {@code if} statements that follow sibling statements.
 */
public final class IfLeadingSpacingCheck extends AbstractCheck {

    /**
     * Creates the check that enforces spacing before {@code if} statements.
     */
    public IfLeadingSpacingCheck() {
    }

    /**
     * Returns the token types that should be inspected for spacing before {@code if} statements.
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
     * Returns the token types that should be inspected for spacing before {@code if} statements.
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
     * Reports an {@code if} statement that is not visually separated from the previous sibling statement.
     *
     * @param ast the {@code if} statement being visited.
     */
    @Override
    public void visitToken(DetailAST ast) {
        DetailAST parent = ast.getParent();

        if (parent == null) {
            return;
        } else if (parent.getType() == TokenTypes.LITERAL_ELSE) {
            return;
        } else if (parent.getType() != TokenTypes.SLIST) {
            return;
        }

        DetailAST previousSibling = StatementAstNavigator.getPreviousSibling(ast);

        if (previousSibling == null) {
            return;
        } else if (BlankLineAnalyzer.hasBlankLineBetween(previousSibling, ast, getFileContents())) {
            return;
        }

        log(ast, HelenaMessageIds.IF_LEADING_SPACING);
    }
}
