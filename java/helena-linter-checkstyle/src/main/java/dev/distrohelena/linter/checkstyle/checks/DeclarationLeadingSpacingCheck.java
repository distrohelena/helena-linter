package dev.distrohelena.linter.checkstyle.checks;

import com.puppycrawl.tools.checkstyle.api.AbstractCheck;
import com.puppycrawl.tools.checkstyle.api.DetailAST;
import com.puppycrawl.tools.checkstyle.api.TokenTypes;
import dev.distrohelena.linter.checkstyle.diagnostics.HelenaMessageIds;
import dev.distrohelena.linter.checkstyle.helpers.BlankLineAnalyzer;
import dev.distrohelena.linter.checkstyle.helpers.StatementAstNavigator;

/**
 * Requires a blank line before local declarations that follow non-declaration statements.
 */
public final class DeclarationLeadingSpacingCheck extends AbstractCheck {

    /**
     * Creates the check that enforces leading spacing before local declarations.
     */
    public DeclarationLeadingSpacingCheck() {
    }

    /**
     * Returns the token types that should be inspected for spacing before local declarations.
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
     * Returns the token types that should be inspected for spacing before local declarations.
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
     * Reports a local declaration that follows a non-declaration sibling without a blank line.
     *
     * @param ast the declaration statement being visited.
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
        } else if (previousSibling.getType() == TokenTypes.VARIABLE_DEF) {
            return;
        } else if (BlankLineAnalyzer.hasBlankLineBetween(previousSibling, ast, getFileContents())) {
            return;
        }

        log(ast, HelenaMessageIds.DECLARATION_LEADING_SPACING);
    }
}
