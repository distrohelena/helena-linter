package dev.distrohelena.linter.checkstyle.checks;

import com.puppycrawl.tools.checkstyle.api.AbstractCheck;
import com.puppycrawl.tools.checkstyle.api.DetailAST;
import com.puppycrawl.tools.checkstyle.api.TokenTypes;
import dev.distrohelena.linter.checkstyle.diagnostics.HelenaMessageIds;
import dev.distrohelena.linter.checkstyle.helpers.BlankLineAnalyzer;
import dev.distrohelena.linter.checkstyle.helpers.StatementAstNavigator;

/**
 * Requires a blank line before control-flow exit statements.
 */
public final class ExitSpacingCheck extends AbstractCheck {

    /**
     * Creates the check that enforces spacing before exit statements.
     */
    public ExitSpacingCheck() {
    }

    /**
     * Returns the token types that should be inspected for spacing before exit statements.
     *
     * @return the token types the check should visit.
     */
    @Override
    public int[] getDefaultTokens() {
        return new int[] {
                TokenTypes.LITERAL_RETURN,
                TokenTypes.LITERAL_THROW,
                TokenTypes.LITERAL_BREAK,
                TokenTypes.LITERAL_CONTINUE
        };
    }

    /**
     * Returns the token types that should be inspected for spacing before exit statements.
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
     * Reports an exit statement that is not visually separated from the previous sibling statement.
     *
     * @param ast the exit statement being visited.
     */
    @Override
    public void visitToken(DetailAST ast) {
        DetailAST previousSibling = StatementAstNavigator.getPreviousSibling(ast);

        if (previousSibling == null) {
            return;
        } else if (BlankLineAnalyzer.hasBlankLineBetween(previousSibling, ast, getFileContents())) {
            return;
        }

        log(ast, HelenaMessageIds.EXIT_SPACING);
    }
}
