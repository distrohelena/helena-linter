package dev.distrohelena.linter.checkstyle.checks;

import com.puppycrawl.tools.checkstyle.api.AbstractCheck;
import com.puppycrawl.tools.checkstyle.api.DetailAST;
import com.puppycrawl.tools.checkstyle.api.TokenTypes;
import dev.distrohelena.linter.checkstyle.diagnostics.HelenaMessageIds;
import dev.distrohelena.linter.checkstyle.helpers.ControlFlowExitAnalyzer;
import dev.distrohelena.linter.checkstyle.helpers.StatementAstNavigator;

/**
 * Reports wrapped happy-path and {@code if / else} control flow that should become a guard clause.
 */
public final class EarlyReturnCheck extends AbstractCheck {

    /**
     * Creates the check that identifies supported early-return guard-clause opportunities.
     */
    public EarlyReturnCheck() {
    }

    /**
     * Returns the token types that should be inspected for early-return opportunities.
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
     * Returns the token types that should be inspected for early-return opportunities.
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
     * Reports supported early-return shapes when the current {@code if} statement can be inverted
     * into a guard clause.
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
        } else if (parent.getType() == TokenTypes.LITERAL_ELSE) {
            return;
        }

        DetailAST elseNode = ast.findFirstToken(TokenTypes.LITERAL_ELSE);

        if (elseNode == null) {
            checkWrappedHappyPath(ast);
        } else {
            checkIfElsePattern(ast, elseNode);
        }
    }

    /**
     * Reports a wrapped happy-path branch when the block is followed by a definite exit.
     *
     * @param ifStatement the {@code if} statement to inspect.
     */
    private void checkWrappedHappyPath(DetailAST ifStatement) {
        DetailAST thenBranch = findThenBranch(ifStatement);

        if (thenBranch == null) {
            return;
        } else if (thenBranch.getType() != TokenTypes.SLIST) {
            return;
        } else if (!hasMeaningfulStatement(thenBranch)) {
            return;
        } else if (ControlFlowExitAnalyzer.doesStatementDefinitelyExit(thenBranch)) {
            return;
        }

        DetailAST nextStatement = StatementAstNavigator.getNextSibling(ifStatement);

        if (nextStatement == null) {
            return;
        } else if (!ControlFlowExitAnalyzer.doesStatementDefinitelyExit(nextStatement)) {
            return;
        }

        log(ifStatement, HelenaMessageIds.EARLY_RETURN);
    }

    /**
     * Reports an {@code if / else} branch when exactly one branch definitely exits.
     *
     * @param ifStatement the {@code if} statement to inspect.
     * @param elseNode the direct {@code else} node beneath the inspected {@code if} statement.
     */
    private void checkIfElsePattern(DetailAST ifStatement, DetailAST elseNode) {
        DetailAST thenBranch = findThenBranch(ifStatement);
        DetailAST elseBranch = resolveBranchRoot(elseNode.getFirstChild());

        if (thenBranch == null) {
            return;
        } else if (elseBranch == null) {
            return;
        }

        boolean thenBranchExits = ControlFlowExitAnalyzer.doesStatementDefinitelyExit(thenBranch);
        boolean elseBranchExits = ControlFlowExitAnalyzer.doesStatementDefinitelyExit(elseBranch);

        if (thenBranchExits == elseBranchExits) {
            return;
        } else if (thenBranchExits && !hasMeaningfulStatement(elseBranch)) {
            return;
        } else if (elseBranchExits && !hasMeaningfulStatement(thenBranch)) {
            return;
        }

        log(ifStatement, HelenaMessageIds.EARLY_RETURN);
    }

    /**
     * Locates the branch statement that follows the condition portion of an {@code if} statement.
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

    /**
     * Resolves the immediate statement root for an {@code else} branch while skipping ignorable trivia.
     *
     * @param branchCandidate the first child beneath the {@code else} node.
     * @return the first real branch node, or {@code null} when none exists.
     */
    private static DetailAST resolveBranchRoot(DetailAST branchCandidate) {
        DetailAST branch = branchCandidate;

        while (branch != null && isIgnorableBranchNode(branch)) {
            branch = branch.getNextSibling();
        }

        return branch;
    }

    /**
     * Determines whether a sibling node is structural noise instead of a statement root.
     *
     * @param node the sibling node to inspect.
     * @return {@code true} when the node should be skipped during branch resolution; otherwise {@code false}.
     */
    private static boolean isIgnorableBranchNode(DetailAST node) {
        int tokenType = node.getType();

        return tokenType == TokenTypes.LPAREN
                || tokenType == TokenTypes.RPAREN
                || tokenType == TokenTypes.SINGLE_LINE_COMMENT
                || tokenType == TokenTypes.BLOCK_COMMENT_BEGIN
                || tokenType == TokenTypes.BLOCK_COMMENT_END
                || tokenType == TokenTypes.COMMENT_CONTENT;
    }

    /**
     * Determines whether a branch contains a statement that can be hoisted or preserved.
     *
     * @param statement the branch root to inspect.
     * @return {@code true} when the branch contains executable content; otherwise {@code false}.
     */
    private static boolean hasMeaningfulStatement(DetailAST statement) {
        if (statement == null) {
            return false;
        } else if (statement.getType() != TokenTypes.SLIST) {
            return statement.getType() != TokenTypes.EMPTY_STAT;
        }

        for (DetailAST child = statement.getFirstChild(); child != null; child = child.getNextSibling()) {
            if (!isIgnorableBlockChild(child)) {
                return true;
            }
        }

        return false;
    }

    /**
     * Determines whether a block child is structural noise instead of executable content.
     *
     * @param node the block child to inspect.
     * @return {@code true} when the node can be ignored while checking for branch content; otherwise {@code false}.
     */
    private static boolean isIgnorableBlockChild(DetailAST node) {
        int tokenType = node.getType();

        return tokenType == TokenTypes.LCURLY
                || tokenType == TokenTypes.RCURLY
                || tokenType == TokenTypes.SEMI
                || tokenType == TokenTypes.EMPTY_STAT
                || tokenType == TokenTypes.SINGLE_LINE_COMMENT
                || tokenType == TokenTypes.BLOCK_COMMENT_BEGIN
                || tokenType == TokenTypes.BLOCK_COMMENT_END
                || tokenType == TokenTypes.COMMENT_CONTENT;
    }
}
