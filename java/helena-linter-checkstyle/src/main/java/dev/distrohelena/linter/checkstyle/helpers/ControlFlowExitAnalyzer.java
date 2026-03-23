package dev.distrohelena.linter.checkstyle.helpers;

import com.puppycrawl.tools.checkstyle.api.DetailAST;
import com.puppycrawl.tools.checkstyle.api.TokenTypes;

/**
 * Evaluates whether a statement definitely exits the current control-flow path.
 */
public final class ControlFlowExitAnalyzer {

    /**
     * Prevents direct instantiation.
     */
    private ControlFlowExitAnalyzer() {
    }

    /**
     * Determines whether the supplied statement definitely exits via {@code return}, {@code throw},
     * {@code break}, or {@code continue}.
     *
     * @param statement the statement to evaluate.
     * @return {@code true} when the statement definitely exits the current path; otherwise {@code false}.
     */
    public static boolean doesStatementDefinitelyExit(DetailAST statement) {
        if (statement == null) {
            return false;
        }

        int tokenType = statement.getType();

        if (tokenType == TokenTypes.LITERAL_RETURN
                || tokenType == TokenTypes.LITERAL_THROW
                || tokenType == TokenTypes.LITERAL_BREAK
                || tokenType == TokenTypes.LITERAL_CONTINUE) {
            return true;
        } else if (tokenType == TokenTypes.SLIST) {
            return doesBlockDefinitelyExit(statement);
        } else if (tokenType == TokenTypes.LITERAL_IF) {
            return doesIfStatementDefinitelyExit(statement);
        }

        return false;
    }

    /**
     * Determines whether a block definitely exits by walking its child statements in order.
     *
     * @param block the block to evaluate.
     * @return {@code true} when an executed statement definitely exits the block; otherwise {@code false}.
     */
    private static boolean doesBlockDefinitelyExit(DetailAST block) {
        for (DetailAST child = block.getFirstChild(); child != null; child = child.getNextSibling()) {
            if (doesStatementDefinitelyExit(child)) {
                return true;
            }
        }

        return false;
    }

    /**
     * Determines whether both branches of an {@code if} statement definitely exit.
     *
     * @param ifStatement the {@code if} statement to evaluate.
     * @return {@code true} when both branches definitely exit; otherwise {@code false}.
     */
    private static boolean doesIfStatementDefinitelyExit(DetailAST ifStatement) {
        DetailAST elseNode = ifStatement.findFirstToken(TokenTypes.LITERAL_ELSE);

        if (elseNode == null) {
            return false;
        }

        DetailAST thenBranch = findIfBranch(ifStatement);
        DetailAST elseBranch = elseNode.getFirstChild();

        return doesStatementDefinitelyExit(thenBranch) && doesStatementDefinitelyExit(elseBranch);
    }

    /**
     * Locates the branch statement that follows the condition portion of an {@code if} statement.
     *
     * @param ifStatement the {@code if} statement to inspect.
     * @return the first branch statement beneath the {@code if} node.
     */
    private static DetailAST findIfBranch(DetailAST ifStatement) {
        DetailAST child = ifStatement.getFirstChild();

        while (child != null && child.getType() != TokenTypes.EXPR) {
            child = child.getNextSibling();
        }

        if (child == null) {
            return null;
        }

        DetailAST branch = child.getNextSibling();

        while (branch != null
                && branch.getType() != TokenTypes.SLIST
                && branch.getType() != TokenTypes.LITERAL_IF
                && branch.getType() != TokenTypes.LITERAL_RETURN
                && branch.getType() != TokenTypes.LITERAL_THROW
                && branch.getType() != TokenTypes.LITERAL_BREAK
                && branch.getType() != TokenTypes.LITERAL_CONTINUE
                && branch.getType() != TokenTypes.LITERAL_FOR
                && branch.getType() != TokenTypes.LITERAL_WHILE
                && branch.getType() != TokenTypes.LITERAL_DO
                && branch.getType() != TokenTypes.LITERAL_SWITCH
                && branch.getType() != TokenTypes.LITERAL_TRY
                && branch.getType() != TokenTypes.LITERAL_SYNCHRONIZED) {
            branch = branch.getNextSibling();
        }

        return branch;
    }
}
