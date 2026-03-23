package dev.distrohelena.linter.checkstyle.checks;

import com.puppycrawl.tools.checkstyle.api.AbstractCheck;
import com.puppycrawl.tools.checkstyle.api.DetailAST;
import com.puppycrawl.tools.checkstyle.api.TokenTypes;
import dev.distrohelena.linter.checkstyle.diagnostics.HelenaMessageIds;
import dev.distrohelena.linter.checkstyle.helpers.BlankLineAnalyzer;
import dev.distrohelena.linter.checkstyle.helpers.StatementAstNavigator;

/**
 * Requires a blank line after local declarations before the next sibling statement.
 */
public final class DeclarationSpacingCheck extends AbstractCheck {

    /**
     * Creates the check that enforces spacing after local declarations.
     */
    public DeclarationSpacingCheck() {
    }

    /**
     * Returns the token types that should be inspected for spacing after local declarations.
     *
     * @return the token types the check should visit.
     */
    @Override
    public int[] getDefaultTokens() {
        return new int[] {
                TokenTypes.VARIABLE_DEF
        };
    }

    /**
     * Returns the token types that should be inspected for spacing after local declarations.
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
     * Reports a local declaration that runs directly into the next sibling statement.
     *
     * @param ast the declaration statement being visited.
     */
    @Override
    public void visitToken(DetailAST ast) {
        DetailAST parent = ast.getParent();

        if (parent == null) {
            return;
        } else if (parent.getType() != TokenTypes.SLIST) {
            return;
        }

        DetailAST nextSibling = StatementAstNavigator.getNextSibling(ast);

        if (nextSibling == null) {
            return;
        } else if (BlankLineAnalyzer.hasBlankLineBetween(ast, nextSibling, getFileContents())) {
            return;
        }

        log(ast, HelenaMessageIds.DECLARATION_SPACING);
    }
}
