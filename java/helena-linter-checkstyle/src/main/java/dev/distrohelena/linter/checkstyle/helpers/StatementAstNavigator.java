package dev.distrohelena.linter.checkstyle.helpers;

import com.puppycrawl.tools.checkstyle.api.DetailAST;
import com.puppycrawl.tools.checkstyle.api.TokenTypes;

/**
 * Provides small navigation helpers for walking across Checkstyle sibling AST nodes.
 */
public final class StatementAstNavigator {

    /**
     * Prevents direct instantiation.
     */
    private StatementAstNavigator() {
    }

    /**
     * Gets the previous statement sibling for the supplied AST node.
     *
     * <p>Hidden comment nodes injected by {@code appendHiddenCommentNodes()} are skipped so callers
     * can reason about actual statement adjacency.</p>
     *
     * @param node the node whose previous statement sibling should be resolved.
     * @return the previous statement sibling node, or {@code null} when the node has none.
     */
    public static DetailAST getPreviousSibling(DetailAST node) {
        if (node == null) {
            return null;
        }

        DetailAST previousSibling = node.getPreviousSibling();

        while (previousSibling != null && isIgnorableSibling(previousSibling)) {
            previousSibling = previousSibling.getPreviousSibling();
        }

        return previousSibling;
    }

    /**
     * Gets the next statement sibling for the supplied AST node.
     *
     * <p>Hidden comment nodes injected by {@code appendHiddenCommentNodes()} are skipped so callers
     * can reason about actual statement adjacency.</p>
     *
     * @param node the node whose next statement sibling should be resolved.
     * @return the next statement sibling node, or {@code null} when the node has none.
     */
    public static DetailAST getNextSibling(DetailAST node) {
        if (node == null) {
            return null;
        }

        DetailAST nextSibling = node.getNextSibling();

        while (nextSibling != null && isIgnorableSibling(nextSibling)) {
            nextSibling = nextSibling.getNextSibling();
        }

        return nextSibling;
    }

    /**
     * Determines whether two nodes share the same parent node.
     *
     * @param first the first node to compare.
     * @param second the second node to compare.
     * @return {@code true} when both nodes belong to the same sibling list; otherwise {@code false}.
     */
    public static boolean areSiblings(DetailAST first, DetailAST second) {
        if (first == null || second == null) {
            return false;
        } else if (first.getParent() == null) {
            return false;
        } else if (second.getParent() == null) {
            return false;
        }

        return first.getParent() == second.getParent();
    }

    /**
     * Determines whether the supplied AST node represents a hidden comment node.
     *
     * @param node the node to inspect.
     * @return {@code true} when the node is comment trivia; otherwise {@code false}.
     */
    private static boolean isIgnorableSibling(DetailAST node) {
        int tokenType = node.getType();

        return tokenType == TokenTypes.SINGLE_LINE_COMMENT
                || tokenType == TokenTypes.BLOCK_COMMENT_BEGIN
                || tokenType == TokenTypes.BLOCK_COMMENT_END
                || tokenType == TokenTypes.COMMENT_CONTENT
                || tokenType == TokenTypes.SEMI
                || tokenType == TokenTypes.LCURLY
                || tokenType == TokenTypes.RCURLY;
    }
}
