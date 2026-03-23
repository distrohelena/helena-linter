package dev.distrohelena.linter.checkstyle.checks;

import com.puppycrawl.tools.checkstyle.api.AbstractCheck;
import com.puppycrawl.tools.checkstyle.api.DetailAST;
import com.puppycrawl.tools.checkstyle.api.TokenTypes;
import dev.distrohelena.linter.checkstyle.diagnostics.HelenaMessageIds;

/**
 * Requires non-empty Java code blocks to use multiline layout.
 */
public final class MultilineBlockLayoutCheck extends AbstractCheck {

    /**
     * Creates the check that enforces multiline layout for non-empty blocks.
     */
    public MultilineBlockLayoutCheck() {
    }

    /**
     * Returns the token types that should be inspected for multiline block layout.
     *
     * @return the token types the check should visit.
     */
    @Override
    public int[] getDefaultTokens() {
        return new int[] {
                TokenTypes.SLIST
        };
    }

    /**
     * Returns the token types that should be inspected for multiline block layout.
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
     * Reports a non-empty block that opens and closes on the same line.
     *
     * @param ast the block being visited.
     */
    @Override
    public void visitToken(DetailAST ast) {
        if (ast.getType() != TokenTypes.SLIST || isEmptyBlock(ast) || !isSingleLineBlock(ast)) {
            return;
        }

        log(ast, HelenaMessageIds.MULTILINE_BLOCK_LAYOUT);
    }

    /**
     * Determines whether the supplied block contains at least one statement.
     *
     * @param block the block being inspected.
     * @return {@code true} when the block is empty; otherwise {@code false}.
     */
    private static boolean isEmptyBlock(DetailAST block) {
        DetailAST firstChild = block.getFirstChild();

        return firstChild == null || firstChild.getType() == TokenTypes.RCURLY;
    }

    /**
     * Determines whether the supplied block starts and ends on the same source line.
     *
     * @param block the block being inspected.
     * @return {@code true} when the block layout stays on a single line; otherwise {@code false}.
     */
    private static boolean isSingleLineBlock(DetailAST block) {
        DetailAST rightBrace = block.getLastChild();

        if (rightBrace == null) {
            return false;
        }

        return block.getLineNo() == rightBrace.getLineNo();
    }
}
