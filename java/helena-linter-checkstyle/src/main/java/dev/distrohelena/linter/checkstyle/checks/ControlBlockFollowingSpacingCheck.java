package dev.distrohelena.linter.checkstyle.checks;

import com.puppycrawl.tools.checkstyle.api.AbstractCheck;
import com.puppycrawl.tools.checkstyle.api.DetailAST;
import com.puppycrawl.tools.checkstyle.api.TokenTypes;
import dev.distrohelena.linter.checkstyle.diagnostics.HelenaMessageIds;
import dev.distrohelena.linter.checkstyle.helpers.BlankLineAnalyzer;
import dev.distrohelena.linter.checkstyle.helpers.StatementAstNavigator;

/**
 * Requires a blank line after completed control blocks such as loops, {@code switch}, and {@code try}.
 */
public final class ControlBlockFollowingSpacingCheck extends AbstractCheck {

    /**
     * Creates the check that enforces blank spacing after non-{@code if} control blocks.
     */
    public ControlBlockFollowingSpacingCheck() {
    }

    /**
     * Returns the token types that should be inspected for blank spacing after a control block.
     *
     * @return the token types the check should visit.
     */
    @Override
    public int[] getDefaultTokens() {
        return new int[] {
                TokenTypes.LITERAL_FOR,
                TokenTypes.LITERAL_WHILE,
                TokenTypes.LITERAL_DO,
                TokenTypes.LITERAL_SWITCH,
                TokenTypes.LITERAL_TRY
        };
    }

    /**
     * Returns the token types that should be inspected for blank spacing after a control block.
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
     * Reports a control block that is followed immediately by another statement.
     *
     * @param ast the control statement being visited.
     */
    @Override
    public void visitToken(DetailAST ast) {
        DetailAST terminalNode = StatementAstNavigator.getLastDescendant(ast);

        if (terminalNode != null
                && !BlankLineAnalyzer.hasBlankLineAfter(terminalNode, getFileContents())) {
            log(ast, HelenaMessageIds.CONTROL_BLOCK_FOLLOWING_SPACING);
        }
    }
}
